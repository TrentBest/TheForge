// File: Assets/Scripts/Workshop/Forge/Builders/GuiBuilders/Casting/Workshop_Gui_GestureBuilder.cs
using System;
using UnityEngine;
using UnityEngine.UIElements;
using Workshop.Gameplay.Cast.Gestures;
using Workshop.UI_And_Tools.Forge.Builders.GuiBuilders;

namespace Workshop.UI_And_Tools.Forge.Builders.Casting
{
    public class Workshop_Gui_GestureBuilder : MonoBehaviour, IForgeBuilder
    {
        public string ToolName => "Procedural Rigging & Gestures";
        public System.Type GetProductType() => typeof(GestureDefinition);

        // Cache the provider so we can access the product during Build()
        private GestureGuiProvider _provider;

        public IGuiProvider GetGuiProvider()
        {
            if (_provider == null) _provider = new GestureGuiProvider();
            return _provider;
        }

        private void OnEnable() => BuilderRegistry.Register(this);

        public object Build()
        {
            if (_provider == null || _provider.ActiveDraft == null)
            {
                Debug.LogWarning("[Gesture Forge] Cannot build. No active draft exists.");
                return null;
            }

            Debug.Log($"[Gesture Forge] Successfully built Gesture Definition: {_provider.ActiveDraft.GestureName}");
            // In the future, this is where you would send it to the DataWarehouse to be saved
            return _provider.ActiveDraft;
        }
    }

    public class GestureGuiProvider : IGuiProvider
    {
        private GameObject _activeRig;
        public GestureDefinition ActiveDraft { get; private set; } // Exposed for the Builder
        private ScrollView _flipbookTimeline;

        // Implement the Title property
        public string Title => "Gesture & Rigging Forge";

        public VisualElement CreateGui(GuiContext ctx)
        {
            if (ActiveDraft == null) ActiveDraft = ScriptableObject.CreateInstance<GestureDefinition>();

            var root = new GraphicalUserInterfaceBuilder("GestureForge")
                .WithFlexLayout(FlexDirection.Row, Justify.FlexStart, Align.Stretch)
                .WithPercentSize(100, 100);

            // --- LEFT: Target Selection & Hierarchy ---
            root.AddChild(new GraphicalUserInterfaceBuilder("RigSelector")
                .WithFlexGrow(1)
                .WithPadding(10)
                .WithBorderRightWidth(1).WithBorderRightColor(Color.gray)
                .AddHeader("1. Rigging Source", Color.cyan)
                .AddChild(c => new Button(BindToSelection) { text = "Bind to Selected Object" })
                .AddChild(c => new Label("Select a GameObject in the scene (e.g., a Soldier) and bind it to begin capturing its transforms.") { style = { whiteSpace = WhiteSpace.Normal, color = Color.gray, marginTop = 10 } })
            );

            // --- MIDDLE: The Flipbook Timeline ---
            root.AddChild(new GraphicalUserInterfaceBuilder("Flipbook")
                .WithFlexGrow(2)
                .WithPadding(10)
                .WithBorderRightWidth(1).WithBorderRightColor(Color.gray)
                .AddHeader("2. Gesture Timeline", Color.yellow)
                .AddChild(c => {
                    _flipbookTimeline = new ScrollView { style = { flexGrow = 1, backgroundColor = new Color(0, 0, 0, 0.2f), marginBottom = 10 } };
                    RefreshTimeline();
                    return _flipbookTimeline;
                })
                .AddButton("+ Capture Current Pose", CapturePose)
            );

            // --- RIGHT: Equation of Motion Tuner ---
            root.AddChild(new GraphicalUserInterfaceBuilder("MotionTuner")
                .WithFlexGrow(1)
                .WithPadding(10)
                .AddHeader("3. Equation of Motion", new Color(1f, 0.4f, 0f))
                .AddChild(c => new Label("Select a frame in the timeline to tune how the actor transitions into it.") { style = { color = Color.gray, whiteSpace = WhiteSpace.Normal } })
            );

            return root.Build();
        }

        private void BindToSelection()
        {
            // For a runtime Forge, you would hook this into your custom selection manager.
            // e.g., _activeRig = ForgeSelectionManager.CurrentSelection;
            Debug.Log("[Gesture Forge] Rig binding invoked.");
        }

        private void CapturePose()
        {
            if (_activeRig == null)
            {
                Debug.LogWarning("[Gesture Forge] No Rig Bound! Cannot capture pose.");
                return;
            }

            var newFrame = new PoseFrame { PoseName = $"Pose_{ActiveDraft.Frames.Count}" };

            // Recursively capture all transforms in the hierarchy
            CaptureHierarchy(_activeRig.transform, _activeRig.transform, newFrame);

            ActiveDraft.Frames.Add(newFrame);
            RefreshTimeline();
        }

        private void CaptureHierarchy(Transform root, Transform current, PoseFrame frame)
        {
            // Replaced UnityEditor method with our runtime-safe path builder
            string path = GetRuntimeTransformPath(root, current);

            frame.JointStates.Add(new JointState
            {
                Path = path,
                LocalPosition = current.localPosition,
                LocalRotation = current.localRotation
            });

            foreach (Transform child in current)
            {
                CaptureHierarchy(root, child, frame);
            }
        }

        /// <summary>
        /// A runtime-safe method to calculate the path from a root transform to a child.
        /// </summary>
        private string GetRuntimeTransformPath(Transform root, Transform current)
        {
            if (current == root) return ""; // Root has no path prefix

            string path = current.name;
            while (current.parent != null && current.parent != root)
            {
                current = current.parent;
                path = current.name + "/" + path;
            }
            return path;
        }

        private void RefreshTimeline()
        {
            if (_flipbookTimeline == null) return;
            _flipbookTimeline.Clear();

            for (int i = 0; i < ActiveDraft.Frames.Count; i++)
            {
                var frame = ActiveDraft.Frames[i];
                var card = new GraphicalUserInterfaceBuilder($"Frame_{i}")
                    .WithBackgroundColor(new Color(0.2f, 0.2f, 0.2f))
                    .WithPadding(8).WithMarginBottom(5)
                    .WithBorderLeftWidth(4).WithBorderLeftColor(Color.yellow)
                    .WithFlexLayout(FlexDirection.Row, Justify.SpaceBetween, Align.Center)
                    .AddChild(new Label($"{i}: {frame.PoseName}") { style = { unityFontStyleAndWeight = FontStyle.Bold } })
                    .AddChild(new Label($"Ease: {frame.ApproachMotion.Easing}") { style = { fontSize = 10, color = Color.gray } });

                _flipbookTimeline.Add(card.Build());
            }
        }

        // --- IGuiProvider Missing Implementations ---

        public Action<VisualElement> GetGuiBuilder()
        {
            // Passes the created VisualElement into whatever root container calls this action
            return root => root.Add(CreateGui(new GuiContext()));
        }

        public void ToUIDocument(string assetPath)
        {
            // Used if you decide to serialize your layouts to standard Unity UXML files
            Debug.Log($"[Gesture Forge] Saving UI layout to {assetPath} is not yet implemented for runtime.");
        }

        public void FromUIDocument(string assetPath)
        {
            // Used if you decide to load pre-made UI Toolkit XML files
            Debug.Log($"[Gesture Forge] Loading UI layout from {assetPath} is not yet implemented for runtime.");
        }
    }
}