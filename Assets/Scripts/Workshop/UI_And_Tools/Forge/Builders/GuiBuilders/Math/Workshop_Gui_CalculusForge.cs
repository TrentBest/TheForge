// File: Assets/Scripts/Workshop/Forge/Builders/GuiBuilders/Math/Workshop_Gui_CalculusForge.cs
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UIElements;

namespace Workshop.UI_And_Tools.Forge.Builders.GuiBuilders.Math
{
    public class Workshop_Gui_CalculusForge : IGuiProvider
    {
        public string Title => "Calculus Synthesizer (Symbolic)";

        private MathEquationDef _originalEq;
        private MathEquationDef _derivedEq;
        private string _targetVariableId = "";

        private VisualElement _leftPane;
        private VisualElement _rightPane;

        public VisualElement CreateGui(GuiContext ctx)
        {
            // Injecting mock data for testing
            if (_originalEq == null) _originalEq = GenerateMockEquation();

            var rootBuilder = new GraphicalUserInterfaceBuilder("CalculusForgeRoot")
                .WithFlexLayout(FlexDirection.Column)
                .WithBackgroundColor(new Color(0.05f, 0.05f, 0.06f))
                .WithFlexGrow(1f)
                .WithPadding(15);

            // --- HEADER & CONTROLS ---
            var controls = new GraphicalUserInterfaceBuilder("CalculusControls")
                .WithFlexLayout(FlexDirection.Row, Justify.SpaceBetween, Align.Center)
                .WithBackgroundColor(new Color(0.08f, 0.08f, 0.09f))
                .WithBorderBottomWidth(2).WithBorderBottomColor(Color.magenta)
                .WithPadding(15).WithMarginBottom(15)
                .AddHeader("SYMBOLIC DIFFERENTIATOR", Color.white);

            var actionRow = new GraphicalUserInterfaceBuilder("Actions").WithFlexLayout(FlexDirection.Row, Justify.FlexEnd, Align.Center);

            var variables = _originalEq.Nodes.Where(n => n.Operation == MathOperation.Input_Variable).Select(n => $"[{n.Id}] {n.NodeName}").ToList();
            if (variables.Count > 0 && string.IsNullOrEmpty(_targetVariableId)) _targetVariableId = variables[0].Substring(1, 6);

            actionRow.AddDropdownData("Derive with respect to:", variables, 0, v => _targetVariableId = v.Substring(1, 6));
            actionRow.AddButton(" EXECUTE DERIVATION ", ExecuteCalculus);

            controls.AddChild(actionRow);
            rootBuilder.AddChild(controls);

            // --- SPLIT PANE WORKSPACE ---
            var workspace = new GraphicalUserInterfaceBuilder("Workspace")
                .WithFlexLayout(FlexDirection.Row, Justify.SpaceBetween, Align.Stretch)
                .WithFlexGrow(1f);

            // Left: Original f(x)
            var leftBuilder = new GraphicalUserInterfaceBuilder("OriginalPane")
                .WithFlexGrow(1f).WithMarginRight(10)
                .WithBackgroundColor(new Color(0.08f, 0.08f, 0.09f))
                .WithBorderTopWidth(2).WithBorderTopColor(Color.cyan)
                .AddHeader("f(x) - BASE EQUATION", Color.cyan);

            _leftPane = new ScrollView { style = { flexGrow = 1, paddingTop = 10, paddingRight = 10, paddingLeft = 10, paddingBottom = 10 } };
            leftBuilder.AddChild(_leftPane);
            workspace.AddChild(leftBuilder);

            // Right: Derived f'(x)
            var rightBuilder = new GraphicalUserInterfaceBuilder("DerivedPane")
                .WithFlexGrow(1f).WithMarginLeft(10)
                .WithBackgroundColor(new Color(0.08f, 0.08f, 0.09f))
                .WithBorderTopWidth(2).WithBorderTopColor(Color.magenta)
                .AddHeader("f'(x) - DERIVATIVE", Color.magenta);

            _rightPane = new ScrollView { style = { flexGrow = 1, paddingTop = 10, paddingBottom = 10, paddingLeft = 10, paddingRight = 10 } };
            rightBuilder.AddChild(_rightPane);
            workspace.AddChild(rightBuilder);

            rootBuilder.AddChild(workspace);

            RefreshPanes();
            return rootBuilder.Build();
        }

        private void ExecuteCalculus()
        {
            if (string.IsNullOrEmpty(_targetVariableId)) return;

            // The Magic Happens Here!
            _derivedEq = CalculusEngine.Derive(_originalEq, _targetVariableId);
            RefreshPanes();
        }

        private void RefreshPanes()
        {
            RenderEquationGraph(_originalEq, _leftPane, Color.cyan);
            if (_derivedEq != null) RenderEquationGraph(_derivedEq, _rightPane, Color.magenta);
        }

        private void RenderEquationGraph(MathEquationDef eq, VisualElement container, Color accent)
        {
            if (container == null || eq == null) return;
            container.Clear();

            // Simple top-down linear rendering. 
            // For true shock and awe, this would eventually be a 2D Node Canvas layout!
            foreach (var node in eq.Nodes)
            {
                bool isOutput = eq.FinalOutputNodeId == node.Id;

                var nodeBox = new GraphicalUserInterfaceBuilder($"Node_{node.Id}")
                    .WithFlexLayout(FlexDirection.Row, Justify.SpaceBetween, Align.Center)
                    .WithBackgroundColor(new Color(0.12f, 0.12f, 0.15f))
                    .WithBorderLeftWidth(4).WithBorderLeftColor(accent)
                    .WithMarginBottom(5).WithPadding(10)
                    .AddChild(new Label($"[{node.Id}] {node.NodeName}") { style = { color = Color.white, unityFontStyleAndWeight = FontStyle.Bold } });

                if (node.Operation == MathOperation.Constant)
                    nodeBox.AddChild(new Label(node.ConstantValue.ToString("F2")) { style = { color = Color.yellow } });
                else
                    nodeBox.AddChild(new Label(node.Operation.ToString()) { style = { color = Color.gray, fontSize = 10 } });

                if (isOutput)
                    nodeBox.AddChild(new Label("OUTPUT") { style = { color = Color.green, backgroundColor = new Color(0, 0.5f, 0, 0.3f), paddingTop = 2, paddingRight = 2, paddingLeft = 2, paddingBottom = 2, fontSize = 9 } });

                container.Add(nodeBox.Build());
            }
        }

        private MathEquationDef GenerateMockEquation()
        {
            // Creates f(x) = X * M
            var eq = new MathEquationDef { EquationName = "Linear Multiply" };
            var varX = new MathNode { Operation = MathOperation.Input_Variable, NodeName = "Variable X" };
            var varM = new MathNode { Operation = MathOperation.Input_Variable, NodeName = "Multiplier M" };
            var mult = new MathNode { Operation = MathOperation.Multiply, NodeName = "X * M", InputA_RefId = varX.Id, InputB_RefId = varM.Id };
            eq.Nodes.AddRange(new[] { varX, varM, mult });
            eq.FinalOutputNodeId = mult.Id;
            return eq;
        }

        public Action<VisualElement> GetGuiBuilder() => (root) => root.Add(CreateGui(new GuiContext()));
        public void ToUIDocument(string assetPath) { }
        public void FromUIDocument(string assetPath) { }
    }
}