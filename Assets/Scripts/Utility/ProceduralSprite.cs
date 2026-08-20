using UnityEngine;

namespace WormWars.Core
{
    // Generates flat-color / rounded-rect placeholder sprites at runtime so the battlefield
    // and HUD have something to show before real art lands in Assets/Art. Replace call sites
    // with real Sprite references as art comes in; nothing else depends on this staying around.
    public static class ProceduralSprite
    {
        static Sprite _solidWhite;

        public static Sprite Solid()
        {
            if (_solidWhite != null) return _solidWhite;

            var tex = new Texture2D(4, 4, TextureFormat.RGBA32, false) { filterMode = FilterMode.Bilinear };
            var pixels = new Color[16];
            for (int i = 0; i < pixels.Length; i++) pixels[i] = Color.white;
            tex.SetPixels(pixels);
            tex.Apply();

            _solidWhite = Sprite.Create(tex, new Rect(0, 0, 4, 4), new Vector2(0.5f, 0.5f), 4f);
            return _solidWhite;
        }

        public static Sprite RoundedRect(int width, int height, int radius, Color fill)
        {
            width = Mathf.Max(2, width);
            height = Mathf.Max(2, height);
            radius = Mathf.Clamp(radius, 0, Mathf.Min(width, height) / 2);

            var tex = new Texture2D(width, height, TextureFormat.RGBA32, false) { filterMode = FilterMode.Bilinear };
            var pixels = new Color[width * height];

            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    bool inside = true;
                    if (radius > 0) inside = !IsOutsideRoundedRect(x, y, width, height, radius);
                    pixels[y * width + x] = inside ? fill : Color.clear;
                }
            }

            tex.SetPixels(pixels);
            tex.Apply();
            // pixelsPerUnit = 1 so a sprite's world size in units equals its pixel size in dp,
            // matching the world-space-units-equal-dp convention used by BattleLayoutBuilder.
            return Sprite.Create(tex, new Rect(0, 0, width, height), new Vector2(0.5f, 0.5f), 1f, 0, SpriteMeshType.Tight, new Vector4(radius, radius, radius, radius));
        }

        static bool IsOutsideRoundedRect(int x, int y, int w, int h, int r)
        {
            int cx = -1, cy = -1;
            if (x < r && y < r) { cx = r; cy = r; }
            else if (x >= w - r && y < r) { cx = w - r - 1; cy = r; }
            else if (x < r && y >= h - r) { cx = r; cy = h - r - 1; }
            else if (x >= w - r && y >= h - r) { cx = w - r - 1; cy = h - r - 1; }
            else return false;

            float dx = x - cx;
            float dy = y - cy;
            return dx * dx + dy * dy > r * r;
        }
    }
}
