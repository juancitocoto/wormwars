using Unity.Netcode;
using UnityEngine;

namespace WormWars.Network
{
    // Owner-driven weapon launcher. The client only ever requests a shot; the server
    // decides the actual spawn point (its own muzzle transform, never a client-supplied
    // one) and owns the projectile from the moment it exists.
    public class WeaponLauncher : NetworkBehaviour
    {
        [SerializeField] Transform muzzle;
        [SerializeField] NetworkObject projectilePrefab;
        [SerializeField] float maxLaunchSpeed = 40f;

        public void RequestFire(Vector3 launchVelocity)
        {
            if (!IsOwner) return;
            FireServerRpc(launchVelocity);
        }

        [ServerRpc]
        void FireServerRpc(Vector3 launchVelocity)
        {
            if (projectilePrefab == null || muzzle == null) return;

            Vector3 clampedVelocity = Vector3.ClampMagnitude(launchVelocity, maxLaunchSpeed);
            Vector3 facing = clampedVelocity.sqrMagnitude > 0.0001f ? clampedVelocity.normalized : muzzle.forward;

            NetworkObject instance = Instantiate(projectilePrefab, muzzle.position, Quaternion.LookRotation(facing, Vector3.up));
            instance.Spawn(true);
            instance.GetComponent<NetworkProjectile>().ServerInitialize(clampedVelocity);
        }
    }
}
