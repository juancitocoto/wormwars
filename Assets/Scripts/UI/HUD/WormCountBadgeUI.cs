using UnityEngine;
using UnityEngine.UI;

namespace WormWars.Core
{
    // Format: "3-2". Counts ALIVE worms, not total roster size.
    public class WormCountBadgeUI : MonoBehaviour
    {
        public Text label;
        public BattleManager battleManager;

        public void Refresh()
        {
            if (battleManager == null) return;
            label.text = $"{battleManager.AliveCount(TeamId.A)}–{battleManager.AliveCount(TeamId.B)}";
        }
    }
}
