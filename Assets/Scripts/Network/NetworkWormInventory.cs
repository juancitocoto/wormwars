using System;
using Unity.Netcode;
using UnityEngine;

namespace WormWars.Network
{
    // Server-authoritative weapon selection. The owning client reads input and requests a
    // switch; the server is the only thing that ever writes activeWeaponID, after
    // validating the requested weapon is actually available. Zero rendering-pipeline
    // dependency - OnActiveWeaponChanged is the hook a custom pipeline uses to swap which
    // 3D weapon mesh is visible in the worm's hands.
    public class NetworkWormInventory : NetworkBehaviour
    {
        [SerializeField] WeaponData[] availableWeapons;

        [Header("Game Manager")]
        [Tooltip("Must implement INetworkGameManager.")]
        [SerializeField] MonoBehaviour gameManagerSource;

        [Header("HUD")]
        [SerializeField] MultiplayerHUDManager hudManager;

        readonly NetworkVariable<int> _activeWeaponID =
            new NetworkVariable<int>(0, writePerm: NetworkVariableWritePermission.Server);

        public int ActiveWeaponID => _activeWeaponID.Value;

        // Fires on every client for every worm, regardless of ownership - a custom render
        // pipeline hooks this to toggle the correct weapon mesh's visibility.
        public event Action<WeaponData> OnActiveWeaponChanged;

        INetworkGameManager _gameManager;

        void Awake()
        {
            _gameManager = gameManagerSource as INetworkGameManager;
        }

        public override void OnNetworkSpawn()
        {
            _activeWeaponID.OnValueChanged += HandleActiveWeaponChanged;
            NotifyActiveWeaponChanged(_activeWeaponID.Value);
        }

        public override void OnNetworkDespawn()
        {
            _activeWeaponID.OnValueChanged -= HandleActiveWeaponChanged;
        }

        void Update()
        {
            if (!IsOwner || !IsLocalPlayersTurn() || availableWeapons == null || availableWeapons.Length == 0) return;
            ReadCycleInput();
        }

        bool IsLocalPlayersTurn()
        {
            return _gameManager != null && NetworkManager.Singleton != null
                && NetworkManager.Singleton.LocalClientId == _gameManager.ActiveClientId.Value;
        }

        void ReadCycleInput()
        {
            int currentIndex = IndexOfWeapon(_activeWeaponID.Value);

            if (Input.GetKeyDown(KeyCode.E))
            {
                RequestWeaponChange(availableWeapons[(currentIndex + 1) % availableWeapons.Length].weaponID);
                return;
            }

            if (Input.GetKeyDown(KeyCode.Q))
            {
                RequestWeaponChange(availableWeapons[(currentIndex - 1 + availableWeapons.Length) % availableWeapons.Length].weaponID);
                return;
            }

            for (int i = 0; i < availableWeapons.Length && i < 9; i++)
            {
                if (Input.GetKeyDown(KeyCode.Alpha1 + i))
                {
                    RequestWeaponChange(availableWeapons[i].weaponID);
                    return;
                }
            }
        }

        int IndexOfWeapon(int weaponID)
        {
            for (int i = 0; i < availableWeapons.Length; i++)
            {
                if (availableWeapons[i] != null && availableWeapons[i].weaponID == weaponID) return i;
            }
            return 0;
        }

        void RequestWeaponChange(int requestedID)
        {
            if (requestedID == _activeWeaponID.Value) return;
            ChangeWeaponServerRpc(requestedID);
        }

        [ServerRpc]
        void ChangeWeaponServerRpc(int requestedID)
        {
            WeaponData requested = FindWeapon(requestedID);
            if (requested == null) return;
            if (requested.ammoCount == 0) return; // not owned / no ammo left

            _activeWeaponID.Value = requestedID;
        }

        WeaponData FindWeapon(int weaponID)
        {
            if (availableWeapons == null) return null;

            foreach (WeaponData weapon in availableWeapons)
            {
                if (weapon != null && weapon.weaponID == weaponID) return weapon;
            }

            return null;
        }

        void HandleActiveWeaponChanged(int previous, int current) => NotifyActiveWeaponChanged(current);

        void NotifyActiveWeaponChanged(int weaponID)
        {
            WeaponData weapon = FindWeapon(weaponID);
            OnActiveWeaponChanged?.Invoke(weapon);

            // Only push to this client's own HUD when it's this worm's owner and it's
            // their turn - otherwise every worm on the field would fight over one label.
            if (weapon != null && IsOwner && IsLocalPlayersTurn() && hudManager != null)
            {
                hudManager.RefreshActiveWeapon(weapon.weaponName);
            }
        }
    }
}
