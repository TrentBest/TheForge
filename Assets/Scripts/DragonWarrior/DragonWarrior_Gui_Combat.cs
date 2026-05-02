using System;
using UnityEngine;
using UnityEngine.UIElements;
using Workshop.Core.Memory;
using Workshop.UI_And_Tools.Forge.Builders.GuiBuilders;

namespace Workshop.Domains.DragonWarrior
{
    public class DragonWarrior_Gui_Combat : IGuiProvider
    {
        private DragonWarriorContext _ctx;
        public string Title => "COMBAT";

        public VisualElement CreateGui(GuiContext ctx)
        {
            DataWarehouse.Default.TryGetAsset<DragonWarriorContext>("ActiveSession", out _ctx);
            if (_ctx == null) return new VisualElement();

            var root = new ForgeContainerBuilder("CombatView")
                .WithFlexLayout(FlexDirection.Column, Justify.SpaceBetween, Align.Center)
                .WithBackgroundColor(Color.black)
                .WithPadding(20)
                .AddChild(new ForgeLabelBuilder(_ctx.EnemyName).WithColor(Color.white).WithFontSize(24))
                .AddChild(CreateCombatLog())
                .AddChild(CreateActionButtons())
                .Build();

            return root;
        }

        private VisualElement CreateCombatLog()
        {
            var log = new ForgeContainerBuilder("Log").WithHeight(100).Build();
            foreach (var line in _ctx.CombatLog)
                log.Add(new ForgeLabelBuilder(line).WithColor(Color.white).Build());
            return log;
        }

        private VisualElement CreateActionButtons()
        {
            var actions = new ForgeContainerBuilder("Actions").WithFlexLayout(FlexDirection.Row, Justify.Center, Align.Center).Build();
            if (_ctx.PlayerTurnActive)
            {
                actions.Add(new ForgeButtonBuilder("ATTACK").OnClick(() => DragonWarriorFsm.PlayerAttack(_ctx)).Build());
                actions.Add(new ForgeButtonBuilder("RUN").OnClick(() => _ctx.Status.TransitionTo("Exploration")).Build());
            }
            return actions;
        }

        public Action<VisualElement> GetGuiBuilder() => (root) => { };
        public void ToUIDocument(string assetPath) { }
        public void FromUIDocument(string assetPath) { }
    }
}