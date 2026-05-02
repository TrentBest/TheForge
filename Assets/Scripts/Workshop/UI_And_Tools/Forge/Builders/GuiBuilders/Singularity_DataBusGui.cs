using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.UIElements;
using Workshop.UI_And_Tools.Forge.Builders;
using Workshop.UI_And_Tools.Forge.Builders.GuiBuilders;
using Workshop.UI_And_Tools.Forge.IO;
using Workshop.UI_And_Tools.Forge.Hermit.Bridge;
using Workshop.Core.Diagnostics;

namespace Workshop.UI_And_Tools.Forge.Builders.GuiBuilders
{
    public class Singularity_DataBusGui : MonoBehaviour, IForgeBuilder
    {
        public string ToolName => "Singularity Data Bus";
        public string Title => ToolName;
        public Type GetProductType() => typeof(DataBusGuiProvider);

        public IGuiProvider GetGuiProvider() => new DataBusGuiProvider();

        private void OnEnable() => BuilderRegistry.Register(this);

        public object Build() => GetGuiProvider();
    }

    public class DataBusGuiProvider : IGuiProvider
    {
        public string Title => "Data Bus Manifestation Chamber";

        private List<HermitDirectivePayload> _pendingDirectives = new List<HermitDirectivePayload>();
        private HermitDirectivePayload _selectedDirective = null;

        private ScrollView _trafficSnifferDisplay;
        private ScrollView _inboxDisplay;
        private VisualElement _inspectorDisplay;
        private Label _metricsLabel;
        private GuiContext _activeCtx;

        private const string MANIFESTATION_TARGET_ID = "Hermit_Output";

        // Aesthetic Palette
        private readonly Color VoidMagenta = new Color(0.8f, 0.0f, 0.8f, 1.0f);
        private readonly Color DeepAmethyst = new Color(0.15f, 0.05f, 0.2f, 1.0f);
        private readonly Color AmberPulse = new Color(1.0f, 0.7f, 0.0f, 1.0f);
        private readonly Color PanelBg = new Color(0.08f, 0.08f, 0.09f, 1.0f);

        // Telemetry Tracking
        private int _totalBytesProcessed = 0;
        private int _totalPacketsRouted = 0;
        private Queue<string> _trafficLog = new Queue<string>();

        public VisualElement CreateGui(GuiContext ctx)
        {
            _activeCtx = ctx;

            // ROOT
            var root = new ForgeContainerBuilder("DataBus_Root")
                .WithFlexLayout(FlexDirection.Column, Justify.FlexStart, Align.Stretch)
                .WithWidth(new StyleLength(Length.Percent(100)))
                .WithHeight(new StyleLength(Length.Percent(100)))
                .WithBackgroundColor(PanelBg);

            // --- TOP METRICS RIBBON ---
            var metricsRibbon = new ForgeContainerBuilder("MetricsRibbon")
                .WithHeight(40).WithBackgroundColor(DeepAmethyst)
                .WithBorderBottomWidth(2).WithBorderBottomColor(VoidMagenta)
                .WithFlexLayout(FlexDirection.Row, Justify.SpaceBetween, Align.Center)
                .WithPaddingLeft(15).WithPaddingRight(15)
                .AddChild(new ForgeLabelBuilder("NERVOUS SYSTEM DIAGNOSTICS [PROMISCUOUS MODE]").WithColor(Color.white).WithBold())
                .OnBuild(ve => {
                    _metricsLabel = new Label("RX: 0 B/s | PACKETS: 0");
                    _metricsLabel.style.color = AmberPulse;
                    _metricsLabel.style.unityFontStyleAndWeight = FontStyle.Bold;
                    ve.Add(_metricsLabel);
                });
            root.AddChild(metricsRibbon);

            // --- MAIN CONTENT SPLIT ---
            var splitContent = new ForgeContainerBuilder("MainSplit")
                .WithFlexLayout(FlexDirection.Row, Justify.FlexStart, Align.Stretch)
                .WithFlexGrow(1);

            // 1. LEFT PANE: Raw Traffic Sniffer
            var snifferPane = new ForgeContainerBuilder("TrafficSniffer")
                .WithWidth(300).WithFlexShrink(0).WithBackgroundColor(new Color(0.05f, 0.05f, 0.06f))
                .WithBorderRightWidth(1).WithBorderRightColor(new Color(0.3f, 0.3f, 0.3f))
                .WithPadding(10)
                .AddChild(new ForgeLabelBuilder("LIVE BINARY TRAFFIC").WithColor(AmberPulse).WithBold().WithMarginBottom(5))
                .AddSeparator(new Color(0.3f, 0.3f, 0.3f), 1)
                .OnBuild(ve => {
                    _trafficSnifferDisplay = new ForgeScrollViewBuilder().WithMode(ScrollViewMode.Vertical).Build() as ScrollView;
                    _trafficSnifferDisplay.style.flexGrow = 1;
                    ve.Add(_trafficSnifferDisplay);
                });
            splitContent.AddChild(snifferPane);

            // 2. MIDDLE PANE: Unpacked Inbox
            var inboxPane = new ForgeContainerBuilder("InboxPane")
                .WithWidth(350).WithFlexShrink(0).WithBackgroundColor(new Color(0.1f, 0.1f, 0.12f))
                .WithBorderRightWidth(1).WithBorderRightColor(new Color(0.3f, 0.3f, 0.3f))
                .WithPadding(10)
                .AddChild(new ForgeLabelBuilder("UNPACKED DIRECTIVES").WithColor(Color.cyan).WithBold().WithMarginBottom(5))
                .AddSeparator(new Color(0.3f, 0.3f, 0.3f), 1)
                .OnBuild(ve => {
                    _inboxDisplay = new ForgeScrollViewBuilder().WithMode(ScrollViewMode.Vertical).Build() as ScrollView;
                    _inboxDisplay.style.flexGrow = 1;
                    ve.Add(_inboxDisplay);
                });
            splitContent.AddChild(inboxPane);

            // 3. RIGHT PANE: Manifestation Inspector
            var inspectorPane = new ForgeContainerBuilder("InspectorPane")
                .WithFlexGrow(1).WithPadding(20)
                .OnBuild(ve => {
                    _inspectorDisplay = new VisualElement { name = "InspectorContainer" };
                    _inspectorDisplay.style.flexGrow = 1;
                    ve.Add(_inspectorDisplay);
                    RefreshInspector();
                });
            splitContent.AddChild(inspectorPane);

            root.AddChild(splitContent);

            var builtRoot = root.CreateGui(ctx);

            // 16ms poll (approx 60 ticks/sec) for high-fidelity visualization
            builtRoot.schedule.Execute(PollDataBus).Every(16);

            return builtRoot;
        }

        private void PollDataBus()
        {
            if (SingularityDataBus.Instance == null) return;

            // NOTE: Assuming SingularityDataBus was updated to expose a diagnostic queue
            // e.g., SingularityDataBus.Instance.GetDiagnosticSnapshot(out List<byte[]> traffic)
            // For now, we simulate sniffing the raw binary bus throughput.

            // 1. Process specific targeted Hermit Directives (Now unpacking from Binary)
            // Replaced JsonUtility with a theoretical binary read
            if (SingularityDataBus.Instance.ReceiveRaw(MANIFESTATION_TARGET_ID, out byte[] rawData))
            {
                _totalBytesProcessed += rawData.Length;
                _totalPacketsRouted++;
                LogTraffic(MANIFESTATION_TARGET_ID, rawData.Length);

                try
                {
                    var payload = UnpackDirective(rawData);
                    if (payload != null && !string.IsNullOrEmpty(payload.Intent))
                    {
                        _pendingDirectives.Add(payload);
                        RefreshInbox();
                    }
                }
                catch (Exception ex) { ForgeLogger.LogWarning($"[DataBus] Binary Unpack failed: {ex.Message}"); }
            }

            // Update Header Metrics
            if (_metricsLabel != null)
                _metricsLabel.text = $"RX: {_totalBytesProcessed} B | PACKETS: {_totalPacketsRouted}";
        }

        private HermitDirectivePayload UnpackDirective(byte[] data)
        {
            // Zero-allocation binary hydration
            using (MemoryStream ms = new MemoryStream(data))
            using (BinaryReader reader = new BinaryReader(ms))
            {
                var payload = new HermitDirectivePayload();
                payload.AgentId = reader.ReadString();
                payload.Intent = reader.ReadString();
                payload.TargetId = reader.ReadString();
                payload.Content = reader.ReadString();
                // Dictionary unpacking logic would follow...
                return payload;
            }
        }

        private void LogTraffic(string route, int byteLength)
        {
            if (_trafficSnifferDisplay == null) return;

            string time = DateTime.Now.ToString("HH:mm:ss.fff");
            string entry = $"<color=#888888>[{time}]</color> <color=#FFBF00>{byteLength} B</color> ➔ {route}";

            _trafficLog.Enqueue(entry);
            if (_trafficLog.Count > 50) _trafficLog.Dequeue(); // Keep visualizer fast

            _trafficSnifferDisplay.Clear();
            foreach (var log in _trafficLog)
            {
                _trafficSnifferDisplay.Add(new Label(log) { style = { fontSize = 10, color = Color.white } });
            }
        }

        private void RefreshInbox()
        {
            if (_inboxDisplay == null) return;
            _inboxDisplay.Clear();

            if (_pendingDirectives.Count == 0)
            {
                _inboxDisplay.Add(new ForgeLabelBuilder("0 Pending").WithColor(Color.gray).WithMarginTop(10).WithTextAlign(TextAnchor.MiddleCenter).Build());
                return;
            }

            foreach (var dir in _pendingDirectives)
            {
                var currentDir = dir;
                Color intentColor = dir.Intent.Contains("Delete") ? Color.red : (dir.Intent.Contains("Create") ? Color.green : AmberPulse);

                var card = new ForgeContainerBuilder($"Card_{dir.TargetId}")
                    .WithBackgroundColor(new Color(0.15f, 0.15f, 0.18f, 1.0f))
                    .WithBorderRadius(5).WithMarginBottom(8).WithPadding(10)
                    .WithBorderLeftWidth(4).WithBorderLeftColor(intentColor)
                    .AddChild(new ForgeLabelBuilder(dir.Intent).WithColor(intentColor).WithBold())
                    .AddChild(new ForgeLabelBuilder(dir.TargetId).WithColor(Color.white).WithFontSize(11))
                    .AddChild(new ForgeButtonBuilder("Examine Memory", () => SelectDirective(currentDir))
                        .WithBackgroundColor(DeepAmethyst).WithTextColor(Color.white).WithMarginTop(5));

                _inboxDisplay.Add(card.CreateGui(_activeCtx));
            }
        }

        private void RefreshInspector()
        {
            if (_inspectorDisplay == null) return;
            _inspectorDisplay.Clear();

            if (_selectedDirective == null)
            {
                var empty = new ForgeContainerBuilder("EmptyState")
                    .WithFlexGrow(1).WithJustifyContent(Justify.Center).WithAlignItems(Align.Center)
                    .AddChild(new ForgeLabelBuilder("AWAITING INSPECTION DIRECTIVE")
                        .WithColor(Color.gray).WithBold().WithFontSize(18));
                _inspectorDisplay.Add(empty.CreateGui(_activeCtx));
                return;
            }

            Color intentColor = _selectedDirective.Intent.Contains("Delete") ? Color.red : (_selectedDirective.Intent.Contains("Create") ? Color.green : AmberPulse);

            var inspector = new ForgeContainerBuilder("ActiveInspector")
                .WithFlexGrow(1)
                .AddChild(new ForgeContainerBuilder("HeaderRow")
                    .WithFlexLayout(FlexDirection.Row, Justify.SpaceBetween, Align.Center).WithMarginBottom(15)
                    .AddChild(new ForgeContainerBuilder("Titles")
                        .AddChild(new ForgeLabelBuilder($"OP: {_selectedDirective.Intent}").WithColor(intentColor).WithFontSize(16).WithBold())
                        .AddChild(new ForgeLabelBuilder($"NODE: {_selectedDirective.TargetId}").WithColor(Color.white).WithFontSize(14)))
                    .AddChild(new ForgeLabelBuilder($"SRC: {_selectedDirective.AgentId}").WithColor(VoidMagenta).WithBold()))
                .AddSeparator(new Color(0.3f, 0.3f, 0.3f), 1)
                .AddChild(new ForgeLabelBuilder("RAW CONTENT PAYLOAD").WithColor(Color.gray).WithMarginTop(10).WithMarginBottom(5));

            inspector.OnBuild(ve => {
                var scroll = new ForgeScrollViewBuilder().WithMode(ScrollViewMode.Vertical).Build();
                scroll.style.flexGrow = 1;
                scroll.style.backgroundColor = new Color(0.02f, 0.02f, 0.02f, 1.0f); // Pitch black for code
                scroll.style.paddingLeft = scroll.style.paddingRight = scroll.style.paddingTop = scroll.style.paddingBottom = 10;
                scroll.style.borderTopLeftRadius = scroll.style.borderTopRightRadius = scroll.style.borderBottomLeftRadius = scroll.style.borderBottomRightRadius = 5;

                scroll.Add(new ForgeLabelBuilder(_selectedDirective.Content)
                    .WithColor(AmberPulse).WithWordWrap(true).Build());
                ve.Add(scroll);
            });

            var actionBar = new ForgeContainerBuilder("ActionBar")
                .WithFlexLayout(FlexDirection.Row, Justify.FlexEnd, Align.Center).WithMarginTop(20)
                .AddChild(new ForgeButtonBuilder("🗑 PURGE", DiscardSelected)
                    .WithBackgroundColor(new Color(0.4f, 0.1f, 0.1f, 1.0f)).WithTextColor(Color.white)
                    .WithHeight(35).WithWidth(120).WithMarginRight(10))
                .AddChild(new ForgeButtonBuilder("⚡ COMPILE & INJECT", ManifestSelected)
                    .WithBackgroundColor(new Color(0.1f, 0.5f, 0.2f, 1.0f)).WithTextColor(Color.white).WithBold()
                    .WithHeight(35).WithWidth(200));

            inspector.AddChild(actionBar);
            _inspectorDisplay.Add(inspector.CreateGui(_activeCtx));
        }

        private void SelectDirective(HermitDirectivePayload payload) { _selectedDirective = payload; RefreshInspector(); }

        private void ManifestSelected()
        {
            if (_selectedDirective == null) return;
            if (ManifestationEngine.TryManifest(_selectedDirective, out string msg))
            {
                ForgeLogger.Log($"<color=green><b>[Manifestation Success]</b></color> {msg}");
                RemoveCurrentDirective();
            }
            else { ForgeLogger.LogError($"<color=red><b>[Manifestation Failed]</b></color> {msg}"); }
        }

        private void DiscardSelected()
        {
            if (_selectedDirective == null) return;
            if (_selectedDirective.Intent == "Delete" || _selectedDirective.Intent == "Trash")
            {
                RestoreFromStasis(_selectedDirective.TargetId);
            }
            RemoveCurrentDirective();
        }

        private void RestoreFromStasis(string targetId)
        {
            var all = Resources.FindObjectsOfTypeAll<Transform>();
            foreach (var t in all)
            {
                if (t.name == targetId && t.gameObject.hideFlags == HideFlags.None)
                {
                    t.gameObject.SetActive(true);
                    return;
                }
            }
        }

        private void RemoveCurrentDirective() { _pendingDirectives.Remove(_selectedDirective); _selectedDirective = null; RefreshInbox(); RefreshInspector(); }

        public Action<VisualElement> GetGuiBuilder() => root => root.Add(CreateGui(new GuiContext()));
        public void ToUIDocument(string assetPath) { }
        public void FromUIDocument(string assetPath) { }
    }
}