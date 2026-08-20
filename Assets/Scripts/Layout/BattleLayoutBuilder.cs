using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using WormWars.Core;
using static WormWars.Core.DesignTokens;

namespace WormWars.Layout
{
    // Builds the entire battle screen at runtime: camera framing, world-space battlefield
    // placeholders, and the HUD/control canvas — all from the design tokens and zone
    // percentages in worm_battle_handoff_spec.md. No hand-authored scene content is required
    // beyond a Main Camera; see BattleBootstrap for the entry point.
    //
    // Scope for this pass: standard-phone reference layout only (small-phone / tablet
    // breakpoints from the Responsive Behavior table are not implemented). Shots resolve
    // instantly against the opposing castle rather than simulating real projectile
    // collision — see BattleBootstrap for that simplification.
    public static class BattleLayoutBuilder
    {
        public class BuildResult
        {
            public Camera WorldCamera;
            public BattleManager BattleManager;
            public TurnManager TurnManager;
            public WeaponTrayUI WeaponTray;
            public AimJoystickUI Joystick;
            public FireButtonUI FireButton;
            public TrajectoryArc TrajectoryArc;
            public TurnBadgeUI TurnBadge;
            public TurnTimerUI TurnTimer;
            public WormCountBadgeUI WormCount;
            public BannerUI Banner;
            public CastleHPBarUI CastleHpA;
            public CastleHPBarUI CastleHpB;
            public Transform WormSpawnA;
            public Transform WormSpawnB;
        }

        const float RefW = ReferenceDevice.WidthDp;
        const float RefH = ReferenceDevice.HeightDp;

        public static BuildResult Build()
        {
            var result = new BuildResult();

            EnsureEventSystem();
            result.WorldCamera = BuildCamera();

            var battleGo = new GameObject("Battle");
            var battleManager = battleGo.AddComponent<BattleManager>();
            var turnManager = battleGo.AddComponent<TurnManager>();
            result.BattleManager = battleManager;
            result.TurnManager = turnManager;

            BuildWorld(battleManager, result);

            var canvas = BuildCanvas();
            BuildTopHud(canvas.transform, result);
            BuildCastleHpRow(canvas.transform, result, battleManager);
            BuildControlBand(canvas.transform, result);
            BuildBanner(canvas.transform, result);

            return result;
        }

        // ---------------------------------------------------------------- camera / world

        static Camera BuildCamera()
        {
            var camGo = GameObject.FindWithTag("MainCamera");
            Camera cam = camGo != null ? camGo.GetComponent<Camera>() : null;
            if (cam == null)
            {
                camGo = new GameObject("Main Camera");
                camGo.tag = "MainCamera";
                cam = camGo.AddComponent<Camera>();
            }

            cam.orthographic = true;
            cam.backgroundColor = Color_.SkyTop;
            cam.clearFlags = CameraClearFlags.SolidColor;

            // Camera only draws into the battlefield+ground band (19%-78% of screen height,
            // top-based) so persistent UI never overlaps it, per "the battlefield is the hero".
            float bandTopPct = Zones.CastleHpRowEnd;
            float bandBottomPct = Zones.GroundEnd;
            cam.rect = new Rect(0f, 1f - bandBottomPct, 1f, bandBottomPct - bandTopPct);

            float worldHeightDp = (bandBottomPct - bandTopPct) * RefH;
            cam.orthographicSize = worldHeightDp / 2f;
            cam.transform.position = new Vector3(0f, 0f, -10f);

            return cam;
        }

        static void BuildWorld(BattleManager battleManager, BuildResult result)
        {
            float bandTopPct = Zones.CastleHpRowEnd;
            float bandBottomPct = Zones.GroundEnd;
            float worldHeightDp = (bandBottomPct - bandTopPct) * RefH;
            float groundHeightDp = (Zones.GroundEnd - Zones.BattlefieldEnd) * RefH;
            float groundLineY = -worldHeightDp / 2f + groundHeightDp;

            var ground = new GameObject("Ground").AddComponent<SpriteRenderer>();
            ground.sprite = ProceduralSprite.Solid();
            ground.color = Color_.Dirt;
            ground.transform.position = new Vector3(0, -worldHeightDp / 2f + groundHeightDp / 2f, 1f);
            ground.transform.localScale = new Vector3(RefW * 1.2f, groundHeightDp, 1f);
            ground.sortingOrder = -10;

            float castleWidth = Zones.CastleWidthPercent * RefW;
            float inset = Zones.CastleInsetPercent * RefW;
            float castleHeight = worldHeightDp * 0.75f;
            float leftX = -RefW / 2f + inset + castleWidth / 2f;
            float rightX = RefW / 2f - inset - castleWidth / 2f;
            float castleY = groundLineY + castleHeight / 2f;

            result.WormSpawnA = SpawnCastle(battleManager, TeamId.A, new Vector3(leftX, castleY, 0), castleWidth, castleHeight, true);
            result.WormSpawnB = SpawnCastle(battleManager, TeamId.B, new Vector3(rightX, castleY, 0), castleWidth, castleHeight, false);

            SpawnTeam(battleManager, TeamId.A, result.WormSpawnA.position, 1);
            SpawnTeam(battleManager, TeamId.B, result.WormSpawnB.position, -1);
        }

        static Transform SpawnCastle(BattleManager battleManager, TeamId team, Vector3 pos, float width, float height, bool interiorFacesRight)
        {
            var go = new GameObject($"Castle_{team}");
            go.transform.position = pos;

            var controller = go.AddComponent<CastleController>();
            controller.teamId = team;

            var view = go.AddComponent<CastleView>();
            view.widthDp = width;
            view.heightDp = height;
            view.interiorFacesRight = interiorFacesRight;
            view.Init();

            if (team == TeamId.A) battleManager.castleA = controller; else battleManager.castleB = controller;
            return go.transform;
        }

        static void SpawnTeam(BattleManager battleManager, TeamId team, Vector3 castlePos, int sideSign)
        {
            var roster = battleManager.Roster(team);
            const int count = 3;
            for (int i = 0; i < count; i++)
            {
                var go = new GameObject($"Worm_{team}_{i}");
                go.transform.position = castlePos + new Vector3(sideSign * (40f + i * 20f), -20f + i * 10f, 0f);

                var worm = go.AddComponent<WormController>();
                worm.teamId = team;
                go.AddComponent<WormView>();

                roster.Add(worm);
            }
        }

        static void EnsureEventSystem()
        {
            if (Object.FindObjectOfType<EventSystem>() != null) return;
            var go = new GameObject("EventSystem");
            go.AddComponent<EventSystem>();
            go.AddComponent<StandaloneInputModule>();
        }

        // ---------------------------------------------------------------- canvas / hud

        static Canvas BuildCanvas()
        {
            var go = new GameObject("HUD Canvas");
            var canvas = go.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;

            var scaler = go.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(RefW, RefH);
            scaler.matchWidthOrHeight = 0.5f;

            go.AddComponent<GraphicRaycaster>();
            return canvas;
        }

        static RectTransform CreateRect(Transform parent, string name, Vector2 anchorMin, Vector2 anchorMax, Vector2 offsetMin, Vector2 offsetMax)
        {
            var go = new GameObject(name, typeof(RectTransform));
            var rt = (RectTransform)go.transform;
            rt.SetParent(parent, false);
            rt.anchorMin = anchorMin;
            rt.anchorMax = anchorMax;
            rt.offsetMin = offsetMin;
            rt.offsetMax = offsetMax;
            return rt;
        }

        static Text CreateLabel(Transform parent, string name, string text, int fontSize, Color color, TextAnchor alignment = TextAnchor.MiddleCenter)
        {
            var rt = CreateRect(parent, name, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);
            var t = rt.gameObject.AddComponent<Text>();
            t.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            t.text = text;
            t.fontSize = fontSize;
            t.fontStyle = FontStyle.Bold;
            t.color = color;
            t.alignment = alignment;
            return t;
        }

        static RectTransform ZoneRect(Transform parent, string name, float topPct, float bottomPct)
        {
            // Anchors are bottom-based in Unity's UI; the spec's zone table is top-based.
            return CreateRect(parent, name, new Vector2(0f, 1f - bottomPct), new Vector2(1f, 1f - topPct), Vector2.zero, Vector2.zero);
        }

        static void BuildTopHud(Transform canvas, BuildResult result)
        {
            var zone = ZoneRect(canvas, "TopHud", Zones.TopHudStart, Zones.TopHudEnd);

            // Turn badge (left)
            var badgeRect = CreateRect(zone, "TurnBadge", new Vector2(0, 0), new Vector2(0, 1), Vector2.zero, Vector2.zero);
            badgeRect.anchoredPosition = new Vector2(Spacing.Lg + 60f, 0);
            badgeRect.sizeDelta = new Vector2(120f, 32f);
            var badgeBg = badgeRect.gameObject.AddComponent<Image>();
            badgeBg.sprite = ProceduralSprite.RoundedRect(120, 32, Mathf.RoundToInt(Radius.Badge), Color.white);
            badgeBg.color = Color_.Cream;
            var badgeLabel = CreateLabel(badgeRect, "Label", "TEAM A", 11, Color_.Outline);
            var badgeOutline = badgeRect.gameObject.AddComponent<Outline>();
            badgeOutline.effectColor = Color_.Outline;

            var badge = badgeRect.gameObject.AddComponent<TurnBadgeUI>();
            badge.background = badgeBg;
            badge.label = badgeLabel;
            badge.outline = badgeOutline;
            result.TurnBadge = badge;

            // Timer (center)
            var timerRect = CreateRect(zone, "TurnTimer", new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), Vector2.zero, Vector2.zero);
            timerRect.sizeDelta = new Vector2(64f, 32f);
            var timerBg = timerRect.gameObject.AddComponent<Image>();
            timerBg.sprite = ProceduralSprite.RoundedRect(64, 32, Mathf.RoundToInt(Radius.Badge), Color.white);
            timerBg.color = Color_.Cream;
            var timerDigits = CreateLabel(timerRect, "Digits", "30", 15, Color_.Outline);

            var timer = timerRect.gameObject.AddComponent<TurnTimerUI>();
            timer.background = timerBg;
            timer.digits = timerDigits;
            timer.Bind(result.TurnManager);
            result.TurnTimer = timer;

            // Worm count badge (right)
            var countRect = CreateRect(zone, "WormCount", new Vector2(1, 0), new Vector2(1, 1), Vector2.zero, Vector2.zero);
            countRect.anchoredPosition = new Vector2(-(Spacing.Lg + 40f), 0);
            countRect.sizeDelta = new Vector2(80f, 32f);
            var countBg = countRect.gameObject.AddComponent<Image>();
            countBg.sprite = ProceduralSprite.RoundedRect(80, 32, Mathf.RoundToInt(Radius.Badge), Color.white);
            countBg.color = Color_.Cream;
            var countLabel = CreateLabel(countRect, "Label", "3–3", 11, Color_.Outline);

            var count = countRect.gameObject.AddComponent<WormCountBadgeUI>();
            count.label = countLabel;
            count.battleManager = result.BattleManager;
            result.WormCount = count;

            // Wind indicator, tucked centered just under the timer.
            var windRect = CreateRect(zone, "WindIndicator", new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), Vector2.zero, Vector2.zero);
            windRect.sizeDelta = new Vector2(40f, 20f);
            var arrowRect = CreateRect(windRect, "Arrow", new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), Vector2.zero, Vector2.zero);
            arrowRect.sizeDelta = new Vector2(16f, 4f);
            var arrowImg = arrowRect.gameObject.AddComponent<Image>();
            arrowImg.color = Color_.Outline;
            var windLabel = CreateLabel(windRect, "Strength", "»", 9, Color_.Outline);
            windLabel.rectTransform.anchoredPosition = new Vector2(0, -12f);

            var wind = windRect.gameObject.AddComponent<WindIndicatorUI>();
            wind.arrow = arrowRect;
            wind.strengthLabel = windLabel;
            wind.SetWind(0f, 0.4f);
        }

        static void BuildCastleHpRow(Transform canvas, BuildResult result, BattleManager battleManager)
        {
            var zone = ZoneRect(canvas, "CastleHpRow", Zones.TopHudEnd, Zones.CastleHpRowEnd);

            result.CastleHpA = BuildCastleHpBar(zone, "CastleHpA", TeamId.A, battleManager.castleA, new Vector2(0, 0.5f), Spacing.Lg);
            result.CastleHpB = BuildCastleHpBar(zone, "CastleHpB", TeamId.B, battleManager.castleB, new Vector2(1, 0.5f), -Spacing.Lg);
        }

        static CastleHPBarUI BuildCastleHpBar(Transform parent, string name, TeamId team, CastleController castle, Vector2 anchor, float xOffset)
        {
            var rect = CreateRect(parent, name, anchor, anchor, Vector2.zero, Vector2.zero);
            rect.sizeDelta = new Vector2(220f, 18f);
            rect.anchoredPosition = new Vector2(xOffset + (anchor.x == 0 ? 110f : -110f), 0);

            var track = rect.gameObject.AddComponent<Image>();
            track.sprite = ProceduralSprite.RoundedRect(220, 18, Mathf.RoundToInt(Radius.Pill / 20f), Color.white);
            track.color = Color_.HpTrack;

            var fillRect = CreateRect(rect, "Fill", Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);
            var fillImg = fillRect.gameObject.AddComponent<Image>();
            fillImg.sprite = track.sprite;
            fillImg.type = Image.Type.Filled;
            fillImg.fillMethod = Image.FillMethod.Horizontal;
            fillImg.fillOrigin = (int)Image.OriginHorizontal.Left;
            fillImg.fillAmount = 1f;
            fillImg.color = Color_.CastleHpStart;

            var labelRect = CreateRect(rect, "Label", Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);
            labelRect.anchoredPosition = new Vector2(0, 14f);
            var label = labelRect.gameObject.AddComponent<Text>();
            label.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            label.fontSize = 9;
            label.fontStyle = FontStyle.Bold;
            label.color = Color_.Outline;
            label.alignment = TextAnchor.MiddleCenter;

            var bar = rect.gameObject.AddComponent<CastleHPBarUI>();
            bar.fill = fillImg;
            bar.label = label;
            bar.Bind(castle);
            return bar;
        }

        static void BuildControlBand(Transform canvas, BuildResult result)
        {
            var zone = ZoneRect(canvas, "ControlBand", Zones.ControlBandStart, Zones.ControlBandEnd);

            // Joystick, pinned left.
            var joyRect = CreateRect(zone, "AimJoystick", new Vector2(0, 0.5f), new Vector2(0, 0.5f), Vector2.zero, Vector2.zero);
            joyRect.sizeDelta = new Vector2(88f, 88f);
            joyRect.anchoredPosition = new Vector2(Spacing.Lg + 44f, 0);
            var joyBase = joyRect.gameObject.AddComponent<Image>();
            joyBase.sprite = ProceduralSprite.RoundedRect(88, 88, 44, Color.white);
            joyBase.color = Color_.Wood;
            joyBase.raycastTarget = true;

            var knobRect = CreateRect(joyRect, "Knob", new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), Vector2.zero, Vector2.zero);
            knobRect.sizeDelta = new Vector2(40f, 40f);
            var knobImg = knobRect.gameObject.AddComponent<Image>();
            knobImg.sprite = ProceduralSprite.RoundedRect(40, 40, 20, Color.white);
            knobImg.color = Color_.TeamA;

            var joystick = joyRect.gameObject.AddComponent<AimJoystickUI>();
            joystick.baseRect = joyRect;
            joystick.knob = knobRect;
            joystick.knobImage = knobImg;
            joystick.knobRadiusDp = 32f;
            result.Joystick = joystick;

            // Fire button, pinned right.
            var fireRect = CreateRect(zone, "FireButton", new Vector2(1, 0.5f), new Vector2(1, 0.5f), Vector2.zero, Vector2.zero);
            fireRect.sizeDelta = new Vector2(72f, 72f);
            fireRect.anchoredPosition = new Vector2(-(Spacing.Lg + 36f), 0);
            var fireVisualRect = CreateRect(fireRect, "Visual", Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);
            var fireBg = fireVisualRect.gameObject.AddComponent<Image>();
            fireBg.sprite = ProceduralSprite.RoundedRect(72, 72, 36, Color.white);
            fireBg.color = Color_.Fire;

            var meterRect = CreateRect(fireRect, "Meter", Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);
            var meterImg = meterRect.gameObject.AddComponent<Image>();
            meterImg.sprite = fireBg.sprite;
            meterImg.type = Image.Type.Filled;
            meterImg.fillMethod = Image.FillMethod.Radial360;
            meterImg.color = new Color(1f, 1f, 1f, 0.4f);
            meterImg.fillAmount = 0f;

            var fireButton = fireRect.gameObject.AddComponent<FireButtonUI>();
            fireButton.background = fireBg;
            fireButton.radialMeter = meterImg;
            fireButton.visualRoot = fireVisualRect;
            result.FireButton = fireButton;

            // Weapon tray, centered.
            var trayRect = CreateRect(zone, "WeaponTray", new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), Vector2.zero, Vector2.zero);
            trayRect.sizeDelta = new Vector2(4 * 44f + 3 * Spacing.Sm + Spacing.Md * 2, 60f);
            var trayBg = trayRect.gameObject.AddComponent<Image>();
            trayBg.sprite = ProceduralSprite.RoundedRect(Mathf.RoundToInt(trayRect.sizeDelta.x), 60, Mathf.RoundToInt(Radius.Tray), Color.white);
            trayBg.color = Color_.Cream;

            var tray = trayRect.gameObject.AddComponent<WeaponTrayUI>();
            var slotsParent = CreateRect(trayRect, "Slots", Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);
            var hlayout = slotsParent.gameObject.AddComponent<HorizontalLayoutGroup>();
            hlayout.spacing = Spacing.Sm;
            hlayout.padding = new RectOffset(Mathf.RoundToInt(Spacing.Md), Mathf.RoundToInt(Spacing.Md), Mathf.RoundToInt(Spacing.Md), Mathf.RoundToInt(Spacing.Md));
            hlayout.childAlignment = TextAnchor.MiddleCenter;
            hlayout.childForceExpandHeight = false;
            hlayout.childForceExpandWidth = false;

            for (int i = 0; i < 4; i++)
            {
                var slot = BuildWeaponSlot(slotsParent, i);
                tray.slots.Add(slot);
            }
            result.WeaponTray = tray;

            // Trajectory arc lives in world space, following the active worm's launch point.
            var arcGo = new GameObject("TrajectoryArc");
            var line = arcGo.AddComponent<LineRenderer>();
            line.useWorldSpace = true;
            result.TrajectoryArc = arcGo.AddComponent<TrajectoryArc>();
        }

        static WeaponSlotUI BuildWeaponSlot(Transform parent, int index)
        {
            var go = new GameObject($"Slot_{index}", typeof(RectTransform));
            var rt = (RectTransform)go.transform;
            rt.SetParent(parent, false);
            var layoutElement = go.AddComponent<LayoutElement>();
            layoutElement.preferredWidth = 44f;
            layoutElement.preferredHeight = 44f;

            var visualRect = CreateRect(rt, "Visual", Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);
            var bg = visualRect.gameObject.AddComponent<Image>();
            bg.sprite = ProceduralSprite.RoundedRect(36, 36, Mathf.RoundToInt(Radius.Slot), Color.white);
            bg.color = Color_.Cream;

            var iconRect = CreateRect(visualRect, "Icon", new Vector2(0.2f, 0.2f), new Vector2(0.8f, 0.8f), Vector2.zero, Vector2.zero);
            var icon = iconRect.gameObject.AddComponent<Image>();
            icon.preserveAspect = true;

            var glowRect = CreateRect(visualRect, "Glow", Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);
            var glow = glowRect.gameObject.AddComponent<Image>();
            glow.sprite = bg.sprite;
            glow.color = Color_.SelectedGlow;
            glow.enabled = false;

            var ammoRect = CreateRect(rt, "Ammo", new Vector2(1, 0), new Vector2(1, 0), Vector2.zero, Vector2.zero);
            ammoRect.sizeDelta = new Vector2(16, 12);
            var ammo = ammoRect.gameObject.AddComponent<Text>();
            ammo.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            ammo.fontSize = 9;
            ammo.alignment = TextAnchor.MiddleCenter;
            ammo.color = Color_.Outline;

            var slot = go.AddComponent<WeaponSlotUI>();
            slot.background = bg;
            slot.icon = icon;
            slot.innerGlow = glow;
            slot.ammoLabel = ammo;
            slot.visualRoot = visualRect;
            return slot;
        }

        static void BuildBanner(Transform canvas, BuildResult result)
        {
            var rect = CreateRect(canvas, "Banner", new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), Vector2.zero, Vector2.zero);
            rect.anchoredPosition = new Vector2(0, -(Zones.CastleHpRowEnd * RefH + 20f));
            rect.sizeDelta = new Vector2(260f, 32f);

            var bg = rect.gameObject.AddComponent<Image>();
            bg.sprite = ProceduralSprite.RoundedRect(260, 32, Mathf.RoundToInt(Radius.Badge), Color.white);
            bg.color = new Color(Color_.Outline.r, Color_.Outline.g, Color_.Outline.b, 0.85f);

            var label = CreateLabel(rect, "Label", "", 11, Color_.Cream);

            var banner = rect.gameObject.AddComponent<BannerUI>();
            banner.root = rect.gameObject;
            banner.label = label;
            rect.gameObject.SetActive(false);
            result.Banner = banner;
        }
    }
}
