using Unity.Netcode;

namespace WormWars.Network
{
    // The slice of a networked match coordinator that HUD/UI code needs. Kept as an
    // interface rather than a hard reference to a concrete GameManager class so UI scripts
    // can be built and wired up independently of whichever NetworkBehaviour ends up owning
    // match state.
    public interface INetworkGameManager
    {
        NetworkVariable<float> TurnTimeRemaining { get; }
        NetworkVariable<ulong> ActiveClientId { get; }
        string ActiveWeaponName { get; }

        string GetPlayerDisplayName(ulong clientId);

        // Server-only. Called the instant a projectile is spawned: pauses the turn timer
        // and revokes movement/jump/weapon-switching input for the rest of the turn.
        void EnterProjectileState();

        // Server-only. Called once a fired projectile has fully resolved (explosion
        // applied, castle debris settled) to hand focus off to the next worm's turn.
        void AdvanceToNextTurn();
    }
}
