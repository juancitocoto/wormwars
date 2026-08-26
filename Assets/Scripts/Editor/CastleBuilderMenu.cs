using UnityEditor;
using UnityEngine;

namespace WormWars.Core
{
    // Editor-only entry points for the 3D castle builder — lets a designer drop a castle into
    // the open scene and cycle cosmetic upgrades without pressing Play. Everything here just
    // calls into Castle3DView/CastleUpgradeCatalog; there is no build logic of its own.
    public static class CastleBuilderMenu
    {
        [MenuItem("WormWars/Build Preview Castle")]
        public static void BuildPreviewCastle()
        {
            var go = new GameObject("Castle3D_Preview");
            var controller = go.AddComponent<CastleController>();
            controller.teamId = TeamId.A;

            var view = go.AddComponent<Castle3DView>();
            view.catalog = CastleUpgradeCatalog.Default();
            view.Init();

            Undo.RegisterCreatedObjectUndo(go, "Build Preview Castle");
            Selection.activeGameObject = go;
            SceneView.lastActiveSceneView?.FrameSelected();
        }

        [MenuItem("WormWars/Build Preview Castle With Upgrades")]
        public static void BuildPreviewCastleWithUpgrades()
        {
            var go = new GameObject("Castle3D_Preview_Upgraded");
            var controller = go.AddComponent<CastleController>();
            controller.teamId = TeamId.B;

            var view = go.AddComponent<Castle3DView>();
            var catalog = CastleUpgradeCatalog.Default();
            view.catalog = catalog;
            view.Init();

            // Apply every non-StructureTier cosmetic upgrade at once so all of them are
            // visible side by side for a quick art pass, without needing a shop UI.
            foreach (var upgrade in catalog.upgrades)
            {
                if (upgrade.category == CastleUpgradeCategory.StructureTier) continue;
                view.ApplyUpgrade(upgrade);
            }

            Undo.RegisterCreatedObjectUndo(go, "Build Preview Castle With Upgrades");
            Selection.activeGameObject = go;
            SceneView.lastActiveSceneView?.FrameSelected();
        }
    }
}
