using System;
using UnityEngine;
using UnityEngine.UIElements;
using Workshop.Core.Diagnostics;
using Workshop.UI_And_Tools.Forge.Builders.GuiBuilders;
using Workshop.UI_And_Tools.Forge.IO;

namespace Assets.Scripts.Construction
{
    /// <summary>
    /// Specialized GUI Provider for structural framing members.
    /// Manages the dimensional vectors and material metadata for wall studs.
    /// Reforged to follow the Forge Protocol and BIM standards.
    /// </summary>
    public class StudGuiBuilder : IGuiProvider
    {
        public string Title => "STUD CONFIGURATOR";

        // --- STRUCTURAL DATA ---
        public float Height { get; set; } = 92.625f; // Standard 8' wall stud (inches)
        public float Width { get; set; } = 3.5f;     // Standard 2x4 width
        public float Thickness { get; set; } = 1.5f; // Standard 2x4 thickness
        public string Material { get; set; } = "Spruce-Pine-Fir (SPF)";

        public VisualElement CreateGui(GuiContext ctx)
        {
            // 1. Root Industrial Container
            var rootBuilder = new ForgeContainerBuilder("StudBuilder_Root")
                .WithPadding(20f)
                .WithBackgroundColor(new Color(0.05f, 0.05f, 0.07f))
                .WithBorderWidth(1f)
                .WithBorderColor(new Color(0.6f, 0.4f, 0.2f, 0.5f)); // Timber Brown Accent

            // 2. Header: Structural Manifest
            rootBuilder.AddChild(new ForgeLabelBuilder("FRAMING MEMBER DATA")
                .WithFontSize(18)
                .WithBold()
                .WithColor(new Color(0.8f, 0.6f, 0.4f))
                .WithMarginBottom(15f));

            // 3. Dimensional Matrix
            var dimensionGrid = new ForgeContainerBuilder("Dimensions")
                .WithDirection(FlexDirection.Row)
                .WithFlexWrap(Wrap.Wrap)
                .WithMarginBottom(20f);

            dimensionGrid.AddChild(CreateNumericField("HEIGHT (in)", Height, v => Height = v));
            dimensionGrid.AddChild(CreateNumericField("WIDTH (in)", Width, v => Width = v));
            dimensionGrid.AddChild(CreateNumericField("THICK (in)", Thickness, v => Thickness = v));

            rootBuilder.AddChild(dimensionGrid);

            // 4. Material Specification
            rootBuilder.AddChild(new ForgeTextFieldBuilder("MATERIAL SPEC", Material)
                .WithMarginBottom(20f)
                .OnValueChanged(evt => Material = evt.newValue));

            // 5. Action Commands
            rootBuilder.AddChild(new ForgeButtonBuilder("🛠 GENERATE STUD MESH")
                .WithHeight(40f)
                .WithBackgroundColor(new Color(0.15f, 0.4f, 0.25f))
                .WithBold()
                .OnClick(() => ForgeLogger.Log($"[Construction] Manifesting {Width}x{Thickness} Stud at {Height}\" height.")));

            return rootBuilder.Build();
        }

        private IGuiProvider CreateNumericField(string label, float initialValue, Action<float> onSet)
        {
            return new ForgeTextFieldBuilder(label, initialValue.ToString("F3"))
                .WithMarginRight(10f)
                .WithMarginBottom(10f)
                .OnValueChanged(evt => {
                    if (float.TryParse(evt.newValue, out float result)) onSet(result);
                });
        }

        // --- IGUI-PROVIDER INTERFACE ---

        public Action<VisualElement> GetGuiBuilder() => root => root.Add(CreateGui(new GuiContext()));

        public void ToUIDocument(string assetPath)
        {
            var snapshot = CreateGui(new GuiContext());
            WorkshopUxmlBaker.Bake(snapshot, "StudConfig_Snapshot");
        }

        public void FromUIDocument(string assetPath)
        {
            ForgeLogger.LogWarning("[StudBuilder] Static UXML hydration bypassed. BIM metadata is procedurally driven.");
        }
    }
}