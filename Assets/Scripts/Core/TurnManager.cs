using System;
using UnityEngine;

namespace WormWars.Core
{
    // Drives the turn-start / player-aiming / projectile-flight / impact-resolve / turn-end
    // cycle for the LOCAL active player. OpponentTurn is not entered by this class directly —
    // in an online match, the non-active client should be driven into OpponentTurn externally
    // (by the network layer) instead of PlayerAiming; that wiring doesn't exist yet.
    public class TurnManager : MonoBehaviour
    {
        public const float TurnStartDuration = 0.6f;
        public const float TurnEndDuration = 0.4f;
        public const float TurnTimeLimitSeconds = 30f; // [TUNE]
        public const float UrgentThresholdSeconds = 10f;

        public TurnState State { get; private set; } = TurnState.TurnStart;
        public TeamId ActiveTeam { get; private set; } = TeamId.A;
        public float TimeRemaining { get; private set; }
        public bool IsUrgent => State == TurnState.PlayerAiming && TimeRemaining <= UrgentThresholdSeconds;
        public bool InputLocked => State != TurnState.PlayerAiming;

        public event Action<TurnState> OnStateChanged;
        public event Action<float> OnTimerTick;
        public event Action OnTurnTimedOut;

        float _stateTimer;
        int _lastWholeSecond = -1;

        public void BeginTurn(TeamId team)
        {
            ActiveTeam = team;
            TimeRemaining = TurnTimeLimitSeconds;
            _lastWholeSecond = -1;
            _stateTimer = 0f;
            SetState(TurnState.TurnStart);
        }

        void Update()
        {
            switch (State)
            {
                case TurnState.TurnStart:
                    _stateTimer += Time.deltaTime;
                    if (_stateTimer >= TurnStartDuration) SetState(TurnState.PlayerAiming);
                    break;

                case TurnState.PlayerAiming:
                    TickTimer();
                    break;

                case TurnState.TurnEnd:
                    _stateTimer += Time.deltaTime;
                    if (_stateTimer >= TurnEndDuration) BeginTurn(Opponent(ActiveTeam));
                    break;
            }
        }

        void TickTimer()
        {
            TimeRemaining = Mathf.Max(0f, TimeRemaining - Time.deltaTime);
            OnTimerTick?.Invoke(TimeRemaining);

            int whole = Mathf.CeilToInt(TimeRemaining);
            if (whole != _lastWholeSecond)
            {
                _lastWholeSecond = whole;
                if (whole <= 5 && whole > 0) HapticsBridge.LightTick();
            }

            if (TimeRemaining <= 0f)
            {
                // Turn timer expired mid-drag: no shot fires, arc must be cancelled by the
                // joystick UI listening for OnTurnTimedOut.
                OnTurnTimedOut?.Invoke();
                _stateTimer = 0f;
                SetState(TurnState.TurnEnd);
            }
        }

        public void FireShot()
        {
            if (State != TurnState.PlayerAiming) return;
            HapticsBridge.MediumImpact();
            SetState(TurnState.ProjectileFlight);
        }

        public void ResolveImpact() => SetState(TurnState.ImpactResolve);

        public void FinishImpactAndEndTurn()
        {
            _stateTimer = 0f;
            SetState(TurnState.TurnEnd);
        }

        static TeamId Opponent(TeamId team) => team == TeamId.A ? TeamId.B : TeamId.A;

        void SetState(TurnState next)
        {
            State = next;
            OnStateChanged?.Invoke(next);
        }
    }
}
