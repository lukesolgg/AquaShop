using UnityEngine;

namespace AquariumShop
{
    public class TankAnchor : MonoBehaviour
    {
        [Tooltip("Stable id saved to disk. Example: island_L, back_03")]
        public string id;

        public string displayName;
        public TankSegment segment = TankSegment.Back;
        public TankProfile profile;
        public Transform waterVolume;

        [Tooltip("Player owns this tank on a new game.")]
        public bool ownedFromStart;

        [Tooltip("Upgrade that unlocks this tank. Leave empty if starter.")]
        public string upgradeId;
    }
}