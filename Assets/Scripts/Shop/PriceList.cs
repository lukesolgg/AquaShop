using System.Collections.Generic;
using UnityEngine;

namespace AquariumShop
{
    [System.Serializable]
    public class PriceEntry
    {
        public FishSpecies species;
        public float wholesale = 2f;
        public float retail = 5f;
        public int weight = 10;
    }

    [CreateAssetMenu(fileName = "PriceList", menuName = "Aquarium Shop/Price List")]
    public class PriceList : ScriptableObject
    {
        public float foodUnitCost = 0.4f;
        public List<PriceEntry> entries = new List<PriceEntry>();

        public PriceEntry Get(FishSpecies species)
        {
            if (species == null) return null;
            foreach (var e in entries)
                if (e != null && e.species == species) return e;
            return null;
        }

        public PriceEntry GetById(string speciesId)
        {
            if (string.IsNullOrEmpty(speciesId)) return null;
            foreach (var e in entries)
                if (e?.species != null && e.species.id == speciesId) return e;
            return null;
        }

        public float Wholesale(FishSpecies species)
        {
            var e = Get(species);
            if (e != null) return e.wholesale;
            return species != null ? species.wholesalePrice : 0f;
        }

        public float Retail(FishSpecies species)
        {
            var e = Get(species);
            if (e != null) return e.retail;
            return species != null ? species.salePrice : 0f;
        }

        public FishSpecies PickWeighted()
        {
            int total = 0;
            foreach (var e in entries)
            {
                if (e?.species == null || e.weight <= 0) continue;
                total += e.weight;
            }
            if (total <= 0) return null;

            int roll = Random.Range(0, total);
            foreach (var e in entries)
            {
                if (e?.species == null || e.weight <= 0) continue;
                roll -= e.weight;
                if (roll < 0) return e.species;
            }
            return null;
        }
    }
}