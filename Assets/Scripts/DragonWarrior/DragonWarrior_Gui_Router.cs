using System;
using UnityEngine;
using UnityEngine.UIElements;
using Workshop.Core.Memory;
using Workshop.UI_And_Tools.Forge.Builders.GuiBuilders;

namespace Workshop.Domains.DragonWarrior
{
    public class DragonWarrior_Gui_Router : IGuiProvider
    {
        public string Title => "DRAGON WARRIOR - SYSTEM";

        public VisualElement CreateGui(GuiContext ctx)
        {
            if (!DataWarehouse.Default.TryGetAsset<DragonWarriorContext>("ActiveSession", out var dwCtx))
            {
                return new ForgeContainerBuilder("SearchState")
                    .WithFlexLayout(FlexDirection.Column, Justify.Center, Align.Center)
                    .AddChild(new ForgeLabelBuilder("NO ACTIVE SESSION").WithColor(Color.red))
                    .AddChild(new ForgeButtonBuilder("BOOT ALEFGARD")
                        .OnClick(() => DragonWarriorFsm.Launch(null)) // Pass actual GridMapData in production
                        .Build())
                    .Build();
            }

            // Route resolution
            return dwCtx.ActiveRoute switch
            {
                "COMBAT" => new DragonWarrior_Gui_Combat().CreateGui(ctx),
                _ => new DragonWarrior_Gui_Main().CreateGui(ctx)
            };
        }

        public Action<VisualElement> GetGuiBuilder() => (root) => { };
        public void ToUIDocument(string assetPath) { }
        public void FromUIDocument(string assetPath) { }
    }
}