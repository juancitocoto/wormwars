using Unity.Netcode;
using UnityEngine;

namespace WormWars.Network
{
    // Server-authoritative worm head movement. Owning clients forward raw input to the
    // server via ServerRpc; the server is the only instance that ever touches the
    // CharacterController, and it publishes the resulting transform through
    // NetworkVariables so every other client can interpolate toward it smoothly.
    [RequireComponent(typeof(CharacterController))]
    public class NetworkWormMovement : NetworkBehaviour
    {
        [Header("Movement Tuning")]
        [SerializeField] float moveSpeed = 6f;
        [SerializeField] float turnSpeed = 180f;
        [SerializeField] float jumpSpeed = 6f;
        [SerializeField] float gravity = -18f;

        [Header("Remote Interpolation")]
        [SerializeField] float positionLerpSpeed = 12f;
        [SerializeField] float rotationSlerpSpeed = 12f;

        [Header("Segments")]
        [SerializeField] WormSegmentChain segmentChain;

        readonly NetworkVariable<Vector3> _networkPosition =
            new NetworkVariable<Vector3>(writePerm: NetworkVariableWritePermission.Server);

        readonly NetworkVariable<Quaternion> _networkRotation =
            new NetworkVariable<Quaternion>(writePerm: NetworkVariableWritePermission.Server);

        CharacterController _controller;
        float _verticalVelocity;

        void Awake()
        {
            _controller = GetComponent<CharacterController>();
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
                // Snap remote instances straight to the current server state so they
                // don't visibly lerp in from wherever the prefab happened to spawn.
                transform.SetPositionAndRotation(_networkPosition.Value, _networkRotation.Value);
            }

            if (segmentChain != null) segmentChain.SnapToHead();
        }

        void Update()
        {
            if (IsOwner) ReadAndSendInput();
            if (!IsServer) InterpolateRemoteTransform();
        }

        void ReadAndSendInput()
        {
            float horizontal = Input.GetAxis("Horizontal");
            float vertical = Input.GetAxis("Vertical");
            bool jumpPressed = Input.GetButtonDown("Jump");

            SubmitMovementInputServerRpc(new Vector2(horizontal, vertical), jumpPressed, Time.deltaTime);
        }

        [ServerRpc]
        void SubmitMovementInputServerRpc(Vector2 axisInput, bool jumpPressed, float deltaTime)
        {
            ApplyServerMovement(axisInput, jumpPressed, deltaTime);
        }

        void ApplyServerMovement(Vector2 axisInput, bool jumpPressed, float deltaTime)
        {
            if (deltaTime <= 0f) return;

            transform.Rotate(Vector3.up, axisInput.x * turnSpeed * deltaTime);

            if (_controller.isGrounded)
            {
                _verticalVelocity = jumpPressed ? jumpSpeed : -0.5f;
            }
            else
            {
                _verticalVelocity += gravity * deltaTime;
            }

            Vector3 forwardMotion = transform.forward * (axisInput.y * moveSpeed);
            Vector3 motion = forwardMotion + Vector3.up * _verticalVelocity;
            _controller.Move(motion * deltaTime);

            _networkPosition.Value = transform.position;
            _networkRotation.Value = transform.rotation;
        }

        void InterpolateRemoteTransform()
        {
            transform.position = Vector3.Lerp(transform.position, _networkPosition.Value, positionLerpSpeed * Time.deltaTime);
            transform.rotation = Quaternion.Slerp(transform.rotation, _networkRotation.Value, rotationSlerpSpeed * Time.deltaTime);
        }
    }
}
