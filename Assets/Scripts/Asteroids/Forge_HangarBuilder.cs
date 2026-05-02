using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;
using Workshop.UI_And_Tools.Forge.Builders.GuiBuilders;

namespace Workshop.UI_And_Tools.Forge.Builders.GuiBuilders
{
    /// <summary>
    /// A Generalized, Fluent Builder for Avatar Selection.
    /// Supports deep reskinning and automatic Gimbal-FSM injection.
    /// </summary>
    public class Forge_HangarBuilder : IGuiProvider
    {
        private string _title = "HANGAR: SELECTION";
        private List<IAvatarIdentity> _registry = new List<IAvatarIdentity>();
        private Action<IAvatarIdentity> _onSelectionChanged;
        private Action<IAvatarIdentity> _onConfirmed;
        private Action _onBack;

        private Color _primaryAccent = new Color(0.8f, 0.4f, 0.1f);
        private Color _bgSidebar = new Color(0.08f, 0.08f, 0.1f);
        private Color _bgMain = new Color(0.02f, 0.02f, 0.03f);

        public string Title => throw new NotImplementedException();

        public Forge_HangarBuilder(string title) { _title = title; }

        public Forge_HangarBuilder WithRegistry(IEnumerable<IAvatarIdentity> items)
        {
            _registry.Clear();
            _registry.AddRange(items);
            return this;
        }

        public Forge_HangarBuilder WithTheme(Color accent, Color sidebar, Color main)
        {
            _primaryAccent = accent; _bgSidebar = sidebar; _bgMain = main;
            return this;
        }

        public Forge_HangarBuilder OnSelect(Action<IAvatarIdentity> callback) { _onSelectionChanged = callback; return this; }
        public Forge_HangarBuilder OnConfirm(Action<IAvatarIdentity> callback) { _onConfirmed = callback; return this; }
        public Forge_HangarBuilder OnBack(Action callback) { _onBack = callback; return this; }

        // FIX: Removed generic type argument from IBuilder
        public IGuiProvider Build()
        {
            return new HangarGuiProvider(_title, _registry, _onSelectionChanged, _onConfirmed, _onBack, _primaryAccent, _bgSidebar, _bgMain);
        }



        public Action<VisualElement> GetGuiBuilder()
        {
            throw new NotImplementedException();
        }

        public VisualElement CreateGui(GuiContext ctx)
        {
            throw new NotImplementedException();
        }

        public void ToUIDocument(string assetPath)
        {
            throw new NotImplementedException();
        }

        public void FromUIDocument(string assetPath)
        {
            throw new NotImplementedException();
        }

        private class HangarGuiProvider : IGuiProvider
        {
            public string Title { get; }
            private List<IAvatarIdentity> _items;
            private IAvatarIdentity _active;
            private Action<IAvatarIdentity> _onSelect, _onConfirm;
            private Action _onBack;
            private Color _accent, _sidebarColor, _mainColor;
            private GameObject _gimbal;
            private VisualElement _root;

            public HangarGuiProvider(string t, List<IAvatarIdentity> items, Action<IAvatarIdentity> s, Action<IAvatarIdentity> c, Action b, Color a, Color sc, Color mc)
            {
                Title = t; _items = items; _onSelect = s; _onConfirm = c; _onBack = b;
                _accent = a; _sidebarColor = sc; _mainColor = mc;
                if (_items.Count > 0) _active = _items[0];
            }

            public VisualElement CreateGui(GuiContext ctx)
            {
                _root = new VisualElement { style = { flexGrow = 1 } };
                RebuildUI(ctx);
                _root.RegisterCallback<DetachFromPanelEvent>(e => Cleanup());
                return _root;
            }

            private void RebuildUI(GuiContext ctx)
            {
                _root.Clear();
                var split = new ForgeSplitPanelBuilder(380, Side.Left)
                    .WithSidebar(BuildListPanel(ctx))
                    .WithMain(BuildPreviewPanel(ctx));

                _root.Add(split.CreateGui(ctx));
            }

            private IGuiProvider BuildListPanel(GuiContext ctx)
            {
                return new GraphicalUserInterfaceBuilder("HangarList")
                    .WithPadding(15).WithBackgroundColor(_sidebarColor)
                    .AddHeader(Title).AddSeparator(_accent, 2)
                    .OnBuild(ve =>
                    {
                        var scroll = new ScrollView { style = { flexGrow = 1, marginTop = 10 } };
                        foreach (var item in _items)
                        {
                            bool isSelected = item == _active;
                            var card = new GraphicalUserInterfaceBuilder($"Card_{item.UniqueId}")
                                .WithHeight(120)
                                // FIX: Margin overload changed to single value or style injection
                                .WithBackgroundColor(isSelected ? _accent : new Color(0.15f, 0.15f, 0.18f))
                                .OnBuild(cVe => {
                                    cVe.style.marginBottom = 10;
                                    cVe.RegisterCallback<ClickEvent>(e => {
                                        _active = item;
                                        _onSelect?.Invoke(item);
                                        RebuildUI(ctx); // FIX: Internal refresh logic
                                    });
                                })
                                .AddChild(new Label(item.DisplayName.ToUpper())
                                {
                                    style = {
                                        alignSelf = Align.Center, marginTop = 80,
                                        unityFontStyleAndWeight = FontStyle.Bold,
                                        color = isSelected ? Color.black : Color.white
                                    }
                                });
                            scroll.Add(card.Build());
                        }
                        ve.Add(scroll);
                    })
                    .AddChild(new ForgeButtonBuilder("<< BACK", () => _onBack?.Invoke()).WithHeight(30).Build());
            }

            private IGuiProvider BuildPreviewPanel(GuiContext ctx)
            {
                return new GraphicalUserInterfaceBuilder("HangarPreview")
                    .WithBackgroundColor(_mainColor).WithPadding(40)
                    .AddChild(new Label(_active?.DisplayName.ToUpper() ?? "EMPTY") { style = { fontSize = 42, color = Color.white, unityFontStyleAndWeight = FontStyle.Bold } })
                    .AddChild(new Label(_active?.Subtitle ?? "") { style = { fontSize = 14, color = _accent, letterSpacing = 2, marginBottom = 20 } })
                    .OnBuild(ve =>
                    {
                        if (_active == null) return;
                        Cleanup();

                        _gimbal = new GameObject("[HANGAR_GIMBAL]");
                        _gimbal.transform.position = new Vector3(0, -2000, 0);

                        var model = UnityEngine.Object.Instantiate(_active.PreviewPrefab, _gimbal.transform);
                        model.transform.localPosition = Vector3.zero;

                        var lmpb = new LiveModelPreviewBuilder(_gimbal)
                            .WithZoom(2.5f).WithMouseControl(true).WithGizmos(false)
                            .WithBackgroundColor(_mainColor);

                        ve.Add(lmpb.CreateGui(ctx));
                    })
                    .AddChild(new GraphicalUserInterfaceBuilder("ActionRow")
                        .WithFlexLayout(FlexDirection.Row, Justify.Center, Align.Center)
                        .AddChild(new ForgeButtonBuilder("INITIATE DEPLOYMENT", () => _onConfirm?.Invoke(_active))
                            .WithHeight(50).WithWidth(300).WithBackgroundColor(_accent).Build()));
            }

            private void Cleanup() { if (_gimbal != null) UnityEngine.Object.DestroyImmediate(_gimbal); }
            public Action<VisualElement> GetGuiBuilder() => root => root.Add(CreateGui(new GuiContext()));
            public void ToUIDocument(string p) { }
            public void FromUIDocument(string p) { }
        }
    }
}