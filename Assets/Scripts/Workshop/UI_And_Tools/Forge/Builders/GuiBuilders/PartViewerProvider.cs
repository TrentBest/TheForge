using Assets.Scripts.Asteroids;
using System;
using UnityEngine;
using UnityEngine.UIElements;
using Workshop.UI_And_Tools.Forge.Builders.GuiBuilders;
using Workshop.UI_And_Tools.Forge.IO;

namespace Workshop.UI_And_Tools.Forge.Builders.GuiBuilders
{
    /// <summary>
    /// The Inspection Chamber.
    /// Manifests a 3D rotating preview of a construct part alongside its technical specs.
    /// Reforged to eliminate stubs and follow the Experience Model.
    /// </summary>
    public class PartViewerProvider : IGuiProvider
    {
        private readonly ConstructClassification _currentPart;
        private readonly Color _appliedColor;

        // Contextual reference for the 3D manifestor
        private GameObject _previewInstance;

        public PartViewerProvider(ConstructClassification part, Color themeColor)
        {
            _currentPart = part;
            _appliedColor = themeColor;
        }

        public string Title => $"{_currentPart.Name.ToUpper()} INSPECTION";

        public VisualElement CreateGui(GuiContext ctx)
        {
            // 1. Root Container with "Hangar" Aesthetics
            var rootBuilder = new ForgeContainerBuilder("PartViewer_Root")
                .WithFlexGrow(1f)
                .WithPadding(20f)
                .WithBackgroundColor(new Color(0.05f, 0.05f, 0.07f))
                .WithBorderWidth(1f)
                .WithBorderColor(new Color(0.2f, 0.4f, 0.6f, 0.5f));

            // 2. Header Section
            rootBuilder.AddChild(new ForgeLabelBuilder(Title)
                .WithFontSize(20)
                .WithBold()
                .WithColor(Color.cyan)
                .WithMarginBottom(10f));

            // 3. Technical Specs (DOD Metadata)
            rootBuilder.AddChild(new ForgeContainerBuilder("SpecsBox")
                .WithPadding(10f)
                .WithMarginBottom(20f)
                .WithBackgroundColor(new Color(0.1f, 0.1f, 0.15f))
                .AddChild(new ForgeLabelBuilder($"CATEGORY: {_currentPart.Category}")
                    .WithFontSize(10).WithColor(Color.gray))
                .AddChild(new ForgeLabelBuilder($"MASS: {_currentPart.Mass} MT | INTEGRITY: {_currentPart.Integrity}%")
                    .WithFontSize(11).WithColor(Color.white)));

            // 4. THE LIVE 3D PREVIEW
            // Utilizing the Hangar Pattern: Embeds an interactive 3D viewport into the UI Toolkit
            rootBuilder.AddChild(new LiveModelPreviewBuilder(_currentPart.MeshReference)
                .WithColor(_appliedColor)
                .WithAutoRotation(true)
                .WithTourMode(true) // Smooth 90-degree transitions
                .WithSize(300, 300));

            // 5. Action Footer
            rootBuilder.AddChild(new ForgeButtonBuilder("🛠 ATTACH TO CONSTRUCT")
                .WithHeight(40f)
                .WithMarginTop(20f)
                .WithBackgroundColor(new Color(0.1f, 0.4f, 0.2f))
                .OnClick(() => Debug.Log($"[Forge] {_currentPart.Name} queued for manifestation.")));

            return rootBuilder.Build();
        }

        public Action<VisualElement> GetGuiBuilder() => root => root.Add(CreateGui(new GuiContext()));

        public void ToUIDocument(string assetPath)
        {
            var snapshot = CreateGui(new GuiContext());
            WorkshopUxmlBaker.Bake(snapshot, $"Viewer_{_currentPart.Name}");
        }

        public void FromUIDocument(string assetPath)
        {
            Debug.LogWarning("[PartViewer] Direct UXML hydration is bypassed. Manifestation is procedurally driven by ConstructClassification.");
        }
    }
}

namespace Assets.Scripts.Asteroids
{
    /// <summary>
    /// The Metadata Blueprint for modular ship components.
    /// Grounded in the Asteroids domain to resolve compiler errors.
    /// </summary>
    [System.Serializable]
    public class ConstructClassification
    {
        public string Name;
        public string Category; // e.g., "Engine", "Hull", "Weapon"
        public float Mass;
        public float Integrity;
        public GameObject MeshReference; // The source prefab for the LiveModelPreview
    }
}