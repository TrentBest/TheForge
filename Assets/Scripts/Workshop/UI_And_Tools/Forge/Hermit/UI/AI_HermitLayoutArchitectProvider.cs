using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using UnityEngine;
using UnityEngine.UIElements;
using Workshop.UI_And_Tools.Forge.Builders.GuiBuilders;

namespace Workshop.UI_And_Tools.Forge.Hermit.UI
{
    public class AI_HermitLayoutArchitectProvider : IGuiProvider
    {
        public string Title => "Layout Architect";
        public Action<string> OnTransmitToHermit;

        // --- DATA MODELS ---
        [Serializable]
        public class ArchitectNode
        {
            public string Id = Guid.NewGuid().ToString();
            public string Name = "New Panel";
            public float X = 10f, Y = 10f;
            public float Width = 150f, Height = 100f;
            public Color Color = new Color(0.2f, 0.4f, 0.6f, 0.8f);
            public string AiPrompt = "Describe functionality...";
        }

        [Serializable]
        public class LayoutSaveData
        {
            public string LayoutName = "New Layout";
            public float CanvasWidth = 512f;
            public float CanvasHeight = 512f;
            public List<ArchitectNode> Nodes = new List<ArchitectNode>();
        }

        [Serializable]
        private class ArchitectDatabase
        {
            public List<LayoutSaveData> SavedLayouts = new List<LayoutSaveData>();
        }

        // --- STATE ---
        private LayoutSaveData _activeLayout = new LayoutSaveData();
        private ArchitectNode _selectedNode = null;
        private ArchitectDatabase _database = new ArchitectDatabase();
        private string _saveFilePath => Path.Combine(Application.dataPath, "../ForgeArchitect_Saves.json");

        public int SeparatorWidth { get; private set; }

        // --- UI ELEMENTS ---
        private ScrollView _canvasScrollView;
        private VisualElement _canvasBoard;
        private VisualElement _inspectorPane;
        private DropdownField _loadDropdown;

        public AI_HermitLayoutArchitectProvider()
        {
            LoadDatabase();
            if (_database.SavedLayouts.Count > 0)
                _activeLayout = CloneLayout(_database.SavedLayouts[0]);
            else
                _activeLayout.Nodes.Add(new ArchitectNode { Name = "Main Content", Width = 400, Height = 300 });
        }

        public VisualElement CreateGui(GuiContext ctx)
        {
            var root = new GraphicalUserInterfaceBuilder("ArchitectRoot")
                .WithFlexLayout(FlexDirection.Row)
                .WithPercentSize(100, 100)
                .WithBackgroundColor(new Color(0.05f, 0.05f, 0.05f));

            // =========================================================
            // 1. LEFT COLUMN: INSPECTOR & CRUD (FIXED WIDTH)
            // =========================================================
            var leftColumn = new GraphicalUserInterfaceBuilder("InspectorColumn")
                .WithWidth(350).WithFlexShrink(0)
                .WithBackgroundColor(new Color(0.12f, 0.12f, 0.14f))
                .WithBorderRightWidth(2).WithBorderRightColor(new Color(0.3f, 0.3f, 0.3f))
                .AddChild(c => {
                    var sv = new ScrollView { style = { flexGrow = 1, paddingBottom = 10, paddingLeft = 10, paddingRight = 10, paddingTop = 10 } };
                    _inspectorPane = sv;
                    return sv;
                });

            root.AddChild(leftColumn);

            // =========================================================
            // 2. RIGHT COLUMN: THE DOMINANT CANVAS
            // =========================================================
            root.AddChild(c => {
                _canvasScrollView = new ScrollView(ScrollViewMode.VerticalAndHorizontal);
                _canvasScrollView.style.flexGrow = 1;
                _canvasScrollView.style.backgroundColor = new Color(0.08f, 0.08f, 0.09f); // Darker backdrop

                // Centering wrapper for the canvas
                var centerWrapper = new VisualElement { style = { flexGrow = 1, justifyContent = Justify.Center, alignItems = Align.Center, paddingBottom = 50, paddingTop = 50, paddingLeft = 50, paddingRight = 50 } };

                _canvasBoard = new VisualElement();
                _canvasBoard.style.position = Position.Relative;
                _canvasBoard.style.backgroundColor = new Color(0.15f, 0.15f, 0.18f); // The actual "Screen"
                _canvasBoard.style.borderTopWidth = 1; _canvasBoard.style.borderBottomWidth = 1; _canvasBoard.style.borderLeftWidth = 1; _canvasBoard.style.borderRightWidth = 1;
                _canvasBoard.style.borderTopColor = Color.gray; _canvasBoard.style.borderBottomColor = Color.gray; _canvasBoard.style.borderLeftColor = Color.gray; _canvasBoard.style.borderRightColor = Color.gray;

                // Background grid pattern via USS (approximated with dots)
                var grid = new Label("+   +   +   +\n\n+   +   +   +\n\n+   +   +   +") { style = { position = Position.Absolute, left = 0, top = 0, right = 0, bottom = 0, color = new Color(0.3f, 0.3f, 0.3f, 0.2f), fontSize = 30, overflow = Overflow.Hidden } };
                _canvasBoard.Add(grid);

                centerWrapper.Add(_canvasBoard);
                _canvasScrollView.Add(centerWrapper);

                // Click background to deselect
                _canvasScrollView.RegisterCallback<PointerDownEvent>(e => {
                    if (e.target == _canvasScrollView || e.target == centerWrapper || e.target == _canvasBoard || e.target == grid)
                    {
                        _selectedNode = null;
                        RefreshInspector();
                        RefreshCanvas();
                    }
                });

                return _canvasScrollView;
            });

            RefreshInspector();
            RefreshCanvas();

            return root.Build();
        }

        // =================================================================================
        // --- INSPECTOR RENDERER ---
        // =================================================================================
        private void RefreshInspector()
        {
            if (_inspectorPane == null) return;
            _inspectorPane.Clear();

            var b = new GraphicalUserInterfaceBuilder("InspectorContent");

            // --- CRUD HEADER ---
            b.AddHeader("LAYOUT DATABASE", Color.white);

            List<string> layoutNames = _database.SavedLayouts.Select(l => l.LayoutName).ToList();
            if (layoutNames.Count == 0) layoutNames.Add("No Saves");

            b.AddChild(c => {
                _loadDropdown = new DropdownField("Load", layoutNames, 0);
                _loadDropdown.RegisterValueChangedCallback(e => {
                    var found = _database.SavedLayouts.FirstOrDefault(l => l.LayoutName == e.newValue);
                    if (found != null)
                    {
                        _activeLayout = CloneLayout(found);
                        _selectedNode = null;
                        RefreshCanvas();
                        RefreshInspector();
                    }
                });
                return _loadDropdown;
            });

            b.AddChild(new GraphicalUserInterfaceBuilder("SaveRow").WithFlexLayout(FlexDirection.Row, Justify.SpaceBetween)
                .AddButton("💾 Save Layout", SaveCurrentLayout)
                .AddButton("🗑 Delete", DeleteCurrentLayout)
            );
            b.AddSeparator(Color.gray, SeparatorWidth);

            // --- CANVAS SETTINGS ---
            b.AddHeader("CANVAS SETTINGS", Color.cyan);
            b.AddStringData("Layout Name", _activeLayout.LayoutName, v => _activeLayout.LayoutName = v);
            b.AddFloatData("Canvas Width", _activeLayout.CanvasWidth, v => { _activeLayout.CanvasWidth = v; RefreshCanvas(); });
            b.AddFloatData("Canvas Height", _activeLayout.CanvasHeight, v => { _activeLayout.CanvasHeight = v; RefreshCanvas(); });

            b.AddSeparator(Color.gray, SeparatorWidth);

            // --- NODE MANAGEMENT ---
            b.AddButton("+ SPAWN NEW PANEL", () => {
                var newNode = new ArchitectNode { Name = $"Panel {_activeLayout.Nodes.Count + 1}", X = 50, Y = 50 };
                _activeLayout.Nodes.Add(newNode);
                _selectedNode = newNode;
                RefreshCanvas();
                RefreshInspector();
            });

            b.AddSeparator(Color.gray, SeparatorWidth);

            // --- SELECTED NODE PROPERTIES ---
            if (_selectedNode != null)
            {
                b.AddHeader("SELECTED PANEL", Color.yellow);
                b.AddStringData("Name", _selectedNode.Name, v => { _selectedNode.Name = v; RefreshCanvas(); });

                b.AddChild(new GraphicalUserInterfaceBuilder("PosRow").WithFlexLayout(FlexDirection.Row)
                    .AddFloatData("X Pos", _selectedNode.X, v => { _selectedNode.X = v; RefreshCanvas(); })
                    .AddFloatData("Y Pos", _selectedNode.Y, v => { _selectedNode.Y = v; RefreshCanvas(); })
                );
                b.AddChild(new GraphicalUserInterfaceBuilder("SizeRow").WithFlexLayout(FlexDirection.Row)
                    .AddFloatData("Width", _selectedNode.Width, v => { _selectedNode.Width = v; RefreshCanvas(); })
                    .AddFloatData("Height", _selectedNode.Height, v => { _selectedNode.Height = v; RefreshCanvas(); })
                );

                b.AddChild(new Label("Hermit Prompt:") { style = { marginTop = 10, color = Color.gray, unityFontStyleAndWeight = FontStyle.Bold } });
                b.AddChild(c => {
                    var tf = new TextField { multiline = true, value = _selectedNode.AiPrompt };
                    tf.style.minHeight = 80; tf.style.whiteSpace = WhiteSpace.Normal;
                    tf.RegisterValueChangedCallback(e => _selectedNode.AiPrompt = e.newValue);
                    return tf;
                });

                b.AddChild(new GraphicalUserInterfaceBuilder("DeleteRow").WithFlexLayout(FlexDirection.Row, Justify.FlexEnd).WithMarginTop(10)
                    .AddButton("🗑 Delete Panel", () => {
                        _activeLayout.Nodes.Remove(_selectedNode);
                        _selectedNode = null;
                        RefreshCanvas();
                        RefreshInspector();
                    })
                );
            }
            else
            {
                b.AddChild(new Label("No panel selected. Click a panel on the canvas to edit.") { style = { color = Color.gray, unityTextAlign = TextAnchor.MiddleCenter, marginTop = 20 } });
            }

            b.AddSeparator(Color.gray, SeparatorWidth);
            b.AddChild(c => {
                var btn = new Button(GenerateAndTransmit) { text = "⚡ TRANSMIT SPEC TO HERMIT" };
                btn.style.backgroundColor = new Color(0.4f, 0.1f, 0.6f); btn.style.color = Color.white;
                btn.style.height = 40; btn.style.marginTop = 20; btn.style.unityFontStyleAndWeight = FontStyle.Bold;
                return btn;
            });

            _inspectorPane.Add(b.Build());
        }

        // =================================================================================
        // --- CANVAS RENDERER ---
        // =================================================================================
        private void RefreshCanvas()
        {
            if (_canvasBoard == null) return;

            // Set canvas size strictly
            _canvasBoard.style.width = _activeLayout.CanvasWidth;
            _canvasBoard.style.height = _activeLayout.CanvasHeight;

            // Keep the grid background but clear old nodes
            for (int i = _canvasBoard.childCount - 1; i >= 0; i--)
            {
                if (_canvasBoard[i] is Label gridLbl && gridLbl.text.Contains("+")) continue; // Keep grid
                _canvasBoard.RemoveAt(i);
            }

            // Draw Nodes
            foreach (var node in _activeLayout.Nodes)
            {
                var n = node; // Capture
                bool isSelected = (_selectedNode != null && _selectedNode.Id == n.Id);

                var visualNode = new VisualElement();
                visualNode.style.position = Position.Absolute;
                visualNode.style.left = n.X;
                visualNode.style.top = n.Y;
                visualNode.style.width = n.Width;
                visualNode.style.height = n.Height;
                visualNode.style.backgroundColor = n.Color;

                // Selection Styling
                visualNode.style.borderTopWidth = 2; visualNode.style.borderBottomWidth = 2; visualNode.style.borderLeftWidth = 2; visualNode.style.borderRightWidth = 2;
                visualNode.style.borderTopColor = isSelected ? Color.white : Color.black;
                visualNode.style.borderBottomColor = isSelected ? Color.white : Color.black;
                visualNode.style.borderLeftColor = isSelected ? Color.white : Color.black;
                visualNode.style.borderRightColor = isSelected ? Color.white : Color.black;
                if (isSelected) visualNode.BringToFront();

                // Name Label
                var lbl = new Label(n.Name) { style = { color = Color.white, unityFontStyleAndWeight = FontStyle.Bold, unityTextAlign = TextAnchor.MiddleCenter, flexGrow = 1 } };
                visualNode.Add(lbl);

                // Resize Handle visual cue
                var resizeHandle = new VisualElement { style = { position = Position.Absolute, bottom = 0, right = 0, width = 12, height = 12, backgroundColor = new Color(1, 1, 1, 0.5f), borderTopLeftRadius = 5 } };
                visualNode.Add(resizeHandle);

                // Attach Manipulator
                visualNode.AddManipulator(new DragResizeManipulator(n, () => {
                    _selectedNode = n; // Select on click/drag
                    RefreshInspector();
                }, () => {
                    RefreshCanvas(); // Re-render when drag completes
                }));

                _canvasBoard.Add(visualNode);
            }
        }

        // =================================================================================
        // --- LOGIC & CRUD ---
        // =================================================================================
        private void LoadDatabase()
        {
            if (File.Exists(_saveFilePath))
            {
                try
                {
                    string json = File.ReadAllText(_saveFilePath);
                    _database = JsonUtility.FromJson<ArchitectDatabase>(json) ?? new ArchitectDatabase();
                }
                catch { _database = new ArchitectDatabase(); }
            }
        }

        private void SaveCurrentLayout()
        {
            var existing = _database.SavedLayouts.FirstOrDefault(l => l.LayoutName == _activeLayout.LayoutName);
            if (existing != null) _database.SavedLayouts.Remove(existing);

            _database.SavedLayouts.Add(CloneLayout(_activeLayout));
            File.WriteAllText(_saveFilePath, JsonUtility.ToJson(_database, true));
            Debug.Log($"[Architect] Saved {_activeLayout.LayoutName} to {_saveFilePath}");
            RefreshInspector();
        }

        private void DeleteCurrentLayout()
        {
            var existing = _database.SavedLayouts.FirstOrDefault(l => l.LayoutName == _activeLayout.LayoutName);
            if (existing != null)
            {
                _database.SavedLayouts.Remove(existing);
                File.WriteAllText(_saveFilePath, JsonUtility.ToJson(_database, true));
            }
            if (_database.SavedLayouts.Count > 0) _activeLayout = CloneLayout(_database.SavedLayouts[0]);
            else { _activeLayout = new LayoutSaveData(); _activeLayout.Nodes.Add(new ArchitectNode()); }

            _selectedNode = null;
            RefreshCanvas();
            RefreshInspector();
        }

        private LayoutSaveData CloneLayout(LayoutSaveData source)
        {
            string json = JsonUtility.ToJson(source);
            return JsonUtility.FromJson<LayoutSaveData>(json);
        }

        private void GenerateAndTransmit()
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendLine($"I have designed a new Absolute GUI Layout named '{_activeLayout.LayoutName}'.");
            sb.AppendLine("Please construct a script implementing `IGuiProvider` using my `GraphicalUserInterfaceBuilder` fluent API.");
            sb.AppendLine("\n### Root Canvas");
            sb.AppendLine($"- Requires a Fixed Size: {_activeLayout.CanvasWidth}px width by {_activeLayout.CanvasHeight}px height.");
            sb.AppendLine("- All children below MUST use absolute positioning (`.WithPosition(Position.Absolute)`).");
            sb.AppendLine("\n### Absolute Panels");

            for (int i = 0; i < _activeLayout.Nodes.Count; i++)
            {
                var n = _activeLayout.Nodes[i];
                sb.AppendLine($"{i + 1}. **{n.Name}**");
                sb.AppendLine($"   - Transform: Left: {n.X}px, Top: {n.Y}px");
                sb.AppendLine($"   - Dimensions: Width: {n.Width}px, Height: {n.Height}px");
                sb.AppendLine($"   - Intent: {n.AiPrompt}");
            }

            sb.AppendLine("\nPlease process this spatial data and generate the layout code.");
            OnTransmitToHermit?.Invoke(sb.ToString());
        }

        public Action<VisualElement> GetGuiBuilder() => root => root.Add(CreateGui(new GuiContext()));
        public void ToUIDocument(string assetPath) { }
        public void FromUIDocument(string assetPath) { }

        // =================================================================================
        // --- THE MOUSE MANIPULATOR ---
        // =================================================================================
        private class DragResizeManipulator : PointerManipulator
        {
            private ArchitectNode _node;
            private Action _onSelect;
            private Action _onDragComplete;

            private Vector2 _startMousePos;
            private float _startX, _startY, _startW, _startH;
            private bool _isDragging = false;
            private bool _isResizing = false;

            public DragResizeManipulator(ArchitectNode node, Action onSelect, Action onDragComplete)
            {
                _node = node;
                _onSelect = onSelect;
                _onDragComplete = onDragComplete;
            }

            protected override void RegisterCallbacksOnTarget()
            {
                target.RegisterCallback<PointerDownEvent>(OnPointerDown);
                target.RegisterCallback<PointerMoveEvent>(OnPointerMove);
                target.RegisterCallback<PointerUpEvent>(OnPointerUp);
                target.RegisterCallback<PointerCaptureOutEvent>(OnPointerCaptureOut);
            }

            protected override void UnregisterCallbacksFromTarget()
            {
                target.UnregisterCallback<PointerDownEvent>(OnPointerDown);
                target.UnregisterCallback<PointerMoveEvent>(OnPointerMove);
                target.UnregisterCallback<PointerUpEvent>(OnPointerUp);
                target.UnregisterCallback<PointerCaptureOutEvent>(OnPointerCaptureOut);
            }

            private void OnPointerDown(PointerDownEvent evt)
            {
                if (target.HasPointerCapture(evt.pointerId)) return;

                _onSelect?.Invoke();

                _startMousePos = evt.position;
                _startX = _node.X; _startY = _node.Y;
                _startW = _node.Width; _startH = _node.Height;

                // If clicked within 15px of bottom right corner, we are resizing
                var localPos = target.WorldToLocal(evt.position);
                if (localPos.x > _node.Width - 15 && localPos.y > _node.Height - 15)
                    _isResizing = true;
                else
                    _isDragging = true;

                target.CapturePointer(evt.pointerId);
                evt.StopPropagation();
            }

            private void OnPointerMove(PointerMoveEvent evt)
            {
                if (!target.HasPointerCapture(evt.pointerId)) return;

                Vector2 diff = (Vector2)evt.position - _startMousePos;

                if (_isDragging)
                {
                    _node.X = _startX + diff.x;
                    _node.Y = _startY + diff.y;
                    target.style.left = _node.X;
                    target.style.top = _node.Y;
                    _onSelect?.Invoke(); // Refresh Inspector values live
                }
                else if (_isResizing)
                {
                    _node.Width = Mathf.Max(20, _startW + diff.x);
                    _node.Height = Mathf.Max(20, _startH + diff.y);
                    target.style.width = _node.Width;
                    target.style.height = _node.Height;
                    _onSelect?.Invoke(); // Refresh Inspector values live
                }

                evt.StopPropagation();
            }

            private void OnPointerUp(PointerUpEvent evt)
            {
                if (!target.HasPointerCapture(evt.pointerId)) return;
                _isDragging = false; _isResizing = false;
                target.ReleasePointer(evt.pointerId);
                _onDragComplete?.Invoke();
                evt.StopPropagation();
            }

            private void OnPointerCaptureOut(PointerCaptureOutEvent evt)
            {
                _isDragging = false; _isResizing = false;
            }
        }
    }
}