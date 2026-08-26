using UnityEngine;

namespace WormWars.Network
{
    [CreateAssetMenu(fileName = "NewWeaponData", menuName = "WormWars/Networked Weapon Data")]
    public class WeaponData : ScriptableObject
    {
        public string weaponName = "Weapon";
        // 0 = not owned/no ammo, negative = infinite ammo, positive = shots remaining.
        public int ammoCount = -1;
        public int damageValue = 10;
        public float explosionRadius = 4f;
        public int weaponID;
    }
}
