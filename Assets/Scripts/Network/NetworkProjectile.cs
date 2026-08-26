using Unity.Netcode;
using UnityEngine;

namespace WormWars.Network
{
    // Server-authoritative flying projectile. The server owns the entire flight
    // simulation (gravity integration + collision) and publishes the result through a
    // NetworkVariable; every client just lerps its visual toward that synced position,
    // so the flight path is identical for everyone regardless of local frame rate.
    public class NetworkProjectile : NetworkBehaviour
    {
        [Header("Flight")]
        [SerializeField] float gravity = -9.81f;
        [SerializeField] float maxLifetimeSeconds = 8f;
        [SerializeField] LayerMask collisionMask = ~0;
        [SerializeField] float collisionCheckRadius = 0.15f;

        [Header("Explosion")]
        [SerializeField] float explosionRadius = 4f;
        [SerializeField] float explosionForce = 700f;
        [SerializeField] NetworkExplosionManager explosionManager;

        [Header("Remote Interpolation")]
        [SerializeField] float positionLerpSpeed = 20f;

        readonly NetworkVariable<Vector3> _networkPosition =
            new NetworkVariable<Vector3>(writePerm: NetworkVariableWritePermission.Server);

        Vector3 _velocity;
        float _elapsed;
        bool _exploded;

        // Called by the server right after spawning the projectile (see
        // WeaponLauncher.FireServerRpc) - never trust a client to set this itself.
        public void ServerInitialize(Vector3 launchVelocity)
        {
            if (!IsServer) return;
            _velocity = launchVelocity;
            _networkPosition.Value = transform.position;
        }

        public override void OnNetworkSpawn()
        {
            if (!IsServer) transform.position = _networkPosition.Value;
        }

        void FixedUpdate()
        {
            if (!IsServer || _exploded) return;

            _elapsed += Time.fixedDeltaTime;
            if (_elapsed >= maxLifetimeSeconds)
            {
                Explode(transform.position);
                return;
            }

            _velocity += Vector3.up * (gravity * Time.fixedDeltaTime);
            Vector3 nextPosition = transform.position + _velocity * Time.fixedDeltaTime;

            if (Physics.SphereCast(transform.position, collisionCheckRadius, _velocity.normalized,
                    out RaycastHit hit, _velocity.magnitude * Time.fixedDeltaTime, collisionMask))
            {
                transform.position = hit.point;
                Explode(hit.point);
                return;
            }

            transform.position = nextPosition;
            _networkPosition.Value = transform.position;
        }

        void Update()
        {
            if (IsServer || _exploded) return;
            transform.position = Vector3.Lerp(transform.position, _networkPosition.Value, positionLerpSpeed * Time.deltaTime);
        }

        void Explode(Vector3 explosionPosition)
        {
            if (_exploded) return;
            _exploded = true;

            if (explosionManager != null) explosionManager.TriggerExplosion(explosionPosition, explosionRadius, explosionForce);

            NetworkObject.Despawn(true);
        }
    }
}
