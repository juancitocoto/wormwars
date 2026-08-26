using Unity.Netcode;
using UnityEngine;
using WormWars.Core;

namespace WormWars.Network
{
    // Single, reusable entry point for "something exploded here" - a projectile impact
    // today, potentially chained block-to-block collapses or scripted castle events later.
    // Owns turning nearby CastleBlocks dynamic and, since every client already runs its own
    // CinematicGameCamera, tells each one to shake proportionally to how close it is to the
    // blast. Zero rendering/shader dependencies - pure physics + a ClientRpc.
    public class NetworkExplosionManager : NetworkBehaviour
    {
        [Header("Structural Impact")]
        [SerializeField] LayerMask castleBlockMask = ~0;
        [SerializeField] float explosionUpwardsModifier = 0.3f;

        [Header("Camera Shake")]
        [SerializeField] float maxShakeDistance = 30f;
        [SerializeField] float baseShakeIntensity = 0.6f;
        [SerializeField] float shakeDuration = 0.4f;
        [SerializeField] CinematicGameCamera localCamera;

        // Server-only. Call this when a projectile impacts a surface (or any other
        // server-side event that should crack open the castle).
        public void TriggerExplosion(Vector3 explosionPoint, float radius, float force)
        {
            if (!IsServer) return;

            Collider[] hits = Physics.OverlapSphere(explosionPoint, radius, castleBlockMask);
            foreach (Collider hit in hits)
            {
                if (hit.TryGetComponent(out CastleBlock block))
                {
                    block.Detonate(explosionPoint, force, radius, explosionUpwardsModifier);
                }
            }

            // CastleBlock already syncs its own dynamic position/rotation via
            // NetworkVariable once detonated, so nothing further is needed for the
            // collapse itself here - only the local camera feedback remains.
            TriggerShakeClientRpc(explosionPoint, force);

            NetworkAudioManager.Instance?.PlaySoundEffect(WormSoundEvent.Explosion, explosionPoint);
        }

        [ClientRpc]
        void TriggerShakeClientRpc(Vector3 explosionPoint, float force)
        {
            CinematicGameCamera cam = ResolveLocalCamera();
            if (cam == null) return;

            float distance = Vector3.Distance(cam.transform.position, explosionPoint);
            float falloff = Mathf.Clamp01(1f - distance / Mathf.Max(0.0001f, maxShakeDistance));
            float intensity = baseShakeIntensity * falloff;
            if (intensity <= 0f) return;

            cam.TriggerShake(intensity, shakeDuration);
        }

        CinematicGameCamera ResolveLocalCamera()
        {
            if (localCamera == null) localCamera = FindFirstObjectByType<CinematicGameCamera>();
            return localCamera;
        }
    }
}
