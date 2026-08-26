using System.Collections;
using Unity.Netcode;
using UnityEngine;

namespace WormWars.Network
{
    // Client-side raycast targeting + server-authoritative missile-rain weapon. Missiles
    // are just NetworkProjectile instances given a downward launch velocity, so they reuse
    // the existing gravity/collision/explosion pipeline instead of duplicating it - only
    // the targeting, spawn stagger, and turn handoff are specific to this weapon.
    public class NetworkAirstrikeTargeter : NetworkBehaviour
    {
        [Header("Targeting")]
        [SerializeField] Camera targetingCamera;
        [SerializeField] LayerMask targetableMask = ~0;
        [Tooltip("A world-space marker (e.g. a red crosshair mesh) shown at the raycast hit point. Positioning/visibility only - no material/shader ownership here.")]
        [SerializeField] Transform targetIndicator;
        [SerializeField] int weaponID;

        [Header("Missile Wave")]
        [SerializeField] NetworkObject missilePrefab;
        [SerializeField] int minMissileCount = 4;
        [SerializeField] int maxMissileCount = 5;
        [SerializeField] float missileSpawnHeight = 30f;
        [SerializeField] float missileLineSpread = 3f;
        [SerializeField] float missileSpeed = 25f;
        [SerializeField] float staggerSeconds = 0.15f;

        [Header("Dependencies")]
        [SerializeField] NetworkWormInventory inventory;
        [Tooltip("Must implement INetworkGameManager.")]
        [SerializeField] MonoBehaviour gameManagerSource;

        INetworkGameManager _gameManager;
        bool _hasTarget;
        Vector3 _pendingTargetPosition;
        int _missilesInFlight;

        void Awake()
        {
            _gameManager = gameManagerSource as INetworkGameManager;
            if (inventory == null) inventory = GetComponent<NetworkWormInventory>();
            if (targetingCamera == null) targetingCamera = Camera.main;
        }

        void Update()
        {
            if (!IsOwner || !IsActiveWeaponSelected() || !IsLocalPlayersTurn())
            {
                SetIndicatorVisible(false);
                return;
            }

            if (Input.GetMouseButtonDown(0)) TryUpdateTarget();
        }

        bool IsActiveWeaponSelected() => inventory != null && inventory.ActiveWeaponID == weaponID;

        bool IsLocalPlayersTurn()
        {
            return _gameManager != null && NetworkManager.Singleton != null
                && NetworkManager.Singleton.LocalClientId == _gameManager.ActiveClientId.Value;
        }

        void TryUpdateTarget()
        {
            if (targetingCamera == null) return;

            Ray ray = targetingCamera.ScreenPointToRay(Input.mousePosition);
            if (!Physics.Raycast(ray, out RaycastHit hit, float.PositiveInfinity, targetableMask)) return;

            _hasTarget = true;
            _pendingTargetPosition = hit.point;
            SetIndicatorVisible(true);
            if (targetIndicator != null) targetIndicator.position = hit.point;
        }

        void SetIndicatorVisible(bool visible)
        {
            if (targetIndicator != null) targetIndicator.gameObject.SetActive(visible);
        }

        // Called by the Fire button UI once a target has been placed.
        public void RequestAirstrike()
        {
            if (!IsOwner || !_hasTarget) return;

            CallAirstrikeServerRpc(_pendingTargetPosition);
            _hasTarget = false;
            SetIndicatorVisible(false);
        }

        [ServerRpc]
        void CallAirstrikeServerRpc(Vector3 targetPosition)
        {
            if (!CanCallAirstrike()) return;

            inventory.ConsumeAmmo(weaponID);
            _gameManager?.EnterProjectileState();

            StartCoroutine(SpawnMissileWave(targetPosition));
        }

        bool CanCallAirstrike()
        {
            if (_gameManager == null || inventory == null || missilePrefab == null) return false;

            // OwnerClientId, not NetworkManager.Singleton.LocalClientId - this runs on the
            // server, which has no "local player" of its own.
            if (OwnerClientId != _gameManager.ActiveClientId.Value) return false;

            return inventory.HasAmmo(weaponID);
        }

        IEnumerator SpawnMissileWave(Vector3 targetPosition)
        {
            int count = Random.Range(minMissileCount, maxMissileCount + 1);
            _missilesInFlight = count;

            for (int i = 0; i < count; i++)
            {
                SpawnMissile(targetPosition);
                yield return new WaitForSeconds(staggerSeconds);
            }
        }

        void SpawnMissile(Vector3 targetPosition)
        {
            // A line of missiles along local X, not a 2D scatter, per the "clustered line"
            // rain pattern.
            Vector3 lineOffset = Vector3.right * Random.Range(-missileLineSpread, missileLineSpread);
            Vector3 spawnPosition = targetPosition + Vector3.up * missileSpawnHeight + lineOffset;

            NetworkObject instance = Instantiate(missilePrefab, spawnPosition, Quaternion.identity);
            instance.Spawn(true);

            if (instance.TryGetComponent(out NetworkProjectile projectile))
            {
                projectile.ServerInitialize(Vector3.down * missileSpeed);
                projectile.OnExploded += HandleMissileExploded;
            }
            else
            {
                HandleMissileExploded();
            }
        }

        void HandleMissileExploded()
        {
            _missilesInFlight--;
            if (_missilesInFlight <= 0) _gameManager?.AdvanceToNextTurn();
        }
    }
}
