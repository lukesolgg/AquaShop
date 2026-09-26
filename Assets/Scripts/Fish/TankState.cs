using System.Collections.Generic;

namespace AquariumShop
{
    [System.Serializable]
    public class TankState
    {
        public int capacity = 6;
        public List<FishInstance> fish = new List<FishInstance>();

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

        public bool CanAdd(FishSpecies species)
        {
            if (species == null) return false;
            return UsedSpace + species.spaceRequired <= capacity;
        }

        public bool Add(FishSpecies species)
        {
            if (!CanAdd(species)) return false;
            fish.Add(new FishInstance(species));
            return true;
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
    }
}
