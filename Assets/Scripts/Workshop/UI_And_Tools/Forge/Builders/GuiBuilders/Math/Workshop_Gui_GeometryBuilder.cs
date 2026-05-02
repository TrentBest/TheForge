// File: Assets/Scripts/Workshop/Forge/Builders/GuiBuilders/Workshop_Gui_GeometryBuilder.cs
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UIElements;

namespace Workshop.UI_And_Tools.Forge.Builders.GuiBuilders.Math
{
    public enum GeoOperation
    {
        Base_Solid,
        Boolean_Subtract,
        Boolean_Add,
        Boolean_Intersect,
        Math_Displacement,
        Domain_Twist
    }

    public enum PrimitiveShape { Cube, Sphere, Cylinder, Capsule, Torus }

    [Serializable]
    public class GeometryCarveStep
    {
        public string Id = Guid.NewGuid().ToString().Substring(0, 6);
        public string StepName = "Carve Operation";
        public bool IsActive = true;

        public GeoOperation Operation = GeoOperation.Boolean_Subtract;
        public PrimitiveShape Shape = PrimitiveShape.Sphere;

        // Transform Data
        public Vector3 Position = Vector3.zero;
        public Vector3 Rotation = Vector3.zero;
        public Vector3 Scale = Vector3.one;

        // Advanced Math/SDF Blending
        public float BlendSmoothness = 0.1f;
        public string MathEquationRefId = ""; // Bridges to MathEquationDef!
    }

    public class Workshop_Gui_GeometryBuilder : IGuiProvider
    {
        public string Title => "SDF Geometry Carver";

        // State
        private List<GeometryCarveStep> _carveStack = new List<GeometryCarveStep>();
        private GeometryCarveStep _selectedStep;

        // Dynamic UI Zones
        private VisualElement _stackZone;
        private VisualElement _inspectorZone;
        private VisualElement _previewZone;

        public Workshop_Gui_GeometryBuilder()
        {
            // Seed the starting "Digital Clay"
            _carveStack.Add(new GeometryCarveStep { StepName = "Base Clay Block", Operation = GeoOperation.Base_Solid, Shape = PrimitiveShape.Cube, Scale = new Vector3(2, 2, 2) });
            _carveStack.Add(new GeometryCarveStep { StepName = "Core Hollow", Operation = GeoOperation.Boolean_Subtract, Shape = PrimitiveShape.Sphere, Scale = new Vector3(1.5f, 1.5f, 1.5f), BlendSmoothness = 0.3f });
            _selectedStep = _carveStack[1];
        }

        public VisualElement CreateGui(GuiContext ctx)
        {
            var rootBuilder = new GraphicalUserInterfaceBuilder("GeometryBuilderRoot")
                .WithFlexLayout(FlexDirection.Row, Justify.SpaceBetween, Align.Stretch)
                .WithBackgroundColor(new Color(0.04f, 0.04f, 0.05f))
                .WithFlexGrow(1f);

            // --- 1. MODIFIER STACK (LEFT) ---
            var stackPanel = new GraphicalUserInterfaceBuilder("StackPanel")
                .WithWidth(300).WithBorderRightWidth(1).WithBorderRightColor(new Color(0.2f, 0.2f, 0.2f))
                .WithBackgroundColor(new Color(0.08f, 0.08f, 0.09f))
                .WithFlexLayout(FlexDirection.Column)
                .AddHeader("CARVING STACK", Color.gray)
                .AddChild(new GraphicalUserInterfaceBuilder("Toolbar").WithFlexLayout(FlexDirection.Row).WithPadding(10)
                    .AddButton("+ ADD SUBTRACT", () => AddStep(GeoOperation.Boolean_Subtract))
                    .AddButton("+ ADD MATH WARP", () => AddStep(GeoOperation.Math_Displacement))
                );

            _stackZone = new ScrollView { style = { flexGrow = 1, paddingTop = 10, paddingRight = 10, paddingLeft = 10, paddingBottom = 10 } };
            stackPanel.AddChild(_stackZone);
            rootBuilder.AddChild(stackPanel);

            // --- 2. PROPERTIES INSPECTOR (CENTER) ---
            var inspectorPanel = new GraphicalUserInterfaceBuilder("InspectorPanel")
                .WithWidth(350).WithBorderRightWidth(1).WithBorderRightColor(new Color(0.2f, 0.2f, 0.2f))
                .WithBackgroundColor(new Color(0.1f, 0.1f, 0.12f));

            _inspectorZone = new ScrollView { style = { flexGrow = 1, paddingTop = 15, paddingBottom = 15, paddingLeft = 15, paddingRight = 15 } };
            inspectorPanel.AddChild(_inspectorZone);
            rootBuilder.AddChild(inspectorPanel);

            // --- 3. 3D RENDER PREVIEW (RIGHT) ---
            var previewPanel = new GraphicalUserInterfaceBuilder("PreviewPanel")
                .WithFlexGrow(1f)
                .WithBackgroundColor(new Color(0.02f, 0.02f, 0.03f))
                .WithFlexLayout(FlexDirection.Column, Justify.Center, Align.Center);

            _previewZone = new VisualElement { style = { flexGrow = 1, width = Length.Percent(100) } };
            previewPanel.AddChild(_previewZone);

            // Add an overlay header for the 3D view
            previewPanel.AddChild(new Label("REAL-TIME SDF RAYMARCH PREVIEW") { style = { position = Position.Absolute, top = 15, left = 15, color = Color.cyan, unityFontStyleAndWeight = FontStyle.Bold } });

            rootBuilder.AddChild(previewPanel);

            RefreshAllZones();
            return rootBuilder.Build();
        }

        private void AddStep(GeoOperation op)
        {
            var step = new GeometryCarveStep { Operation = op, StepName = $"New {op}" };
            _carveStack.Add(step);
            _selectedStep = step;
            RefreshAllZones();
        }

        private void RefreshAllZones()
        {
            RebuildStackZone();
            RebuildInspectorZone();
            RebuildPreviewZone(); // Triggers the 3D visualization update
        }

        private void RebuildStackZone()
        {
            if (_stackZone == null) return;
            _stackZone.Clear();

            // Render top-down execution
            for (int i = 0; i < _carveStack.Count; i++)
            {
                var step = _carveStack[i];
                bool isSelected = _selectedStep == step;
                Color opColor = GetOperationColor(step.Operation);

                var stepBuilder = new GraphicalUserInterfaceBuilder($"Step_{step.Id}")
                    .WithFlexLayout(FlexDirection.Row, Justify.SpaceBetween, Align.Center)
                    .WithPadding(10).WithMarginBottom(5)
                    .WithBackgroundColor(isSelected ? new Color(0.2f, 0.2f, 0.25f) : new Color(0.12f, 0.12f, 0.15f))
                    .WithBorderLeftWidth(4).WithBorderLeftColor(opColor)
                    .WithBorderWidth(1).WithBorderAllColor(isSelected ? Color.white : new Color(0.2f, 0.2f, 0.2f))
                    .WithBorderRadius(4);

                // Layer Info
                var infoCol = new GraphicalUserInterfaceBuilder("Info").WithFlexLayout(FlexDirection.Column)
                    .AddChild(new Label(step.StepName) { style = { color = Color.white, unityFontStyleAndWeight = FontStyle.Bold } })
                    .AddChild(new Label(step.Operation.ToString().Replace("_", " ")) { style = { color = opColor, fontSize = 10 } });

                stepBuilder.AddChild(infoCol);

                // Visibility Toggle
                stepBuilder.AddChild(new Toggle() { value = step.IsActive, style = { alignSelf = Align.Center } }.OnValueChanged(v => { step.IsActive = v; RebuildPreviewZone(); }));

                var uiNode = stepBuilder.Build();
                uiNode.RegisterCallback<ClickEvent>(e => { _selectedStep = step; RefreshAllZones(); });
                _stackZone.Add(uiNode);
            }
        }

        private void RebuildInspectorZone()
        {
            if (_inspectorZone == null) return;
            _inspectorZone.Clear();

            if (_selectedStep == null)
            {
                _inspectorZone.Add(new Label("Select a carving step to inspect.") { style = { color = Color.gray, alignSelf = Align.Center, marginTop = 100 } });
                return;
            }

            Color opColor = GetOperationColor(_selectedStep.Operation);

            var inspector = new GraphicalUserInterfaceBuilder("Properties")
                .AddHeader("STEP PROPERTIES", opColor)
                .AddStringData("Step Name", _selectedStep.StepName, v => { _selectedStep.StepName = v; RebuildStackZone(); })
                .AddEnumData("Operation", _selectedStep.Operation, v => { _selectedStep.Operation = v; RefreshAllZones(); });

            if (_selectedStep.Operation != GeoOperation.Math_Displacement && _selectedStep.Operation != GeoOperation.Domain_Twist)
            {
                inspector.AddEnumData("Primitive Shape", _selectedStep.Shape, v => { _selectedStep.Shape = v; RebuildPreviewZone(); });
            }

            inspector.AddSeparator(Color.gray, 1);

            // --- TRANSFORM CONTROLS ---
            inspector.AddHeader("TRANSFORM", Color.gray);
            inspector.AddChild(CreateVector3Field("Position", _selectedStep.Position, v => { _selectedStep.Position = v; RebuildPreviewZone(); }));
            inspector.AddChild(CreateVector3Field("Rotation", _selectedStep.Rotation, v => { _selectedStep.Rotation = v; RebuildPreviewZone(); }));
            inspector.AddChild(CreateVector3Field("Scale", _selectedStep.Scale, v => { _selectedStep.Scale = v; RebuildPreviewZone(); }));

            // --- ADVANCED BLENDING / MATH ---
            inspector.AddSeparator(Color.gray, 1);

            if (_selectedStep.Operation == GeoOperation.Boolean_Subtract || _selectedStep.Operation == GeoOperation.Boolean_Add)
            {
                inspector.AddHeader("SDF SMOOTHING", new Color(1f, 0.5f, 0f));
                inspector.AddSliderData("Blend Radius", 0f, 2f, _selectedStep.BlendSmoothness, v => { _selectedStep.BlendSmoothness = v; RebuildPreviewZone(); });
                inspector.AddChild(new Label("Smooths the intersection between formulas (Gooey effect).") { style = { color = Color.gray, fontSize = 10, whiteSpace = WhiteSpace.Normal } });
            }
            else if (_selectedStep.Operation == GeoOperation.Math_Displacement)
            {
                inspector.AddHeader("EQUATION BINDING", Color.magenta);
                // TODO: Wire this to DataWarehouse.GetAll<MathEquationDef>()
                inspector.AddStringData("Equation ID", _selectedStep.MathEquationRefId, v => { _selectedStep.MathEquationRefId = v; RebuildPreviewZone(); });
                inspector.AddChild(new Label("Pipes the vertex position (x,y,z) through your Math Synthesizer FSM to displace the surface.") { style = { color = Color.magenta, fontSize = 10, whiteSpace = WhiteSpace.Normal, marginTop = 5 } });
            }

            // --- DELETE ---
            inspector.AddSeparator(Color.gray, 1);
            inspector.AddButton("DELETE STEP", () => {
                _carveStack.Remove(_selectedStep);
                _selectedStep = _carveStack.Count > 0 ? _carveStack.Last() : null;
                RefreshAllZones();
            });

            _inspectorZone.Add(inspector.Build());
        }

        private void RebuildPreviewZone()
        {
            if (_previewZone == null) return;
            _previewZone.Clear();

            // Here we instantiate the actual Unity rendering pipeline view.
            // Since this is an SDF raymarcher, we would compile the _carveStack into a Compute Shader string,
            // and pass it to a RenderTexture displayed via your LiveModelPreviewBuilder.

            // For the Forge UI architectural representation:
            var dummyPreview = new GraphicalUserInterfaceBuilder("SDF_Preview")
                .WithFlexGrow(1f)
                .WithFlexLayout(FlexDirection.Column, Justify.Center, Align.Center)
                .AddImage("Textures/Placeholders/WireframeSphere", 400, 400) // Assuming you have a generic preview image
                .AddChild(new Label($"Compiling {_carveStack.Count(s => s.IsActive)} active mathematical steps into Compute Shader...") { style = { color = Color.gray, marginTop = 20 } })
                .Build();

            _previewZone.Add(dummyPreview);
        }

        // --- UTILS ---
        private Color GetOperationColor(GeoOperation op)
        {
            return op switch
            {
                GeoOperation.Base_Solid => Color.white,
                GeoOperation.Boolean_Subtract => new Color(1f, 0.3f, 0.3f),   // Red carving
                GeoOperation.Boolean_Add => new Color(0.3f, 1f, 0.3f),        // Green adding
                GeoOperation.Boolean_Intersect => new Color(1f, 0.9f, 0.1f),  // Yellow keeping
                GeoOperation.Math_Displacement => Color.magenta,              // Magenta math
                GeoOperation.Domain_Twist => Color.cyan,                      // Cyan spatial warping
                _ => Color.gray
            };
        }

        private VisualElement CreateVector3Field(string label, Vector3 value, Action<Vector3> onChanged)
        {
            var row = new GraphicalUserInterfaceBuilder($"{label}_Row").WithFlexLayout(FlexDirection.Column).WithMarginBottom(8);
            row.AddChild(new Label(label) { style = { color = Color.white, fontSize = 11, marginBottom = 2 } });

            var inputs = new GraphicalUserInterfaceBuilder("Inputs").WithFlexLayout(FlexDirection.Row, Justify.SpaceBetween);

            inputs.AddFloatData("X", value.x, v => { value.x = v; onChanged(value); });
            inputs.AddFloatData("Y", value.y, v => { value.y = v; onChanged(value); });
            inputs.AddFloatData("Z", value.z, v => { value.z = v; onChanged(value); });

            row.AddChild(inputs);
            return row.Build();
        }

        public Action<VisualElement> GetGuiBuilder() => (root) => root.Add(CreateGui(new GuiContext()));
        public void ToUIDocument(string assetPath) { }
        public void FromUIDocument(string assetPath) { }
    }
}
// Extension for Toggle builder
public static class ToggleExtensions
{
    public static Toggle OnValueChanged(this Toggle t, Action<bool> action)
    {
        t.RegisterValueChangedCallback(e => action(e.newValue));
        return t;
    }
}