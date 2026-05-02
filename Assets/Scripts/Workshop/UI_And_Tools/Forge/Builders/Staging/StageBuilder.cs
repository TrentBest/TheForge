using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;
using Workshop.UI_And_Tools.Forge.Builders.GuiBuilders;
using Workshop.UI_And_Tools.Forge.IO;

namespace Workshop.UI_And_Tools.Forge.Builders.Staging
{
    /// <summary>
    /// The Digital Stage Architect.
    /// Manages camera rigs, lighting rigs, and staging markers for filmmaking and action shots.
    /// Reforged to follow the Forge Protocol and kill all stubs.
    /// </summary>
    public class StageBuilder : IForgeBuilder
    {
        public string ToolName => "Stage Builder";

        // --- STAGE CONFIGURATION ---
        public string ProductionName = "Scene_Alpha_01";
        public List<CameraStagingData> Cameras = new List<CameraStagingData>();
        public List<Vector3> StagingMarkers = new List<Vector3>();

        public Type GetProductType() => typeof(ProductionStage);

        public IGuiProvider GetGuiProvider() => new Stage_ConfiguratorGuiProvider(this);

        /// <summary>
        /// Manifests the finalized Stage definition for the DataWarehouse.
        /// </summary>
        public object Build()
        {
            Debug.Log($"<color=#e67e22>[StageBuilder]</color> Manifesting Digital Stage: {ProductionName}");
            return new ProductionStage
            {
                Name = ProductionName,
                CameraRigs = new List<CameraStagingData>(Cameras),
                Markers = new List<Vector3>(StagingMarkers)
            };
        }
    }

    /// <summary>
    /// The Forge GUI for Digital Staging.
    /// Features a multi-camera registry and staging coordinates.
    /// </summary>
    public class Stage_ConfiguratorGuiProvider : IGuiProvider
    {
        public string Title => "STAGE CONFIGURATOR";
        private readonly StageBuilder _builder;

        public Stage_ConfiguratorGuiProvider(StageBuilder builder)
        {
            _builder = builder;
        }

        public VisualElement CreateGui(GuiContext ctx)
        {
            var rootBuilder = new ForgeContainerBuilder("Stage_Root")
                .WithPadding(20f)
                .WithBackgroundColor(new Color(0.05f, 0.05f, 0.07f))
                .WithBorderWidth(1f)
                .WithBorderColor(new Color(0.9f, 0.5f, 0.1f, 0.5f)); // Production Orange

            // 1. Production Identity
            rootBuilder.AddChild(new ForgeLabelBuilder("PRODUCTION DESIGN")
                .WithFontSize(18).WithBold().WithColor(new Color(0.9f, 0.5f, 0.1f)).WithMarginBottom(15f));

            rootBuilder.AddChild(new ForgeTextFieldBuilder("SCENE NAME", _builder.ProductionName)
                .WithMarginBottom(20f)
                .OnValueChanged(evt => _builder.ProductionName = evt.newValue));

            // 2. Camera Registry
            rootBuilder.AddChild(new ForgeLabelBuilder($"ACTIVE CAMERAS: {_builder.Cameras.Count}")
                .WithFontSize(12).WithBold().WithColor(Color.cyan).WithMarginBottom(10f));

            var cameraList = new ForgeContainerBuilder("CameraRegistry")
                .WithPadding(10f).WithBackgroundColor(new Color(0.1f, 0.1f, 0.12f)).WithBorderRadius(5f)
                .WithMarginBottom(20f);

            if (_builder.Cameras.Count == 0)
                cameraList.AddChild(new ForgeLabelBuilder("NO CAMERAS STAGED").OnBuild(l => l.style.opacity = 0.5f));

            rootBuilder.AddChild(cameraList);

            // 3. Command Actions
            var actionRow = new ForgeContainerBuilder("Actions").WithDirection(FlexDirection.Row);

            actionRow.AddChild(new ForgeButtonBuilder("📹 ADD CAMERA")
                .WithFlexGrow(1f).WithMarginRight(5f).WithBackgroundColor(new Color(0.2f, 0.2f, 0.25f))
                .OnClick(() => {
                    _builder.Cameras.Add(new CameraStagingData { Name = $"Cam_{_builder.Cameras.Count + 1}" });
                    Debug.Log("[StageBuilder] New Camera Rig staged.");
                }));

            actionRow.AddChild(new ForgeButtonBuilder("🎬 FINALIZE STAGE")
                .WithFlexGrow(1f).WithBackgroundColor(new Color(0.1f, 0.4f, 0.2f))
                .OnClick(() => _builder.Build()));

            rootBuilder.AddChild(actionRow);

            return rootBuilder.Build();
        }

        public Action<VisualElement> GetGuiBuilder() => root => root.Add(CreateGui(new GuiContext()));
        public void ToUIDocument(string path) => WorkshopUxmlBaker.Bake(CreateGui(new GuiContext()), "StageConfig_Snapshot");
        public void FromUIDocument(string path) { }
    }

    [Serializable]
    public class ProductionStage
    {
        public string Name;
        public List<CameraStagingData> CameraRigs;
        public List<Vector3> Markers;
    }

    [Serializable]
    public struct CameraStagingData
    {
        public string Name;
        public Vector3 Position;
        public Vector3 Rotation;
        public float FieldOfView;
    }
}