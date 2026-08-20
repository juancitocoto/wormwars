using UnityEngine;
using UnityEngine.UI;

namespace WormWars.Core
{
    // Top-center. Arrow rotates to wind direction; strength communicated via arrow scale
    // plus a repeated-glyph count so it isn't scale-only (helps at a glance and for a11y text).
    public class WindIndicatorUI : MonoBehaviour
    {
        public RectTransform arrow;
        public Text strengthLabel;
        public float minScale = 0.6f;
        public float maxScale = 1.4f;

        public void SetWind(float directionDeg, float strength01)
        {
            arrow.localRotation = Quaternion.Euler(0, 0, directionDeg);
            float scale = Mathf.Lerp(minScale, maxScale, Mathf.Clamp01(strength01));
            arrow.localScale = new Vector3(scale, scale, 1f);

            if (strengthLabel != null)
            {
                int notches = Mathf.Clamp(Mathf.RoundToInt(strength01 * 3f), 0, 3);
                strengthLabel.text = new string('»', Mathf.Max(1, notches));
            }
        }
    }
}
