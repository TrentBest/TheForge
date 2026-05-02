// File: Assets/Scripts/Workshop/Forge/Builders/GuiBuilders/Workshop_Gui_MathEquation.cs
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UIElements;

namespace Workshop.UI_And_Tools.Forge.Builders.GuiBuilders.Math
{
    public class Workshop_Gui_MathEquation : IGuiProvider
    {
        public string Title => "Equation Synthesizer";

        private List<MathEquationDef> _savedEquations = new List<MathEquationDef>();
        private MathEquationDef _activeEq;

        // Active Editing State
        private MathNode _selectedNode;
        private VisualElement _nodeChainZone;
        private VisualElement _nodeInspectorZone;

        public Workshop_Gui_MathEquation()
        {
            // Seed a sample Neural Network Forward Pass equation
            var eq = new MathEquationDef { EquationName = "NN_HiddenLayer_ForwardPass" };

            var weightNode = new MathNode { NodeName = "Weights", Operation = MathOperation.Input_Variable };
            var inputNode = new MathNode { NodeName = "Inputs", Operation = MathOperation.Input_Variable };
            var dotNode = new MathNode { NodeName = "Dot(W, X)", Operation = MathOperation.Dot_Product, InputA_RefId = weightNode.Id, InputB_RefId = inputNode.Id };
            var biasNode = new MathNode { NodeName = "Bias", Operation = MathOperation.Constant, ConstantValue = 0.5f };
            var addNode = new MathNode { NodeName = "Sum + Bias", Operation = MathOperation.Add, InputA_RefId = dotNode.Id, InputB_RefId = biasNode.Id };
            var reluNode = new MathNode { NodeName = "Activation", Operation = MathOperation.ReLU, InputA_RefId = addNode.Id };

            eq.Nodes.AddRange(new[] { weightNode, inputNode, dotNode, biasNode, addNode, reluNode });
            eq.FinalOutputNodeId = reluNode.Id;

            _savedEquations.Add(eq);
        }

        public VisualElement CreateGui(GuiContext ctx)
        {
            var crudProvider = new CRUD_Builder<MathEquationDef>(
                title: "EQUATION CORES",
                dataSource: () => _savedEquations,
                getDisplayName: eq => string.IsNullOrEmpty(eq.EquationName) ? "New Equation" : eq.EquationName,
                buildEditorForm: BuildEquationEditor,
                onSave: eq => { if (!_savedEquations.Contains(eq)) _savedEquations.Add(eq); },
                onDelete: eq => _savedEquations.Remove(eq),
                getSubtitle: eq => $"{eq.Nodes.Count} Operations"
            );

            // Style for the Math Synthesizer
            crudProvider.Style.RootBackground = new Color(0.04f, 0.05f, 0.06f);
            crudProvider.Style.ListBackground = new Color(0.06f, 0.07f, 0.08f);
            crudProvider.Style.SelectedItemBackground = new Color(0.1f, 0.15f, 0.2f);
            crudProvider.Style.AccentColor = new Color(0f, 0.8f, 1f); // Cyber Blue
            crudProvider.Style.ListWidth = 250f;

            return crudProvider.CreateGui(ctx);
        }

        private VisualElement BuildEquationEditor(MathEquationDef eq)
        {
            _activeEq = eq;
            if (_activeEq.Nodes.Count > 0 && _selectedNode == null) _selectedNode = _activeEq.Nodes[0];

            var container = new GraphicalUserInterfaceBuilder("MathEditorRoot")
                .WithFlexLayout(FlexDirection.Column).WithFlexGrow(1f)
                .AddStringData("Equation Identifier", _activeEq.EquationName, v => _activeEq.EquationName = v)
                .AddStringData("Description", _activeEq.Description, v => _activeEq.Description = v)
                .AddSeparator(new Color(0.2f, 0.2f, 0.2f), 1)
                .Build();

            var workspace = new GraphicalUserInterfaceBuilder("Workspace")
                .WithFlexLayout(FlexDirection.Row, Justify.FlexStart, Align.Stretch)
                .WithFlexGrow(1f)
                .Build();

            // 1. PALETTE (Add Nodes)
            var palette = new GraphicalUserInterfaceBuilder("Palette")
                .WithWidth(150).WithPadding(10)
                .WithBorderRightWidth(1).WithBorderRightColor(new Color(0.2f, 0.2f, 0.2f))
                .AddHeader("OPERATIONS", Color.gray);

            foreach (MathOperation op in Enum.GetValues(typeof(MathOperation)))
            {
                palette.AddButton($"+ {op}", () => {
                    var newNode = new MathNode { NodeName = op.ToString(), Operation = op };
                    _activeEq.Nodes.Add(newNode);
                    _selectedNode = newNode;
                    RefreshWorkspace();
                });
            }
            workspace.Add(palette.Build());

            // 2. THE EXECUTION CHAIN
            var chainContainer = new GraphicalUserInterfaceBuilder("Chain")
                .WithFlexGrow(1f).WithPadding(15)
                .WithBorderRightWidth(1).WithBorderRightColor(new Color(0.2f, 0.2f, 0.2f))
                .WithBackgroundColor(new Color(0.02f, 0.02f, 0.03f))
                .AddHeader("EVALUATION CHAIN", new Color(0f, 0.8f, 1f));

            _nodeChainZone = new ScrollView { style = { flexGrow = 1 } };
            chainContainer.AddChild(_nodeChainZone);
            workspace.Add(chainContainer.Build());

            // 3. NODE INSPECTOR
            var inspectorContainer = new GraphicalUserInterfaceBuilder("Inspector")
                .WithWidth(280).WithPadding(15)
                .WithBackgroundColor(new Color(0.08f, 0.08f, 0.09f))
                .AddHeader("NODE INSPECTOR", Color.cyan);

            _nodeInspectorZone = new ScrollView { style = { flexGrow = 1 } };
            inspectorContainer.AddChild(_nodeInspectorZone);
            workspace.Add(inspectorContainer.Build());

            container.Add(workspace);

            RefreshWorkspace();
            return container;
        }

        private void RefreshWorkspace()
        {
            if (_nodeChainZone == null || _nodeInspectorZone == null) return;

            _nodeChainZone.Clear();
            _nodeInspectorZone.Clear();

            // RENDER CHAIN
            foreach (var node in _activeEq.Nodes)
            {
                bool isSelected = _selectedNode == node;
                bool isOutput = _activeEq.FinalOutputNodeId == node.Id;

                Color nodeColor = GetNodeColor(node.Operation);

                var nodeBox = new GraphicalUserInterfaceBuilder($"Node_{node.Id}")
                    .WithFlexLayout(FlexDirection.Row, Justify.SpaceBetween, Align.Center)
                    .WithBackgroundColor(isSelected ? new Color(0.15f, 0.15f, 0.2f) : new Color(0.1f, 0.1f, 0.12f))
                    .WithBorderLeftWidth(4).WithBorderLeftColor(nodeColor)
                    .WithBorderWidth(1).WithBorderAllColor(isSelected ? Color.white : new Color(0.2f, 0.2f, 0.2f))
                    .WithMarginBottom(5).WithPadding(10)
                    .AddChild(new Label($"[{node.Id}] {node.NodeName}") { style = { color = Color.white, unityFontStyleAndWeight = FontStyle.Bold } })
                    .AddChild(new Label(node.Operation.ToString()) { style = { color = nodeColor, fontSize = 10 } });

                if (isOutput)
                {
                    nodeBox.AddChild(new Label("FINAL OUTPUT") { style = { color = Color.green, backgroundColor = new Color(0, 0.5f, 0, 0.3f), paddingTop = 2, paddingBottom = 2, paddingLeft = 2, paddingRight = 2, fontSize = 9 } });
                }

                var visualNode = nodeBox.Build();
                visualNode.RegisterCallback<ClickEvent>(e => { _selectedNode = node; RefreshWorkspace(); });
                _nodeChainZone.Add(visualNode);
            }

            // RENDER INSPECTOR
            if (_selectedNode != null)
            {
                var availableNodes = _activeEq.Nodes.Where(n => n.Id != _selectedNode.Id).Select(n => $"[{n.Id}] {n.NodeName}").ToList();
                availableNodes.Insert(0, "[ None ]");

                var inspector = new GraphicalUserInterfaceBuilder("NodeProperties")
                    .AddStringData("Node Name", _selectedNode.NodeName, v => { _selectedNode.NodeName = v; RefreshWorkspace(); })
                    .AddEnumData("Operation", _selectedNode.Operation, v => { _selectedNode.Operation = v; RefreshWorkspace(); })
                    .AddSeparator(Color.gray, 1);

                if (_selectedNode.Operation == MathOperation.Constant)
                {
                    inspector.AddFloatData("Constant Value", _selectedNode.ConstantValue, v => _selectedNode.ConstantValue = v);
                }
                else if (_selectedNode.Operation != MathOperation.Input_Variable)
                {
                    // Map Input A
                    int indexA = string.IsNullOrEmpty(_selectedNode.InputA_RefId) ? 0 : availableNodes.FindIndex(n => n.Contains(_selectedNode.InputA_RefId));
                    inspector.AddDropdownData("Input A", availableNodes, Mathf.Max(0, indexA), val => {
                        _selectedNode.InputA_RefId = val == "[ None ]" ? "" : val.Substring(1, 6);
                    });

                    // Map Input B (if operation requires 2 inputs)
                    if (RequiresTwoInputs(_selectedNode.Operation))
                    {
                        int indexB = string.IsNullOrEmpty(_selectedNode.InputB_RefId) ? 0 : availableNodes.FindIndex(n => n.Contains(_selectedNode.InputB_RefId));
                        inspector.AddDropdownData("Input B", availableNodes, Mathf.Max(0, indexB), val => {
                            _selectedNode.InputB_RefId = val == "[ None ]" ? "" : val.Substring(1, 6);
                        });
                    }
                }

                inspector.AddSeparator(Color.gray, 1);

                var btnRow = new GraphicalUserInterfaceBuilder("Btns").WithFlexLayout(FlexDirection.Row, Justify.SpaceBetween)
                    .AddButton("Set as Output", () => { _activeEq.FinalOutputNodeId = _selectedNode.Id; RefreshWorkspace(); })
                    .AddButton("Delete Node", () => { _activeEq.Nodes.Remove(_selectedNode); _selectedNode = null; RefreshWorkspace(); });

                inspector.AddChild(btnRow);
                _nodeInspectorZone.Add(inspector.Build());
            }
        }

        private bool RequiresTwoInputs(MathOperation op)
        {
            return op == MathOperation.Add || op == MathOperation.Subtract || op == MathOperation.Multiply ||
                   op == MathOperation.Divide || op == MathOperation.Power || op == MathOperation.Dot_Product;
        }

        private Color GetNodeColor(MathOperation op)
        {
            switch (op)
            {
                case MathOperation.Input_Variable: return Color.cyan;
                case MathOperation.Constant: return Color.yellow;
                case MathOperation.Sigmoid:
                case MathOperation.ReLU:
                case MathOperation.Tanh: return new Color(1f, 0f, 0.5f); // Cyber Magenta for Activations
                default: return new Color(0f, 1f, 0.5f); // Green for Math logic
            }
        }

        public Action<VisualElement> GetGuiBuilder() => (root) => root.Add(CreateGui(new GuiContext()));
        public void ToUIDocument(string assetPath) { }
        public void FromUIDocument(string assetPath) { }
    }
}