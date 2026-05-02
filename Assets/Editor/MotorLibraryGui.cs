using Assets.Scripts.Builders;
using Assets.Scripts.Builders.GuiBuilders;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;
using Workshop.UI_And_Tools.Forge.Builders.GuiBuilders;
using Workshop.UI_And_Tools.Forge.IO;
using Random = UnityEngine.Random;

namespace TheSingularityWorkshop.Editors
{
    /// <summary>
    /// The Management Hub for Motor Blueprints.
    /// Handles the CRUD lifecycle of propulsion units within the Singularity Workshop.
    /// Reforged to follow the Forge Protocol and kill raw instantiations.
    /// </summary>
    public class MotorLibraryGui : IGuiProvider
    {
        public string Title => "MOTOR LIBRARY";

        private MotorBuilder _currentSelection;
        private string _selectedKey;
        private VisualElement _rightPane;
        private VisualElement _listContainer;

        public VisualElement CreateGui(GuiContext ctx)
        {
            // 1. Root Split Layout
            var rootBuilder = new ForgeContainerBuilder("MotorLibrary_Root")
                .WithDirection(FlexDirection.Row)
                .WithFlexGrow(1f);

            // 2. LEFT PANE: Navigation & Creation
            var leftPane = new ForgeContainerBuilder("NavigationPane")
                .WithWidth(200f)
                .WithPadding(10f)
                .WithBorderWidth(0, 1f, 0, 0)
                .WithBorderColor(new Color(0.3f, 0.3f, 0.3f))
                .AddChild(new ForgeButtonBuilder("+ Create New Motor")
                    .WithHeight(30f)
                    .WithMarginBottom(10f)
                    .OnClick(() => {
                        // Utilizing the updated 2-argument constructor
                        var newMotor = new MotorBuilder("New_Motor_" + Random.Range(100, 999), 100f);
                        BlueprintLibrary.Register(newMotor);
                        SelectMotor(newMotor, ctx);
                        RefreshList(ctx);
                    }));

            // Wrap ScrollView in DynamicGuiProvider for reactive list updates
            leftPane.AddChild(new DynamicGuiProvider(c => {
                _listContainer = new ScrollView(ScrollViewMode.Vertical);
                RefreshList(ctx);
                return _listContainer;
            }));

            // 3. RIGHT PANE: The Architect
            var rightPaneBuilder = new ForgeContainerBuilder("EditorPane")
                .WithFlexGrow(1f)
                .WithPadding(20f)
                .OnBuild(ve => {
                    _rightPane = ve;
                    // Initial empty state
                    ve.Add(new ForgeLabelBuilder("SELECT A MOTOR TO INITIALIZE ARCHITECT")
                        .WithBold()
                        .OnBuild(l => l.style.opacity = 0.5f)
                        .Build());
                });

            rootBuilder.AddChild(leftPane);
            rootBuilder.AddChild(rightPaneBuilder);

            return rootBuilder.Build();
        }

        private void SelectMotor(MotorBuilder motor, GuiContext ctx)
        {
            _currentSelection = motor;
            _selectedKey = motor?.Name;

            if (_rightPane == null) return;
            _rightPane.Clear();

            if (motor == null) return;

            // Header Actions (Save / Delete)
            var actionHeader = new ForgeContainerBuilder("ActionHeader")
                .WithDirection(FlexDirection.Row)
                .WithJustifyContent(Justify.FlexEnd)
                .WithMarginBottom(10f)
                .AddChild(new ForgeButtonBuilder("DELETE")
                    .WithBackgroundColor(new Color(0.6f, 0.2f, 0.2f))
                    .WithMarginRight(5f)
                    .OnClick(() => {
                        BlueprintLibrary.DeleteMotor(_selectedKey);
                        _currentSelection = null;
                        _rightPane.Clear();
                        RefreshList(ctx);
                    }))
                .AddChild(new ForgeButtonBuilder("SAVE / UPDATE")
                    .WithWidth(120f)
                    .WithBackgroundColor(new Color(0.1f, 0.4f, 0.2f))
                    .OnClick(() => {
                        BlueprintLibrary.RenameMotor(_selectedKey, _currentSelection);
                        _selectedKey = _currentSelection.Name;
                        RefreshList(ctx);
                    }));

            _rightPane.Add(actionHeader.Build());

            // 4. Embed the Motor Property Architect
            var motorEditor = new MotorGuiBuilder(_currentSelection);
            _rightPane.Add(motorEditor.CreateGui(ctx));
        }

        private void RefreshList(GuiContext ctx)
        {
            if (_listContainer == null) return;
            _listContainer.Clear();

            foreach (var name in BlueprintLibrary.GetMotorNames())
            {
                var capturedName = name;
                var isSelected = _currentSelection != null && capturedName == _currentSelection.Name;

                var motorBtn = new ForgeButtonBuilder(capturedName)
                    .WithMarginBottom(4f)
                    .WithBackgroundColor(isSelected ? new Color(0.2f, 0.35f, 0.5f) : new Color(0.15f, 0.15f, 0.18f))
                    .OnClick(() => {
                        var m = BlueprintLibrary.GetMotor(capturedName);
                        SelectMotor(m, ctx);
                    });

                if (isSelected) motorBtn.WithBold();

                _listContainer.Add(motorBtn.Build());
            }
        }

        public Action<VisualElement> GetGuiBuilder() => root => root.Add(CreateGui(new GuiContext()));

        public void ToUIDocument(string assetPath)
        {
            var root = CreateGui(new GuiContext());
            string name = string.IsNullOrEmpty(assetPath) ? "MotorLibrary_Snapshot" : assetPath;
            WorkshopUxmlBaker.Bake(root, name);
        }

        public void FromUIDocument(string assetPath)
        {
            Debug.LogWarning("[MotorLibrary] Static hydration from UXML is bypassed. Library state is synchronized via BlueprintLibrary.");
        }
    }
}