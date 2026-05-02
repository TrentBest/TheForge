using System;
using UnityEngine;
using UnityEngine.UIElements;

namespace Workshop.UI_And_Tools.Forge.Builders.GuiBuilders
{
    public class Workshop_Gui_ShowcaseGuiComposer : IGuiProvider
    {
        public string Title => "UNIVERSAL UI FORGE";

        private VisualElement _liveCanvas;
        private Label _inspectorTitle;
        private Label _inspectorData;
        private int _elementCount = 0;

        public VisualElement CreateGui(GuiContext ctx)
        {
            var root = new VisualElement { style = { flexGrow = 1, flexDirection = FlexDirection.Row, backgroundColor = new Color(0.05f, 0.05f, 0.08f) } };

            // ==========================================
            // PANEL 1: THE TOOLBOX (Left)
            // ==========================================
            var toolbox = new VisualElement { style = { width = 250, backgroundColor = new Color(0.08f, 0.08f, 0.1f), borderRightWidth = 2, borderRightColor = Color.cyan, paddingLeft = 10, paddingRight = 10, paddingTop = 15 } };
            toolbox.Add(new Label("FORGE TOOLBOX") { style = { color = Color.cyan, fontSize = 18, unityFontStyleAndWeight = FontStyle.Bold, marginBottom = 15 } });

            toolbox.Add(CreateToolButton("Instantiate Data Panel", () => AddCanvasElement("Data Panel", new Color(0.2f, 0.2f, 0.2f, 0.8f))));
            toolbox.Add(CreateToolButton("Instantiate Telemetry Feed", () => AddCanvasElement("Live Telemetry", new Color(0.1f, 0.3f, 0.1f, 0.8f))));
            toolbox.Add(CreateToolButton("Instantiate FSM Trigger", () => AddCanvasElement("Action Button", new Color(0.8f, 0.1f, 0.8f, 0.8f))));

            toolbox.Add(new VisualElement { style = { flexGrow = 1 } });
            toolbox.Add(new Label("Click components to add them to the Live Canvas.") { style = { color = Color.gray, fontSize = 10, whiteSpace = WhiteSpace.Normal, paddingBottom = 10 } });
            root.Add(toolbox);

            // ==========================================
            // PANEL 2: THE LIVE CANVAS (Center)
            // ==========================================
            var canvasContainer = new VisualElement { style = { flexGrow = 1, paddingLeft = 20, paddingRight = 20, paddingTop = 20, paddingBottom = 20, alignItems = Align.Center, justifyContent = Justify.Center } };

            _liveCanvas = new VisualElement { style = { width = new StyleLength(new Length(100, LengthUnit.Percent)), height = new StyleLength(new Length(100, LengthUnit.Percent)), backgroundColor = new Color(0.1f, 0.1f, 0.15f), borderLeftWidth = 1, borderRightWidth = 1, borderTopWidth = 1, borderBottomWidth = 1, borderLeftColor = Color.gray, borderRightColor = Color.gray, borderTopColor = Color.gray, borderBottomColor = Color.gray, flexDirection = FlexDirection.Row, flexWrap = Wrap.Wrap, alignContent = Align.FlexStart } };

            canvasContainer.Add(_liveCanvas);
            root.Add(canvasContainer);

            // ==========================================
            // PANEL 3: THE BYTE-STATE INSPECTOR (Right)
            // ==========================================
            var inspector = new VisualElement { style = { width = 300, backgroundColor = new Color(0.08f, 0.08f, 0.1f), borderLeftWidth = 2, borderLeftColor = new Color(0.8f, 0.1f, 0.8f), paddingLeft = 15, paddingRight = 15, paddingTop = 15 } };
            inspector.Add(new Label("FSM BYTE-STATE BINDING") { style = { color = new Color(0.8f, 0.1f, 0.8f), fontSize = 16, unityFontStyleAndWeight = FontStyle.Bold, marginBottom = 15 } });

            var inspectorBox = new VisualElement { style = { backgroundColor = new Color(0.15f, 0.15f, 0.15f), paddingBottom = 15, paddingTop = 15, paddingLeft = 15, paddingRight = 15, borderLeftWidth = 3, borderLeftColor = Color.yellow } };
            _inspectorTitle = new Label("No Element Selected") { style = { color = Color.white, fontSize = 14, unityFontStyleAndWeight = FontStyle.Bold } };
            _inspectorData = new Label("Select an element on the canvas to inspect its deterministic logic binding.") { style = { color = Color.silver, fontSize = 12, marginTop = 10, whiteSpace = WhiteSpace.Normal } };

            inspectorBox.Add(_inspectorTitle);
            inspectorBox.Add(_inspectorData);
            inspector.Add(inspectorBox);

            root.Add(inspector);

            return root;
        }

        private Button CreateToolButton(string text, Action onClick)
        {
            return new Button(onClick)
            {
                text = text,
                style = { backgroundColor = new Color(0.15f, 0.15f, 0.2f), color = Color.white, height = 35, marginBottom = 10, borderLeftWidth = 2, borderLeftColor = Color.cyan }
            };
        }

        private void AddCanvasElement(string typeName, Color bgColor)
        {
            _elementCount++;
            int localId = _elementCount;
            byte mockByteState = (byte)UnityEngine.Random.Range(1, 255); // Simulating the byte-state FSM link

            var el = new VisualElement
            {
                style =
                {
                    backgroundColor = bgColor,
                    width = 150, height = 100,
                    marginLeft = 10, marginRight = 10, marginTop = 10, marginBottom = 10,
                    justifyContent = Justify.Center, alignItems = Align.Center,
                    borderLeftWidth = 1, borderRightWidth = 1, borderTopWidth = 1, borderBottomWidth = 1, borderLeftColor = Color.white, borderRightColor = Color.white, borderTopColor = Color.white, borderBottomColor = Color.white
                }
            };

            el.Add(new Label(typeName) { style = { color = Color.white, unityFontStyleAndWeight = FontStyle.Bold } });
            el.Add(new Label($"ID: {localId:000}") { style = { color = Color.silver, fontSize = 10 } });

            // Interactive Binding
            el.RegisterCallback<ClickEvent>(ev =>
            {
                _inspectorTitle.text = $"Selected: {typeName} [{localId:000}]";
                _inspectorData.text = $"Memory Allocation: 1 Byte\n\nFSM Target State: 0x{mockByteState:X2}\nCompute Target: GPU Indirect\n\nStatus: Deterministic transition locked.";

                // Highlight logic
                foreach (var child in _liveCanvas.Children()) child.style.borderBottomWidth = 1;
                el.style.borderBottomWidth = 4;
                el.style.borderBottomColor = Color.yellow;
            });

            _liveCanvas.Add(el);
        }

        public Action<VisualElement> GetGuiBuilder() => root => root.Add(CreateGui(new GuiContext()));
        public void ToUIDocument(string assetPath) { }
        public void FromUIDocument(string assetPath) { }
    }
}