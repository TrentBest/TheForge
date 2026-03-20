using Singularity;
using System;
using System.Collections.Generic;
using TheSingularityWorkshop.Builders.GuiBuilders;
using TheSingularityWorkshop.Forge.Builders.GuiBuilders;
using TheSingularityWorkshop.Forge.Builders.GuiBuilders.Themes;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace Assets.Editor.Singularity
{
    public class SingularityWorkshopHub : IGuiProvider
    {
        public string Title => "EXPERIENCE FORGE";

        private string _activeTab = "FSMs";
        private VisualElement _rightWorkspace;

        // MISSION: Implement CreateGui to satisfy IGuiProvider
        public VisualElement CreateGui(GuiContext ctx)
        {
            // MISSION: Split Panel Architecture (2 Columns, 1 Row)
            var layout = new SplitPanelBuilder(sidebarWidth: 300, side: Side.Left)
                // LEFT: Telemetry Dashboard
                .WithSidebar(new Workshop_Gui_FsmEngineDashboard())
                // RIGHT: The Tabbed Workspace
                .WithMain(new ActionGuiProvider(BuildTabbedWorkspace));

            return layout.CreateGui(ctx);
        }

        private VisualElement BuildTabbedWorkspace(GuiContext ctx)
        {
            var main = new VisualElement().FlexGrow(1);

            // 1. The Ribbon Menu (Tabs)
            var tabs = new List<string> { "FSMs", "States", "Transitions", "Behaviors", "Conditions" };
            var ribbon = new GraphicalUserInterfaceBuilder("TabRibbon")
                .WithHeight(40)
                .WithBackgroundColor(new Color(0.05f, 0.05f, 0.05f))
                .WithFlexLayout(FlexDirection.Row, Justify.FlexStart, Align.Center);

            foreach (var tName in tabs)
            {
                ribbon.AddChild(new ActionGuiProvider(c => CreateTabBtn(tName)));
            }
            main.Add(ribbon.Build());

            // 2. The Content Viewport (The Anvil)
            _rightWorkspace = new VisualElement().FlexGrow(1).Padding(15);
            RefreshActiveTab();
            main.Add(_rightWorkspace);

            return main;
        }

        private VisualElement CreateTabBtn(string name)
        {
            bool isActive = _activeTab == name;
            var btn = new Button(() => {
                _activeTab = name;
                RefreshActiveTab();
            })
            { text = name.ToUpper() };

            btn.ApplyProfile(isActive ? GuiElementType.ButtonPrimary : GuiElementType.ButtonGhost);
            btn.style.flexGrow = 1;
            btn.style.borderBottomWidth = isActive ? 3 : 0;
            btn.style.borderBottomColor = GuiSkin.Active.PrimaryAccent;
            return btn;
        }

        private void RefreshActiveTab()
        {
            if (_rightWorkspace == null) return;
            _rightWorkspace.Clear();

            IGuiProvider provider = _activeTab switch
            {
                "FSMs" => new Workshop_Gui_FsmCrudTabBuilder(),
                "States" => new Workshop_Gui_FsmStateCrudTabBuilder(),
                "Transitions" => new Workshop_Gui_FsmTransitionBuilderTab(),
                "Behaviors" => new Workshop_Gui_FsmStateMethodsCrudTabBuilder(),
                "Conditions" => new Workshop_Gui_FsmLogicMethodsCrudTabBuilder(),
                _ => null
            };

            if (provider != null)
                _rightWorkspace.Add(provider.CreateGui(new GuiContext()));
        }

        // --- IGuiProvider Boilerplate ---
        public Action<VisualElement> GetGuiBuilder() => root => root.Add(CreateGui(new GuiContext()));
        public void ToUIDocument(string path) { }
        public void FromUIDocument(string path) { }
    }
}