using TMPro;
using Unity.Netcode;
using UnityEngine;

namespace WormWars.Network
{
    // Client-side HUD for a networked match: turn timer, wind readout, active player/weapon
    // labels, and the "waiting for opponent" / weapon-lockout panels. Pure Canvas UI - no
    // URP/HDRP dependency - driven entirely by NetworkVariable reads and change events, so
    // every client's HUD reflects the same server-authoritative state.
    public class MultiplayerHUDManager : MonoBehaviour
    {
        [Header("Text Elements")]
        [SerializeField] TMP_Text turnTimerText;
        [SerializeField] TMP_Text windDisplayText;
        [SerializeField] TMP_Text activePlayerNameText;
        [SerializeField] TMP_Text activeWeaponNameText;

        [Header("Wind Indicator")]
        [SerializeField] RectTransform windArrow;
        [SerializeField] float minWindArrowScale = 0.5f;
        [SerializeField] float maxWindArrowScale = 1.5f;
        [SerializeField] float windSpeedAtMaxScale = 5f;

        [Header("Turn Phase Panels")]
        [SerializeField] GameObject waitingForOpponentPanel;
        [SerializeField] GameObject weaponSelectionPanel;

        [Header("Timer Warning")]
        [SerializeField] float urgentThresholdSeconds = 5f;
        [SerializeField] Color normalTimerColor = Color.white;
        [SerializeField] Color urgentTimerColor = Color.red;

        [Header("Data Sources")]
        [Tooltip("Must implement INetworkGameManager.")]
        [SerializeField] MonoBehaviour gameManagerSource;
        [SerializeField] NetworkTrajectoryPredictor trajectoryPredictor;

        INetworkGameManager _gameManager;

        void Awake()
        {
            _gameManager = gameManagerSource as INetworkGameManager;
            if (_gameManager == null)
            {
                Debug.LogError($"{nameof(MultiplayerHUDManager)}: gameManagerSource must implement {nameof(INetworkGameManager)}.", this);
            }
        }

        void OnEnable()
        {
            if (_gameManager != null)
            {
                _gameManager.ActiveClientId.OnValueChanged += HandleActiveClientChanged;
                RefreshActivePlayer(_gameManager.ActiveClientId.Value);
            }

            if (trajectoryPredictor != null)
            {
                trajectoryPredictor.OnWindChanged += RefreshWindIndicator;
                RefreshWindIndicator(trajectoryPredictor.CurrentWind);
            }
        }

        void OnDisable()
        {
            if (_gameManager != null) _gameManager.ActiveClientId.OnValueChanged -= HandleActiveClientChanged;
            if (trajectoryPredictor != null) trajectoryPredictor.OnWindChanged -= RefreshWindIndicator;
        }

        void Update()
        {
            if (_gameManager == null) return;

            RefreshTurnTimer(_gameManager.TurnTimeRemaining.Value);
            RefreshActiveWeapon(_gameManager.ActiveWeaponName);
        }

        void RefreshTurnTimer(float secondsRemaining)
        {
            if (turnTimerText == null) return;

            turnTimerText.text = Mathf.CeilToInt(Mathf.Max(0f, secondsRemaining)).ToString();
            turnTimerText.color = secondsRemaining <= urgentThresholdSeconds ? urgentTimerColor : normalTimerColor;
        }

        void RefreshActiveWeapon(string weaponName)
        {
            if (activeWeaponNameText != null) activeWeaponNameText.text = weaponName;
        }

        void HandleActiveClientChanged(ulong previous, ulong current) => RefreshActivePlayer(current);

        void RefreshActivePlayer(ulong activeClientId)
        {
            bool isLocalPlayerActive = NetworkManager.Singleton != null && NetworkManager.Singleton.LocalClientId == activeClientId;

            if (activePlayerNameText != null) activePlayerNameText.text = _gameManager.GetPlayerDisplayName(activeClientId);
            if (waitingForOpponentPanel != null) waitingForOpponentPanel.SetActive(!isLocalPlayerActive);

            // Weapon selection is fully hidden (not just visually disabled) unless it's this
            // client's turn, so an inactive player can never even attempt to fire.
            if (weaponSelectionPanel != null) weaponSelectionPanel.SetActive(isLocalPlayerActive);
        }

        void RefreshWindIndicator(Vector3 wind)
        {
            Vector2 windXZ = new Vector2(wind.x, wind.z);
            float speed = windXZ.magnitude;

            if (windArrow != null)
            {
                // Assumes the arrow art points "up" (+Y) at zero rotation - a standard
                // compass-needle mapping from a world-space horizontal vector.
                if (speed > 0.0001f)
                {
                    float headingDeg = Mathf.Atan2(windXZ.x, windXZ.y) * Mathf.Rad2Deg;
                    windArrow.localRotation = Quaternion.Euler(0f, 0f, -headingDeg);
                }

                float scale01 = Mathf.InverseLerp(0f, Mathf.Max(0.0001f, windSpeedAtMaxScale), speed);
                float scale = Mathf.Lerp(minWindArrowScale, maxWindArrowScale, scale01);
                windArrow.localScale = Vector3.one * scale;
            }

            if (windDisplayText != null) windDisplayText.text = $"WIND {speed:0.0}";
        }
    }
}
