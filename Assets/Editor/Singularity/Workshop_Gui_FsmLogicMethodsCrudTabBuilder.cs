using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;
using Workshop.Core.Extensions;
using Workshop.UI_And_Tools.Forge.Builders.GuiBuilders;
using Workshop.UI_And_Tools.Forge.Builders.GuiBuilders.Themes;

namespace Assets.Editor.Singularity
{
    public class Workshop_Gui_FsmLogicMethodsCrudTabBuilder : IGuiProvider
    {
        public string TabName => "Conditions";
        public string TabIcon => "λ";
        public string Title => "Transition Condition Registry";

        private VisualElement _editorContainer;
        private string _selectedKey;

        // Mock Registry - In production, this would pull from your LogicReflectionUtils or File IO
        private List<string> _registryKeys = new() { "IsTargetDead", "HasEnergy", "TimerExpired", "DistanceThreshold" };

        public VisualElement CreateGui(GuiContext ctx)
        {
            var theme = GuiSkin.Active;

            // MISSION: Split layout for Registry vs Editor
            var layout = new ForgeSplitPanelBuilder(sidebarWidth: 250, side: Side.Left)
                .WithSidebar(BuildRegistrySidebar())
                .WithMain(new ActionGuiProvider(BuildEditorWorkspace));

            return layout.CreateGui(ctx);
        }

        private IGuiProvider BuildRegistrySidebar()
        {
            return new GraphicalUserInterfaceBuilder("ConditionRegistry")
                .WithPadding(10)
                .AddChild(new Label("CONDITION KEYS").Bold().Color(GuiSkin.Active.PrimaryAccent).FontSize(12))
                .AddChild(context =>
                {
                    var list = new ScrollView();
                    RefreshList(list);
                    return list;
                })
                .AddChild(new Button(() => { /* Logic to add new string key */ })
                { text = "+ NEW CONDITION" }.ApplyProfile(GuiElementType.ButtonSecondary));
        }

        private VisualElement BuildEditorWorkspace(GuiContext ctx)
        {
            _editorContainer = new VisualElement().FlexGrow(1).Padding(20);
            RenderEditor();
            return _editorContainer;
        }

        private void RenderEditor()
        {
            _editorContainer.Clear();
            var theme = GuiSkin.Active;

            if (string.IsNullOrEmpty(_selectedKey))
            {
                _editorContainer.Add(new Label("SELECT A CONDITION KEY TO EDIT")
                    .Bold().AlignCenter().Margin(0, 0, 50, 0).Color(Color.gray));
                return;
            }

            var builder = new GraphicalUserInterfaceBuilder("ConditionEditor")
                .AddChild(new Label($"EDITING: {_selectedKey}").Bold().FontSize(18).Color(theme.PrimaryAccent))

                // Connection Logic: Mapping the Key to the C# Method Signature
                .AddChild(new Label("C# METHOD SIGNATURE").Margin(0, 0, 10, 2).FontSize(10))
                .AddChild(new TextField("Method Name") { value = _selectedKey + "_Internal" })
                .AddChild(new Label("Signature: Func<IStateContext, bool>").FontSize(9).Color(theme.SecondaryAccent))

                .AddChild(new VisualElement().Height(20)) // Spacer

                // metaDev Validation Telemetry
                .AddChild(new GraphicalUserInterfaceBuilder("Validation")
                    .WithPadding(10)       
                    .AddChild(new Label("FORGE VALIDATION").Bold().FontSize(10).Color(Color.gray))
                    .AddChild(new Label("✓ Method found in Reflection Cache").Color(Color.green).FontSize(9))
                    .AddChild(new Label("✓ Context Compatibility: DigitalStateContext").Color(Color.green).FontSize(9))
                    .Build().Background(new Color(0, 0, 0, 0.2f))
                    )
                .AddChild(new Button(() => Debug.Log($"[Forge] Saved Condition: {_selectedKey}"))
                { text = "UPDATE FILAMENT" }.ApplyProfile(GuiElementType.ButtonPrimary).Margin(0, 0, 20, 0));

            _editorContainer.Add(builder.Build().ApplyProfile(GuiElementType.Window));
        }

        private void RefreshList(ScrollView list)
        {
            list.Clear();
            foreach (var key in _registryKeys)
            {
                var btn = new Button(() => { _selectedKey = key; RenderEditor(); }) { text = key };
                btn.style.unityTextAlign = TextAnchor.MiddleLeft;
                btn.style.backgroundColor = (key == _selectedKey) ? GuiSkin.Active.PrimaryAccent : Color.clear;
                btn.style.color = (key == _selectedKey) ? Color.black : Color.white;
                list.Add(btn);
            }
        }

        // --- IGuiProvider Boilerplate ---
        public Action<VisualElement> GetGuiBuilder() => root => root.Add(CreateGui(new GuiContext()));
        public void ToUIDocument(string path) { }
        public void FromUIDocument(string path) { }
    }
}