using Assets.Scripts.Workshop.Forge.Builders.GuiBuilders;
using System;
using System.IO;
using TheSingularityWorkshop.Forge.IO;
using UnityEngine;
using UnityEngine.UIElements;

namespace TheSingularityWorkshop.Forge.Builders.GuiBuilders
{
    public class Workshop_Gui_ImageEditor : IGuiProvider
    {
        public string Title => "Singularity Image Forge";

        private string _targetSavePath = Path.Combine(Application.persistentDataPath, "ForgeOutput.png");
        private IForgeFileBrowser _fileBrowser = new EditorNativeFileBrowser();

        public VisualElement CreateGui(GuiContext ctx)
        {
            var canvas = new DrawableCanvasGuiBuilder(512, 512);

            var root = new GraphicalUserInterfaceBuilder("ImageEditorRoot")
                .WithFlexLayout(FlexDirection.Row, Justify.FlexStart, Align.Stretch)
                .WithPercentSize(100, 100)
                .WithBackgroundColor(new Color(0.1f, 0.1f, 0.12f));

            var toolbar = new GraphicalUserInterfaceBuilder("Toolbar")
                .WithWidth(380)
                .WithPadding(15)
                .WithScrollable(true)
                .WithBackgroundColor(new Color(0.15f, 0.15f, 0.18f))
                .WithBorderRightWidth(2)
                .WithBorderRightColor(Color.cyan);

            toolbar.AddChild(new Label("Singularity Image Forge") { style = { fontSize = 22, unityFontStyleAndWeight = FontStyle.Bold, color = Color.cyan, marginBottom = 15 } });

            // --- IO & SERIALIZATION ---
            toolbar.AddChild(new Label("I/O Operations") { style = { color = Color.white, unityFontStyleAndWeight = FontStyle.Bold, marginBottom = 5 } });

            toolbar.AddButton("Load Image (Browser)", () =>
            {
                _fileBrowser.OpenImageFile(
                    onTextureLoaded: (tex) => canvas.LoadTexture(tex),
                    onCanceled: () => Debug.Log("[ImageForge] Image load canceled by user.")
                );
            });

            toolbar.AddSeparator(Color.gray, 1);

            toolbar.AddStringData("Save Path", _targetSavePath, (val) => _targetSavePath = val);
            toolbar.AddButton("Save PNG", () => canvas.SaveToDisk(_targetSavePath));

            toolbar.AddSeparator(Color.gray, 2);

            // --- PRO BRUSH SETTINGS ---
            toolbar.AddChild(new Label("Brush Settings") { style = { color = Color.white, unityFontStyleAndWeight = FontStyle.Bold, marginTop = 10, marginBottom = 5 } });

            toolbar.AddIntSliderData("Radius (Size)", 1, 200, 5, size => canvas.SetBrushSize(size));
            toolbar.AddSliderData("Opacity", 0f, 1f, 1f, op => canvas.SetBrushOpacity(op));
            toolbar.AddSliderData("Hardness", 0f, 1f, 1f, hd => canvas.SetBrushHardness(hd));

            toolbar.AddSeparator(Color.gray, 2);

            // --- COLOR PICKER ---
            toolbar.AddChild(new Label("Color Forge") { style = { color = Color.white, unityFontStyleAndWeight = FontStyle.Bold, marginTop = 10, marginBottom = 5 } });
            var colorPicker = new ColorForgePicker(Color.white, newColor => canvas.SetBrushColor(newColor));
            toolbar.AddChild(colorPicker);
            toolbar.AddSeparator(Color.gray, 2);

            // --- PROCEDURAL & FILTERS ---
            toolbar.AddChild(new Label("Singularity Processors") { style = { color = Color.white, unityFontStyleAndWeight = FontStyle.Bold, marginTop = 10, marginBottom = 5 } });

            toolbar.AddButton("Invert Colors", () => canvas.ApplyInvert());
            toolbar.AddButton("Convert to Grayscale", () => canvas.ApplyGrayscale());
            toolbar.AddButton("Generate Plasma Nebula", () => canvas.GeneratePlasmaNoise());

            toolbar.AddSeparator(Color.gray, 2);
            toolbar.AddButton("Clear (White)", () => canvas.Clear(Color.white));
            toolbar.AddButton("Clear (Transparent)", () => canvas.Clear(Color.clear));

            // --- CANVAS CONTAINER ---
            var canvasContainer = new GraphicalUserInterfaceBuilder("CanvasArea")
                .WithAutoGrow(true)
                .WithFlexLayout(FlexDirection.Column, Justify.Center, Align.Center)
                .WithBackgroundColor(new Color(0.08f, 0.08f, 0.08f));

            canvasContainer.AddChild(canvas);

            root.AddChild(toolbar);
            root.AddChild(canvasContainer);

            return root.CreateGui(ctx);
        }

        public Action<VisualElement> GetGuiBuilder() => (root) => root.Add(CreateGui(new GuiContext()));
        public void ToUIDocument(string assetPath) { }
        public void FromUIDocument(string assetPath) { }
    }
}