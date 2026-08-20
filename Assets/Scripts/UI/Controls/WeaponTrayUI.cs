using System;
using System.Collections.Generic;
using UnityEngine;

namespace WormWars.Core
{
    public class WeaponTrayUI : MonoBehaviour
    {
        public List<WeaponSlotUI> slots = new List<WeaponSlotUI>();
        public int SelectedIndex { get; private set; }

        public event Action<WeaponDefinition> OnWeaponSelected;
        public event Action OnLockedTapped;

        // weapons[0] must be the always-available default/melee weapon (maxAmmo = -1) so a
        // player can never be softlocked out of firing — see the "zero ammo" edge case.
        public void Setup(IReadOnlyList<WeaponDefinition> weapons, int lockedFromIndex)
        {
            for (int i = 0; i < slots.Count; i++)
            {
                var slot = slots[i];
                bool locked = i >= lockedFromIndex;
                var weapon = i < weapons.Count ? weapons[i] : null;
                slot.Setup(weapon, locked || weapon == null);

                int captured = i;
                slot.OnSlotClicked += _ => Select(captured);
                slot.OnLockedTapped += () => OnLockedTapped?.Invoke();
            }

            Select(0);
        }

        public void Select(int index)
        {
            if (index < 0 || index >= slots.Count) return;
            if (slots[index].IsLocked || slots[index].IsDepleted) return;

            SelectedIndex = index;
            for (int i = 0; i < slots.Count; i++) slots[i].SetSelected(i == index);
            HapticsBridge.SelectionTick();
            OnWeaponSelected?.Invoke(slots[index].Weapon);
        }

        public WeaponDefinition SelectedWeapon => SelectedIndex >= 0 && SelectedIndex < slots.Count ? slots[SelectedIndex].Weapon : null;

        public void ConsumeSelectedAmmo()
        {
            if (SelectedIndex < 0 || SelectedIndex >= slots.Count) return;
            var slot = slots[SelectedIndex];
            slot.ConsumeAmmo();

            // If this slot just ran dry, fall back to the infinite default weapon (index 0)
            // rather than leaving a depleted, unusable slot selected.
            if (slot.IsDepleted && SelectedIndex != 0) Select(0);
        }
    }
}
