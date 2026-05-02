using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;
using Workshop.UI_And_Tools.Forge.Builders.GuiBuilders;
using Workshop.UI_And_Tools.Forge.IO;
using Workshop.Core.Diagnostics;

namespace Workshop.UI_And_Tools.Forge.Diagnostics
{
    public class Workshop_Gui_Logs : IGuiProvider
    {
        // We hold the actual built elements to inject dynamic tabs and logs at runtime
        private VisualElement _tabBarElement;
        private ScrollView _logFeedView;

        private List<LogEntry> _masterLogList = new List<LogEntry>();
        private HashSet<string> _knownTabs = new HashSet<string>();
        private string _activeTab = "ALL";

        public string Title => "Singularity Telemetry";

        public VisualElement CreateGui(GuiContext context)
        {
            // === 1. BUILD THE ROOT ===
            var rootBuilder = new ForgeContainerBuilder("LogRoot")
                .WithFlexGrow(1)
                .WithBackgroundColor(new Color(0.05f, 0.05f, 0.05f));

            // === 2. BUILD THE TAB BAR ===
            var tabBarBuilder = new ForgeContainerBuilder("TabBar")
                .WithFlexLayout(FlexDirection.Row, Justify.FlexStart, Align.FlexStart)
                .WithBackgroundColor(new Color(0.1f, 0.1f, 0.1f))
                .WithBorderColor(new Color(0.2f, 0.2f, 0.2f));

            // === 3. BUILD THE LOG FEED ===
            var logFeedBuilder = new ForgeScrollViewBuilder("LogFeed")
                .WithFlexGrow(1)
                .WithPadding(10, 10, 10, 10);

            // Materialize the structure
            var root = rootBuilder.Build();

            _tabBarElement = tabBarBuilder.Build();

            // Bypass the missing .WithFlexWrap() builder extension by applying it to the materialized element
            _tabBarElement.style.flexWrap = Wrap.Wrap;

            _logFeedView = logFeedBuilder.Build() as ScrollView;

            root.Add(_tabBarElement);
            root.Add(_logFeedView);

            // Initialize the default tab
            CreateTab("ALL", "#FFFFFF");

            // Connect to the Nervous System using the strict type <LogEntry>
            if (SingularityDataBus.Instance != null)
            {
                SingularityDataBus.Instance.Subscribe<LogEntry>("Telemetry_LogAdded", OnLogReceived);
            }

            return root;
        }

        // Stripped the object casting. Pure O(1) type routing.
        private void OnLogReceived(LogEntry entry)
        {
            if (entry == null) return;

            _masterLogList.Add(entry);

            string finalColor = entry.OverrideHexColor ?? "#FFFFFF";

            // Automatically spawn a new tab using the Button Builder if a new system reports in
            if (!_knownTabs.Contains(entry.HeaderId))
            {
                CreateTab(entry.HeaderId, finalColor);
            }

            if (_activeTab == "ALL" || _activeTab == entry.HeaderId)
            {
                RenderLogEntry(entry, finalColor);
            }
        }

        private void CreateTab(string headerId, string hexColor)
        {
            _knownTabs.Add(headerId);
            ColorUtility.TryParseHtmlString(hexColor, out Color tabColor);

            var tabElement = new ForgeButtonBuilder(headerId)
                .OnClick(() => SwitchTab(headerId))
                .WithBackgroundColor(new Color(0.15f, 0.15f, 0.15f))
                .WithColor(tabColor)
                .WithFontStyle(FontStyle.Bold)
                .WithPadding(5, 5, 15, 15)   // Top, Bottom, Left, Right
                .WithMargin(5, 5, 0, 5)      // Top, Bottom, Left, Right
                .WithBorderRadius(5, 5, 0, 0) // TopLeft, TopRight, BottomLeft, BottomRight
                .WithBorderWidth(0)
                .Build();

            _tabBarElement.Add(tabElement);
        }

        private void SwitchTab(string headerId)
        {
            _activeTab = headerId;
            RefreshFeed();
        }

        private void RefreshFeed()
        {
            _logFeedView.Clear();

            foreach (var entry in _masterLogList)
            {
                if (_activeTab == "ALL" || _activeTab == entry.HeaderId)
                {
                    string finalColor = entry.OverrideHexColor ?? "#FFFFFF";
                    RenderLogEntry(entry, finalColor);
                }
            }

            _logFeedView.schedule.Execute(() => _logFeedView.ScrollTo(_logFeedView.contentContainer)).StartingIn(10);
        }

        private void RenderLogEntry(LogEntry entry, string hexColor)
        {
            ColorUtility.TryParseHtmlString(hexColor, out Color styleColor);

            var logRowElement = new ForgeContainerBuilder($"LogRow_{entry.HeaderId}")
                .WithFlexLayout(FlexDirection.Row, Justify.SpaceEvenly, Align.Auto)
                .WithMarginBottom(2)
                .AddChild(new ForgeLabelBuilder($"[{entry.HeaderId}]")
                    .WithWidth(150)
                    .WithColor(styleColor)
                    .WithFontStyle(FontStyle.Bold))
                .AddChild(new ForgeLabelBuilder($"({entry.Timestamp:HH:mm:ss.fff})")
                    .WithWidth(100)
                    .WithColor(Color.gray))
                .AddChild(new ForgeLabelBuilder(entry.Message)
                    .WithColor(Color.white)
                    .WithFlexGrow(1))
                .Build();

            _logFeedView.Add(logRowElement);
        }

        public Action<VisualElement> GetGuiBuilder()
        {
            return (root) => root.Add(CreateGui(new GuiContext()));
        }

        public void ToUIDocument(string assetPath)
        {
            // Utilize the WorkshopUxmlBaker to serialize the runtime layout to disk
            var snapshotRoot = CreateGui(new GuiContext());
            string fileName = string.IsNullOrEmpty(assetPath) ? "TelemetryLog_Snapshot" : System.IO.Path.GetFileNameWithoutExtension(assetPath);
            WorkshopUxmlBaker.Bake(snapshotRoot, fileName);
        }

        public void FromUIDocument(string assetPath)
        {
            // Reversing dynamic logs back into the UI tree requires a deep JSON parser, soft-failing here.
            Debug.LogWarning("[Workshop_Gui_Logs] FromUIDocument is not supported. This UI is dynamically driven via GuiBuilders.");
        }
    }
}