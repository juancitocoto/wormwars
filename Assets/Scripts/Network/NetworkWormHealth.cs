using System;
using Unity.Netcode;
using UnityEngine;

namespace WormWars.Network
{
    // Server-authoritative worm health. Every client reads currentHealth purely through
    // the NetworkVariable's change event - no polling - and the server is the only one
    // that can ever call TakeDamage. Zero rendering-pipeline dependency: the health bar is
    // an IWorldSpaceHealthBar the inspector wires up, not anything this script draws itself.
    public class NetworkWormHealth : NetworkBehaviour
    {
        [SerializeField] int maxHealth = 100;
        [SerializeField] bool despawnOnDeath = true;
        [Tooltip("Must implement IWorldSpaceHealthBar.")]
        [SerializeField] MonoBehaviour healthBarSource;

        readonly NetworkVariable<int> _currentHealth =
            new NetworkVariable<int>(100, writePerm: NetworkVariableWritePermission.Server);

        public int CurrentHealth => _currentHealth.Value;
        public bool IsDead { get; private set; }

        public event Action<int, int> OnHealthChanged; // previous, current
        public event Action OnDied;

        IWorldSpaceHealthBar _healthBar;

        void Awake()
        {
            _healthBar = healthBarSource as IWorldSpaceHealthBar;
        }

        public override void OnNetworkSpawn()
        {
            if (IsServer) _currentHealth.Value = maxHealth;

            _currentHealth.OnValueChanged += HandleHealthChanged;
            RefreshHealthBar(_currentHealth.Value);
        }

        public override void OnNetworkDespawn()
        {
            _currentHealth.OnValueChanged -= HandleHealthChanged;
        }

        // Server-only. Call from wherever damage is resolved, e.g. an explosion radius
        // check finding this worm among the affected colliders.
        public void TakeDamage(int amount)
        {
            if (!IsServer || IsDead || amount <= 0) return;

            _currentHealth.Value -= amount;
            if (_currentHealth.Value <= 0) HandleDeath();
        }

        void HandleDeath()
        {
            IsDead = true;
            PlayDeathEffectsClientRpc();

            if (despawnOnDeath)
            {
                NetworkObject.Despawn(true);
            }
            else
            {
                SetGameplayComponentsEnabled(false);
            }
        }

        void SetGameplayComponentsEnabled(bool enable)
        {
            // Disables by type rather than assuming one specific movement/weapon script,
            // so this stays decoupled from whichever gameplay components end up on the
            // worm prefab.
            foreach (NetworkBehaviour behaviour in GetComponents<NetworkBehaviour>())
            {
                if (behaviour == this) continue;
                behaviour.enabled = enable;
            }
        }

        [ClientRpc]
        void PlayDeathEffectsClientRpc()
        {
            OnDied?.Invoke();
        }

        void HandleHealthChanged(int previous, int current)
        {
            OnHealthChanged?.Invoke(previous, current);
            RefreshHealthBar(current);
        }

        void RefreshHealthBar(int current)
        {
            if (_healthBar == null) return;
            float percent01 = maxHealth <= 0 ? 0f : Mathf.Clamp01((float)current / maxHealth);
            _healthBar.SetHealthPercent01(percent01);
        }
    }
}
