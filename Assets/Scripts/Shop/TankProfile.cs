using UnityEngine;

namespace AquariumShop
{
    [CreateAssetMenu(fileName = "TankProfile", menuName = "Aquarium Shop/Tank Profile")]
    public class TankProfile : ScriptableObject
    {
        public string id = "small";
        public string displayName = "Small";
        public int capacity = 6;
        public int litres = 45;
    }
}