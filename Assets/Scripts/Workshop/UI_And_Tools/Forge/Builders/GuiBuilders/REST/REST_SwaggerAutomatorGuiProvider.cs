// File: Assets/Scripts/Workshop/Forge/Builders/GuiBuilders/REST/REST_SwaggerAutomatorGuiProvider.cs
using System;
using UnityEngine;
using UnityEngine.UIElements;
using Workshop.UI_And_Tools.Forge.Builders.GuiBuilders;
using Workshop.UI_And_Tools.Forge.IO;

namespace Workshop.UI_And_Tools.Forge.Builders.GuiBuilders.REST
{
    public class REST_SwaggerAutomatorGuiProvider : IGuiProvider
    {
        public string Title => "Automated API Ingestor";

        private VisualElement _root;
        private ScrollView _consoleContainer;
        private ScrollView _resultsContainer;
        private Button _connectBtn;
        private TextField _urlInput;

        public VisualElement CreateGui(GuiContext ctx)
        {
            _root = new VisualElement();
            _root.style.flexDirection = FlexDirection.Row;
            _root.style.flexGrow = 1;
            _root.style.backgroundColor = new Color(0.12f, 0.12f, 0.12f, 0.95f);

            // --- LEFT COLUMN: INPUT & STATUS CONSOLE ---
            var leftCol = new VisualElement { style = { width = 350, borderRightWidth = 1, borderRightColor = Color.gray, paddingTop = 15, paddingBottom = 15, paddingLeft = 15, paddingRight = 15 } };

            leftCol.Add(new Label("Swagger / OpenAPI Target") { style = { color = Color.cyan, fontSize = 16, unityFontStyleAndWeight = FontStyle.Bold, marginBottom = 10 } });

            _urlInput = new TextField("Target URL:")
            {
                // Defaulting to a free, public API for immediate testing
                value = "https://petstore.swagger.io/v2/swagger.json",
                style = { marginBottom = 10 }
            };
            leftCol.Add(_urlInput);

            _connectBtn = new Button(OnConnectClicked) { text = "Connect & Automate API", style = { backgroundColor = new Color(0.1f, 0.5f, 0.8f), height = 30, marginBottom = 15 } };
            leftCol.Add(_connectBtn);

            leftCol.Add(new Label("Ingestion Console") { style = { color = Color.white, marginTop = 10, marginBottom = 5 } });

            _consoleContainer = new ScrollView { style = { flexGrow = 1, backgroundColor = new Color(0.05f, 0.05f, 0.05f), paddingTop = 5, paddingRight = 5, paddingLeft = 5, paddingBottom = 5, borderTopWidth = 2, borderTopColor = Color.gray } };
            leftCol.Add(_consoleContainer);

            _root.Add(leftCol);

            // --- RIGHT COLUMN: EXTRACTED ENDPOINTS GUI ---
            _resultsContainer = new ScrollView { style = { flexGrow = 1, paddingTop = 15, paddingBottom = 15, paddingLeft = 15, paddingRight = 15 } };
            _root.Add(_resultsContainer);

            LogToConsole("System Ready. Awaiting Target URL.");

            return _root;
        }

        private void OnConnectClicked()
        {
            string targetUrl = _urlInput.value;
            if (string.IsNullOrWhiteSpace(targetUrl)) return;

            _connectBtn.SetEnabled(false);
            _resultsContainer.Clear();
            _consoleContainer.Clear();

            LogToConsole("Packaging request for ForgeNetworkBus...");
            LogToConsole($"Target: {targetUrl}");

            // Create a network intent for the Forge Bus to handle centrally
            var intent = new NetworkIntent
            {
                FullUrl = targetUrl,
                Method = "GET",
                OnComplete = (result) =>
                {
                    if (result.IsSuccess)
                    {
                        LogToConsole($"<color=#55ff55>Payload received from Bus. Size: {result.Payload.Length}</color>");
                        LogToConsole("Passing stream to FSM Lexical Analyzer...");
                        ProcessLexicalResults(result.Payload);
                    }
                    else
                    {
                        LogToConsole($"<color=#ff5555>Bus Error: {result.StatusCode}</color>");
                    }
                    _connectBtn.SetEnabled(true);
                }
            };
            if (ForgeNetworkBus.Instance == null)
            {
                LogToConsole("<color=#ff5555>Error: ForgeNetworkBus not found in scene.</color>");
                _connectBtn.SetEnabled(true);
                return;
            }
            // Hand it to the centralized bus.
            ForgeNetworkBus.Instance.QueueRequest(intent);
        }

        private void LogToConsole(string message)
        {
            var logEntry = new Label($"> {message}")
            {
                style = { color = Color.gray, fontSize = 11, whiteSpace = WhiteSpace.Normal }
            };
            logEntry.EnableInClassList("rich-text", true);

            _consoleContainer.Add(logEntry);

            // Auto-scroll to bottom
            _consoleContainer.schedule.Execute(() => _consoleContainer.ScrollTo(logEntry));
        }

        private void ProcessLexicalResults(string rawJson)
        {
            LogToConsole("Initializing FSM Lexical Analyzer...");

            var lexer = new SwaggerFsmLexer();
            RestApiDefinition extractedApi = lexer.Parse(rawJson);

            // Quick extraction of BaseUrl assuming standard swagger pathing
            extractedApi.BaseUrl = _urlInput.value.Replace("/swagger.json", "").Replace("/v2", "");

            LogToConsole($"<color=#55ffff>Analysis Complete. Found {extractedApi.Endpoints.Count} valid endpoints.</color>");
            LogToConsole("Generating Dynamic Workspace...");

            _resultsContainer.Clear();
            _resultsContainer.Add(new Label($"Extracted API Workspace") { style = { fontSize = 18, color = Color.white, marginBottom = 15 } });

            if (extractedApi.Endpoints.Count == 0)
            {
                _resultsContainer.Add(new Label("No endpoints could be parsed from the provided URL.") { style = { color = Color.red } });
                return;
            }

            // Generate interactive UI cards from the FSM output using the internal GuiProviders
            foreach (var endpoint in extractedApi.Endpoints)
            {
                // Instantiate the builder and immediately Build() it down to a VisualElement
                var interactiveCardProvider = new REST_EndpointActionCardProvider(extractedApi, endpoint);
                _resultsContainer.Add(interactiveCardProvider.Build());
            }
        }

        private VisualElement CreateEndpointCard(string method, string route, string summary)
        {
            // Standard Swagger Hex Colors
            string hexColor = method switch
            {
                "GET" => "#61affe",
                "POST" => "#49cc90",
                "PUT" => "#fca130",
                "DELETE" => "#f93e3e",
                "PATCH" => "#50e3c2",
                _ => "#888888"
            };
            ColorUtility.TryParseHtmlString(hexColor, out Color methodColor);

            var epBox = new VisualElement { style = { backgroundColor = new Color(0.15f, 0.15f, 0.15f, 1f), paddingTop = 10, paddingRight = 10, paddingLeft = 10, paddingBottom = 10, marginTop = 10, borderLeftWidth = 6, borderLeftColor = methodColor } };

            var headerRow = new VisualElement { style = { flexDirection = FlexDirection.Row, alignItems = Align.Center, marginBottom = 5 } };
            headerRow.Add(new Label(method) { style = { width = 60, color = methodColor, unityFontStyleAndWeight = FontStyle.Bold, fontSize = 14 } });

            // Route label
            var routeLabel = new Label(route) { style = { color = Color.white, flexGrow = 1, whiteSpace = WhiteSpace.Normal } };
            headerRow.Add(routeLabel);

            epBox.Add(headerRow);

            // Summary description
            epBox.Add(new Label(summary) { style = { color = Color.gray, fontSize = 12, whiteSpace = WhiteSpace.Normal } });

            return epBox;
        }

        // --- IGuiProvider Contract ---
        public Action<VisualElement> GetGuiBuilder() => (root) => root.Add(CreateGui(new GuiContext()));
        public void ToUIDocument(string assetPath) { }
        public void FromUIDocument(string assetPath) { }
    }
}