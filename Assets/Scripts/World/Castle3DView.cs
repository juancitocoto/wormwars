using System.Collections.Generic;
using UnityEngine;

namespace WormWars.Core
{
    // Builds an "open dollhouse" 3D castle shell entirely from procedural primitives (see
    // ProceduralPrimitive3D) — no imported meshes or materials required, matching how
    // CastleView/ProceduralSprite generate the 2D battle screen. The front wall is
    // deliberately never built: that's the "dollhouse cutaway" the handoff spec describes for
    // the 2D castle, carried into 3D. Corner towers stand at all four corners regardless —
    // it's the connecting wall between the two front towers that is missing, not the towers.
    //
    // Each of the three walls that *are* built (Back/Left/Right) gets exactly one ledge
    // mounted on its interior face — a CastleBattleStation worms can be parked on. Three walls,
    // three battle stations; there's no fourth because there's no fourth wall.
    //
    // Cosmetic upgrades (see CastleUpgradeDefinition/CastleUpgradeCatalog) recolor or add to
    // this shell without rebuilding it — ApplyUpgrade only ever touches materials and toggles,
    // so switching skins is instant and ApplyUpgrade can be called freely in a shop/build UI.
    [RequireComponent(typeof(CastleController))]
    public class Castle3DView : MonoBehaviour
    {
        [Header("Shell dimensions (meters)")]
        public float width = 6f;
        public float depth = 5f;
        public float height = 4f;
        public float wallThickness = 0.5f;

        [Header("Towers")]
        public float towerRadius = 0.6f;
        public float towerHeightOverhang = 1f;
        public float towerCapRadius = 0.8f;
        public float towerCapHeight = 0.4f;

        [Header("Crenellations")]
        public int merlonsPerWall = 5;
        public float merlonSize = 0.35f;

        [Header("Battle station ledges")]
        public float ledgeHeight = 1.4f;
        public float ledgeDepth = 0.9f;
        public float ledgeThickness = 0.25f;

        public CastleUpgradeCatalog catalog;

        CastleController _castle;
        bool _initialized;

        Transform _wallsRoot, _towersRoot, _merlonsRoot, _ledgesRoot;
        readonly List<Renderer> _outerWallRenderers = new List<Renderer>();
        readonly List<Renderer> _interiorTrimRenderers = new List<Renderer>();
        readonly List<Renderer> _merlonRenderers = new List<Renderer>();
        readonly List<Renderer> _towerCapRenderers = new List<Renderer>();
        readonly Dictionary<CastleWallSide, Renderer> _ledgeRenderers = new Dictionary<CastleWallSide, Renderer>();
        readonly List<CastleBattleStation> _battleStations = new List<CastleBattleStation>();
        GameObject _bannerRoot;
        Renderer _bannerClothRenderer;

        // One upgrade equipped per category at a time — WallSkin, TowerCaps, Banner, LedgeTrim
        // and StructureTier are independent cosmetic slots, so buying "Gold Tower Caps" doesn't
        // undo an already-equipped "Slate Walls". ApplyUpgrade only ever writes its own
        // upgrade's category slot; Rebuild() re-derives every visual from the full set each
        // time, so slots never fight each other regardless of application order.
        readonly Dictionary<CastleUpgradeCategory, CastleUpgradeDefinition> _equipped = new Dictionary<CastleUpgradeCategory, CastleUpgradeDefinition>();

        // The "true" base wall color for the current equipped set, kept separate from whatever
        // soot tint RefreshDamageStage last blended on top — see RefreshDamageStage for why.
        Color _baseWallColor = DesignTokens.Color_.Stone;

        public IReadOnlyList<CastleBattleStation> BattleStations => _battleStations;

        // AddComponent() runs Awake() synchronously, before a caller gets the chance to set
        // dimension fields — same reasoning as CastleView.Init(), see that file.
        public void Init()
        {
            if (_initialized) return;
            _initialized = true;

            _castle = GetComponent<CastleController>();

            _wallsRoot = NewChild("Walls");
            _towersRoot = NewChild("Towers");
            _merlonsRoot = NewChild("Merlons");
            _ledgesRoot = NewChild("BattleStations");

            BuildFloor();
            BuildWalls();
            BuildTowers();
            BuildMerlons();
            BuildLedgesAndBattleStations();

            _castle.OnStageChanged += _ => RefreshDamageStage();
            RefreshDamageStage();
        }

        Transform NewChild(string name)
        {
            var go = new GameObject(name);
            go.transform.SetParent(transform, false);
            return go.transform;
        }

        void BuildFloor()
        {
            ProceduralPrimitive3D.Block("Floor", transform, new Vector3(width, 0.3f, depth), new Vector3(0f, -0.15f, depth / 2f), DesignTokens.Color_.WoodDark);
        }

        void BuildWalls()
        {
            // Back wall (+Z), Left wall (-X), Right wall (+X). No front wall — the open side.
            BuildWall(CastleWallSide.Back, new Vector3(width, height, wallThickness), new Vector3(0f, height / 2f, depth - wallThickness / 2f),
                new Vector3(width * 0.82f, height * 0.82f, wallThickness * 0.3f), new Vector3(0f, height / 2f, depth - wallThickness - 0.05f));

            BuildWall(CastleWallSide.Left, new Vector3(wallThickness, height, depth), new Vector3(-width / 2f + wallThickness / 2f, height / 2f, depth / 2f),
                new Vector3(wallThickness * 0.3f, height * 0.82f, depth * 0.82f), new Vector3(-width / 2f + wallThickness + 0.05f, height / 2f, depth / 2f));

            BuildWall(CastleWallSide.Right, new Vector3(wallThickness, height, depth), new Vector3(width / 2f - wallThickness / 2f, height / 2f, depth / 2f),
                new Vector3(wallThickness * 0.3f, height * 0.82f, depth * 0.82f), new Vector3(width / 2f - wallThickness - 0.05f, height / 2f, depth / 2f));
        }

        void BuildWall(CastleWallSide side, Vector3 outerSize, Vector3 outerPos, Vector3 trimSize, Vector3 trimPos)
        {
            var outer = ProceduralPrimitive3D.Block($"{side}Wall_Outer", _wallsRoot, outerSize, outerPos, DesignTokens.Color_.Stone);
            _outerWallRenderers.Add(outer.GetComponent<Renderer>());

            // A thin cream slab just inside the outer wall reads as the dollhouse "cut open"
            // interior surface, the 3D equivalent of CastleView's Interior sprite layer.
            var trim = ProceduralPrimitive3D.Block($"{side}Wall_Interior", _wallsRoot, trimSize, trimPos, DesignTokens.Color_.Interior);
            _interiorTrimRenderers.Add(trim.GetComponent<Renderer>());
        }

        void BuildTowers()
        {
            float towerHeight = height + towerHeightOverhang;
            Vector3[] corners =
            {
                new Vector3(-width / 2f, 0f, 0f),
                new Vector3(width / 2f, 0f, 0f),
                new Vector3(-width / 2f, 0f, depth),
                new Vector3(width / 2f, 0f, depth),
            };

            foreach (var corner in corners)
            {
                ProceduralPrimitive3D.Cylinder("Tower", _towersRoot, towerRadius, towerHeight, corner + new Vector3(0f, towerHeight / 2f, 0f), DesignTokens.Color_.StoneDark);
                var cap = ProceduralPrimitive3D.Cylinder("TowerCap", _towersRoot, towerCapRadius, towerCapHeight, corner + new Vector3(0f, towerHeight + towerCapHeight / 2f, 0f), DesignTokens.Color_.StoneDark);
                _towerCapRenderers.Add(cap.GetComponent<Renderer>());
            }
        }

        void BuildMerlons()
        {
            // Back wall: spaced along X. Left/right walls: spaced along Z. Each merlon is
            // narrower than its share of the wall length, so gaps appear automatically.
            BuildMerlonRow(merlonsPerWall, i =>
            {
                float segment = width / merlonsPerWall;
                float x = -width / 2f + segment * (i + 0.5f);
                return new Vector3(x, height + merlonSize / 2f, depth - wallThickness / 2f);
            }, new Vector3(merlonSize, merlonSize, wallThickness));

            BuildMerlonRow(merlonsPerWall, i =>
            {
                float segment = depth / merlonsPerWall;
                float z = segment * (i + 0.5f);
                return new Vector3(-width / 2f + wallThickness / 2f, height + merlonSize / 2f, z);
            }, new Vector3(wallThickness, merlonSize, merlonSize));

            BuildMerlonRow(merlonsPerWall, i =>
            {
                float segment = depth / merlonsPerWall;
                float z = segment * (i + 0.5f);
                return new Vector3(width / 2f - wallThickness / 2f, height + merlonSize / 2f, z);
            }, new Vector3(wallThickness, merlonSize, merlonSize));
        }

        void BuildMerlonRow(int count, System.Func<int, Vector3> position, Vector3 size)
        {
            for (int i = 0; i < count; i++)
            {
                var merlon = ProceduralPrimitive3D.Block("Merlon", _merlonsRoot, size, position(i), DesignTokens.Color_.Stone);
                _merlonRenderers.Add(merlon.GetComponent<Renderer>());
            }
        }

        void BuildLedgesAndBattleStations()
        {
            BuildBattleStation(CastleWallSide.Back,
                new Vector3(width * 0.5f, ledgeThickness, ledgeDepth),
                new Vector3(0f, ledgeHeight, depth - wallThickness - ledgeDepth / 2f),
                Vector3.back);

            BuildBattleStation(CastleWallSide.Left,
                new Vector3(ledgeDepth, ledgeThickness, depth * 0.5f),
                new Vector3(-width / 2f + wallThickness + ledgeDepth / 2f, ledgeHeight, depth / 2f),
                Vector3.right);

            BuildBattleStation(CastleWallSide.Right,
                new Vector3(ledgeDepth, ledgeThickness, depth * 0.5f),
                new Vector3(width / 2f - wallThickness - ledgeDepth / 2f, ledgeHeight, depth / 2f),
                Vector3.left);
        }

        void BuildBattleStation(CastleWallSide side, Vector3 size, Vector3 position, Vector3 outward)
        {
            var ledge = ProceduralPrimitive3D.Block($"Ledge_{side}", _ledgesRoot, size, position, DesignTokens.Color_.Wood);
            _ledgeRenderers[side] = ledge.GetComponent<Renderer>();

            // Parented to the (unscaled) ledges root rather than the ledge block itself, so the
            // block's own non-uniform localScale never leaks into this transform's position.
            var spawnGo = new GameObject($"SpawnPoint_{side}");
            spawnGo.transform.SetParent(_ledgesRoot, false);
            spawnGo.transform.localPosition = position + Vector3.up * (ledgeThickness / 2f);
            spawnGo.transform.localRotation = Quaternion.LookRotation(outward, Vector3.up);

            var stationGo = new GameObject($"BattleStation_{side}");
            stationGo.transform.SetParent(_ledgesRoot, false);
            var station = stationGo.AddComponent<CastleBattleStation>();
            station.wallSide = side;
            station.SpawnPoint = spawnGo.transform;
            _battleStations.Add(station);
        }

        // ---------------------------------------------------------------- upgrades

        // Equips `upgrade` into its own category slot (see _equipped). Pass null with a
        // category to unequip that slot, e.g. ApplyUpgrade(null) has no effect on its own —
        // use ClearUpgrade(category) to unequip a specific slot instead.
        public void ApplyUpgrade(CastleUpgradeDefinition upgrade)
        {
            if (upgrade == null) return;

            _equipped[upgrade.category] = upgrade;
            if (upgrade.category == CastleUpgradeCategory.StructureTier) _castle.SetTier(upgrade.structureTier);

            Rebuild();
        }

        public void ClearUpgrade(CastleUpgradeCategory category)
        {
            _equipped.Remove(category);
            Rebuild();
        }

        public void ApplyUpgradeId(string upgradeId)
        {
            if (catalog == null) return;
            var upgrade = catalog.Find(upgradeId);
            if (upgrade != null) ApplyUpgrade(upgrade);
        }

        CastleUpgradeDefinition Equipped(CastleUpgradeCategory category)
        {
            return _equipped.TryGetValue(category, out var u) ? u : null;
        }

        // Re-derives every cosmetic from the full equipped set. Called after any single slot
        // changes so slots never have to know about each other.
        void Rebuild()
        {
            var wallSkin = Equipped(CastleUpgradeCategory.WallSkin);
            var towerCaps = Equipped(CastleUpgradeCategory.TowerCaps);
            var banner = Equipped(CastleUpgradeCategory.Banner);
            var ledgeTrim = Equipped(CastleUpgradeCategory.LedgeTrim);

            _baseWallColor = wallSkin != null && wallSkin.overridesWallColor ? wallSkin.wallColor : DesignTokens.Color_.Stone;
            Color interiorColor = wallSkin != null && wallSkin.overridesInteriorColor ? wallSkin.interiorColor : DesignTokens.Color_.Interior;
            Color towerCapColor = towerCaps != null && towerCaps.overridesTowerCapColor ? towerCaps.towerCapColor : DesignTokens.Color_.StoneDark;
            Color ledgeColor = ledgeTrim != null && ledgeTrim.overridesLedgeColor ? ledgeTrim.ledgeColor : DesignTokens.Color_.Wood;

            foreach (var r in _interiorTrimRenderers) ProceduralPrimitive3D.Tint(r.gameObject, interiorColor);
            foreach (var r in _towerCapRenderers) ProceduralPrimitive3D.Tint(r.gameObject, towerCapColor);
            foreach (var kv in _ledgeRenderers) ProceduralPrimitive3D.Tint(kv.Value.gameObject, ledgeColor);

            SetBannerVisible(banner != null && banner.addsBanner, banner != null ? banner.bannerColor : Color.white);

            // Wall/merlon colors are derived from _baseWallColor inside RefreshDamageStage so
            // the current damage stage's soot tint is re-applied on top of the new skin rather
            // than skipped.
            RefreshDamageStage();
        }

        void SetBannerVisible(bool visible, Color color)
        {
            if (!visible)
            {
                if (_bannerRoot != null) _bannerRoot.SetActive(false);
                return;
            }

            if (_bannerRoot == null)
            {
                _bannerRoot = new GameObject("Banner");
                _bannerRoot.transform.SetParent(_ledgesRoot, false);
                _bannerRoot.transform.localPosition = new Vector3(0f, height, depth - wallThickness);

                ProceduralPrimitive3D.Block("BannerPole", _bannerRoot.transform, new Vector3(0.08f, 1.2f, 0.08f), new Vector3(0f, 0.6f, 0f), DesignTokens.Color_.WoodDark);
                var cloth = ProceduralPrimitive3D.Block("BannerCloth", _bannerRoot.transform, new Vector3(0.5f, 0.7f, 0.05f), new Vector3(0.3f, 0.9f, 0f), color);
                _bannerClothRenderer = cloth.GetComponent<Renderer>();
            }

            ProceduralPrimitive3D.Tint(_bannerClothRenderer.gameObject, color);
            _bannerRoot.SetActive(true);
        }

        // ---------------------------------------------------------------- damage stage

        static readonly Color SootColor = new Color(0.12f, 0.1f, 0.09f);

        // Always lerps from _baseWallColor (the current skin), never from a renderer's current
        // color — otherwise repeated stage transitions would compound soot on top of soot
        // instead of reflecting a single stage's worth of damage.
        void RefreshDamageStage()
        {
            float soot = SootAmountForStage(_castle.Stage);
            bool destroyed = _castle.Stage == CastleDamageStage.Destroyed;
            Color wallColor = Color.Lerp(_baseWallColor, SootColor, soot);

            foreach (var r in _outerWallRenderers)
            {
                r.enabled = !destroyed;
                if (!destroyed) ProceduralPrimitive3D.Tint(r.gameObject, wallColor);
            }
            foreach (var r in _interiorTrimRenderers) r.enabled = !destroyed;
            foreach (var r in _merlonRenderers)
            {
                r.enabled = !destroyed;
                if (!destroyed) ProceduralPrimitive3D.Tint(r.gameObject, wallColor);
            }
        }

        static float SootAmountForStage(CastleDamageStage stage)
        {
            switch (stage)
            {
                case CastleDamageStage.Shockwave: return 0.08f;
                case CastleDamageStage.Smoking: return 0.20f;
                case CastleDamageStage.Rubble: return 0.35f;
                case CastleDamageStage.Breached: return 0.55f;
                default: return 0f;
            }
        }
    }
}
