using System;
using UnityEngine;

namespace WormWars.Core
{
    public class WormController : MonoBehaviour
    {
        public TeamId teamId;
        public int maxHP = 100;
        public float strengthMultiplier = 1f;

        public int CurrentHP { get; private set; }
        public WormVisualState VisualState { get; private set; } = WormVisualState.Idle;
        public bool IsActive { get; private set; }
        public bool IsEliminated => VisualState == WormVisualState.Eliminated;
        public float HPPercent01 => maxHP <= 0 ? 0f : Mathf.Clamp01((float)CurrentHP / maxHP);

        public event Action<WormController> OnHit;
        public event Action<WormController> OnEliminated;
        public event Action<WormController> OnStateChanged;

        void Awake()
        {
            CurrentHP = maxHP;
        }

        public void SetActive(bool active)
        {
            IsActive = active;
            if (!IsEliminated) SetState(active ? WormVisualState.Active : WormVisualState.Idle);
        }

        public void SetAiming(bool aiming)
        {
            if (!IsActive || IsEliminated) return;
            SetState(aiming ? WormVisualState.Aiming : WormVisualState.Active);
        }

        public void ApplyDamage(float rawDamage)
        {
            if (IsEliminated) return;

            CurrentHP = Mathf.Max(0, CurrentHP - Mathf.RoundToInt(rawDamage));
            SetState(WormVisualState.Hit);
            OnHit?.Invoke(this);

            if (CurrentHP <= 0) Eliminate();
        }

        // Also used for the out-of-bounds-knockback edge case: instant elimination,
        // no damage roll needed.
        public void Eliminate()
        {
            if (IsEliminated) return;
            CurrentHP = 0;
            SetState(WormVisualState.Eliminated);
            OnEliminated?.Invoke(this);
        }

        void SetState(WormVisualState next)
        {
            if (VisualState == next) return;
            VisualState = next;
            OnStateChanged?.Invoke(this);
        }
    }
}
