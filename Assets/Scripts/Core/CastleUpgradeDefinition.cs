using UnityEngine;

namespace WormWars.Core
{
    // A single purchasable/unlockable castle cosmetic. This is pure data — no store or
    // purchase-flow logic lives here — so the real IAP integration can be dropped in later
    // (billing SDK, receipt validation, etc.) without touching how castles are built or
    // reskinned. Castle3DView.ApplyUpgrade reads these fields; add a field here + handle it
    // there when a new kind of cosmetic is needed.
    [CreateAssetMenu(menuName = "WormWars/Castle Upgrade", fileName = "NewCastleUpgrade")]
    public class CastleUpgradeDefinition : ScriptableObject
    {
        public string upgradeId = "upgrade_id";
        public string displayName = "New Upgrade";
        [TextArea] public string description;
        public CastleUpgradeCategory category = CastleUpgradeCategory.WallSkin;

        [Header("Store placeholder — wire to real IAP product data later")]
        public string storeProductId;
        public float priceUsd;

        [Header("WallSkin")]
        public bool overridesWallColor;
        public Color wallColor = Color.white;
        public bool overridesInteriorColor;
        public Color interiorColor = Color.white;

        [Header("TowerCaps")]
        public bool overridesTowerCapColor;
        public Color towerCapColor = Color.white;

        [Header("Banner")]
        public bool addsBanner;
        public Color bannerColor = Color.white;

        [Header("LedgeTrim")]
        public bool overridesLedgeColor;
        public Color ledgeColor = Color.white;

        [Header("StructureTier")]
        // Matches the Castle tier / max HP table in worm_battle_handoff_spec.md — a
        // StructureTier upgrade both reskins and calls into CastleController to raise maxHP,
        // so buying a tier upgrade is felt in gameplay, not just cosmetically.
        public int structureTier = 1;
    }
}
