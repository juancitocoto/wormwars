using System;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace WormWars.Core
{
    // AimJoystick: fixed position (not floating), knob tinted with the active team color.
    // Sets aim DIRECTION via drag. Shot POWER is driven by FireButtonUI's hold-to-charge
    // meter, not by pull distance — the spec lists "power"/"maxPower" on both this component
    // and FireButton; treating FireButton as the single source of truth for power avoids two
    // conflicting charge mechanics. The knob's pull distance is still tracked (0-1) purely to
    // flip this control into its "charging" visual variant when fully extended.
    public class AimJoystickUI : MonoBehaviour, IPointerDownHandler, IDragHandler, IPointerUpHandler
    {
        public RectTransform baseRect;
        public RectTransform knob;
        public Image knobImage;
        public float knobRadiusDp = 32f;

        public float AngleDeg { get; private set; }
        public float PullNormalized { get; private set; }
        public bool IsDragging { get; private set; }
        public bool Interactable { get; set; } = true;

        public event Action OnDragStarted;
        public event Action OnDragChanged;
        public event Action OnDragEnded;
        public event Action OnDragCancelled;

        public void Tint(TeamId team)
        {
            if (knobImage != null) knobImage.color = DesignTokens.Color_.ForTeam(team);
        }

        public void OnPointerDown(PointerEventData eventData)
        {
            if (!Interactable) return;
            IsDragging = true;
            UpdateKnob(eventData);
            OnDragStarted?.Invoke();
        }

        public void OnDrag(PointerEventData eventData)
        {
            if (!IsDragging) return;
            UpdateKnob(eventData);
            OnDragChanged?.Invoke();
        }

        public void OnPointerUp(PointerEventData eventData)
        {
            if (!IsDragging) return;
            IsDragging = false;
            ResetKnob();
            OnDragEnded?.Invoke();
        }

        // Called by TurnManager.OnTurnTimedOut so an in-progress drag doesn't leave a
        // dangling trajectory arc after the shot can no longer fire.
        public void CancelDrag()
        {
            if (!IsDragging) return;
            IsDragging = false;
            ResetKnob();
            OnDragCancelled?.Invoke();
        }

        void UpdateKnob(PointerEventData eventData)
        {
            RectTransformUtility.ScreenPointToLocalPointInRectangle(baseRect, eventData.position, eventData.pressEventCamera, out var localPoint);
            Vector2 clamped = Vector2.ClampMagnitude(localPoint, knobRadiusDp);
            knob.anchoredPosition = clamped;

            PullNormalized = clamped.magnitude / knobRadiusDp;
            if (clamped.sqrMagnitude > 0.0001f)
            {
                AngleDeg = Mathf.Atan2(clamped.y, clamped.x) * Mathf.Rad2Deg;
            }
        }

        void ResetKnob()
        {
            knob.anchoredPosition = Vector2.zero;
            PullNormalized = 0f;
        }
    }
}
