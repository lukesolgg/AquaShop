using System.Collections.Generic;
using UnityEngine;

namespace AquariumShop
{
    public class TankView : MonoBehaviour
    {
        [SerializeField] Transform waterVolume;

        readonly List<GameObject> _spawned = new List<GameObject>();
        int _lastCount = -1;

        void Update()
        {
            var shop = GameManager.Instance?.shop;
            if (shop == null) return;

            int count = shop.tank.AliveCount;
            if (count == _lastCount) return;
            _lastCount = count;
            Rebuild(shop.tank);
        }

        public void Rebuild(TankState tank)
        {
            foreach (var go in _spawned)
                if (go != null) Destroy(go);
            _spawned.Clear();

            if (tank == null) return;

            int i = 0;
            foreach (var fish in tank.fish)
            {
                if (fish == null || !fish.IsAlive || fish.species == null) continue;

                // Use the species prefab if assigned, otherwise skip
                GameObject prefabToSpawn = fish.species.fishPrefab;
                if (prefabToSpawn == null) continue;

                Transform parent = waterVolume != null ? waterVolume : transform;
                var go = Instantiate(prefabToSpawn, parent);
                
                go.name = fish.species.id;
                go.transform.localPosition = Slot(i);
                
                // Adjust scale if needed for your specific clownfish model size
                go.transform.localScale = Vector3.one * 0.15f;

                _spawned.Add(go);
                i++;
            }
        }

        static Vector3 Slot(int i)
        {
            float x = -0.25f + (i % 3) * 0.25f;
            float y = -0.15f + (i / 3) * 0.2f;
            float z = (i % 2 == 0) ? -0.1f : 0.1f;
            return new Vector3(x, y, z);
        }
    }
}