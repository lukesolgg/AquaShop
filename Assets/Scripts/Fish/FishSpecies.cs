using UnityEngine;

namespace AquariumShop
{
    [CreateAssetMenu(fileName = "NewFishSpecies", menuName = "Aquarium Shop/Fish Species")]
    public class FishSpecies : ScriptableObject
    {
        [Header("Identification")]
        public string id;
        public string displayName;
        public Color colour = Color.white;

        [Header("Shop & Pricing")]
        public int wholesalePrice = 10;
        public int salePrice = 20;
        public int spaceRequired = 1;

        [Header("Stats")]
        public float hungerPerHour = 0.1f;

        [Header("Visuals & Prefabs")]
        public Sprite icon;
        public GameObject fishPrefab;
    }
}