#if UNITY_EDITOR
using TheSingularityWorkshop.Forge.Builders.GuiBuilders;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UIElements;

namespace TheSingularityWorkshop.Forge.Builders.GuiBuilders
{
    public class HTML_GuiBuilder : IGuiProvider
    {
        public string Title { get; private set; }

        private GuiContext _guiContext;
        private VisualElement _root;
        private VisualElement _tabContainer;
        private VisualElement _searchContainer;
        private ScrollView _contentArea;
        private TextField _urlInput;

        // Fluent Configuration
        private bool _useMultiTabs = false;
        private bool _showSearchBar = false;

        // Tab Management
        private class WebTab
        {
            public string Url;
            public string Title;
            public string HtmlContent;
        }

        private List<WebTab> _tabs = new List<WebTab>();
        private int _activeTabIndex = 0;
        private HttpClient _httpClient;

        public HTML_GuiBuilder(string title, string initialHtml, string initialUrl = "local://forge")
        {
            Title = title;
            _httpClient = new HttpClient();
            _httpClient.DefaultRequestHeaders.Add("User-Agent", "TheSingularityForge/1.0");

            _tabs.Add(new WebTab { Title = title, HtmlContent = initialHtml, Url = initialUrl });
        }

        // --- FLUENT API ---
        public HTML_GuiBuilder WithSingleTab() { _useMultiTabs = false; return this; }
        public HTML_GuiBuilder WithMultiTabs() { _useMultiTabs = true; return this; }
        public HTML_GuiBuilder WithSearch() { _showSearchBar = true; return this; }

        public VisualElement CreateGui(GuiContext ctx)
        {
            _guiContext = ctx;

            _root = new VisualElement { style = { flexGrow = 1, backgroundColor = new Color(0.08f, 0.08f, 0.09f) } };

            // 1. The Search Bar (URL Input)
            _searchContainer = new VisualElement { style = { flexDirection = FlexDirection.Row, paddingLeft = 10, paddingRight = 10, paddingTop = 10, paddingBottom = 10, backgroundColor = new Color(0.12f, 0.12f, 0.15f), borderBottomWidth = 1, borderBottomColor = Color.cyan } };
            _urlInput = new TextField() { style = { flexGrow = 1, marginRight = 10 } };
            _urlInput.RegisterCallback<KeyDownEvent>(e => { if (e.keyCode == KeyCode.Return) NavigateTo(_urlInput.value, _useMultiTabs); });
            var goBtn = new Button(() => NavigateTo(_urlInput.value, _useMultiTabs)) { text = "GOTO", style = { width = 50, backgroundColor = new Color(0.1f, 0.4f, 0.1f), color = Color.white, unityFontStyleAndWeight = FontStyle.Bold } };

            _searchContainer.Add(_urlInput);
            _searchContainer.Add(goBtn);

            if (_showSearchBar) _root.Add(_searchContainer);

            // 2. The Tab Bar
            _tabContainer = new VisualElement { style = { flexDirection = FlexDirection.Row, flexWrap = Wrap.Wrap, backgroundColor = new Color(0.05f, 0.05f, 0.05f), borderBottomWidth = 2, borderBottomColor = Color.gray } };
            if (_useMultiTabs) _root.Add(_tabContainer);

            // 3. The Render Canvas
            _contentArea = new ScrollView(ScrollViewMode.Vertical) { style = { flexGrow = 1, paddingLeft = 20, paddingRight = 20, paddingTop = 20, paddingBottom = 20 } };
            _root.Add(_contentArea);

            RefreshUI();
            return _root;
        }

        private void RefreshUI()
        {
            if (_tabs.Count == 0) return;

            // Safety bounds
            if (_activeTabIndex >= _tabs.Count) _activeTabIndex = _tabs.Count - 1;
            WebTab activeTab = _tabs[_activeTabIndex];

            // Update URL bar
            if (_showSearchBar) _urlInput.value = activeTab.Url;

            // Render Tab Buttons
            if (_useMultiTabs)
            {
                _tabContainer.Clear();
                for (int i = 0; i < _tabs.Count; i++)
                {
                    int tabIndex = i; // Local copy for lambda
                    bool isActive = (i == _activeTabIndex);

                    var tabBtn = new Button(() => { _activeTabIndex = tabIndex; RefreshUI(); })
                    {
                        text = _tabs[i].Title.Length > 20 ? _tabs[i].Title.Substring(0, 17) + ".." : _tabs[i].Title,
                        style = {
                            height = 30, paddingLeft = 10, paddingRight = 10, marginRight = 2, marginTop = 5,
                            backgroundColor = isActive ? new Color(0.2f, 0.3f, 0.4f) : new Color(0.1f, 0.1f, 0.1f),
                            color = isActive ? Color.white : Color.gray,
                            borderTopLeftRadius = 5, borderTopRightRadius = 5,
                            borderBottomWidth = 0
                        }
                    };
                    _tabContainer.Add(tabBtn);
                }
            }

            // Render HTML
            _contentArea.Clear();
            ParseAndAppendHTML(activeTab.HtmlContent, _contentArea, activeTab.Url);
        }

        // --- THE BROWSER ENGINE ---
        private async void NavigateTo(string targetUrl, bool openInNewTab)
        {
            if (string.IsNullOrWhiteSpace(targetUrl)) return;

            // Auto-append protocol if they just typed "google.com"
            if (!targetUrl.StartsWith("http") && !targetUrl.StartsWith("local://"))
                targetUrl = "https://" + targetUrl;

            // Update UI to show loading state
            _contentArea.Clear();
            _contentArea.Add(new Label($"Transpiling Web Matrix: {targetUrl}..") { style = { color = Color.yellow, fontSize = 16, marginTop = 20 } });
            if (_showSearchBar) _urlInput.value = targetUrl;

            try
            {
                // Fetch the raw internet!
                string rawHtml = await _httpClient.GetStringAsync(targetUrl);

                // Try to extract the page Title
                string pageTitle = "Web Page";
                var titleMatch = Regex.Match(rawHtml, @"<title>\s*(.*?)\s*</title>", RegexOptions.IgnoreCase);
                if (titleMatch.Success) pageTitle = titleMatch.Groups[1].Value.Trim();

                if (openInNewTab && _useMultiTabs)
                {
                    _tabs.Add(new WebTab { Url = targetUrl, Title = pageTitle, HtmlContent = rawHtml });
                    _activeTabIndex = _tabs.Count - 1;
                }
                else
                {
                    _tabs[_activeTabIndex].Url = targetUrl;
                    _tabs[_activeTabIndex].Title = pageTitle;
                    _tabs[_activeTabIndex].HtmlContent = rawHtml;
                }

                RefreshUI();
            }
            catch (Exception ex)
            {
                _contentArea.Clear();
                _contentArea.Add(new Label("CONNECTION INTERCEPTED") { style = { color = Color.red, fontSize = 24, unityFontStyleAndWeight = FontStyle.Bold } });
                _contentArea.Add(new Label($"The reality matrix rejected the request:\n\n{ex.Message}") { style = { color = new Color(0.8f, 0.5f, 0.5f), whiteSpace = WhiteSpace.Normal, marginTop = 10 } });
            }
        }

        // --- THE TRANSPILER ---
        private void ParseAndAppendHTML(string html, VisualElement container, string currentBaseUrl)
        {
            if (string.IsNullOrWhiteSpace(html)) return;

            // STRIP THE NOISE: We must obliterate Scripts and Styles, otherwise raw JS text bleeds onto the screen!
            html = Regex.Replace(html, @"<script[^>]*>.*?</script>", "", RegexOptions.Singleline | RegexOptions.IgnoreCase);
            html = Regex.Replace(html, @"<style[^>]*>.*?</style>", "", RegexOptions.Singleline | RegexOptions.IgnoreCase);
            html = Regex.Replace(html, @"<svg[^>]*>.*?</svg>", "[SVG Image Emulated]", RegexOptions.Singleline | RegexOptions.IgnoreCase);

            // Match block elements: h1-h6, p, a, hr
            string pattern = @"<(h[1-6]|p|a|hr)([^>]*)>(.*?)</\1>|<hr\s*/?>";
            MatchCollection matches = Regex.Matches(html, pattern, RegexOptions.Singleline | RegexOptions.IgnoreCase);

            if (matches.Count == 0)
            {
                container.Add(new Label("No readable DOM nodes detected in this sector.") { style = { color = Color.gray } });
                return;
            }

            foreach (Match match in matches)
            {
                string tag = match.Groups[1].Value.ToLower();
                string attributes = match.Groups[2].Value;
                string innerText = Regex.Replace(match.Groups[3].Value.Trim(), @"<[^>]*>", ""); // Strip nested tags for safety

                // Clean up newlines
                innerText = Regex.Replace(innerText, @"\s+", " ");

                switch (tag)
                {
                    case "h1":
                    case "h2":
                        container.Add(new Label(innerText) { style = { fontSize = tag == "h1" ? 22 : 18, color = Color.cyan, unityFontStyleAndWeight = FontStyle.Bold, marginTop = 15, marginBottom = 5 } });
                        break;
                    case "h3":
                    case "h4":
                        container.Add(new Label(innerText) { style = { fontSize = 14, color = Color.yellow, unityFontStyleAndWeight = FontStyle.Bold, marginTop = 10 } });
                        break;
                    case "p":
                        if (!string.IsNullOrWhiteSpace(innerText))
                            container.Add(new Label(innerText) { style = { fontSize = 13, color = new Color(0.8f, 0.8f, 0.8f), whiteSpace = WhiteSpace.Normal, marginBottom = 10 } });
                        break;
                    case "a":
                        string rawHref = ExtractAttribute(attributes, "href");
                        if (string.IsNullOrWhiteSpace(rawHref) || rawHref.StartsWith("javascript") || rawHref.StartsWith("#")) break;

                        // Resolve relative URLs against the current domain
                        string absoluteUrl = ResolveUrl(currentBaseUrl, rawHref);

                        var btn = new Button(() => {
                            Debug.Log($"[Forge Browser] Intercepting click: Redirecting to {absoluteUrl}");
                            NavigateTo(absoluteUrl, _useMultiTabs);
                        })
                        {
                            text = $"🔗 {innerText}",
                            style = { color = Color.cyan, backgroundColor = new Color(0.1f, 0.15f, 0.2f), borderLeftWidth = 2, borderLeftColor = Color.cyan, marginTop = 2, marginBottom = 2, alignSelf = Align.FlexStart }
                        };
                        container.Add(btn);
                        break;
                    case "hr":
                    case "":
                        container.Add(new VisualElement { style = { height = 1, backgroundColor = new Color(0.3f, 0.3f, 0.3f), marginTop = 10, marginBottom = 10 } });
                        break;
                }
            }
        }

        private string ExtractAttribute(string attributes, string attributeName)
        {
            var match = Regex.Match(attributes, $@"{attributeName}\s*=\s*['""]([^'""]+)['""]", RegexOptions.IgnoreCase);
            return match.Success ? match.Groups[1].Value : string.Empty;
        }

        private string ResolveUrl(string baseUrl, string href)
        {
            if (href.StartsWith("http")) return href;
            try { return new Uri(new Uri(baseUrl), href).ToString(); }
            catch { return href; }
        }

        public Action<VisualElement> GetGuiBuilder() => (root) => root.Add(CreateGui(new GuiContext()));
        public void FromUIDocument(string assetPath) => _guiContext?.OnBuilt?.Invoke(GraphicalUserInterfaceBuilder.ConvertFromUIDocument(assetPath));
        public void ToUIDocument(string assetPath) => GraphicalUserInterfaceBuilder.ConvertToUIDocument(CreateGui(new GuiContext()), assetPath);
    }
}
#endif