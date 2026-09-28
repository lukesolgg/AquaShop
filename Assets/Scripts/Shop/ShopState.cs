using System.Collections.Generic;

namespace AquariumShop
{
    [System.Serializable]
    public class ShopState
    {
        public float money = 2500f;
        public int foodUnits = 40;
        public float foodUnitCost = 0.4f;
        public int level = 1;
        public List<TankInstance> tanks = new List<TankInstance>();
        public List<Order> orders = new List<Order>();

        public int OpenOrderCount
        {
            get
            {
                int n = 0;
                foreach (var o in orders)
                    if (o != null && o.IsOpen) n++;
                return n;
            }
        }

        public TankInstance GetTank(string id)
        {
            if (string.IsNullOrEmpty(id)) return null;
            foreach (var t in tanks)
                if (t != null && t.id == id) return t;
            return null;
        }

        public TankInstance PrimaryTank()
        {
            foreach (var t in tanks)
                if (t != null && t.segment == TankSegment.Island) return t;
            return tanks.Count > 0 ? tanks[0] : null;
        }

        public int AliveCount
        {
            get
            {
                int n = 0;
                foreach (var t in tanks)
                    if (t != null) n += t.AliveCount;
                return n;
            }
        }

        public int CountSpecies(FishSpecies species)
        {
            int n = 0;
            foreach (var t in tanks)
                if (t != null) n += t.CountAlive(species);
            return n;
        }

        public bool AnyStarving
        {
            get
            {
                foreach (var t in tanks)
                {
                    if (t == null || t.IsEmpty) continue;
                    if (t.AverageHunger >= 0.7f) return true;
                }
                return false;
            }
        }

        public TankInstance EnsureTank(TankAnchor anchor)
        {
            if (anchor == null || string.IsNullOrEmpty(anchor.id)) return null;
            var tank = GetTank(anchor.id);
            if (tank == null)
            {
                tank = new TankInstance
                {
                    id = anchor.id,
                    displayName = string.IsNullOrEmpty(anchor.displayName) ? anchor.id : anchor.displayName,
                    segment = anchor.segment,
                    capacity = anchor.profile != null ? anchor.profile.capacity : 6
                };
                tanks.Add(tank);
            }
            else
            {
                tank.displayName = string.IsNullOrEmpty(anchor.displayName) ? tank.displayName : anchor.displayName;
                tank.segment = anchor.segment;
                if (anchor.profile != null) tank.capacity = anchor.profile.capacity;
            }
            return tank;
        }

        public bool TryBuyFish(FishSpecies species, string tankId, PriceList prices, out string error)
        {
            error = null;
            if (species == null) { error = "No species."; return false; }
            var tank = GetTank(tankId);
            if (tank == null) { error = "Pick a tank."; return false; }
            float cost = prices != null ? prices.Wholesale(species) : species.wholesalePrice;
            if (money < cost) { error = "Not enough money."; return false; }
            if (!tank.Accepts(species))
            {
                if (tank.LockedSpecies != null && tank.LockedSpecies != species)
                    error = $"{tank.displayName} already holds {tank.LockedSpecies.displayName}.";
                else
                    error = $"{tank.displayName} is full.";
                return false;
            }
            money -= cost;
            tank.Add(species);
            return true;
        }

        public bool TryBuyFood(int units, PriceList prices, out string error)
        {
            error = null;
            float unit = prices != null ? prices.foodUnitCost : foodUnitCost;
            float cost = unit * units;
            if (money < cost) { error = "Not enough money."; return false; }
            money -= cost;
            foodUnits += units;
            foodUnitCost = unit;
            return true;
        }

        public bool TryFeedTank(string tankId, out string error)
        {
            error = null;
            var tank = GetTank(tankId);
            if (tank == null) { error = "No tank."; return false; }
            int alive = tank.AliveCount;
            if (alive == 0) { error = "No fish in that tank."; return false; }
            if (foodUnits < alive) { error = "Not enough food."; return false; }
            foodUnits -= alive;
            tank.Feed();
            return true;
        }

        public bool TryRefuse(int openIndex, out string error)
        {
            error = null;
            var order = GetOpenOrder(openIndex);
            if (order == null) { error = "No order."; return false; }
            order.status = OrderStatus.Refused;
            return true;
        }

        public bool TryFulfill(int openIndex, bool partial, out string error)
        {
            error = null;
            var order = GetOpenOrder(openIndex);
            if (order == null) { error = "No order."; return false; }
            if (order.wanted == null) { error = "Broken order."; return false; }

            int need = order.QtyLeft;
            int have = CountSpecies(order.wanted);
            int take = partial ? System.Math.Min(need, have) : need;
            if (!partial && have < need)
            {
                error = $"Need {need} {order.wanted.displayName}, have {have}.";
                return false;
            }
            if (take <= 0)
            {
                error = $"No {order.wanted.displayName} in any tank.";
                return false;
            }

            int left = take;
            foreach (var tank in tanks)
            {
                if (left <= 0) break;
                if (tank == null) continue;
                left -= tank.RemoveUpTo(order.wanted, left);
            }

            int sold = take - left;
            order.qtyFilled += sold;
            money += order.PayoutPerFish * sold;
            if (order.qtyFilled >= order.qty)
                order.status = OrderStatus.Done;
            return true;
        }

        public Order GetOpenOrder(int index)
        {
            int i = 0;
            foreach (var o in orders)
            {
                if (o == null || !o.IsOpen) continue;
                if (i == index) return o;
                i++;
            }
            return null;
        }

        public void TickHour()
        {
            foreach (var t in tanks)
                t?.TickHour();

            foreach (var o in orders)
            {
                if (o == null || !o.IsOpen) continue;
                o.hoursRemaining -= 1f;
                if (o.hoursRemaining <= 0f)
                {
                    o.status = OrderStatus.Failed;
                    money -= o.penalty;
                    if (money < 0f) money = 0f;
                }
            }
        }
    }
}