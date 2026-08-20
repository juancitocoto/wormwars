using UnityEngine;
using UnityEngine.UI;

namespace WormWars.Core
{
    public class TurnTimerUI : MonoBehaviour
    {
        public Image background;
        public Text digits;
        public TurnManager turnManager;

        float _pulseTimer;

        // Called explicitly by BattleLayoutBuilder after turnManager is assigned — see the
        // matching note on CastleView.Init().
        public void Bind(TurnManager tm)
        {
            if (turnManager != null) turnManager.OnTimerTick -= HandleTick;
            turnManager = tm;
            turnManager.OnTimerTick += HandleTick;
        }

        void OnDestroy()
        {
            if (turnManager != null) turnManager.OnTimerTick -= HandleTick;
        }

        void HandleTick(float secondsRemaining)
        {
            digits.text = Mathf.CeilToInt(secondsRemaining).ToString();
            digits.color = turnManager.IsUrgent ? DesignTokens.Color_.Fire : DesignTokens.Color_.Outline;
        }

        void Update()
        {
            if (turnManager == null || !turnManager.IsUrgent)
            {
                transform.localScale = Vector3.one;
                return;
            }

            _pulseTimer += Time.deltaTime;
            float t = (_pulseTimer % 1f) / 1f;
            float scale = 1f + Mathf.Sin(t * Mathf.PI) * 0.08f;
            transform.localScale = new Vector3(scale, scale, 1f);
        }
    }
}
