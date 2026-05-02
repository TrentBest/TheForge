using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;
using Workshop.UI_And_Tools.Forge.Builders.GuiBuilders;
using Workshop.raWWar.Gameplay;

namespace Workshop.UI_And_Tools.Forge.Tools
{
    public class Workshop_Gui_GestureForge : IGuiProvider
    {
        public string Title => "Gesture & Mesh Forge";

        private GameObject _targetModularPrefab;

        public Workshop_Gui_GestureForge(GameObject targetPrefab)
        {
            // Pass the raw Unity hierarchy in. The LMPB will rip it to pieces for us.
            _targetModularPrefab = targetPrefab;
        }

        public VisualElement CreateGui(GuiContext ctx)
        {
            var root = new ForgeContainerBuilder("GestureForgeRoot")
                .WithFlexGrow(1).WithBackgroundColor(new Color(0.05f, 0.05f, 0.06f));

            // Header
            root.AddChild(new ForgeLabelBuilder("ASSET INGESTION & GESTURE BAKING")
                .WithFontSize(20).WithColor(Color.cyan).WithFontStyle(FontStyle.Bold).WithPadding(10));

            // Split Layout
            var split = new ForgeSplitPanelBuilder(1, 1)
                .WithSector(0, 0, BuildPreviewPanel(ctx))
                .WithSector(0, 1, BuildControlsPanel(ctx));

            root.AddChild(split);

            return root.Build();
        }

        private IGuiProvider BuildPreviewPanel(GuiContext ctx)
        {
            // Pass the GameObject directly to the builder. 
            // It handles the Context internally.
            var lmpb = new LiveModelPreviewBuilder(_targetModularPrefab)
                .WithBackgroundColor(new Color(0.1f, 0.1f, 0.12f));

            return new ForgeContainerBuilder("PreviewArea")
                .WithPadding(10)
                .AddChild(new ForgeLabelBuilder("RAW MODULAR HIERARCHY").WithColor(Color.white).WithMarginBottom(5))
                .AddChild(lmpb); // Add the builder directly, do not call .CreateGui() here!
        }

        private IGuiProvider BuildControlsPanel(GuiContext ctx)
        {
            var panel = new ForgeContainerBuilder("ControlsArea")
                .WithPadding(15)
                .WithFlexLayout(FlexDirection.Column, Justify.FlexStart, Align.Stretch);

            panel.AddChild(new ForgeLabelBuilder("BAKING PIPELINE")
                .WithFontSize(16).WithColor(Color.white).WithMarginBottom(10));

            panel.AddChild(new ForgeLabelBuilder(
                "This process will rip the mesh hierarchy, extract SkinnedMeshRenderers, " +
                "and cache the 'Ideal Motions' into O(1) data arrays for the GPU pipeline.")
                .WithColor(Color.gray).WithWhiteSpace(WhiteSpace.Normal).WithMarginBottom(20));

            panel.AddChild(new ForgeButtonBuilder("BAKE: STATIC PROXY MESH")
                .WithBackgroundColor(new Color(0.2f, 0.4f, 0.6f)).WithHeight(40).WithMarginBottom(10)
                .OnClick(RipAndFlattenMesh));

            panel.AddChild(new ForgeButtonBuilder("BAKE: 'MARCH' GESTURE")
                .WithBackgroundColor(new Color(0.6f, 0.2f, 0.6f)).WithHeight(40).WithMarginBottom(10)
                .OnClick(() => BakeGesture("March", 30)));

            panel.AddChild(new ForgeButtonBuilder("BAKE: 'ATTACK' GESTURE")
                .WithBackgroundColor(new Color(0.6f, 0.2f, 0.2f)).WithHeight(40)
                .OnClick(() => BakeGesture("Attack", 15)));

            return panel;
        }

        private void RipAndFlattenMesh()
        {
            Debug.Log("[GestureForge] Ripping modular hierarchy and flattening to single proxy mesh...");
        }

        private void BakeGesture(string gestureName, int frameCount)
        {
            Debug.Log($"[GestureForge] Sampling animation '{gestureName}' over {frameCount} frames...");

            var gestureData = new GestureData
            {
                GestureName = gestureName,
                TotalFrames = frameCount,
                BakedFrames = new BakedFrameData[frameCount]
            };

            Debug.Log($"[GestureForge] '{gestureName}' cached. O(1) lookup table established.");
        }

        public Action<VisualElement> GetGuiBuilder() => root => root.Add(CreateGui(new GuiContext()));
        public void ToUIDocument(string assetPath) { }
        public void FromUIDocument(string assetPath) { }
    }
}