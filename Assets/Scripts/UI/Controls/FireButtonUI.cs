using System;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace WormWars.Core
{
    public enum FireButtonState { Ready, Charging, Disabled }

    public class FireButtonUI : MonoBehaviour, IPointerDownHandler, IPointerUpHandler
    {
        public Image background;
        public Image radialMeter; // Image.Type.Filled, Radial360
        public RectTransform visualRoot;
        public float readyShadowOffsetDp = 6f;
        public float pressedShadowOffsetDp = 2f;
        public float maxChargeSeconds = 1.5f;

        public FireButtonState State { get; private set; } = FireButtonState.Ready;
        public float ChargeNormalized { get; private set; }

        // charge01 in [0,1] mapped by the caller to an actual launch power/velocity.
        public event Action<float> OnFired;

        float _chargeTimer;

        public void SetDisabled(bool disabled)
        {
            if (State == FireButtonState.Charging && disabled) return; // let an in-flight charge resolve
            State = disabled ? FireButtonState.Disabled : FireButtonState.Ready;
            RefreshVisual();
        }

        public void OnPointerDown(PointerEventData eventData)
        {
            if (State != FireButtonState.Ready) return;
            State = FireButtonState.Charging;
            _chargeTimer = 0f;
            RefreshVisual();
        }

        public void OnPointerUp(PointerEventData eventData)
        {
            if (State != FireButtonState.Charging) return;
            float charge = ChargeNormalized;
            State = FireButtonState.Ready;
            RefreshVisual();
            OnFired?.Invoke(charge);
        }

        void Update()
        {
            if (State != FireButtonState.Charging) return;
            _chargeTimer = Mathf.Min(maxChargeSeconds, _chargeTimer + Time.deltaTime);
            ChargeNormalized = _chargeTimer / maxChargeSeconds;
            if (radialMeter != null) radialMeter.fillAmount = ChargeNormalized;
        }

        void RefreshVisual()
        {
            if (visualRoot != null)
            {
                float offset = State == FireButtonState.Charging ? pressedShadowOffsetDp : readyShadowOffsetDp;
                visualRoot.anchoredPosition = new Vector2(0f, -( readyShadowOffsetDp - offset));
            }

            if (background != null)
            {
                Color c = DesignTokens.Color_.Fire;
                if (State == FireButtonState.Disabled) c.a = 0.45f;
                background.color = c;
            }

            if (State != FireButtonState.Charging && radialMeter != null) radialMeter.fillAmount = 0f;
        }
    }
}
