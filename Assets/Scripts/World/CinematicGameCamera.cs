using UnityEngine;

namespace WormWars.Core
{
    public enum CameraState { Idle, FollowWorm, FollowProjectile, FocusExplosion }

    // Local (non-networked) "open-faced dollhouse" camera rig: a fixed side-profile viewing
    // angle at a clamped distance, gliding between whatever target the Game Manager points
    // it at. Entirely engine-camera-agnostic - it only ever moves this Transform, so it
    // works the same under any render pipeline.
    public class CinematicGameCamera : MonoBehaviour
    {
        [Header("Dollhouse Framing")]
        [SerializeField] Vector3 sideProfileDirection = new Vector3(0f, 0.35f, -1f);
        [SerializeField] float distance = 12f;
        [SerializeField] float minDistance = 6f;
        [SerializeField] float maxDistance = 20f;

        [Header("Movement")]
        [SerializeField] float followSmoothTime = 0.35f;
        [SerializeField] float targetTransitionDuration = 0.5f;

        public CameraState State { get; private set; } = CameraState.Idle;

        Transform _target;
        Vector3 _focusPoint;
        Vector3 _previousFocusPoint;
        float _targetBlendTimer;
        bool _isBlendingTarget;

        Vector3 _basePosition;
        Vector3 _positionVelocity;

        float _shakeIntensity;
        float _shakeDuration;
        float _shakeTimer;

        void Awake()
        {
            distance = Mathf.Clamp(distance, minDistance, maxDistance);
            _basePosition = transform.position;
        }

        void OnValidate()
        {
            maxDistance = Mathf.Max(minDistance, maxDistance);
            distance = Mathf.Clamp(distance, minDistance, maxDistance);
        }

        // Called by the Game Manager whenever it wants the camera looking at something new.
        public void SetTarget(Transform newTarget, CameraState state)
        {
            if (newTarget == null) return;

            _previousFocusPoint = _target != null ? _focusPoint : newTarget.position;
            _target = newTarget;
            State = state;
            _targetBlendTimer = 0f;
            _isBlendingTarget = true;
        }

        public void SetDistance(float newDistance)
        {
            distance = Mathf.Clamp(newDistance, minDistance, maxDistance);
        }

        public void TriggerShake(float intensity, float duration)
        {
            _shakeIntensity = intensity;
            _shakeDuration = Mathf.Max(0.0001f, duration);
            _shakeTimer = 0f;
        }

        void LateUpdate()
        {
            if (_target != null)
            {
                UpdateFocusPoint();
                Vector3 desiredPosition = _focusPoint + sideProfileDirection.normalized * distance;
                _basePosition = Vector3.SmoothDamp(_basePosition, desiredPosition, ref _positionVelocity, followSmoothTime);
                transform.position = _basePosition + ComputeShakeOffset();
                transform.LookAt(_focusPoint);
            }
            else
            {
                transform.position = _basePosition + ComputeShakeOffset();
            }
        }

        void UpdateFocusPoint()
        {
            if (!_isBlendingTarget)
            {
                _focusPoint = _target.position;
                return;
            }

            _targetBlendTimer += Time.deltaTime;
            float t = Mathf.Clamp01(_targetBlendTimer / targetTransitionDuration);
            _focusPoint = Vector3.Lerp(_previousFocusPoint, _target.position, t);

            if (t >= 1f) _isBlendingTarget = false;
        }

        Vector3 ComputeShakeOffset()
        {
            if (_shakeTimer >= _shakeDuration) return Vector3.zero;

            _shakeTimer += Time.deltaTime;
            float remaining01 = 1f - Mathf.Clamp01(_shakeTimer / _shakeDuration);
            float currentIntensity = _shakeIntensity * remaining01;

            Vector2 randomOffset = Random.insideUnitCircle * currentIntensity;
            return transform.right * randomOffset.x + transform.up * randomOffset.y;
        }
    }
}
