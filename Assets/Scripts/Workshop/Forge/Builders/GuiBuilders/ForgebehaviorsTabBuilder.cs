using TheSingularityWorkshop.Forge.Builders.GuiBuilders;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;
using TheSingularityWorkshop.Forge.Architectures;

namespace TheSingularityWorkshop.Forge.Gui
{
    public class BehaviorProfile
    {
        public string Name = "New_Behavior";
        public string TargetArchitectureId = "NativeCPU"; // Default
    }

    public class ForgeBehaviorsTabBuilder : IGuiProvider
    {
        public string Title => "BEHAVIORS (FSM FORGE)";

        private CRUD_Builder<BehaviorProfile> _crudInterface;
        private Dictionary<string, IComputeArchitecture> _architectures = new Dictionary<string, IComputeArchitecture>();

        // Mock database
        private static List<BehaviorProfile> _behaviorDatabase = new List<BehaviorProfile>
        {
            new BehaviorProfile { Name = "Standard_Wander", TargetArchitectureId = "NativeCPU" },
            new BehaviorProfile { Name = "Zerg_Swarm_Pathing", TargetArchitectureId = "BurstJobs" },
            new BehaviorProfile { Name = "Ant_Cellular_Automata", TargetArchitectureId = "GPUCompute" }
        };

        public ForgeBehaviorsTabBuilder()
        {
            // DYNAMICALLY DISCOVER ALL COMPUTE ARCHITECTURES
            var archTypes = TypeCache.GetTypesDerivedFrom<IComputeArchitecture>()
                .Where(t => !t.IsAbstract && !t.IsInterface);

            foreach (var type in archTypes)
            {
                var instance = (IComputeArchitecture)Activator.CreateInstance(type);
                _architectures[instance.Id] = instance;
            }
        }

        public VisualElement CreateGui(GuiContext context)
        {
            _crudInterface = new CRUD_Builder<BehaviorProfile>(
                title: "FORGED BEHAVIORS",
                dataSource: () => _behaviorDatabase,
                getDisplayName: (b) => string.IsNullOrEmpty(b.Name) ? "Unregistered Behavior" : b.Name,

                // Group by the Display Name of the Architecture Plugin!
                getGroupCategory: (b) => _architectures.ContainsKey(b.TargetArchitectureId) ? _architectures[b.TargetArchitectureId].DisplayName : "Unknown Target",

                buildEditorForm: (b) => BuildBehaviorEditorForm(b),
                onSave: (b) => { if (!_behaviorDatabase.Contains(b)) _behaviorDatabase.Add(b); },
                onDelete: (b) => { _behaviorDatabase.Remove(b); },
                getSubtitle: (b) => $"[{b.TargetArchitectureId}]"
            );

            return _crudInterface.CreateGui(context);
        }

        private VisualElement BuildBehaviorEditorForm(BehaviorProfile behavior)
        {
            var form = new VisualElement { style = { flexGrow = 1, flexDirection = FlexDirection.Column, paddingTop = 20, paddingBottom = 20, paddingLeft = 20, paddingRight = 20 } };

            form.Add(new Label($"EDITING: {behavior.Name}") { style = { fontSize = 24, color = Color.white, unityFontStyleAndWeight = FontStyle.Bold, marginBottom = 20 } });

            var nameField = new TextField("Behavior Name") { value = behavior.Name, style = { marginBottom = 15 } };
            nameField.RegisterValueChangedCallback(e => behavior.Name = e.newValue);
            form.Add(nameField);

            var dynamicTargetContainer = new VisualElement();

            // --- THE DYNAMIC PLUGIN ROUTER ---
            var hardwareContainer = new VisualElement { style = { flexDirection = FlexDirection.Row, backgroundColor = new Color(0.15f, 0.15f, 0.15f), paddingTop = 15, paddingBottom = 15, paddingLeft = 15, paddingRight = 15, borderTopLeftRadius = 8, borderBottomLeftRadius = 8, borderBottomRightRadius=8, borderTopRightRadius = 8, marginBottom = 20, borderLeftWidth = 4 } };

            // Build the dropdown choices from our discovered plugins
            var archChoices = _architectures.Keys.ToList();
            int defaultIndex = archChoices.IndexOf(behavior.TargetArchitectureId);
            if (defaultIndex < 0) defaultIndex = 0;

            var targetDropdown = new DropdownField("COMPUTE ARCHITECTURE:", archChoices, defaultIndex)
            {
                style = { flexGrow = 1, color = Color.white, unityFontStyleAndWeight = FontStyle.Bold }
            };

            Action updateDynamicUI = () =>
            {
                dynamicTargetContainer.Clear();

                if (_architectures.TryGetValue(behavior.TargetArchitectureId, out var activeArch))
                {
                    // Style the header based on the plugin's theme color
                    hardwareContainer.style.borderLeftColor = activeArch.ThemeColor;

                    // Info Box from the plugin
                    var infoBox = new VisualElement { style = { backgroundColor = new Color(activeArch.ThemeColor.r * 0.3f, activeArch.ThemeColor.g * 0.3f, activeArch.ThemeColor.b * 0.3f, 1f), paddingTop = 10, paddingRight = 10, paddingLeft = 10, paddingBottom = 10, borderTopLeftRadius = 5, borderTopRightRadius = 5, borderBottomRightRadius = 5, borderBottomLeftRadius = 5, marginBottom = 20 } };
                    infoBox.Add(new Label(activeArch.Description) { style = { color = Color.white } });
                    dynamicTargetContainer.Add(infoBox);

                    // ASK THE PLUGIN TO DRAW ITS OWN UI!
                    dynamicTargetContainer.Add(activeArch.BuildEditorUI());
                }
            };

            targetDropdown.RegisterValueChangedCallback(evt =>
            {
                behavior.TargetArchitectureId = evt.newValue;
                updateDynamicUI();
            });

            hardwareContainer.Add(targetDropdown);
            form.Add(hardwareContainer);
            form.Add(dynamicTargetContainer);

            updateDynamicUI();

            return form;
        }

        public Action<VisualElement> GetGuiBuilder() => (root) => root.Add(CreateGui(new GuiContext()));
        public void FromUIDocument(string assetPath) { }
        public void ToUIDocument(string assetPath) { }
    }
}