using UnityEngine;

namespace WormWars.Core
{
    [CreateAssetMenu(fileName = "NewWeapon", menuName = "WormWars/Weapon Definition")]
    public class WeaponDefinition : ScriptableObject
    {
        public string weaponName = "Weapon";
        public WeaponTier tier = WeaponTier.Starter;
        public Sprite icon;

        public float baseDamage = 1f;

        // Indexed by the target castle's hitCount (see CastleController.HitCount). Values
        // ramp from <1.0 toward 1.0 per the weapon's tier — see damage formula in the spec.
        public float[] escalationCurve = { 0.3f, 0.6f, 1f };

        // Rare/Super weapons apply a smaller tick every turn in addition to impact damage.
        public bool hasDamageOverTime;
        public float dotDamagePerTick;

        // -1 = infinite (the always-available default/melee weapon must set this).
        public int maxAmmo = 6;

        public float EscalationAt(int hitCount)
        {
            if (escalationCurve == null || escalationCurve.Length == 0) return 1f;
            int idx = Mathf.Clamp(hitCount, 0, escalationCurve.Length - 1);
            return escalationCurve[idx];
        }
    }
}
