using Unity.Netcode;
using UnityEngine;

namespace WormWars.Network
{
    // A single structural piece of a castle (wall/floor/pillar). Blocks start as kinematic
    // rigidbodies - cheap to keep around in bulk - and only turn into simulated physics
    // objects, and start paying the cost of network sync, once an explosion actually
    // dislodges them.
    [RequireComponent(typeof(Rigidbody))]
    public class CastleBlock : NetworkBehaviour
    {
        [SerializeField] float positionLerpSpeed = 12f;
        [SerializeField] float rotationSlerpSpeed = 12f;

        readonly NetworkVariable<bool> _isDynamic =
            new NetworkVariable<bool>(writePerm: NetworkVariableWritePermission.Server);

        readonly NetworkVariable<Vector3> _networkPosition =
            new NetworkVariable<Vector3>(writePerm: NetworkVariableWritePermission.Server);

        readonly NetworkVariable<Quaternion> _networkRotation =
            new NetworkVariable<Quaternion>(writePerm: NetworkVariableWritePermission.Server);

        Rigidbody _rigidbody;

        void Awake()
        {
            _rigidbody = GetComponent<Rigidbody>();
            _rigidbody.isKinematic = true;
        }

        public override void OnNetworkSpawn()
        {
            if (IsServer)
            {
                _networkPosition.Value = transform.position;
                _networkRotation.Value = transform.rotation;
            }
            else
            {
                transform.SetPositionAndRotation(_networkPosition.Value, _networkRotation.Value);
            }
        }

        // Called server-side by whatever detonates nearby (see NetworkProjectile.Explode).
        // Safe to call more than once - a block already caught in a blast just gets an
        // additional impulse layered on top.
        public void Detonate(Vector3 explosionPosition, float explosionForce, float explosionRadius, float upwardsModifier)
        {
            if (!IsServer) return;

            if (!_isDynamic.Value)
            {
                _rigidbody.isKinematic = false;
                _isDynamic.Value = true;
            }

            _rigidbody.AddExplosionForce(explosionForce, explosionPosition, explosionRadius, upwardsModifier, ForceMode.Impulse);
        }

        void FixedUpdate()
        {
            if (!IsServer || !_isDynamic.Value) return;

            _networkPosition.Value = _rigidbody.position;
            _networkRotation.Value = _rigidbody.rotation;
        }

        void Update()
        {
            if (IsServer || !_isDynamic.Value) return;

            transform.position = Vector3.Lerp(transform.position, _networkPosition.Value, positionLerpSpeed * Time.deltaTime);
            transform.rotation = Quaternion.Slerp(transform.rotation, _networkRotation.Value, rotationSlerpSpeed * Time.deltaTime);
        }
    }
}
