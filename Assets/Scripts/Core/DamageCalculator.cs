using UnityEngine;

namespace WormWars.Core
{
    public static class DamageCalculator
    {
        // damage = baseDamage x escalationCurve[castle.hitCount] x strengthMultiplier x breachBonus
        // Escalation only applies to castle targets; worm damage never escalates.
        public static float AgainstCastle(WeaponDefinition weapon, int castleHitCount, float strengthMultiplier, bool targetIsBreached, float breachBonusMultiplier)
        {
            float escalation = weapon.EscalationAt(castleHitCount);
            float breachBonus = targetIsBreached ? breachBonusMultiplier : 1f;
            return Mathf.Max(0f, weapon.baseDamage * escalation * strengthMultiplier * breachBonus);
        }

        public static float AgainstWorm(WeaponDefinition weapon, float strengthMultiplier)
        {
            return Mathf.Max(0f, weapon.baseDamage * strengthMultiplier);
        }
    }
}
