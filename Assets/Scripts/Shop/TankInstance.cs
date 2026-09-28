using System.Collections.Generic;

namespace AquariumShop
{
    [System.Serializable]
    public class TankInstance
    {
        public string id;
        public string displayName;
        public TankSegment segment;
        public int capacity = 6;
        public List<FishInstance> fish = new List<FishInstance>();

        public FishSpecies LockedSpecies
        {
            get
            {
                foreach (var f in fish)
                    if (f != null && f.IsAlive && f.species != null) return f.species;
                return null;
            }
        }

        public bool IsEmpty => AliveCount == 0;

        public int UsedSpace
        {
            get
            {
                int used = 0;
                foreach (var f in fish)
                    if (f != null && f.IsAlive) used += f.Space;
                return used;
            }
        }

        public int AliveCount
        {
            get
            {
                int n = 0;
                foreach (var f in fish)
                    if (f != null && f.IsAlive) n++;
                return n;
            }
        }

        public float AverageHunger
        {
            get
            {
                int n = 0;
                float sum = 0f;
                foreach (var f in fish)
                {
                    if (f == null || !f.IsAlive) continue;
                    sum += f.hunger;
                    n++;
                }
                return n == 0 ? 0f : sum / n;
            }
        }

        public bool Accepts(FishSpecies species)
        {
            if (species == null) return false;
            var locked = LockedSpecies;
            if (locked != null && locked != species) return false;
            return UsedSpace + species.spaceRequired <= capacity;
        }

        public bool Add(FishSpecies species)
        {
            if (!Accepts(species)) return false;
            fish.Add(new FishInstance(species));
            return true;
        }

        public int CountAlive(FishSpecies species)
        {
            int n = 0;
            foreach (var f in fish)
                if (f != null && f.IsAlive && f.species == species) n++;
            return n;
        }

        public bool RemoveFirstAlive(FishSpecies species)
        {
            for (int i = 0; i < fish.Count; i++)
            {
                if (fish[i] != null && fish[i].IsAlive && fish[i].species == species)
                {
                    fish.RemoveAt(i);
                    return true;
                }
            }
            return false;
        }

        public int RemoveUpTo(FishSpecies species, int qty)
        {
            int removed = 0;
            for (int i = fish.Count - 1; i >= 0 && removed < qty; i--)
            {
                if (fish[i] != null && fish[i].IsAlive && fish[i].species == species)
                {
                    fish.RemoveAt(i);
                    removed++;
                }
            }
            return removed;
        }

        public void TickHour()
        {
            foreach (var f in fish)
            {
                if (f == null || !f.IsAlive || f.species == null) continue;
                f.hunger += f.species.hungerPerHour;
            }
            fish.RemoveAll(f => f == null || !f.IsAlive);
        }

        public void Feed()
        {
            foreach (var f in fish)
                if (f != null && f.IsAlive) f.hunger = 0f;
        }
    }
}