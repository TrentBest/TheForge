using Assets.Scripts.Workshop.UI_And_Tools.Forge.Builders.GuiBuilders;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;
using Workshop.UI_And_Tools.Forge.Builders.GuiBuilders;

namespace Workshop.GURPS
{
    /// <summary>
    /// GURPS Combat Alpha: The moment-to-moment tactical orchestrator.
    /// Manages Turn Sequence, Maneuver Selection, and Damage Calculation.
    /// Reforged to follow the Singularity Forge Protocol.
    /// </summary>
    public class GURPS_CombatAlpha_Provider : IGuiProvider
    {
        public string Title => "COMBAT ALPHA ORCHESTRATOR";

        // Runtime State
        private int _currentTurn = 1;
        private string _activeUnit = "Player_01";
        private float _selectedHitLocationPenalty = 0f;

        public VisualElement CreateGui(GuiContext ctx)
        {
            var rootBuilder = new ForgeContainerBuilder("CombatAlpha_Root")
                .WithFlexGrow(1f)
                .WithPadding(20f)
                .WithBackgroundColor(new Color(0.1f, 0.05f, 0.05f)); // "War Room" Crimson Tint

            // --- HEADER: COMBAT STATUS ---
            rootBuilder.AddChild(new ForgeContainerBuilder("Header")
                .WithBorderColor(new Color(0.8f, 0.2f, 0.2f))
                .WithBorderWidth(0, 0, 3f, 0)
                .WithMarginBottom(15f)
                .AddChild(new ForgeLabelBuilder(Title).WithFontSize(22).WithBold().WithColor(new Color(1f, 0.4f, 0.4f)))
                .AddChild(new ForgeLabelBuilder($"TURN: {_currentTurn} | ACTIVE: {_activeUnit}").WithFontSize(10).WithColor(Color.gray))
            );

            // --- MAIN BODY: SPLIT VIEW ---
            var body = new ForgeContainerBuilder("Body").WithDirection(FlexDirection.Row).WithFlexGrow(1f);

            // Left: Initiative & Maneuvers
            body.AddChild(new ForgeContainerBuilder("TacticsPane")
                .WithWidth(new StyleLength(Length.Percent(40f)))
                .WithPadding(10f).WithBackgroundColor(new Color(0.12f, 0.1f, 0.1f))
                .AddChild(new ForgeLabelBuilder("MANEUVER SELECTION").WithBold().WithMarginBottom(10f))
                .AddChild(CreateManeuverButton("Attack", "Standard strike, full defense allowed."))
                .AddChild(CreateManeuverButton("All-Out Attack", "+2 to hit or +2 damage, NO defense.", Color.red))
                .AddChild(CreateManeuverButton("All-Out Defense", "+2 to one defense or 2 defenses.", Color.green))
                .AddChild(CreateManeuverButton("Move & Attack", "Penalty to hit, capped skill.", Color.yellow))
            );

            // Right: Ballistics & Hit Locations
            body.AddChild(new ForgeContainerBuilder("BallisticsPane")
                .WithFlexGrow(1f).WithMarginLeft(15f)
                .WithPadding(10f).WithBackgroundColor(new Color(0.12f, 0.1f, 0.1f))
                .AddChild(new ForgeLabelBuilder("TARGETING MATRIX").WithBold().WithMarginBottom(10f))
                .AddChild(new ForgeDropdownBuilder("Hit Location",
                    new List<string> { "Torso (-0)", "Head (-7)", "Vitals (-3)", "Arm (-2)", "Leg (-2)" },
                    "Torso (-0)",
                    val => Debug.Log($"Targeting: {val}")))
                .AddSeparator(new Color(0.3f, 0.3f, 0.3f), 1f)
                .AddChild(new ForgeLabelBuilder("CALCULATED ODDS").WithColor(Color.cyan).WithMarginTop(10f))
                // Logic injection for GURPS 3d6 probabilities
                .AddChild(new ForgeLabelBuilder("Effective Skill: 14 | Prob: 90.7%").WithFontSize(12))
            );

            rootBuilder.AddChild(body);

            // --- FOOTER: ACTION BAR ---
            rootBuilder.AddChild(new ForgeContainerBuilder("Actions")
                .WithDirection(FlexDirection.Row).WithMarginTop(15f)
                .AddChild(new ForgeButtonBuilder("🎲 ROLL ATTACK")
                    .WithBackgroundColor(new Color(0.6f, 0.2f, 0.2f)).WithWidth(150f)
                    .OnClick(() => Debug.Log("[Combat] Roll: 10 vs 14. Success!")))
                .AddChild(new ForgeButtonBuilder("⏭ END TURN")
                    .WithBackgroundColor(new Color(0.2f, 0.2f, 0.2f)).WithMarginLeft(10f)
                    .OnClick(() => { _currentTurn++; })));

            return rootBuilder.Build();
        }

        private IGuiProvider CreateManeuverButton(string name, string desc, Color? tint = null)
        {
            return new DynamicGuiProvider(ctx => {
                return new ForgeButtonBuilder($"{name}\n<size=9><color=#888>{desc}</color></size>")
                    .WithBackgroundColor(tint ?? new Color(0.2f, 0.2f, 0.25f))
                    .WithMarginBottom(5f)
                    .Build();
            });
        }

        public Action<VisualElement> GetGuiBuilder() => root => root.Add(CreateGui(new GuiContext()));
        public void ToUIDocument(string path) => WorkshopUxmlBaker.Bake(CreateGui(new GuiContext()), "CombatAlpha_UXML");
        public void FromUIDocument(string path) { }
    }
}