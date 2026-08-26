using System;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

namespace WormWars.Network
{
    // Server-authoritative win/loss/draw detection. Registers both teams once
    // NetworkMatchSpawner finishes, watches every worm's death, and declares a result the
    // moment either roster is wiped. Pure game-state logic - the only thing it touches
    // visually is telling MultiplayerHUDManager which end-game text to show.
    public class NetworkMatchEndMonitor : NetworkBehaviour
    {
        [SerializeField] NetworkMatchSpawner matchSpawner;
        [SerializeField] MultiplayerHUDManager hudManager;

        [Tooltip("Must implement INetworkGameManager.")]
        [SerializeField] MonoBehaviour gameManagerSource;

        readonly List<NetworkWormHealth> _player1Worms = new List<NetworkWormHealth>();
        readonly List<NetworkWormHealth> _player2Worms = new List<NetworkWormHealth>();
        readonly Dictionary<NetworkWormHealth, Action> _deathHandlers = new Dictionary<NetworkWormHealth, Action>();

        INetworkGameManager _gameManager;
        ulong _player1ClientId;
        ulong _player2ClientId;
        bool _matchEnded;

        void Awake()
        {
            _gameManager = gameManagerSource as INetworkGameManager;
        }

        public override void OnNetworkSpawn()
        {
            if (!IsServer) return;
            if (matchSpawner != null) matchSpawner.OnMatchSpawned += HandleMatchSpawned;
        }

        public override void OnNetworkDespawn()
        {
            if (!IsServer) return;

            if (matchSpawner != null) matchSpawner.OnMatchSpawned -= HandleMatchSpawned;
            UnregisterAll();
        }

        void HandleMatchSpawned(MatchSpawnResult result)
        {
            _player1ClientId = result.Player1ClientId;
            _player2ClientId = result.Player2ClientId;

            RegisterTeam(_player1Worms, result.Player1Worms);
            RegisterTeam(_player2Worms, result.Player2Worms);
        }

        void RegisterTeam(List<NetworkWormHealth> team, IReadOnlyList<NetworkWormHealth> spawnedWorms)
        {
            foreach (NetworkWormHealth worm in spawnedWorms)
            {
                if (worm == null) continue;

                team.Add(worm);

                Action handler = () => HandleWormDied(worm, team);
                _deathHandlers[worm] = handler;
                worm.OnDied += handler;
            }
        }

        void HandleWormDied(NetworkWormHealth worm, List<NetworkWormHealth> team)
        {
            if (!IsServer || _matchEnded) return;

            if (_deathHandlers.TryGetValue(worm, out Action handler))
            {
                worm.OnDied -= handler;
                _deathHandlers.Remove(worm);
            }

            team.Remove(worm);
            EvaluateVictoryCondition();
        }

        void EvaluateVictoryCondition()
        {
            bool player1Wiped = _player1Worms.Count == 0;
            bool player2Wiped = _player2Worms.Count == 0;
            if (!player1Wiped && !player2Wiped) return;

            // Both castles can collapse on the same simultaneous explosion - check the
            // double-wipe case before either single-winner case.
            if (player1Wiped && player2Wiped) DeclareResult(0, isDraw: true);
            else if (player1Wiped) DeclareResult(_player2ClientId, isDraw: false);
            else DeclareResult(_player1ClientId, isDraw: false);
        }

        void DeclareResult(ulong winningClientId, bool isDraw)
        {
            if (_matchEnded) return;
            _matchEnded = true;

            UnregisterAll();
            _gameManager?.EndMatch(winningClientId, isDraw);
            DisplayMatchResultClientRpc(winningClientId, isDraw);
        }

        void UnregisterAll()
        {
            foreach (KeyValuePair<NetworkWormHealth, Action> entry in _deathHandlers)
            {
                if (entry.Key != null) entry.Key.OnDied -= entry.Value;
            }

            _deathHandlers.Clear();
        }

        [ClientRpc]
        void DisplayMatchResultClientRpc(ulong winningClientId, bool isDraw)
        {
            if (hudManager == null) return;

            bool localPlayerWon = !isDraw && NetworkManager.Singleton.LocalClientId == winningClientId;
            hudManager.ShowMatchResult(localPlayerWon, isDraw);
        }
    }
}
