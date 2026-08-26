using Unity.Netcode;
using UnityEngine;

namespace WormWars.Network
{
    // Server-authoritative fire request -> ammo check -> projectile spawn -> turn-state
    // handoff, tying NetworkWormInventory, WeaponLauncher, and the network GameManager
    // together for one worm. Pure gameplay/turn logic - spawning is delegated to
    // WeaponLauncher, so there's no rendering-pipeline dependency here at all.
    public class NetworkCombatTurnHandler : NetworkBehaviour
    {
        [SerializeField] WeaponLauncher weaponLauncher;
        [SerializeField] NetworkWormInventory inventory;

        [Tooltip("Must implement INetworkGameManager.")]
        [SerializeField] MonoBehaviour gameManagerSource;

        INetworkGameManager _gameManager;

        void Awake()
        {
            _gameManager = gameManagerSource as INetworkGameManager;
            if (weaponLauncher == null) weaponLauncher = GetComponent<WeaponLauncher>();
            if (inventory == null) inventory = GetComponent<NetworkWormInventory>();
        }

        // Called by the owning client's Fire input.
        public void RequestFire(Vector3 firePosition, Vector3 fireVelocity)
        {
            if (!IsOwner) return;
            FireWeaponServerRpc(firePosition, fireVelocity);
        }

        // firePosition is part of the request for completeness/telemetry, but the actual
        // spawn point always comes from WeaponLauncher's own trusted muzzle transform -
        // never a client-supplied position.
        [ServerRpc]
        void FireWeaponServerRpc(Vector3 firePosition, Vector3 fireVelocity)
        {
            if (!CanFire()) return;

            int weaponID = inventory.ActiveWeaponID;
            inventory.ConsumeAmmo(weaponID);

            NetworkProjectile projectile = weaponLauncher.SpawnProjectile(fireVelocity);
            if (projectile != null) projectile.OnExploded += OnProjectileExploded;

            _gameManager?.EnterProjectileState();
        }

        bool CanFire()
        {
            if (_gameManager == null || inventory == null || weaponLauncher == null) return false;

            // OwnerClientId, not NetworkManager.Singleton.LocalClientId - this runs on the
            // server, which has no "local player" of its own.
            if (OwnerClientId != _gameManager.ActiveClientId.Value) return false;

            return inventory.HasAmmo(inventory.ActiveWeaponID);
        }

        // Server-only. Called once the fired projectile has fully resolved (explosion
        // applied, castle debris settled) - normally via NetworkProjectile.OnExploded above,
        // but exposed publicly in case some other resolution path needs to trigger it.
        public void OnProjectileExploded()
        {
            if (!IsServer) return;
            _gameManager?.AdvanceToNextTurn();
        }
    }
}
