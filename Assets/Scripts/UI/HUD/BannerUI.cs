using UnityEngine;
using UnityEngine.UI;

namespace WormWars.Core
{
    // Persistent banner for the "last worm down but castle standing" / "castle down but
    // team standing" edge cases, so the player isn't confused about why the match continues.
    public class BannerUI : MonoBehaviour
    {
        public Text label;
        public GameObject root;

        public void SetBanner(string text)
        {
            bool show = !string.IsNullOrEmpty(text);
            root.SetActive(show);
            if (show) label.text = text;
        }
    }
}
