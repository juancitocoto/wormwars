using System;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace WormWars.Core
{
    public class WeaponSlotUI : MonoBehaviour, IPointerClickHandler
    {
        public Image background;
        public Image icon;
        public Image innerGlow;
        public Text ammoLabel;
        public RectTransform visualRoot;

        public WeaponDefinition Weapon { get; private set; }
        public bool IsLocked { get; private set; }
        public int RemainingAmmo { get; private set; }
        public bool IsSelected { get; private set; }
        public bool IsDepleted => !IsLocked && Weapon != null && Weapon.maxAmmo >= 0 && RemainingAmmo <= 0;

        public event Action<WeaponSlotUI> OnSlotClicked;
        public event Action OnLockedTapped;

        public void Setup(WeaponDefinition weapon, bool locked)
        {
            Weapon = weapon;
            IsLocked = locked;
            RemainingAmmo = weapon != null ? weapon.maxAmmo : 0;
            icon.sprite = weapon != null ? weapon.icon : null;
            Refresh();
        }

        public void ConsumeAmmo()
        {
            if (Weapon != null && Weapon.maxAmmo >= 0) RemainingAmmo = Mathf.Max(0, RemainingAmmo - 1);
            Refresh();
        }

        public void SetSelected(bool selected)
        {
            IsSelected = selected;
            Refresh();
        }

        public void OnPointerClick(PointerEventData eventData)
        {
            if (IsLocked)
            {
                // Tap shows an unlock toast; must never deep-link out of an active match.
                OnLockedTapped?.Invoke();
                return;
            }
            if (IsDepleted) return;
            OnSlotClicked?.Invoke(this);
        }

        void Refresh()
        {
            if (IsLocked)
            {
                background.color = DesignTokens.Color_.Locked;
                icon.color = DesignTokens.Color_.LockedFg;
                innerGlow.enabled = false;
                if (visualRoot != null) visualRoot.anchoredPosition = Vector2.zero;
                if (ammoLabel != null) ammoLabel.gameObject.SetActive(false);
                return;
            }

            background.color = IsSelected ? DesignTokens.Color_.SelectedSlot : DesignTokens.Color_.Cream;
            innerGlow.enabled = IsSelected;
            if (innerGlow.enabled) innerGlow.color = DesignTokens.Color_.SelectedGlow;
            if (visualRoot != null) visualRoot.anchoredPosition = IsSelected ? new Vector2(0f, 2f) : Vector2.zero;

            icon.color = IsDepleted ? new Color(1, 1, 1, 0.35f) : Color.white;

            if (ammoLabel != null)
            {
                bool infinite = Weapon != null && Weapon.maxAmmo < 0;
                ammoLabel.gameObject.SetActive(!infinite);
                if (!infinite) ammoLabel.text = RemainingAmmo.ToString();
            }
        }
    }
}
