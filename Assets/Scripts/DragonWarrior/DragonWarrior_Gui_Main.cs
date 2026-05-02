using System;
using UnityEngine;
using UnityEngine.UIElements;
using Workshop.Core.Memory;
using Workshop.UI_And_Tools.Forge.Builders.GuiBuilders;

namespace Workshop.Domains.DragonWarrior
{
    public class DragonWarrior_Gui_Main : IGuiProvider
    {
        private DragonWarriorContext _ctx;
        public string Title => "EXPLORATION";

        public VisualElement CreateGui(GuiContext ctx)
        {
            DataWarehouse.Default.TryGetAsset<DragonWarriorContext>("ActiveSession", out _ctx);
            if (_ctx == null) return new ForgeLabelBuilder("LOADING...").Build();

            return new ForgeContainerBuilder("ExplorationView")
                .WithFlexLayout(FlexDirection.Column, Justify.Center, Align.Center)
                .WithBackgroundColor(Color.black)
                .AddChild(CreateStatsBar())
                .AddChild(CreateWorldGrid())
                .AddChild(CreateMessageBox())
                .Build();
        }

        private VisualElement CreateStatsBar()
        {
            return new ForgeContainerBuilder("Stats")
                .WithFlexLayout(FlexDirection.Row, Justify.SpaceBetween, Align.Auto)
                .WithPadding(10)
                .WithBorderWidth(2)
                .WithBorderColor(Color.white)
                .AddChild(new ForgeLabelBuilder($"{_ctx.HeroName}").WithColor(Color.white))
                .AddChild(new ForgeLabelBuilder($"HP: {_ctx.HP}").WithColor(Color.white))
                .Build();
        }

        private VisualElement CreateWorldGrid()
        {
            return new ForgeGridGuiBuilder("Map", 11, 11)
                .OnCellRender((cell, x, y) => {
                    cell.style.backgroundColor = (x == 5 && y == 5) ? Color.red : Color.green;
                }).Build();
        }

        private VisualElement CreateMessageBox()
        {
            return new ForgeContainerBuilder("Msgs")
                .WithHeight(60)
                .WithBorderWidth(1)
                .WithBorderColor(Color.white)
                .AddChild(new ForgeLabelBuilder(_ctx.ActiveDialogue).WithColor(Color.white))
                .Build();
        }

        public Action<VisualElement> GetGuiBuilder() => (root) => { };
        public void ToUIDocument(string assetPath) { }
        public void FromUIDocument(string assetPath) { }
    }
}