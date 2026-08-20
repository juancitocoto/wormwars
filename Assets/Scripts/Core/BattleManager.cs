using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace WormWars.Core
{
    // Top-level orchestrator: owns win-condition evaluation and the "FINISH THE CASTLE" /
    // "ELIMINATE THE TEAM" banner state described in the spec's Edge Cases section.
    public class BattleManager : MonoBehaviour
    {
        public CastleController castleA;
        public CastleController castleB;
        public List<WormController> teamA = new List<WormController>();
        public List<WormController> teamB = new List<WormController>();
        public float breachBonusMultiplier = 1.5f; // [TUNE]

        public bool MatchOver { get; private set; }

        public event Action<TeamId> OnMatchWon;
        public event Action OnMatchDraw;
        public event Action<string> OnBannerChanged; // null clears the banner

        string _currentBanner;

        public int AliveCount(TeamId team) => Roster(team).Count(w => !w.IsEliminated);
        public List<WormController> Roster(TeamId team) => team == TeamId.A ? teamA : teamB;
        public CastleController Castle(TeamId team) => team == TeamId.A ? castleA : castleB;

        // Call once after applying ALL damage for a given moment (a single shot's impact, or
        // a batch of simultaneous DoT ticks) — not once per individual hit — so the
        // "both castles destroyed same turn" and "last one down wins" edge cases resolve
        // against a consistent snapshot instead of a race between event handlers.
        public void CheckWinCondition()
        {
            if (MatchOver) return;

            bool aTeamWiped = AliveCount(TeamId.A) == 0;
            bool bTeamWiped = AliveCount(TeamId.B) == 0;
            bool aCastleDown = castleA.Stage == CastleDamageStage.Destroyed;
            bool bCastleDown = castleB.Stage == CastleDamageStage.Destroyed;

            bool aFullyDefeated = aTeamWiped && aCastleDown;
            bool bFullyDefeated = bTeamWiped && bCastleDown;

            if (aFullyDefeated && bFullyDefeated) { Draw(); return; }
            if (bFullyDefeated) { Win(TeamId.A); return; }
            if (aFullyDefeated) { Win(TeamId.B); return; }

            UpdateBanner(aTeamWiped, bTeamWiped, aCastleDown, bCastleDown);
        }

        public float ApplyCastleDamage(CastleController target, WeaponDefinition weapon, float strengthMultiplier)
        {
            float dmg = DamageCalculator.AgainstCastle(weapon, target.HitCount, strengthMultiplier, target.IsBreached, breachBonusMultiplier);
            target.ApplyDamage(dmg);
            return dmg;
        }

        public float ApplyWormDamage(WormController target, WeaponDefinition weapon, float strengthMultiplier)
        {
            float dmg = DamageCalculator.AgainstWorm(weapon, strengthMultiplier);
            target.ApplyDamage(dmg);
            return dmg;
        }

        void UpdateBanner(bool aTeamWiped, bool bTeamWiped, bool aCastleDown, bool bCastleDown)
        {
            string banner = null;
            if ((aTeamWiped && !aCastleDown) || (bTeamWiped && !bCastleDown)) banner = "FINISH THE CASTLE";
            else if ((aCastleDown && !aTeamWiped) || (bCastleDown && !bTeamWiped)) banner = "ELIMINATE THE TEAM";

            if (banner == _currentBanner) return;
            _currentBanner = banner;
            OnBannerChanged?.Invoke(banner);
        }

        void Win(TeamId team)
        {
            MatchOver = true;
            OnMatchWon?.Invoke(team);
        }

        void Draw()
        {
            MatchOver = true;
            OnMatchDraw?.Invoke();
        }
    }
}
