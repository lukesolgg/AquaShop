using UnityEngine;

namespace AquariumShop
{
    public class TankAnchor : MonoBehaviour
    {
        public string id;
        public string displayName;
        public TankSegment segment = TankSegment.Back;
        public TankProfile profile;
        public Transform waterVolume;
    }
}