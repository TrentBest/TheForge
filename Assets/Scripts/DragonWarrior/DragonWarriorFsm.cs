using System;
using UnityEngine;
using TheSingularityWorkshop.FSM_API;
using Workshop.Core.Memory;
using Assets.Scripts.Workshop.Gameplay.Grid;

namespace Workshop.Domains.DragonWarrior
{
    public static class DragonWarriorFsm
    {
        public const string FSM_NAME = "DragonWarrior_Core";
        public const string GROUP_RUNTIME = "DragonWarrior_Runtime";

        public static void Register()
        {
            if (FSM_API.Interaction.Exists(FSM_NAME, GROUP_RUNTIME)) return;

            FSM_API.Create.CreateFiniteStateMachine(FSM_NAME, processRate: 1, processingGroup: GROUP_RUNTIME)
                .State("Exploration",
                    onEnter: (ctx) => {
                        var dw = (DragonWarriorContext)ctx;
                        dw.ActiveDialogue = "Thou art in Alefgard.";
                        dw.ActiveRoute = "EXPLORATION";
                    },
                    onUpdate: OnExplorationUpdate,
                    onExit: null)
                .State("Moving", null, OnMovingUpdate, null)
                .State("Menu",
                    onEnter: (ctx) => ((DragonWarriorContext)ctx).IsMenuOpen = true,
                    onUpdate: null,
                    onExit: (ctx) => ((DragonWarriorContext)ctx).IsMenuOpen = false)

                // Combat States
                .State("Combat_Intro",
                    onEnter: OnCombatIntro,
                    onUpdate: null,
                    onExit: null)
                .State("Combat_PlayerTurn",
                    onEnter: (ctx) => {
                        var dw = (DragonWarriorContext)ctx;
                        dw.PlayerTurnActive = true;
                        dw.ActiveRoute = "COMBAT";
                    },
                    onUpdate: null,
                    onExit: (ctx) => ((DragonWarriorContext)ctx).PlayerTurnActive = false)
                .State("Combat_EnemyTurn",
                    onEnter: OnEnemyTurn,
                    onUpdate: null,
                    onExit: null)
                .State("Combat_Victory",
                    onEnter: OnVictory,
                    onUpdate: null,
                    onExit: null)

                .WithInitialState("Exploration")

                // Transitions
                .Transition("Exploration", "Menu", (ctx) => ((DragonWarriorContext)ctx).IsMenuOpen)
                .Transition("Menu", "Exploration", (ctx) => !((DragonWarriorContext)ctx).IsMenuOpen)
                .Transition("Combat_Intro", "Combat_PlayerTurn", (ctx) => true)
                .Transition("Combat_PlayerTurn", "Combat_EnemyTurn", (ctx) => !((DragonWarriorContext)ctx).PlayerTurnActive && ((DragonWarriorContext)ctx).EnemyHP > 0)
                .Transition("Combat_EnemyTurn", "Combat_PlayerTurn", (ctx) => ((DragonWarriorContext)ctx).EnemyHP > 0 && ((DragonWarriorContext)ctx).HP > 0)
                .Transition("Combat_PlayerTurn", "Combat_Victory", (ctx) => ((DragonWarriorContext)ctx).EnemyHP <= 0)
                .Transition("Combat_Victory", "Exploration", (ctx) => Input.anyKeyDown)

                .BuildDefinition();
        }

        public static void Launch(GridMapData map)
        {
            Register();
            var ctx = new DragonWarriorContext(map);
            ctx.Status = FSM_API.Create.CreateInstance(FSM_NAME, ctx, GROUP_RUNTIME);
            DataWarehouse.Default.RegisterAsset("ActiveSession", ctx);
        }

        private static void OnExplorationUpdate(IStateContext context)
        {
            if (!context.IsValid) return;
            var ctx = (DragonWarriorContext)context;

            if (Input.GetKeyDown(KeyCode.W)) TryMove(ctx, Vector2Int.up);
            else if (Input.GetKeyDown(KeyCode.S)) TryMove(ctx, Vector2Int.down);
            else if (Input.GetKeyDown(KeyCode.A)) TryMove(ctx, Vector2Int.left);
            else if (Input.GetKeyDown(KeyCode.D)) TryMove(ctx, Vector2Int.right);
            else if (Input.GetKeyDown(KeyCode.M)) ctx.IsMenuOpen = true;
        }

        private static void TryMove(DragonWarriorContext ctx, Vector2Int dir)
        {
            ctx.FacingDirection = dir;
            ctx.Status.TransitionTo("Moving");
        }

        private static void OnMovingUpdate(IStateContext context)
        {
            var ctx = (DragonWarriorContext)context;
            Vector2Int targetPos = ctx.MapPosition + ctx.FacingDirection;

            if (ctx.WorldMap != null && ctx.WorldMap.GetCell(targetPos.x, targetPos.y) != 3)
            {
                ctx.MapPosition = targetPos;
                if (UnityEngine.Random.value < 0.1f) TriggerEncounter(ctx);
            }
            ctx.Status.TransitionTo("Exploration");
        }

        private static void TriggerEncounter(DragonWarriorContext ctx)
        {
            ctx.EnemyName = "Slime";
            ctx.EnemyMaxHP = 3;
            ctx.EnemyHP = 3;
            ctx.CombatLog.Clear();
            ctx.Status.TransitionTo("Combat_Intro");
        }

        private static void OnCombatIntro(IStateContext context)
        {
            var ctx = (DragonWarriorContext)context;
            ctx.ActiveDialogue = $"A {ctx.EnemyName} draws near!";
            ctx.CombatLog.Add($"Encountered {ctx.EnemyName}!");
        }

        private static void OnEnemyTurn(IStateContext context)
        {
            var ctx = (DragonWarriorContext)context;
            ctx.HP -= 1;
            ctx.CombatLog.Add($"{ctx.EnemyName} attacks! Thy HP decreased by 1.");
        }

        private static void OnVictory(IStateContext context)
        {
            var ctx = (DragonWarriorContext)context;
            ctx.ActiveDialogue = $"Thou hast slain the {ctx.EnemyName}!";
            ctx.Gold += 2;
        }

        public static void PlayerAttack(DragonWarriorContext ctx)
        {
            ctx.EnemyHP -= 2;
            ctx.CombatLog.Add($"{ctx.HeroName} attacks! The {ctx.EnemyName}'s HP decreased by 2.");
            ctx.PlayerTurnActive = false;
        }
    }
}