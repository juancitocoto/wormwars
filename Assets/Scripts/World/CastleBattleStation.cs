using UnityEngine;

namespace WormWars.Core
{
    // Marks one of a Castle3DView's three ledges as a spot where a worm can be placed for
    // battle. Castle3DView creates one of these per interior wall (Back/Left/Right — there is
    // no Front wall, so there are always exactly three). Gameplay code that wants to seat a
    // worm reads SpawnPoint rather than guessing ledge geometry.
    public class CastleBattleStation : MonoBehaviour
    {
        public CastleWallSide wallSide;
        public Transform SpawnPoint;
        public WormController occupant;

        public bool IsOccupied => occupant != null;
    }
}
