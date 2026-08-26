using System;
using System.Collections.Generic;
using System.Linq;
using Unity.Netcode;
using UnityEngine;

namespace WormWars.Network
{
    // The result of a completed NetworkMatchSpawner.SpawnMatch() call - which client owns
    // which spawned worms - for anything downstream (e.g. a match-end monitor) that needs
    // to track both teams without re-deriving them from the spawn points itself.
    public readonly struct MatchSpawnResult
    {
        public readonly ulong Player1ClientId;
        public readonly IReadOnlyList<NetworkWormHealth> Player1Worms;
        public readonly ulong Player2ClientId;
        public readonly IReadOnlyList<NetworkWormHealth> Player2Worms;

        public MatchSpawnResult(ulong player1ClientId, IReadOnlyList<NetworkWormHealth> player1Worms, ulong player2ClientId, IReadOnlyList<NetworkWormHealth> player2Worms)
        {
            Player1ClientId = player1ClientId;
            Player1Worms = player1Worms;
            Player2ClientId = player2ClientId;
            Player2Worms = player2Worms;
        }
    }

    // Server-authoritative match setup: spawns each connected player's worms inside their
    // own dollhouse castle and owns nothing about how those worms look or render - just
    // which prefab, where, and who owns it.
    public class NetworkMatchSpawner : NetworkBehaviour
    {
        [SerializeField] NetworkObject wormPrefab;

        [Header("Castle A - Player 1")]
        [SerializeField] Transform[] player1CastleSpawnPoints;

        [Header("Castle B - Player 2")]
        [SerializeField] Transform[] player2CastleSpawnPoints;

        [Header("Game Manager")]
        [Tooltip("Must implement INetworkGameManager.")]
        [SerializeField] MonoBehaviour gameManagerSource;

        public event Action<MatchSpawnResult> OnMatchSpawned;

        INetworkGameManager _gameManager;
        bool _matchStarted;

        void Awake()
        {
            _gameManager = gameManagerSource as INetworkGameManager;
        }

        // Server-only. Call once when the match should begin, e.g. from a lobby/ready-check
        // system once both players have connected.
        public void SpawnMatch()
        {
            if (!IsServer || _matchStarted) return;

            if (wormPrefab == null)
            {
                Debug.LogError($"{nameof(NetworkMatchSpawner)}: wormPrefab is not assigned.", this);
                return;
            }

            List<ulong> orderedClientIds = NetworkManager.Singleton.ConnectedClientsIds.OrderBy(id => id).ToList();
            if (orderedClientIds.Count < 2)
            {
                Debug.LogError($"{nameof(NetworkMatchSpawner)}: need at least 2 connected clients to start a match, found {orderedClientIds.Count}.", this);
                return;
            }

            _matchStarted = true;

            ulong player1ClientId = orderedClientIds[0];
            ulong player2ClientId = orderedClientIds[1];

            List<NetworkWormHealth> player1Worms = SpawnTeam(player1ClientId, player1CastleSpawnPoints);
            List<NetworkWormHealth> player2Worms = SpawnTeam(player2ClientId, player2CastleSpawnPoints);

            OnMatchSpawned?.Invoke(new MatchSpawnResult(player1ClientId, player1Worms, player2ClientId, player2Worms));
            _gameManager?.BeginMatch(player1ClientId);
        }

        List<NetworkWormHealth> SpawnTeam(ulong ownerClientId, Transform[] spawnPoints)
        {
            var spawnedWorms = new List<NetworkWormHealth>();
            if (spawnPoints == null) return spawnedWorms;

            foreach (Transform spawnPoint in spawnPoints)
            {
                if (spawnPoint == null) continue;

                // The owner can disconnect mid-setup - stop spawning worms for a client
                // that's no longer connected rather than handing ownership to a ghost.
                if (!NetworkManager.Singleton.ConnectedClientsIds.Contains(ownerClientId))
                {
                    Debug.LogWarning($"{nameof(NetworkMatchSpawner)}: client {ownerClientId} disconnected during setup - skipping their remaining spawns.", this);
                    return spawnedWorms;
                }

                NetworkObject instance = Instantiate(wormPrefab, spawnPoint.position, spawnPoint.rotation);
                instance.SpawnWithOwnership(ownerClientId);

                if (instance.TryGetComponent(out NetworkWormHealth health)) spawnedWorms.Add(health);
            }

            return spawnedWorms;
        }
    }
}
