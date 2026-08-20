using UnityEngine;
using UnityEngine.SceneManagement;
using WormWars.Core;
using WormWars.Layout;

namespace WormWars.Bootstrap
{
    // Entry point for the battle screen. Runs after any scene loads; only builds the screen
    // when the loaded scene is "BattleScreen", so this stays harmless once other scenes
    // (menu, customization, etc.) exist in the project. No component needs to be placed in
    // the .unity file for this to run — see BattleScreen.unity.
    public static class BattleBootstrap
    {
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        static void OnAfterSceneLoad()
        {
            if (SceneManager.GetActiveScene().name != "BattleScreen") return;

            var result = BattleLayoutBuilder.Build();
            Wire(result);

            result.TurnManager.BeginTurn(TeamId.A);
        }

        static void Wire(BattleLayoutBuilder.BuildResult r)
        {
            var weapons = BuildWeaponRoster();
            r.WeaponTray.Setup(weapons, lockedFromIndex: 3); // 3 unlocked + 1 locked placeholder, per spec

            r.WormCount.Refresh();
            foreach (var w in r.BattleManager.teamA) w.OnEliminated += _ => r.WormCount.Refresh();
            foreach (var w in r.BattleManager.teamB) w.OnEliminated += _ => r.WormCount.Refresh();

            r.BattleManager.OnBannerChanged += r.Banner.SetBanner;
            r.BattleManager.OnMatchWon += team => Debug.Log($"Match won by {team}");
            r.BattleManager.OnMatchDraw += () => Debug.Log("Match ended in a draw");

            r.Joystick.OnDragStarted += () => r.TrajectoryArc.SetVisible(true);
            r.Joystick.OnDragChanged += () => UpdateArcPreview(r);
            r.Joystick.OnDragEnded += () => r.TrajectoryArc.SetVisible(false);
            r.Joystick.OnDragCancelled += () => r.TrajectoryArc.SetVisible(false);

            r.FireButton.OnFired += charge => ResolveShot(r, charge);

            r.TurnManager.OnTurnTimedOut += () => r.Joystick.CancelDrag();
            r.TurnManager.OnStateChanged += state => OnTurnStateChanged(r, state);

            // Prime HUD for the first turn.
            OnTurnStateChanged(r, r.TurnManager.State);
        }

        static WeaponDefinition[] BuildWeaponRoster()
        {
            var melee = ScriptableObject.CreateInstance<WeaponDefinition>();
            melee.weaponName = "Melee";
            melee.tier = WeaponTier.Starter;
            melee.baseDamage = 0.6f;
            melee.escalationCurve = new[] { 0.3f, 0.6f, 1f, 1f };
            melee.maxAmmo = -1; // always available — see the zero-ammo edge case

            var starter = ScriptableObject.CreateInstance<WeaponDefinition>();
            starter.weaponName = "Starter Shot";
            starter.tier = WeaponTier.Starter;
            starter.baseDamage = 1.2f;
            starter.escalationCurve = new[] { 0.3f, 0.6f, 1f, 1f };
            starter.maxAmmo = 6;

            var upgraded = ScriptableObject.CreateInstance<WeaponDefinition>();
            upgraded.weaponName = "Upgraded Shot";
            upgraded.tier = WeaponTier.Upgraded;
            upgraded.baseDamage = 1.8f;
            upgraded.escalationCurve = new[] { 0.4f, 1f, 1f };
            upgraded.maxAmmo = 3;

            return new[] { melee, starter, upgraded };
        }

        static void UpdateArcPreview(BattleLayoutBuilder.BuildResult r)
        {
            var attackingTeam = r.TurnManager.ActiveTeam;
            var origin = (attackingTeam == TeamId.A ? r.WormSpawnA : r.WormSpawnB).position;
            float speed = Mathf.Lerp(40f, 160f, Mathf.Max(0.15f, r.Joystick.PullNormalized));
            r.TrajectoryArc.Draw(origin, r.Joystick.AngleDeg, speed, Vector2.zero);
        }

        // Simplified v1 resolution: no projectile collision simulation exists yet, so a shot
        // always lands on the opposing castle. Hitting worms will need real projectile
        // physics + collision, which is the next major piece of gameplay work.
        static void ResolveShot(BattleLayoutBuilder.BuildResult r, float charge01)
        {
            if (r.TurnManager.State != TurnState.PlayerAiming) return;

            var weapon = r.WeaponTray.SelectedWeapon;
            if (weapon == null) return;

            r.TurnManager.FireShot();

            var attackingTeam = r.TurnManager.ActiveTeam;
            var defendingTeam = attackingTeam == TeamId.A ? TeamId.B : TeamId.A;
            var targetCastle = r.BattleManager.Castle(defendingTeam);
            var origin = (attackingTeam == TeamId.A ? r.WormSpawnA : r.WormSpawnB).position;

            float speed = Mathf.Lerp(40f, 160f, charge01);
            r.TrajectoryArc.Draw(origin, r.Joystick.AngleDeg, speed, Vector2.zero);

            r.BattleManager.ApplyCastleDamage(targetCastle, weapon, strengthMultiplier: 1f); // [TUNE] no strength stat modeled yet
            r.WeaponTray.ConsumeSelectedAmmo();
            r.BattleManager.CheckWinCondition();

            r.TurnManager.ResolveImpact();
            r.TurnManager.FinishImpactAndEndTurn();
        }

        static void OnTurnStateChanged(BattleLayoutBuilder.BuildResult r, TurnState state)
        {
            bool aiming = state == TurnState.PlayerAiming;
            r.Joystick.Interactable = aiming;
            r.FireButton.SetDisabled(!aiming);
            r.Joystick.Tint(r.TurnManager.ActiveTeam);

            r.TurnBadge.teamId = r.TurnManager.ActiveTeam;
            r.TurnBadge.SetActive(true);
            int aliveTotal = r.BattleManager.AliveCount(r.TurnManager.ActiveTeam);
            r.TurnBadge.SetWorm(1, aliveTotal, showSubLabel: true);
        }
    }
}
