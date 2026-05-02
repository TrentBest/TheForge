using Assets.Scripts.Workshop.UI_And_Tools.Forge.Builders.GuiBuilders;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

namespace Workshop.UI_And_Tools.Forge.Builders.GuiBuilders.Tools
{
    // --- THE PURE DATA MODELS ---
    [Serializable]
    public class SemanticWireframeModel
    {
        public string LayoutName = "New_Forge_App";
        public List<WireframeZone> Zones = new List<WireframeZone>();
    }

    [Serializable]
    public class WireframeZone
    {
        public string ZoneId = Guid.NewGuid().ToString();
        public string ZoneName = "Unnamed Zone";
        public Rect DrawnRect;

        // Tab 2
        public string AssignedComponentType = "None";
        public string SemanticPrompt = "";

        // Tab 3 & 4
        public string BoundBehavior = "None";
        public string BoundActionTarget = "None";
    }

    // --- THE 4-TAB ARCHITECT ---
    public class Workshop_Gui_SemanticWireframer : IGuiProvider
    {
        public string Title => "Semantic Wireframer";

        private SemanticWireframeModel _model = new SemanticWireframeModel();
        private int _activeTab = 0; // 0=Draw, 1=Components, 2=Behaviors, 3=Bindings

        // Drawing State
        private Vector2 _dragStartPos;
        private VisualElement _drawingGhost;
        private VisualElement _canvasArea;

        // Hermit Chassis Injection
        private GameObject _hermitModelGo;

        public Workshop_Gui_SemanticWireframer(GameObject hermitModelGo = null)
        {
            _hermitModelGo = hermitModelGo;
            // Dummy start zone
            _model.Zones.Add(new WireframeZone { ZoneName = "Main Header", DrawnRect = new Rect(10, 10, 800, 80) });
        }

        public VisualElement CreateGui(GuiContext ctx)
        {
            var root = new ForgeContainerBuilder("WireframerRoot")
                .WithFlexLayout(FlexDirection.Column, Justify.FlexStart, Align.Stretch)
                .WithBackgroundColor(new Color(0.1f, 0.1f, 0.12f))
                .WithFlexGrow(1)
                .Build();

            // --- THE TAB BAR ---
            var tabBar = new ForgeContainerBuilder("TabBar")
                .WithFlexLayout(FlexDirection.Row, Justify.FlexStart, Align.Center)
                .WithBackgroundColor(new Color(0.05f, 0.05f, 0.05f))
                .WithBorderBottomWidth(2).WithBorderBottomColor(Color.cyan)
                .WithHeight(50)
                .AddChild(BuildTabButton(0, "1. Sketch Canvas"))
                .AddChild(BuildTabButton(1, "2. Components"))
                .AddChild(BuildTabButton(2, "3. Behaviors"))
                .AddChild(BuildTabButton(3, "4. Bindings"))
                .Build();

            root.Add(tabBar);

            // --- THE CONTENT AREA ---
            var contentArea = new ForgeContainerBuilder("ContentArea")
                .WithFlexGrow(1)
                .OnBuild(ve => ve.style.position = Position.Relative)
                .Build();

            if (_activeTab == 0)
            {
                contentArea.Add(BuildDrawingCanvas());
            }
            else if (_activeTab == 1)
            {
                contentArea.Add(BuildComponentsTab());
            }
            else
            {
                var loadingLabel = new ForgeLabelBuilder("WIP: Rest/FSM Discovery Loading...")
                    .OnBuild(ve =>
                    {
                        ve.style.color = Color.white;
                        ve.style.marginTop = 20; ve.style.marginBottom = 20;
                        ve.style.marginLeft = 20; ve.style.marginRight = 20;
                    })
                    .Build();
                contentArea.Add(loadingLabel);
            }

            root.Add(contentArea);

            // --- INJECT THE EXPANDING HERMIT PORTAL ---
            if (_hermitModelGo != null)
            {
                var hermitPortal = new ForgeHermitChatPortalBuilder("WireframeAssistant", _hermitModelGo).Build(ctx);
                root.Add(hermitPortal);
            }

            return root;
        }

        private VisualElement BuildTabButton(int index, string text)
        {
            return new ForgeButtonBuilder($"TabButton_{index}")
                .OnBuild(ve =>
                {
                    // Cast to Button to access .text and .clicked
                    if (ve is Button btn)
                    {
                        btn.text = text;
                        btn.clicked += () => { _activeTab = index; }; // In a real setup, trigger a router reload here
                    }

                    ve.style.height = 40;
                    ve.style.width = 150;
                    ve.style.backgroundColor = _activeTab == index ? new Color(0.2f, 0.4f, 0.6f) : new Color(0.15f, 0.15f, 0.15f);
                    ve.style.color = Color.white;
                    ve.style.unityFontStyleAndWeight = FontStyle.Bold;
                    ve.style.borderTopLeftRadius = 8;
                    ve.style.borderTopRightRadius = 8;
                })
                .Build();
        }

        // ====================================================================
        // TAB 2: COMPONENTS & SEMANTICS
        // ====================================================================
        private VisualElement BuildComponentsTab()
        {
            var scroll = new ForgeScrollViewBuilder("ComponentsScrollView")
                .WithFlexGrow(1)
                .OnBuild(sv =>
                {
                    sv.style.paddingTop = 20; sv.style.paddingBottom = 20;
                    sv.style.paddingLeft = 20; sv.style.paddingRight = 20;
                })
                .Build();

            foreach (var zone in _model.Zones)
            {
                var card = new ForgeContainerBuilder($"Zone_{zone.ZoneId}")
                    .WithBackgroundColor(new Color(0.2f, 0.2f, 0.25f))
                    .WithBorderRadius(8)
                    .WithFlexLayout(FlexDirection.Column, Justify.FlexStart, Align.Stretch)
                    .OnBuild(ve =>
                    {
                        ve.style.paddingTop = 15; ve.style.paddingBottom = 15;
                        ve.style.paddingLeft = 15; ve.style.paddingRight = 15;
                        ve.style.marginBottom = 10;
                    })
                    .Build();

                var header = new ForgeTextFieldBuilder("Zone Name:")
                    .OnBuild(ve =>
                    {
                        // Cast to TextField to access .value and callbacks
                        if (ve is TextField tf)
                        {
                            tf.value = zone.ZoneName;
                            tf.RegisterValueChangedCallback(e => zone.ZoneName = e.newValue);
                        }
                    })
                    .Build();
                card.Add(header);

                // Fixed the constructor error and casted to DropdownField
                var typeDropdown = new ForgeDropdownBuilder()
                    .OnBuild(ve =>
                    {
                        if (ve is DropdownField dd)
                        {
                            dd.label = "Component Type:";
                            dd.choices = new List<string> { "None", "ForgeButtonBuilder", "ForgeTextFieldBuilder", "ForgeContainerBuilder" };
                            dd.value = zone.AssignedComponentType;
                            dd.RegisterValueChangedCallback(e => zone.AssignedComponentType = e.newValue);
                        }
                    })
                    .Build();
                card.Add(typeDropdown);

                var promptField = new ForgeTextFieldBuilder("Hermit Instructions:")
                    .OnBuild(ve =>
                    {
                        if (ve is TextField tf)
                        {
                            tf.multiline = true;
                            tf.value = zone.SemanticPrompt;
                            tf.RegisterValueChangedCallback(e => zone.SemanticPrompt = e.newValue);
                        }
                        ve.style.minHeight = 60;
                    })
                    .Build();
                card.Add(promptField);

                scroll.Add(card);
            }

            return scroll;
        }

        // ====================================================================
        // TAB 1: THE ABSOLUTE DRAWING CANVAS
        // ====================================================================
        private VisualElement BuildDrawingCanvas()
        {
            _canvasArea = new ForgeContainerBuilder("CanvasArea")
                .WithFlexGrow(1)
                .WithBackgroundColor(new Color(0.15f, 0.15f, 0.15f))
                .Build();

            // Render existing zones
            foreach (var zone in _model.Zones)
            {
                // FIXED: Passed zone.ZoneName into the constructor, removed l.text from OnBuild
                var boxLabel = new ForgeLabelBuilder(zone.ZoneName)
                    .OnBuild(ve =>
                    {
                        ve.name = $"Label_{zone.ZoneId}";
                        ve.style.color = Color.white;
                        ve.style.unityTextAlign = TextAnchor.MiddleCenter;
                        ve.style.flexGrow = 1;
                    })
                    .Build();

                var box = new ForgeContainerBuilder($"ZoneBox_{zone.ZoneId}")
                    .WithBackgroundColor(new Color(0.3f, 0.3f, 0.4f, 0.5f))
                    .OnBuild(ve =>
                    {
                        ve.style.position = Position.Absolute;
                        ve.style.left = zone.DrawnRect.x; ve.style.top = zone.DrawnRect.y;
                        ve.style.width = zone.DrawnRect.width; ve.style.height = zone.DrawnRect.height;

                        ve.style.borderTopColor = Color.white; ve.style.borderBottomColor = Color.white;
                        ve.style.borderLeftColor = Color.white; ve.style.borderRightColor = Color.white;
                        ve.style.borderTopWidth = 1; ve.style.borderBottomWidth = 1;
                        ve.style.borderLeftWidth = 1; ve.style.borderRightWidth = 1;
                    })
                    .AddChild(boxLabel)
                    .Build();

                _canvasArea.Add(box);
            }

            // The Click-and-Click Drawing Logic
            _canvasArea.RegisterCallback<PointerDownEvent>(evt =>
            {
                if (evt.button != 0) return; // Only left click
                _canvasArea.CapturePointer(evt.pointerId);
                _dragStartPos = evt.localPosition;

                _drawingGhost = new ForgeContainerBuilder("DrawingGhost")
                    .WithBackgroundColor(new Color(0, 1, 1, 0.2f))
                    .OnBuild(ve =>
                    {
                        ve.style.position = Position.Absolute;
                        ve.style.borderTopColor = Color.cyan; ve.style.borderBottomColor = Color.cyan;
                        ve.style.borderLeftColor = Color.cyan; ve.style.borderRightColor = Color.cyan;
                        ve.style.borderTopWidth = 2; ve.style.borderBottomWidth = 2;
                        ve.style.borderLeftWidth = 2; ve.style.borderRightWidth = 2;
                    })
                    .Build();

                _canvasArea.Add(_drawingGhost);
            });

            _canvasArea.RegisterCallback<PointerMoveEvent>(evt =>
            {
                if (_drawingGhost == null) return;

                float x = Mathf.Min(_dragStartPos.x, evt.localPosition.x);
                float y = Mathf.Min(_dragStartPos.y, evt.localPosition.y);
                float w = Mathf.Abs(_dragStartPos.x - evt.localPosition.x);
                float h = Mathf.Abs(_dragStartPos.y - evt.localPosition.y);

                _drawingGhost.style.left = x; _drawingGhost.style.top = y;
                _drawingGhost.style.width = w; _drawingGhost.style.height = h;
            });

            _canvasArea.RegisterCallback<PointerUpEvent>(evt =>
            {
                if (_drawingGhost == null) return;
                _canvasArea.ReleasePointer(evt.pointerId);

                // Commit the new zone
                Rect finalRect = new Rect(_drawingGhost.style.left.value.value, _drawingGhost.style.top.value.value, _drawingGhost.style.width.value.value, _drawingGhost.style.height.value.value);

                if (finalRect.width > 20 && finalRect.height > 20) // Prevent accidental tiny clicks
                {
                    _model.Zones.Add(new WireframeZone { ZoneName = $"Zone {_model.Zones.Count + 1}", DrawnRect = finalRect });
                }

                _drawingGhost.RemoveFromHierarchy();
                _drawingGhost = null;
            });

            return _canvasArea;
        }



        public Action<VisualElement> GetGuiBuilder() => (root) => root.Add(CreateGui(new GuiContext()));
        public void ToUIDocument(string assetPath) { }
        public void FromUIDocument(string assetPath) { }
    }
}