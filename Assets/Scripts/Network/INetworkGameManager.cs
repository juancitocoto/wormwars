using Unity.Netcode;

namespace WormWars.Network
{
    // The slice of a networked match coordinator that other server-side systems (HUD/UI,
    // match setup) need. Kept as an interface rather than a hard reference to a concrete
    // GameManager class so those scripts can be built and wired up independently of
    // whichever NetworkBehaviour ends up owning match state.
    public interface INetworkGameManager
    {
        NetworkVariable<float> TurnTimeRemaining { get; }
        NetworkVariable<ulong> ActiveClientId { get; }
        string ActiveWeaponName { get; }

        string GetPlayerDisplayName(ulong clientId);

        // Server-only. Called once match setup (worm spawning) has finished, to hand off
        // into the very first player's active turn phase.
        void BeginMatch(ulong firstActiveClientId);

        // Server-only. Called once a victory/defeat/draw condition is met. Implementations
        // are expected to transition into a GameOver state that pauses all turn timers and
        // locks every client's movement/weapon input.
        void EndMatch(ulong winningClientId, bool isDraw);
    }
}
