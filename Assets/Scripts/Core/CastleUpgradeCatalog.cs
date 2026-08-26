using System.Collections.Generic;
using UnityEngine;

namespace WormWars.Core
{
    // A shoppable set of CastleUpgradeDefinitions. Designers can author real .asset instances
    // via the Create > WormWars > Castle Upgrade menu and drop them in `upgrades`, but nothing
    // requires that — Default() builds a starter catalog entirely in code at runtime, the same
    // way ProceduralSprite/ProceduralPrimitive3D generate placeholder visuals instead of
    // depending on imported assets. That keeps the castle-builder skill demoable without
    // anyone having opened the Unity Editor first.
    [CreateAssetMenu(menuName = "WormWars/Castle Upgrade Catalog", fileName = "NewCastleUpgradeCatalog")]
    public class CastleUpgradeCatalog : ScriptableObject
    {
        public List<CastleUpgradeDefinition> upgrades = new List<CastleUpgradeDefinition>();

        public CastleUpgradeDefinition Find(string upgradeId)
        {
            foreach (var u in upgrades)
                if (u != null && u.upgradeId == upgradeId) return u;
            return null;
        }

        // Starter IAP-shaped catalog: one cosmetic per upgrade category, plus the tier-2
        // structure upgrade called out in the handoff spec's Castle HP table. Prices are
        // placeholders pending real store integration and monetization design.
        public static CastleUpgradeCatalog Default()
        {
            var catalog = CreateInstance<CastleUpgradeCatalog>();
            catalog.name = "DefaultCastleUpgradeCatalog";

            catalog.upgrades.Add(Make("wall_skin_slate", "Slate Walls", CastleUpgradeCategory.WallSkin, 1.99f, u =>
            {
                u.overridesWallColor = true;
                u.wallColor = DesignTokens.Color_.StoneDark;
                u.overridesInteriorColor = true;
                u.interiorColor = DesignTokens.Color_.InteriorDark;
            }));

            catalog.upgrades.Add(Make("tower_caps_gold", "Gold Tower Caps", CastleUpgradeCategory.TowerCaps, 2.99f, u =>
            {
                u.overridesTowerCapColor = true;
                u.towerCapColor = DesignTokens.Color_.CastleHpStart;
            }));

            catalog.upgrades.Add(Make("banner_team", "Team Banner", CastleUpgradeCategory.Banner, 0.99f, u =>
            {
                u.addsBanner = true;
                u.bannerColor = DesignTokens.Color_.TeamA;
            }));

            catalog.upgrades.Add(Make("ledge_trim_wood", "Reinforced Ledges", CastleUpgradeCategory.LedgeTrim, 1.49f, u =>
            {
                u.overridesLedgeColor = true;
                u.ledgeColor = DesignTokens.Color_.Wood;
            }));

            catalog.upgrades.Add(Make("structure_tier_2", "Castle Tier 2", CastleUpgradeCategory.StructureTier, 4.99f, u =>
            {
                u.structureTier = 2;
                u.overridesWallColor = true;
                u.wallColor = DesignTokens.Color_.Stone;
            }));

            return catalog;
        }

        static CastleUpgradeDefinition Make(string id, string name, CastleUpgradeCategory category, float priceUsd, System.Action<CastleUpgradeDefinition> configure)
        {
            var u = CreateInstance<CastleUpgradeDefinition>();
            u.name = id;
            u.upgradeId = id;
            u.displayName = name;
            u.category = category;
            u.priceUsd = priceUsd;
            u.storeProductId = $"wormwars.castle.{id}";
            configure(u);
            return u;
        }
    }
}
