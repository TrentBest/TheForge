using Assets.Scripts.Workshop.UI_And_Tools.Forge.Builders.GuiBuilders;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UIElements;
using Workshop.UI_And_Tools.Forge.Builders.GuiBuilders;
using Workshop.UI_And_Tools.Forge.Builders.GuiBuilders.Themes;

namespace Assets.Scripts.SC2.UI
{
    /// <summary>
    /// The SC2 Bot Assembly Forge.
    /// Orchestrates recursive behavior modules and space-aware configuration panels.
    /// </summary>
    public class Workshop_Gui_SC2_BotBuilder : IGuiProvider
    {
        public string Title => "SC2 Autonomous Entity Forge";

        private SC2BotBlueprint _activeBlueprint;
        private VisualElement _behaviorContainer;
        private GuiContext _lastCtx;

        public Workshop_Gui_SC2_BotBuilder()
        {
            _activeBlueprint = new SC2BotBlueprint { BotName = "New_Combatant" };
        }

        public VisualElement CreateGui(GuiContext context)
        {
            _lastCtx = context;

            // ROOT: Forge Container using a Row layout for side-by-side orchestration
            var root = new ForgeContainerBuilder("SC2BotForge_Root")
                .WithFlexLayout(FlexDirection.Row, Justify.FlexStart, Align.Stretch)
                .WithFlexGrow(1)
                .WithBackgroundColor(new Color(0.12f, 0.12f, 0.12f, 1.0f));

            // --- LEFT PANEL: Base Configuration ---
            var configPanel = new ForgeContainerBuilder("BaseConfig")
                .WithWidth(280)
                .WithPadding(15)
                .WithBorderWidth(1)
                .WithBorderColor(Color.gray)
                .AddChild(new ForgeLabelBuilder("ENTITY IDENTITY")
                    .WithBold().WithColor(GuiSkin.Active.PrimaryAccent).WithMarginBottom(15));

            configPanel.AddChild(new ForgeTextFieldBuilder("Bot Name", _activeBlueprint.BotName)
                .OnChanged(val => _activeBlueprint.BotName = val));

            configPanel.AddChild(new ForgeDropdownBuilder("Strategic Race",
                Enum.GetNames(typeof(SC2Race)).ToList(),
                _activeBlueprint.PlayableRace.ToString())
                .OnChanged(val => { if (Enum.TryParse(val, out SC2Race res)) _activeBlueprint.PlayableRace = res; }));

            configPanel.AddSeparator(Color.gray, 1);

            configPanel.AddChild(new ForgeLabelBuilder("MODULE INJECTION").WithBold().WithMarginBottom(8));

            configPanel.AddChild(new ForgeButtonBuilder("Add Cartography Module", () => AddBehaviorProvider(context, new CartographyBehaviorData()))
                .WithHeight(30).WithBackgroundColor(new Color(0.2f, 0.3f, 0.4f)));

            root.AddChild(configPanel);

            // --- RIGHT PANEL: Recursive Behavior Staging ---
            var stagingPanel = new ForgeContainerBuilder("BehaviorStaging")
                .WithFlexGrow(1)
                .WithPadding(15)
                .AddChild(new ForgeLabelBuilder("ACTIVE BEHAVIOR WEAVE")
                    .WithFontSize(18).WithBold().WithColor(Color.white).WithMarginBottom(10));

            // Behavior Scroll Area
            stagingPanel.OnBuild(ve => {
                _behaviorContainer = new ScrollView(ScrollViewMode.Vertical) { name = "ModuleList" };
                _behaviorContainer.style.flexGrow = 1;
                ve.Add(_behaviorContainer);
            });

            return root.CreateGui(context);
        }

        private void AddBehaviorProvider(GuiContext context, IBotBehaviorData behaviorData)
        {
            _activeBlueprint.Behaviors.Add(behaviorData);

            // RECURSIVE RESOLUTION:
            // We resolve the specific builder for the data type.
            IGuiProvider specificBuilder = behaviorData switch
            {
                CartographyBehaviorData carto => new Workshop_Gui_CartographyBehaviorBuilder(carto),
                _ => null
            };

            if (specificBuilder != null)
            {
                // Inject the behavior into the UI wrapped in our Space-Aware Forge Container
                var wrappedElement = CreateSpaceAwareWrapper(specificBuilder, behaviorData.ModuleName);
                _behaviorContainer.Add(wrappedElement.CreateGui(context));
            }
        }

        /// <summary>
        /// A Space-Aware wrapper that collapses complex logic when the container is restricted.
        /// Integrated into the Forge Builder pipeline via OnBuild hooks.
        /// </summary>
        private IGuiProvider CreateSpaceAwareWrapper(IGuiProvider contentBuilder, string title)
        {
            var wrapper = new ForgeContainerBuilder($"Wrapper_{title}")
                .WithMarginTop(10)
                .WithPadding(8)
                .WithBackgroundColor(new Color(0.18f, 0.18f, 0.2f, 1.0f))
                .WithBorderRadius(4)
                .WithBorderWidth(1)
                .WithBorderColor(new Color(0.3f, 0.3f, 0.3f));

            // Header Row
            var header = new ForgeContainerBuilder("ModuleHeader")
                .WithDirection(FlexDirection.Row)
                .WithJustifyContent(Justify.SpaceBetween)
                .WithAlignItems(Align.Center)
                .WithMarginBottom(5)
                .AddChild(new ForgeLabelBuilder(title).WithBold().WithColor(Color.cyan))
                .AddChild(new ForgeButtonBuilder("▣ Pop-out", () => Debug.Log($"[SYSTEM] Detaching {title} to Floating View..."))
                    .WithHeight(20).WithFontSize(9).WithPadding(2, 5, 2, 5));

            wrapper.AddChild(header);

            // Responsive Content Logic
            wrapper.OnBuild(ve => {
                // Build the inner content
                var innerContent = contentBuilder.CreateGui(_lastCtx ?? new GuiContext());
                ve.Add(innerContent);

                // GEOMETRY TRACKER: The "Delayed Vision" implementation
                ve.RegisterCallback<GeometryChangedEvent>(evt => {
                    bool isTooSmall = evt.newRect.width < 200 || evt.newRect.height < 60;
                    innerContent.style.display = isTooSmall ? DisplayStyle.None : DisplayStyle.Flex;

                    // If too small, we could swap for a mini-icon or Sparkline here
                    if (isTooSmall)
                    {
                        ve.tooltip = $"{title} (Collapsed - Insufficient Space)";
                    }
                });
            });

            return wrapper;
        }

        // --- IGuiProvider Lifecycle ---

        public Action<VisualElement> GetGuiBuilder() => container => container.Add(CreateGui(new GuiContext()));

        public void ToUIDocument(string assetPath)
        {
            var root = CreateGui(_lastCtx ?? new GuiContext());
            string fileName = string.IsNullOrEmpty(assetPath) ? "SC2_BotForge_Bake" : System.IO.Path.GetFileNameWithoutExtension(assetPath);
            WorkshopUxmlBaker.Bake(root, fileName);
        }

        public void FromUIDocument(string assetPath) => Debug.Log($"[SC2_Forge] Hydrating entity blueprint from {assetPath}");
    }
}