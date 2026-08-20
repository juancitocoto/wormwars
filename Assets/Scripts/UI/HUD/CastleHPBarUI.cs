using UnityEngine;
using UnityEngine.UI;

namespace WormWars.Core
{
    public class CastleHPBarUI : MonoBehaviour
    {
        public CastleController castle;
        public Image fill;
        public Text label; // "A CASTLE" / "B CASTLE" — never color-only per accessibility notes

        float _pulseTimer;
        float _targetFill;

        // Called explicitly by BattleLayoutBuilder after castle/fill/label are assigned —
        // see the matching note on CastleView.Init().
        public void Bind(CastleController c)
        {
            castle = c;
            castle.OnDamaged += _ => Refresh();
            label.text = castle.teamId == TeamId.A ? "A CASTLE" : "B CASTLE";
            Refresh();
        }

        void Refresh()
        {
            _targetFill = castle.HPPercent01;
        }

        void Update()
        {
            if (fill == null) return;

            fill.fillAmount = Mathf.MoveTowards(fill.fillAmount, _targetFill, Time.deltaTime * 2f);

            bool critical = castle != null && castle.HPPercent01 <= 0.25f;
            if (critical)
            {
                _pulseTimer += Time.deltaTime;
                float t = (_pulseTimer % 0.8f) / 0.8f;
                float alpha = Mathf.Lerp(1f, 0.6f, Mathf.PingPong(t * 2f, 1f));
                var c = fill.color;
                c.a = alpha;
                fill.color = c;
            }
            else
            {
                var c = fill.color;
                c.a = 1f;
                fill.color = c;
            }
        }
    }
}
