using System;
using UnityEngine;

namespace WormWars.Core
{
    public class CastleController : MonoBehaviour
    {
        public TeamId teamId;
        public int upgradeTier = 1;
        public int maxHP = 10;

        public int CurrentHP { get; private set; }
        // Hits landed against this castle, regardless of weapon used. Escalation curves are
        // indexed by this, not per-weapon, so switching weapons mid-siege doesn't reset it.
        public int HitCount { get; private set; }
        public CastleDamageStage Stage { get; private set; } = CastleDamageStage.Intact;
        public float HPPercent01 => maxHP <= 0 ? 0f : Mathf.Clamp01((float)CurrentHP / maxHP);
        public bool IsBreached => Stage == CastleDamageStage.Breached;

        public event Action<CastleController> OnDamaged;
        public event Action<CastleController> OnStageChanged;
        public event Action<CastleController> OnDestroyed;

        void Awake()
        {
            CurrentHP = maxHP;
        }

        // Castle HP by tier, transcribed from the Castle HP table in worm_battle_handoff_spec.md.
        // [TUNE] pending playtest, same as the source table.
        public static int MaxHPForTier(int tier)
        {
            if (tier <= 1) return 10;
            if (tier == 2) return 16;
            return 24;
        }

        // Applied when a StructureTier CastleUpgradeDefinition is bought — see Castle3DView.
        // Full-heals to the new max rather than preserving damage percentage: a tier purchase
        // happens in the build/shop screen between battles, not mid-siege.
        public void SetTier(int tier)
        {
            upgradeTier = tier;
            maxHP = MaxHPForTier(tier);
            CurrentHP = maxHP;
            HitCount = 0;
            Stage = CastleDamageStage.Intact;
        }

        public void ApplyDamage(float rawDamage)
        {
            if (Stage == CastleDamageStage.Destroyed) return;

            CurrentHP = Mathf.Max(0, CurrentHP - Mathf.RoundToInt(rawDamage));
            HitCount++;
            OnDamaged?.Invoke(this);
            UpdateStage();
        }

        void UpdateStage()
        {
            var next = DeriveStage(HPPercent01);
            if (next == Stage) return;

            Stage = next;
            OnStageChanged?.Invoke(this);
            if (Stage == CastleDamageStage.Destroyed) OnDestroyed?.Invoke(this);
        }

        public static CastleDamageStage DeriveStage(float hpPercent01)
        {
            float p = hpPercent01 * 100f;
            if (p <= 0f) return CastleDamageStage.Destroyed;
            if (p <= 39f) return CastleDamageStage.Breached;
            if (p <= 69f) return CastleDamageStage.Rubble;
            if (p <= 84f) return CastleDamageStage.Smoking;
            if (p <= 99f) return CastleDamageStage.Shockwave;
            return CastleDamageStage.Intact;
        }
    }
}
