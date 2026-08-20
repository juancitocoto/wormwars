using UnityEngine;

namespace WormWars.Core
{
    // Values transcribed from worm_battle_handoff_spec.md. Keep this file in sync with
    // the spec's token tables rather than hardcoding hex/dp values elsewhere.
    public static class DesignTokens
    {
        public static class Color_
        {
            public static readonly Color Outline = Hex("#3d2b1f");
            public static readonly Color SkyTop = Hex("#7fd4ff");
            public static readonly Color SkyBottom = Hex("#cdf1ff");
            public static readonly Color Stone = Hex("#b7b0a6");
            public static readonly Color StoneDark = Hex("#8d867c");
            public static readonly Color Interior = Hex("#f3e6c8");
            public static readonly Color InteriorDark = Hex("#e3d1a4");
            public static readonly Color Dirt = Hex("#a5672f");
            public static readonly Color DirtDark = Hex("#7a4720");
            public static readonly Color Wood = Hex("#c98a4b");
            public static readonly Color WoodDark = Hex("#8a5a2b");

            public static readonly Color TeamA = Hex("#6a5cff");
            public static readonly Color TeamADark = Hex("#4636c9");
            public static readonly Color TeamB = Hex("#ff8a3d");
            public static readonly Color TeamBDark = Hex("#d9631a");

            public static readonly Color Cream = Hex("#fff3d6");
            public static readonly Color Fire = Hex("#ff5a5a");
            public static readonly Color FireDark = Hex("#c73a3a");

            public static readonly Color HpFillStart = Hex("#8ce05a");
            public static readonly Color HpFillEnd = Hex("#4fae2e");
            public static readonly Color HpTrack = Hex("#4a2e1e");
            public static readonly Color CastleHpStart = Hex("#ffd873");
            public static readonly Color CastleHpEnd = Hex("#e0a52c");
            public static readonly Color Shield = Hex("#8fd8ff");
            public static readonly Color SelectedSlot = Hex("#ffe27a");
            public static readonly Color SelectedGlow = Hex("#fff59a");
            public static readonly Color Locked = Hex("#b3aeb0");
            public static readonly Color LockedFg = Hex("#6d6a6c");

            public static Color ForTeam(TeamId team) => team == TeamId.A ? TeamA : TeamB;
            public static Color DarkForTeam(TeamId team) => team == TeamId.A ? TeamADark : TeamBDark;

            static Color Hex(string html)
            {
                ColorUtility.TryParseHtmlString(html, out var c);
                return c;
            }
        }

        public static class Spacing
        {
            public const float Xs = 4f;
            public const float Sm = 8f;
            public const float Md = 12f;
            public const float Lg = 16f;
            public const float Xl = 24f;
            public const float CourtyardMin = 240f;
        }

        public static class Radius
        {
            public const float Pill = 999f;
            public const float Slot = 10f;
            public const float Badge = 14f;
            public const float Tray = 16f;
        }

        // Reference device: 844 x 390 dp landscape. All fixed dp values in the spec are
        // authored against this reference; BattleLayoutBuilder scales the standard-phone
        // breakpoint layout using it. Small-phone / tablet breakpoints are not yet implemented.
        public static class ReferenceDevice
        {
            public const float WidthDp = 844f;
            public const float HeightDp = 390f;
        }

        public static class Zones
        {
            // Percent of screen height, top to bottom.
            public const float TopHudStart = 0.00f;
            public const float TopHudEnd = 0.11f;
            public const float CastleHpRowEnd = 0.19f;
            public const float BattlefieldEnd = 0.70f;
            public const float GroundEnd = 0.78f;
            public const float ControlBandStart = 0.70f;
            public const float ControlBandEnd = 1.00f;

            public const float CastleWidthPercent = 0.33f;
            public const float CastleInsetPercent = 0.015f;
        }
    }
}
