using System;
using Unity.Netcode;
using UnityEngine;
using WormWars.Core;

namespace WormWars.Network
{
    // Server-authoritative wind plus a pure-math trajectory simulation clients use to draw
    // an aiming preview. The prediction is plain data (a Vector3[]) with no LineRenderer or
    // render-pipeline dependency, so a custom pipeline can grab it and draw however it likes.
    public class NetworkTrajectoryPredictor : NetworkBehaviour
    {
        [Header("Wind")]
        [SerializeField] TurnManager turnManager;
        [SerializeField] float maxWindStrength = 3f;

        [Header("Aiming Preview")]
        [SerializeField] Transform aimOrigin;
        [SerializeField] int previewResolution = 30;
        [SerializeField] float previewTimeStep = 0.08f;
        [SerializeField] float projectileGravity = -9.81f;

        readonly NetworkVariable<Vector3> _currentWind =
            new NetworkVariable<Vector3>(writePerm: NetworkVariableWritePermission.Server);

        public Vector3 CurrentWind => _currentWind.Value;
        public Vector3[] PreviewPoints { get; private set; } = Array.Empty<Vector3>();

        public event Action<Vector3[]> OnTrajectoryUpdated;

        bool _isAiming;
        Vector3 _aimDirection = Vector3.forward;
        float _aimPower;
        float _aimAngleDegrees;

        public override void OnNetworkSpawn()
        {
            if (!IsServer) return;

            RandomizeWind();
            if (turnManager != null) turnManager.OnStateChanged += HandleTurnStateChanged;
        }

        public override void OnNetworkDespawn()
        {
            if (IsServer && turnManager != null) turnManager.OnStateChanged -= HandleTurnStateChanged;
        }

        void HandleTurnStateChanged(TurnState state)
        {
            if (state == TurnState.TurnStart) RandomizeWind();
        }

        void RandomizeWind()
        {
            _currentWind.Value = new Vector3(
                UnityEngine.Random.Range(-maxWindStrength, maxWindStrength),
                0f,
                UnityEngine.Random.Range(-maxWindStrength, maxWindStrength));
        }

        // Called by the aiming UI (joystick/weapon controls) each frame while the local
        // player is dragging their shot.
        public void SetAimInput(Vector3 horizontalDirection, float power, float angleDegrees)
        {
            _aimDirection = horizontalDirection.sqrMagnitude > 0.0001f ? horizontalDirection.normalized : _aimDirection;
            _aimPower = power;
            _aimAngleDegrees = angleDegrees;
        }

        public void SetAiming(bool aiming) => _isAiming = aiming;

        void Update()
        {
            if (!IsOwner || !_isAiming || aimOrigin == null) return;

            Vector3 initialVelocity = ComputeLaunchVelocity();
            PreviewPoints = GetTrajectoryPoints(aimOrigin.position, initialVelocity, projectileGravity, previewResolution, previewTimeStep);
            OnTrajectoryUpdated?.Invoke(PreviewPoints);
        }

        Vector3 ComputeLaunchVelocity()
        {
            float rad = _aimAngleDegrees * Mathf.Deg2Rad;
            return (_aimDirection * Mathf.Cos(rad) + Vector3.up * Mathf.Sin(rad)) * _aimPower;
        }

        // Pure math simulation - no physics engine calls - so it can run many times a frame
        // on the client without touching the actual Rigidbody/CharacterController world.
        public Vector3[] GetTrajectoryPoints(Vector3 startPosition, Vector3 initialVelocity, float projectileGravity, int resolution, float timeStep)
        {
            var points = new Vector3[Mathf.Max(0, resolution)];
            Vector3 position = startPosition;
            Vector3 velocity = initialVelocity;

            for (int i = 0; i < points.Length; i++)
            {
                points[i] = position;
                velocity += (Vector3.up * projectileGravity + _currentWind.Value) * timeStep;
                position += velocity * timeStep;
            }

            return points;
        }
    }
}
