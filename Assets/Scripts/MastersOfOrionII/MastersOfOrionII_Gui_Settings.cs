using System;
using UnityEngine;
using UnityEngine.UIElements;
using Workshop.UI_And_Tools.Forge.Builders.GuiBuilders;
using Workshop.UI_And_Tools.Forge.IO;

namespace Assets.Scripts.MastersOfOrionII
{
    public class MastersOfOrionII_Gui_Settings : IGuiProvider
    {
        public string Title => "System Configuration";

        public VisualElement CreateGui(GuiContext ctx)
        {
            var game = GameObject.FindAnyObjectByType<MastersOfOrionII_Game>();

            var rootBuilder = new ForgeContainerBuilder("Settings_Root")
                .WithFlexGrow(1f)
                .WithBackgroundColor(new Color(0.05f, 0.05f, 0.05f)) // Darker space void
                .WithFlexLayout(FlexDirection.Column, Justify.Center, Align.Center);

            // FIX: Using DynamicGuiProvider to wrap the internal warning panel lambda
            rootBuilder.AddChild(new DynamicGuiProvider(c => {
                var panel = new ForgeContainerBuilder("WarningPanel")
                    .WithPadding(40f)
                    .WithBorderWidth(2f)
                    .WithBorderColor(Color.yellow)
                    .WithBackgroundColor(new Color(0.2f, 0.2f, 0f, 0.5f))
                    .WithAlignItems(Align.Center)

                    .AddChild(new ForgeLabelBuilder("SYSTEM ALERT")
                        .WithFontSize(30)
                        .WithColor(Color.yellow)
                        .WithBold()
                        .WithMarginBottom(20f))

                    .AddChild(new ForgeLabelBuilder("Configuration subsystems are currently hard-coded.\nAudio and Video protocols operating at default parameters.")
                        .WithFontSize(14)
                        .WithColor(Color.white)
                        .WithTextAlign(TextAnchor.MiddleCenter)
                        .WithMarginBottom(40f))

                    .AddChild(new ForgeButtonBuilder("< RETURN TO COMMAND")
                        .WithHeight(50f)
                        .WithWidth(250f)
                        .WithBackgroundColor(new Color(0.3f, 0.3f, 0.3f))
                        .WithBold()
                        .OnClick(() => {
                            if (game != null) game.SwitchGui("MainMenu");
                        }));

                return panel.Build();
            }));

            return rootBuilder.Build();
        }

        public Action<VisualElement> GetGuiBuilder() => (root) => root.Add(CreateGui(new GuiContext()));

        public void ToUIDocument(string assetPath)
        {
            var snapshot = CreateGui(new GuiContext());
            WorkshopUxmlBaker.Bake(snapshot, "Settings_Snapshot");
        }

        public void FromUIDocument(string assetPath)
        {
            Debug.LogWarning("[SettingsGUI] Static import bypassed. Logic is procedurally generated.");
        }
    }
}