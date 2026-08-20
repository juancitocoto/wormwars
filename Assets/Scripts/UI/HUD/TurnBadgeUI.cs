using UnityEngine;
using UnityEngine.UI;

namespace WormWars.Core
{
    // TurnBadge component: shows the active team's badge filled with their color; the
    // waiting team's badge uses color-cream with an outline. Only one is ever "active".
    public class TurnBadgeUI : MonoBehaviour
    {
        public TeamId teamId;
        public Image background;
        public Text label;
        public Text subLabel;
        public Outline outline;

        public void SetActive(bool isActive)
        {
            background.color = isActive ? DesignTokens.Color_.ForTeam(teamId) : DesignTokens.Color_.Cream;
            if (outline != null) outline.enabled = !isActive;
        }

        public void SetWorm(int wormIndex, int wormTotal, bool showSubLabel)
        {
            label.text = TruncateName(teamId == TeamId.A ? "TEAM A" : "TEAM B");
            if (subLabel != null)
            {
                subLabel.gameObject.SetActive(showSubLabel);
                if (showSubLabel) subLabel.text = $"Worm {wormIndex} of {wormTotal}";
            }
        }

        // Very long team names in customization truncate to 12 chars with an ellipsis here;
        // full name is only shown on the victory/summary screen.
        static string TruncateName(string name) => name.Length <= 12 ? name : name.Substring(0, 12) + "…";
    }
}
