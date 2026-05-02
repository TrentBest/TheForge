using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;
using Workshop.UI_And_Tools.Forge.Builders.GuiBuilders;
using Workshop.UI_And_Tools.Forge.Builders.GuiBuilders.Tools;

namespace Workshop.UI_And_Tools.Forge.Builders.GuiBuilders
{
    public class GuiTools_Hub : IGuiProvider
    {
        public string Title => "Singularity GUI Hub";

        // --- Core State ---
        private List<ForgeCanvasItem> _canvasItems = new List<ForgeCanvasItem>();
        private ForgeCanvasItem _selectedItem = null;
        private bool _isPreviewMode = false;

        // --- Live Containers ---
        private VisualElement _canvasContainer;
        private VisualElement _inspectorContainer;

        public VisualElement CreateGui(GuiContext ctx)
        {
            return new GraphicalUserInterfaceBuilder("TriFoldMaster")
                .WithFlexLayout(FlexDirection.Row, Justify.FlexStart, Align.Stretch)
                .WithFlexGrow(1)
                .WithBackgroundColor(Color.black)
                .AddChild(BuildArmoryPane())
                .AddChild(BuildCanvasPane())
                .AddChild(BuildInspectorPane())
                .Build();
        }

        // ==========================================
        // PANE 1: THE ARMORY (Left)
        // ==========================================
        private GraphicalUserInterfaceBuilder BuildArmoryPane()
        {
            var armory = new GraphicalUserInterfaceBuilder("ArmoryPane")
                .WithWidth(250)
                .WithBackgroundColor(new Color(0.08f, 0.08f, 0.1f))
                .WithBorderRightColor(Color.cyan)
                .WithBorderRightWidth(1)
                .WithPadding(10)
                .WithTitle("The Armory");

            // Corrected: Using DropdownData for string list choices
            armory.AddDropdownData("Mode", new List<string> { "Edit", "Preview" }, _isPreviewMode ? 1 : 0, (val) => {
                _isPreviewMode = (val == "Preview");
                _selectedItem = null; // Clear selection on mode switch
                RefreshAll();
            });

            armory.AddSeparator(Color.gray, 1);
            armory.AddHeader("Basic Elements", Color.white);

            armory.AddButton("+ Add Container", () => AddNewItem("Container"));
            armory.AddButton("+ Add Button", () => AddNewItem("Button"));
            armory.AddButton("+ Add Label", () => AddNewItem("Label"));

            return armory;
        }

        private void AddNewItem(string type)
        {
            var newItem = new ForgeCanvasItem { Name = $"New {type}", ElementType = type };
            _canvasItems.Add(newItem);
            _selectedItem = newItem;
            RefreshAll();
        }

        // ==========================================
        // PANE 2: THE CANVAS (Center)
        // ==========================================
        private GraphicalUserInterfaceBuilder BuildCanvasPane()
        {
            return new GraphicalUserInterfaceBuilder("CanvasWrapper")
                .WithFlexGrow(1)
                .WithBackgroundColor(new Color(0.05f, 0.05f, 0.05f))
                .OnBuild(root => {
                    _canvasContainer = root;
                    RefreshCanvas(); // Initial Draw
                });
        }

        private void RefreshCanvas()
        {
            if (_canvasContainer == null) return;
            _canvasContainer.Clear();

            var board = new GraphicalUserInterfaceBuilder("Board")
                .WithFlexGrow(1)
                .WithPadding(20)
                .WithFlexLayout(FlexDirection.Column, Justify.FlexStart, Align.FlexStart);

            foreach (var item in _canvasItems)
            {
                var elementBuilder = new GraphicalUserInterfaceBuilder(item.Id)
                    .WithWidth(item.Width)
                    .WithHeight(item.Height)
                    .WithBackgroundColor(item.BgColor)
                    .WithFlexLayout(item.FlowDirection, Justify.Center, Align.Center)
                    .WithMarginBottom(10);

                if (!_isPreviewMode && _selectedItem != null && _selectedItem.Id == item.Id)
                {
                    elementBuilder.WithBorderColor(Color.yellow).WithBorderWidth(2);
                }

                if (item.ElementType == "Button")
                {
                    elementBuilder.AddChild(ctx => {
                        var lbl = new Label(item.Name);
                        lbl.style.color = Color.white;
                        lbl.style.unityFontStyleAndWeight = FontStyle.Bold;
                        return lbl;
                    });
                }
                else if (item.ElementType == "Label")
                {
                    elementBuilder.AddChild(ctx => {
                        var lbl = new Label(item.Name);
                        lbl.style.color = Color.white;
                        return lbl;
                    });
                }

                elementBuilder.OnBuild(ve => {
                    if (!_isPreviewMode)
                    {
                        ve.AddManipulator(new ForgeResizeSelectionManipulator((selectedVe) => {
                            _selectedItem = item;
                            RefreshAll();
                        }));
                    }
                    else
                    {
                        if (item.ElementType == "Button")
                        {
                            ve.RegisterCallback<ClickEvent>(e => {
                                Debug.Log($"[Mock Execution] Endpoint: {item.BoundEndpoint}\nPayload: {item.MockPayload}");
                            });
                        }
                    }
                });

                board.AddChild(elementBuilder);
            }

            _canvasContainer.Add(board.Build());
        }

        // ==========================================
        // PANE 3: THE INSPECTOR (Right)
        // ==========================================
        private GraphicalUserInterfaceBuilder BuildInspectorPane()
        {
            return new GraphicalUserInterfaceBuilder("InspectorWrapper")
                .WithWidth(350)
                .WithBackgroundColor(new Color(0.08f, 0.08f, 0.1f))
                .WithBorderLeftColor(Color.cyan)
                .WithBorderLeftWidth(1)
                .OnBuild(root => {
                    _inspectorContainer = root;
                    RefreshInspector(); // Initial Draw
                });
        }

        private void RefreshInspector()
        {
            if (_inspectorContainer == null) return;
            _inspectorContainer.Clear();

            if (_selectedItem == null || _isPreviewMode)
            {
                var emptyMsg = _isPreviewMode ? "Preview Mode Active" : "Select an element to inspect.";

                // Corrected: Using the builder for margin layout instead of raw IStyle properties
                var emptyStateBuilder = new GraphicalUserInterfaceBuilder("EmptyState")
                    .WithMargin(20)
                    .OnBuild(ve => {
                        var lbl = new Label(emptyMsg);
                        lbl.style.color = Color.gray;
                        lbl.style.unityTextAlign = TextAnchor.MiddleCenter;
                        ve.Add(lbl);
                    });

                _inspectorContainer.Add(emptyStateBuilder.Build());
                return;
            }

            var inspector = new GraphicalUserInterfaceBuilder("Inspector")
                .WithPadding(12)
                .WithScrollable(true)
                .WithTitle($"Inspecting: {_selectedItem.Name}");

            inspector.AddHeader("Dimensions", Color.white);
            inspector.AddStringData("Name", _selectedItem.Name, (v) => { _selectedItem.Name = v; RefreshCanvas(); });
            inspector.AddStringData("Width (px)", _selectedItem.Width.ToString(), (v) => { if (float.TryParse(v, out float w)) { _selectedItem.Width = w; RefreshCanvas(); } });
            inspector.AddStringData("Height (px)", _selectedItem.Height.ToString(), (v) => { if (float.TryParse(v, out float h)) { _selectedItem.Height = h; RefreshCanvas(); } });

            inspector.AddSeparator(Color.gray, 1);
            inspector.AddHeader("REST Configuration", Color.cyan);
            inspector.AddStringData("Endpoint", _selectedItem.BoundEndpoint, (v) => _selectedItem.BoundEndpoint = v);

            inspector.AddChild(ctx => {
                var label = new Label("Mock Payload");
                label.style.color = Color.gray;
                label.style.marginTop = 10;
                return label;
            });

            // Corrected: Safe VisualElement instantiation without shorthand style hacks
            inspector.AddChild(ctx => {
                var payloadBox = new TextField { multiline = true, value = _selectedItem.MockPayload };
                payloadBox.style.minHeight = 80;
                payloadBox.style.backgroundColor = Color.black;
                payloadBox.style.color = Color.green;
                payloadBox.RegisterValueChangedCallback(e => _selectedItem.MockPayload = e.newValue);
                return payloadBox;
            });

            inspector.AddSeparator(Color.gray, 1);
            inspector.AddHeader("Sensory Bindings", Color.yellow);
            inspector.AddButton("Attach SFX (Open Tool)", () => {
                Debug.Log("TODO: Spawn Workshop_Gui_SFX_Builder overlay here");
            });

            _inspectorContainer.Add(inspector.Build());
        }

        // --- Helpers ---
        private void RefreshAll()
        {
            RefreshCanvas();
            RefreshInspector();
        }

        // --- Contract ---
        public Action<VisualElement> GetGuiBuilder() => (root) => root.Add(CreateGui(new GuiContext()));
        public void ToUIDocument(string assetPath) { }
        public void FromUIDocument(string assetPath) { }
    }
}