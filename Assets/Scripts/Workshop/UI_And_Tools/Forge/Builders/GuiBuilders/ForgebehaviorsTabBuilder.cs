using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UIElements;
using Workshop.UI_And_Tools.Forge.Architectures;
using Assets.Scripts.Workshop.UI_And_Tools.Forge.Builders.GuiBuilders;


#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Workshop.UI_And_Tools.Forge.Builders.GuiBuilders
{
    [Serializable]
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

        // Mock database - Registry for the Forge to manifest known behaviors
        private static List<BehaviorProfile> _behaviorDatabase = new List<BehaviorProfile>
        {
            new BehaviorProfile { Name = "Standard_Wander", TargetArchitectureId = "NativeCPU" },
            new BehaviorProfile { Name = "Zerg_Swarm_Pathing", TargetArchitectureId = "BurstJobs" },
            new BehaviorProfile { Name = "Ant_Cellular_Automata", TargetArchitectureId = "GPUCompute" }
        };

        public ForgeBehaviorsTabBuilder()
        {
            // --- UNIFIED ARCHITECTURE DISCOVERY ---
            // Resolves the 'TypeCache' missing context error in builds
            IEnumerable<Type> archTypes;

#if UNITY_EDITOR
            archTypes = TypeCache.GetTypesDerivedFrom<IComputeArchitecture>()
                .Where(t => !t.IsAbstract && !t.IsInterface);
#else
            archTypes = AppDomain.CurrentDomain.GetAssemblies()
                .SelectMany(s => s.GetTypes())
                .Where(p => typeof(IComputeArchitecture).IsAssignableFrom(p) && !p.IsInterface && !p.IsAbstract);
#endif

            foreach (var type in archTypes)
            {
                try
                {
                    var instance = (IComputeArchitecture)Activator.CreateInstance(type);
                    _architectures[instance.Id] = instance;
                }
                catch (Exception e)
                {
                    Debug.LogError($"[Forge] Failed to initialize Architecture Plugin '{type.Name}': {e.Message}");
                }
            }
        }

        public VisualElement CreateGui(GuiContext context)
        {
            _crudInterface = new CRUD_Builder<BehaviorProfile>(
                title: "FORGED BEHAVIORS",
                dataSource: () => _behaviorDatabase,
                getDisplayName: (b) => string.IsNullOrEmpty(b.Name) ? "Unregistered Behavior" : b.Name,
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
            // Anchors for the dynamic update loop
            VisualElement dynamicTargetContainer = null;
            VisualElement hardwareContainer = null;

            var archChoices = _architectures.Keys.ToList();
            if (archChoices.Count == 0) archChoices.Add("None_Discovered");

            // --- THE DYNAMIC PLUGIN ROUTER ---
            // Reactive logic to swap architecture-specific UI components
            Action updateDynamicUI = () =>
            {
                if (dynamicTargetContainer == null || hardwareContainer == null) return;
                dynamicTargetContainer.Clear();

                if (_architectures.TryGetValue(behavior.TargetArchitectureId, out var activeArch))
                {
                    hardwareContainer.style.borderLeftColor = activeArch.ThemeColor;

                    // Build InfoBox using our internal GUI Builder
                    var infoBox = new GraphicalUserInterfaceBuilder("Architecture_Meta")
                        .WithBackgroundColor(new Color(activeArch.ThemeColor.r * 0.3f, activeArch.ThemeColor.g * 0.3f, activeArch.ThemeColor.b * 0.3f, 1f))
                        .WithPadding(10)
                        .WithBorderRadius(5)
                        .WithMarginBottom(20)
                        .AddChild(new ForgeLabelBuilder(activeArch.Description)
                            .WithColor(Color.white)
                            .WithWordWrap())
                        .Build();

                    dynamicTargetContainer.Add(infoBox);
                    dynamicTargetContainer.Add(activeArch.BuildEditorUI());
                }
            };

            // Main Form construction using fluent Singularity Builders
            var builder = new GraphicalUserInterfaceBuilder("BehaviorEditor")
                .WithFlexGrow(1)
                .WithPadding(20)
                .AddChild(new ForgeLabelBuilder($"EDITING: {behavior.Name}")
                    .WithFontSize(24)
                    .WithColor(Color.white)
                    .WithBold()
                    .WithMarginBottom(20))
                .AddChild(new ForgeTextFieldBuilder("Behavior Name", behavior.Name)
                    .WithMarginBottom(15)
                    .OnChanged(val => behavior.Name = val))

                // Nested row for compute target selection
                .AddChild(new GraphicalUserInterfaceBuilder("HardwareSelector")
                    .WithFlexDirection(FlexDirection.Row)
                    .WithBackgroundColor(new Color(0.15f, 0.15f, 0.15f))
                    .WithPadding(15)
                    .WithBorderRadius(8)
                    .WithMarginBottom(20)
                    .WithBorderLeftWidth(4)
                    .OnBuild(ve =>
                    {
                        hardwareContainer = ve;
                        var dropdown = new ForgeDropdownBuilder("COMPUTE ARCHITECTURE:", archChoices, behavior.TargetArchitectureId)
                            .WithFlexGrow(1)
                            .OnBuild(d => d.style.unityFontStyleAndWeight = FontStyle.Bold)
                            .OnChanged(val =>
                            {
                                behavior.TargetArchitectureId = val;
                                updateDynamicUI();
                            })
                            .CreateGui(new GuiContext());
                        ve.Add(dropdown);
                    }))

                // Placeholder for dynamic plugin manifesting
                .AddChild(new GraphicalUserInterfaceBuilder("DynamicPluginContainer")
                    .OnBuild(ve => dynamicTargetContainer = ve));

            var root = builder.Build();
            updateDynamicUI(); // Synchronize initial state
            return root;
        }

        public Action<VisualElement> GetGuiBuilder() => (root) => root.Add(CreateGui(new GuiContext()));

        public void FromUIDocument(string assetPath)
        {
            Debug.Log($"[Forge] Hydrating Behavior Tab from: {assetPath}");
        }

        public void ToUIDocument(string assetPath)
        {
#if UNITY_EDITOR
            // Bakes the current Behavior Registry into a persistent UXML asset
            if (_crudInterface != null)
            {
                var root = _crudInterface.CreateGui(new GuiContext { Name = "Sovereign_Bake_Context" });
                GraphicalUserInterfaceBuilder.ConvertToUIDocument(root, assetPath);
                Debug.Log($"[Forge] SST Serialization complete: {assetPath}");
            }
#endif
        }
    }
}