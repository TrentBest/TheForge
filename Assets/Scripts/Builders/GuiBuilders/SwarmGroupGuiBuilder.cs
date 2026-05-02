using Assets.Scripts.Asteroids;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UIElements;
using Workshop.Core.Diagnostics;
using Workshop.UI_And_Tools.Forge.Builders.GuiBuilders;
using Workshop.UI_And_Tools.Forge.IO;

namespace Assets.Scripts.Builders.GuiBuilders
{
    /// <summary>
    /// The Orchestrator for massive entity collectives.
    /// Manages composition, logic-to-GPU handshakes, and remote telemetry spawning.
    /// Reforged to follow the Forge Protocol and DOD standards.
    /// </summary>
    public class SwarmGroupGuiBuilder : IGuiProvider
    {
        private readonly SwarmGroupBuilder _builder;
        private List<DroneBuilder> _availableDrones;
        private VisualElement _compositionContainer;

        public SwarmGroupGuiBuilder(SwarmGroupBuilder builder)
        {
            _builder = builder;

            // TODO: Replace with DataWarehouse.RetrieveRegistry<DroneBuilder>()
            _availableDrones = new List<DroneBuilder>()
            {
                new DroneBuilder().WithName("Scout_MK1").WithBehavior("Scout"),
                new DroneHelper().WithName("Heavy_Lifter").WithBehavior("Cargo"),
                new DroneBuilder().WithName("Interceptor").WithBehavior("Aggressive")
            };
        }

        public string Title { get; set; } = "SWARM CONFIGURATOR";

        public VisualElement CreateGui(GuiContext ctx)
        {
            var rootBuilder = new ForgeContainerBuilder("Swarm_Root")
                .WithPadding(20f)
                .WithBackgroundColor(new Color(0.05f, 0.05f, 0.07f))
                .WithBorderWidth(1f)
                .WithBorderColor(new Color(0.2f, 0.8f, 0.4f, 0.5f)); // "Lifeform" Green

            // 1. Swarm Identity
            rootBuilder.AddChild(new ForgeLabelBuilder("SWARM COLLECTIVE DESIGN")
                .WithFontSize(18).WithBold().WithColor(new Color(0.4f, 1f, 0.6f)).WithMarginBottom(15f));

            rootBuilder.AddChild(new ForgeTextFieldBuilder("COLLECTIVE DESIGNATION", _builder.GroupName)
                .WithMarginBottom(15f)
                .OnValueChanged(evt => _builder.WithName(evt.newValue)));

            // 2. Composition Manifest
            rootBuilder.AddChild(new ForgeLabelBuilder("ENTITY COMPOSITION")
                .WithFontSize(12).WithBold().WithColor(Color.cyan).WithMarginBottom(10f));

            rootBuilder.AddChild(new ForgeContainerBuilder("CompositionArea")
                .WithPadding(10f).WithBackgroundColor(new Color(0.1f, 0.1f, 0.12f)).WithBorderRadius(5f)
                .OnBuild(ve => {
                    _compositionContainer = ve;
                    RefreshCompositionList(ctx);
                }));

            // 3. Spawning & Logic Controls
            var actionRow = new ForgeContainerBuilder("AddEntityRow")
                .WithDirection(FlexDirection.Row)
                .WithMarginTop(10f)
                .WithPadding(10f)
                .WithBorderWidth(1f, 0, 0, 0).WithBorderColor(new Color(0.3f, 0.3f, 0.3f));

            // Dropdown for entity selection
            var droneNames = _availableDrones.Select(d => d.DroneName).ToList();
            actionRow.AddChild(new DynamicGuiProvider(c => {
                var dropdown = new DropdownField("ADD UNIT TYPE", droneNames, 0);
                dropdown.style.flexGrow = 1f;

                var addBtn = new ForgeButtonBuilder("+")
                    .WithWidth(40f).WithMarginLeft(5f)
                    .OnClick(() => {
                        var proto = _availableDrones.Find(d => d.DroneName == dropdown.value);
                        if (proto != null)
                        {
                            _builder.AddDroneType(proto, 1);
                            RefreshCompositionList(ctx);
                        }
                    }).Build();

                var container = new VisualElement { style = { flexDirection = FlexDirection.Row, flexGrow = 1 } };
                container.Add(dropdown);
                container.Add(addBtn);
                return container;
            }));

            rootBuilder.AddChild(actionRow);

            // 4. Manifestation Command
            rootBuilder.AddChild(new ForgeButtonBuilder("🚀 MANIFEST SWARM")
                .WithHeight(40f).WithMarginTop(20f)
                .WithBackgroundColor(new Color(0.1f, 0.4f, 0.2f))
                .WithBold()
                .OnClick(() => {
                    _builder.Build(Vector3.zero);
                    ForgeLogger.Log($"<color=green>[SwarmForge]</color> Entity wave manifested: {_builder.GroupName}");
                }));

            return rootBuilder.Build();
        }

        private void RefreshCompositionList(GuiContext ctx)
        {
            if (_compositionContainer == null) return;
            _compositionContainer.Clear();

            var comp = _builder.GetComposition();
            if (comp.Count == 0)
            {
                _compositionContainer.Add(new ForgeLabelBuilder("EMPTY HIVE - NO ENTITIES ASSIGNED")
                    .OnBuild(l => l.style.opacity = 0.5f).Build());
                return;
            }

            foreach (var entry in comp)
            {
                var drone = entry.Key;
                var count = entry.Value;

                var row = new ForgeContainerBuilder($"Row_{drone.DroneName}")
                    .WithDirection(FlexDirection.Row)
                    .WithPadding(5f)
                    .WithMarginBottom(2f)
                    .WithBackgroundColor(new Color(0.15f, 0.15f, 0.18f))
                    .AddChild(new ForgeLabelBuilder(drone.DroneName).WithBold().WithFlexGrow(1f))
                    .AddChild(new DynamicGuiProvider(c => {
                        var intField = new IntegerField { value = count };
                        intField.style.width = 60f;
                        intField.RegisterValueChangedCallback(evt => _builder.AddDroneType(drone, Mathf.Max(1, evt.newValue)));
                        return intField;
                    }))
                    .AddChild(new ForgeButtonBuilder("X")
                        .WithWidth(30f).WithColor(Color.red)
                        .OnClick(() => {
                            _builder.RemoveDroneType(drone);
                            RefreshCompositionList(ctx);
                        }));

                _compositionContainer.Add(row.Build());
            }
        }

        public Action<VisualElement> GetGuiBuilder() => root => root.Add(CreateGui(new GuiContext()));
        public void ToUIDocument(string path) => WorkshopUxmlBaker.Bake(CreateGui(new GuiContext()), "SwarmArchitect_Snapshot");
        public void FromUIDocument(string path) { }
    }
}