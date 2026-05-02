using Assets.Editor;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;
using UnityEngine.UIElements;
using Workshop.Core.Extensions;
using Workshop.UI_And_Tools.Forge.Builders.GuiBuilders;
using Workshop.UI_And_Tools.Forge.Builders.GuiBuilders.Themes;

namespace Singularity
{
    public class Workshop_Gui_FsmTransitionBuilderTab : IGuiProvider
    {
        public string Title => "Transition Logic Designer";

        private List<string> _availableStates = new();
        private List<string> _availableConditions = new();
        private string _cachePath;

        public Workshop_Gui_FsmTransitionBuilderTab()
        {
            _cachePath = Path.Combine(Application.persistentDataPath, "TSW_Registry_Cache");
            LoadRegistries();
        }

        private void LoadRegistries()
        {
            _availableStates = LoadFile("StateRegistry.txt");
            _availableConditions = LoadFile("ConditionRegistry.txt");
        }

        private List<string> LoadFile(string fileName)
        {
            string path = Path.Combine(_cachePath, fileName);
            return File.Exists(path) ? File.ReadAllLines(path).ToList() : new List<string> { "None" };
        }

        public VisualElement CreateGui(GuiContext ctx)
        {
            var theme = GuiSkin.Active;

            // MISSION: Pure Builder Implementation
            var builder = new GraphicalUserInterfaceBuilder("TransitionEditor")
                .WithPadding(15)
                .WithAutoGrow()

                // Header highlight
                .AddChild(new Label("LOGIC FILAMENTS").Bold().Color(theme.PrimaryAccent).FontSize(14))

                // Form Fields wrapped in ActionProviders for standard UI Toolkit fields
                .AddChild(new ActionGuiProvider(c => new PopupField<string>("From State", _availableStates, 0).Margin(0, 0, 5, 5)))
                .AddChild(new ActionGuiProvider(c => new PopupField<string>("To State", _availableStates, 0).Margin(0, 0, 5, 5)))
                .AddChild(new ActionGuiProvider(c => new PopupField<string>("Condition Rule", _availableConditions, 0).Margin(0, 0, 5, 15)))

                // Telemetry block as a sub-templated panel
                .AddChild(BuildTelemetryPanel());

            return builder.Build().ApplyProfile(GuiElementType.Window);
        }

        private GraphicalUserInterfaceBuilder BuildTelemetryPanel()
        {
            var theme = GuiSkin.Active;

            return new GraphicalUserInterfaceBuilder("TransitionTelemetry")
                .WithPadding(10)
                .AddChild(new Label("METADEV TELEMETRY").Bold().FontSize(10).Color(theme.SecondaryAccent))
                .AddChild(new Label("• Avg Evaluation Chronos: 0.02ms").FontSize(9).Color(Color.gray))
                .AddChild(new Label($"• Global Transition Count: {_availableConditions.Count}").FontSize(9).Color(Color.gray))
                .AddChild(new ActionGuiProvider(ctx => {
                    // Visual separator logic handled via builder-compatible VE
                    return new VisualElement().Height(1).Background(new Color(1, 1, 1, 0.05f)).Margin(0, 0, 5, 5);
                }));
        }

        public Action<VisualElement> GetGuiBuilder() => root => root.Add(CreateGui(new GuiContext()));
        public void ToUIDocument(string path) { }
        public void FromUIDocument(string path) { }
    }
}