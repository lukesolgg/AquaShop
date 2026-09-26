using System.Collections.Generic;

namespace AquariumShop
{
    [System.Serializable]
    public class ShopState
    {
        public float money = 150f;
        public int foodUnits = 10;
        public float foodUnitCost = 2f;
        public TankState tank = new TankState();
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

        public bool TryBuyFish(FishSpecies species, out string error)
        {
            error = null;
            if (species == null) { error = "No species."; return false; }
            if (money < species.wholesalePrice) { error = "Not enough money."; return false; }
            if (!tank.CanAdd(species)) { error = "Tank is full."; return false; }
            money -= species.wholesalePrice;
            tank.Add(species);
            return true;
        }

        public bool TryBuyFood(int units, out string error)
        {
            error = null;
            float cost = foodUnitCost * units;
            if (money < cost) { error = "Not enough money."; return false; }
            money -= cost;
            foodUnits += units;
            return true;
        }

        public bool TryFeedAll(out string error)
        {
            int alive = tank.AliveCount;
            if (alive == 0) { error = "No fish to feed."; return false; }
            if (foodUnits < alive) { error = "Not enough food."; return false; }
            foodUnits -= alive;
            foreach (var f in tank.fish)
                if (f != null && f.IsAlive) f.hunger = 0f;
            error = null;
            return true;
        }

        public bool TryFulfill(int index, out string error)
        {
            error = null;
            var order = GetOpenOrder(index);
            if (order == null) { error = "No order."; return false; }
            if (order.wanted == null) { error = "Broken order."; return false; }
            if (!tank.RemoveFirstAlive(order.wanted))
            {
                error = $"No {order.wanted.displayName} in the tank.";
                return false;
            }
            money += order.payout;
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
            foreach (var f in tank.fish)
            {
                if (f == null || !f.IsAlive || f.species == null) continue;
                f.hunger += f.species.hungerPerHour;
            }
            tank.fish.RemoveAll(f => f == null || !f.IsAlive);

            foreach (var o in orders)
            {
                if (o == null || !o.IsOpen) continue;
                o.hoursRemaining -= 1f;
                if (o.hoursRemaining <= 0f)
                    o.status = OrderStatus.Failed;
            }
        }
    }
}