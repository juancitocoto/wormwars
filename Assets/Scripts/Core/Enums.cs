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
}
