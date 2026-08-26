namespace WormWars.Network
{
    // What NetworkWormHealth needs from whatever world-space health bar hovers above a
    // worm's head. Kept as an interface, not a concrete SpriteRenderer/Canvas/UI Toolkit
    // implementation, so the health script stays entirely independent of any specific
    // rendering pipeline - the visual is somebody else's problem.
    public interface IWorldSpaceHealthBar
    {
        void SetHealthPercent01(float percent01);
    }
}
