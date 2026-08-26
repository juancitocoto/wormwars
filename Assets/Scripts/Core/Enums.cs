namespace WormWars.Core
{
    public enum TeamId { A, B }

    public enum TurnState
    {
        TurnStart,
        PlayerAiming,
        ProjectileFlight,
        ImpactResolve,
        TurnEnd,
        OpponentTurn
    }

    public enum WeaponTier { Starter, Upgraded, Rare }

    public enum CastleDamageStage { Intact, Shockwave, Smoking, Rubble, Breached, Destroyed }

    public enum WormVisualState { Idle, Active, Aiming, Hit, Smoking, Eliminated }

    // Which of the castle's three interior walls a battle station ledge is mounted on.
    // The front is deliberately absent — see Castle3DView — so there is no Front value.
    public enum CastleWallSide { Back, Left, Right }

    // What part of a Castle3DView a CastleUpgradeDefinition reskins or adds. Keeping this
    // as a category (rather than one bool per possible cosmetic) is what lets the upgrade
    // catalog describe an in-app-purchase without the purchasing/store code needing to know
    // anything about castle geometry.
    public enum CastleUpgradeCategory { WallSkin, TowerCaps, Banner, LedgeTrim, StructureTier }
}
