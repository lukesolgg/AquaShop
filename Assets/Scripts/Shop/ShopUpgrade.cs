using UnityEngine;

namespace AquariumShop
{
    [CreateAssetMenu(menuName = "Aquarium Shop/Shop Upgrade", fileName = "Upgrade_")]
    public class ShopUpgrade : ScriptableObject
    {
        [Tooltip("Stable id saved to disk. Example: island_R")]
        public string id;

        public string displayName = "New upgrade";
        [Min(0)] public float cost = 400f;

        [Tooltip("Must already own this upgrade id. Leave empty if none.")]
        public string requiresUpgradeId;

        [Tooltip("TankAnchor ids this purchase unlocks.")]
        public string[] unlockTankIds;

        [Tooltip("0 = do not change shop level. Otherwise set shop.level to this.")]
        public int setsLevel;
    }
}