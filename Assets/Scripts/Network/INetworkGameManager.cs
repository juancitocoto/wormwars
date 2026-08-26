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
    }
}
