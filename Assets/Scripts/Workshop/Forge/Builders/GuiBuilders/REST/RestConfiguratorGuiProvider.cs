// File: Assets/Scripts/Workshop/Forge/Builders/GuiBuilders/REST/RestConfiguratorGuiProvider.cs
using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using UnityEngine;
using UnityEngine.UIElements;

namespace TheSingularityWorkshop.Forge.Builders.GuiBuilders.REST
{
    public class RestConfiguratorGuiProvider : IGuiProvider
    {
        public string Title => "External Systems Configurator";

        // Pre-loaded with your Local SST Environment
        private List<RestApiDefinition> _savedApis = new List<RestApiDefinition>
        {
            new RestApiDefinition
            {
                SystemName = "Local SST Fabric v1",
                BaseUrl = "http://localhost:7112",
                Authentication = new AuthConfiguration { AuthHeaderName = "Authorization", TokenOrKey = "" },
                Endpoints = new List<RestEndpoint>
                {
                    new RestEndpoint { Name = "Identity Assertion", Method = "POST", Route = "/auth/authenticate", DefaultPayload = "{\n  \"username\": \"trent.best\",\n  \"password\": \"******\"\n}" },
                    new RestEndpoint { Name = "Squirrel3 Challenge", Method = "GET", Route = "/auth/challenge/{user}" },
                    new RestEndpoint { Name = "State Verification", Method = "POST", Route = "/auth/verify", DefaultPayload = "{\n  \"userId\": \"...\",\n  \"challengeIndex\": 0,\n  \"answer\": \"...\"\n}" }
                }
            }
        };

        private RestApiDefinition _activeDefinition;
        private VisualElement _editorContainer;
        private VisualElement _root; // Keep a reference to root for our modal overlays

        public VisualElement CreateGui(GuiContext ctx)
        {
            _root = new VisualElement();
            _root.style.flexDirection = FlexDirection.Row;
            _root.style.flexGrow = 1;
            _root.style.backgroundColor = new Color(0.12f, 0.12f, 0.12f, 0.95f);

            // --- LEFT COLUMN: CRUD LIST ---
            var sidebar = new VisualElement { style = { width = 250, borderRightWidth = 1, borderRightColor = Color.gray, paddingTop = 10, paddingRight = 10, paddingLeft = 10, paddingBottom = 10 } };

            sidebar.Add(new Label("Saved Systems") { style = { color = Color.white, unityFontStyleAndWeight = FontStyle.Bold, marginBottom = 10 } });

            var createBtn = new Button(CreateNewApi) { text = "+ New REST API", style = { backgroundColor = new Color(0.2f, 0.6f, 0.3f) } };
            sidebar.Add(createBtn);

            var listContainer = new ScrollView { style = { flexGrow = 1, marginTop = 10 } };
            foreach (var api in _savedApis)
            {
                listContainer.Add(new Button(() => SelectApi(api)) { text = api.SystemName });
            }
            sidebar.Add(listContainer);
            _root.Add(sidebar);

            // --- RIGHT COLUMN: ACTIVE EDITOR ---
            _editorContainer = new ScrollView { style = { flexGrow = 1, paddingTop = 15, paddingBottom = 15, paddingLeft = 15, paddingRight = 15 } };
            _root.Add(_editorContainer);

            if (_savedApis.Count > 0) SelectApi(_savedApis[0]);

            return _root;
        }

        private void CreateNewApi()
        {
            var newApi = new RestApiDefinition();
            _savedApis.Add(newApi);
            SelectApi(newApi);
        }

        private void SelectApi(RestApiDefinition api)
        {
            _activeDefinition = api;
            _editorContainer.Clear();

            _editorContainer.Add(new Label($"Editing: {api.SystemName}") { style = { fontSize = 18, color = Color.white, marginBottom = 15 } });

            // Base Config
            _editorContainer.Add(CreateFieldRow("System Name:", api.SystemName, val => api.SystemName = val));
            _editorContainer.Add(CreateFieldRow("Base URL:", api.BaseUrl, val => api.BaseUrl = val));

            // Auth Config
            _editorContainer.Add(new Label("Authentication Strategy") { style = { color = Color.cyan, marginTop = 15, marginBottom = 5 } });
            _editorContainer.Add(CreateFieldRow("Auth Header:", api.Authentication.AuthHeaderName, val => api.Authentication.AuthHeaderName = val));
            _editorContainer.Add(CreateFieldRow("Global Token/Key:", api.Authentication.TokenOrKey, val => api.Authentication.TokenOrKey = val, true));

            // Endpoints Header
            var endpointsHeaderRow = new VisualElement { style = { flexDirection = FlexDirection.Row, justifyContent = Justify.SpaceBetween, marginTop = 20 } };
            endpointsHeaderRow.Add(new Label("Defined Endpoints") { style = { color = Color.cyan, fontSize = 14 } });
            endpointsHeaderRow.Add(new Button(() => { api.Endpoints.Add(new RestEndpoint()); SelectApi(api); }) { text = "+ Add Route" });
            _editorContainer.Add(endpointsHeaderRow);

            // Render Endpoints
            foreach (var endpoint in api.Endpoints)
            {
                var epBox = new VisualElement { style = { backgroundColor = new Color(0.2f, 0.2f, 0.2f, 1f), paddingTop = 10, paddingRight = 10, paddingLeft = 10, paddingBottom = 10, marginTop = 10, borderLeftWidth = 6, borderLeftColor = GetMethodColor(endpoint.Method) } };
                var epRow = new VisualElement { style = { flexDirection = FlexDirection.Row, alignItems = Align.Center } };

                var methodField = new DropdownField(new List<string> { "GET", "POST", "PUT", "DELETE", "PATCH" }, endpoint.Method) { style = { width = 70 } };
                methodField.RegisterValueChangedCallback(evt => { endpoint.Method = evt.newValue; epBox.style.borderLeftColor = GetMethodColor(evt.newValue); });

                var routeField = new TextField { value = endpoint.Route, style = { flexGrow = 1, marginLeft = 5 } };
                routeField.RegisterValueChangedCallback(evt => endpoint.Route = evt.newValue);

                var nameField = new TextField { value = endpoint.Name, style = { width = 150, marginLeft = 5 } };
                nameField.RegisterValueChangedCallback(evt => endpoint.Name = evt.newValue);

                // THE TEST BUTTON
                var testBtn = new Button(() => InitiateEndpointTest(api, endpoint)) { text = "TEST", style = { width = 60, marginLeft = 10, backgroundColor = new Color(0.3f, 0.3f, 0.3f) } };

                epRow.Add(methodField);
                epRow.Add(routeField);
                epRow.Add(nameField);
                epRow.Add(testBtn);
                epBox.Add(epRow);

                _editorContainer.Add(epBox);
            }
        }

        private void InitiateEndpointTest(RestApiDefinition api, RestEndpoint endpoint)
        {
            // Scrape the route for {variables}
            var matches = Regex.Matches(endpoint.Route, @"\{(.*?)\}");

            if (matches.Count > 0)
            {
                // We need data! Spawn the variable collection modal.
                SpawnVariableCollectionModal(api, endpoint, matches);
            }
            else
            {
                // No variables needed, fire immediately!
                ExecuteTest(api, endpoint, endpoint.Route);
            }
        }

        private void SpawnVariableCollectionModal(RestApiDefinition api, RestEndpoint endpoint, MatchCollection variables)
        {
            // Create an absolute positioned overlay blocking the rest of the UI
            var overlay = new VisualElement { style = { position = Position.Absolute, top = 0, bottom = 0, left = 0, right = 0, backgroundColor = new Color(0, 0, 0, 0.8f), justifyContent = Justify.Center, alignItems = Align.Center } };

            var dialog = new VisualElement { style = { width = 350, backgroundColor = new Color(0.15f, 0.15f, 0.15f), paddingTop = 20, paddingBottom = 20, paddingLeft = 20, paddingRight = 20, borderTopWidth = 4, borderTopColor = GetMethodColor(endpoint.Method) } };
            dialog.Add(new Label($"Configure Variables for {endpoint.Method}") { style = { color = Color.white, fontSize = 16, marginBottom = 15 } });
            dialog.Add(new Label(endpoint.Route) { style = { color = Color.gray, marginBottom = 15 } });

            // Dictionary to store user inputs
            Dictionary<string, TextField> inputFields = new Dictionary<string, TextField>();

            foreach (Match match in variables)
            {
                string varName = match.Groups[1].Value;

                // Smart Default: If the variable is 'user', guess a good default!
                string defaultValue = varName.ToLower().Contains("user") ? "trent.best" : "test_value";

                var row = new VisualElement { style = { flexDirection = FlexDirection.Row, marginBottom = 10, alignItems = Align.Center } };
                row.Add(new Label($"{{{varName}}}:") { style = { width = 80, color = Color.cyan } });

                var input = new TextField { value = defaultValue, style = { flexGrow = 1 } };
                inputFields.Add(varName, input);

                row.Add(input);
                dialog.Add(row);
            }

            var btnRow = new VisualElement { style = { flexDirection = FlexDirection.Row, justifyContent = Justify.FlexEnd, marginTop = 15 } };

            var cancelBtn = new Button(() => _root.Remove(overlay)) { text = "Cancel" };
            var runBtn = new Button(() =>
            {
                // Construct final route
                string finalRoute = endpoint.Route;
                foreach (var kvp in inputFields)
                {
                    finalRoute = finalRoute.Replace($"{{{kvp.Key}}}", kvp.Value.value);
                }

                _root.Remove(overlay);
                ExecuteTest(api, endpoint, finalRoute);
            })
            { text = "Run Test", style = { backgroundColor = new Color(0.1f, 0.5f, 0.8f) } };

            btnRow.Add(cancelBtn);
            btnRow.Add(runBtn);
            dialog.Add(btnRow);

            overlay.Add(dialog);
            _root.Add(overlay);
        }

        private void ExecuteTest(RestApiDefinition api, RestEndpoint endpoint, string resolvedRoute)
        {
            string fullUrl = $"{api.BaseUrl.TrimEnd('/')}/{resolvedRoute.TrimStart('/')}";

            var loadingOverlay = SpawnLoadingModal($"Transmitting {endpoint.Method} to {fullUrl} via Forge Bus...");
            _root.Add(loadingOverlay);

            // Create the Intent
            var intent = new TheSingularityWorkshop.Forge.IO.NetworkIntent
            {
                FullUrl = fullUrl,
                Endpoint = endpoint,
                Auth = api.Authentication,
                Payload = endpoint.DefaultPayload,

                // Define what happens when the Bus brings the data back
                OnComplete = (result) =>
                {
                    if (_root.Contains(loadingOverlay)) _root.Remove(loadingOverlay);
                    SpawnResponseModal(endpoint.Method, fullUrl, result.IsSuccess, result.StatusCode, result.Payload);
                }
            };

            // Hand it to the Bus. 
            // The UI thread keeps running. At the end of the frame, this fires.
            TheSingularityWorkshop.Forge.IO.ForgeNetworkBus.Instance.QueueRequest(intent);
        }

        private VisualElement SpawnLoadingModal(string message)
        {
            var overlay = new VisualElement { style = { position = Position.Absolute, top = 0, bottom = 0, left = 0, right = 0, backgroundColor = new Color(0, 0, 0, 0.7f), justifyContent = Justify.Center, alignItems = Align.Center } };

            var box = new VisualElement { style = { backgroundColor = new Color(0.15f, 0.15f, 0.15f), paddingTop = 20, paddingBottom = 20, paddingLeft = 20, paddingRight = 20, borderTopWidth = 2, borderTopColor = Color.cyan } };
            box.Add(new Label(message) { style = { color = Color.white, fontSize = 14 } });

            overlay.Add(box);
            return overlay;
        }

        private void SpawnResponseModal(string method, string fullUrl, bool isSuccess, long statusCode, string payload)
        {
            var overlay = new VisualElement { style = { position = Position.Absolute, top = 0, bottom = 0, left = 0, right = 0, backgroundColor = new Color(0, 0, 0, 0.85f), justifyContent = Justify.Center, alignItems = Align.Center } };

            // Dynamic color based on HTTP Success
            Color statusColor = isSuccess ? new Color(0.38f, 0.81f, 0.38f) : new Color(0.9f, 0.3f, 0.3f);

            var dialog = new VisualElement { style = { width = 500, height = 400, backgroundColor = new Color(0.12f, 0.12f, 0.12f), paddingTop = 20, paddingBottom = 20, paddingLeft = 20, paddingRight = 20, borderTopWidth = 4, borderTopColor = statusColor } };

            // Header Row
            var headerRow = new VisualElement { style = { flexDirection = FlexDirection.Row, justifyContent = Justify.SpaceBetween, marginBottom = 10 } };
            headerRow.Add(new Label($"Status: {statusCode}") { style = { color = statusColor, fontSize = 18, unityFontStyleAndWeight = FontStyle.Bold } });
            headerRow.Add(new Label(method) { style = { color = GetMethodColor(method), fontSize = 14, unityFontStyleAndWeight = FontStyle.Bold } });
            dialog.Add(headerRow);

            dialog.Add(new Label(fullUrl) { style = { color = Color.gray, marginBottom = 15, fontSize = 10 } });

            // Response Payload Area
            dialog.Add(new Label("Response Body:") { style = { color = Color.white, marginBottom = 5 } });

            var responseField = new TextField { multiline = true, isReadOnly = true, value = payload };
            responseField.style.flexGrow = 1;
            responseField.style.marginBottom = 15;
            dialog.Add(responseField);

            // Close Button
            var btnRow = new VisualElement { style = { flexDirection = FlexDirection.Row, justifyContent = Justify.FlexEnd } };
            var closeBtn = new Button(() => _root.Remove(overlay)) { text = "Close", style = { backgroundColor = new Color(0.3f, 0.3f, 0.3f), width = 80, height = 30 } };
            btnRow.Add(closeBtn);
            dialog.Add(btnRow);

            overlay.Add(dialog);
            _root.Add(overlay);
        }

        private VisualElement CreateFieldRow(string labelText, string initialValue, Action<string> onValueChanged, bool isPassword = false)
        {
            var row = new VisualElement { style = { flexDirection = FlexDirection.Row, marginBottom = 5, alignItems = Align.Center } };
            row.Add(new Label(labelText) { style = { width = 120, color = Color.white } });
            var field = new TextField { value = initialValue, isPasswordField = isPassword, style = { flexGrow = 1 } };
            field.RegisterValueChangedCallback(evt => onValueChanged(evt.newValue));
            row.Add(field);
            return row;
        }

        // SWAGGER SPECIFIC HEX COLORS
        private Color GetMethodColor(string method)
        {
            string hex = method switch
            {
                "GET" => "#61affe",   // Swagger Blue
                "POST" => "#49cc90",  // Swagger Green
                "PUT" => "#fca130",   // Swagger Orange
                "DELETE" => "#f93e3e",// Swagger Red
                "PATCH" => "#50e3c2", // Swagger Teal
                _ => "#888888"        // Gray fallback
            };
            ColorUtility.TryParseHtmlString(hex, out Color color);
            return color;
        }

        // --- IGuiProvider Contract Satisfaction ---

        public Action<VisualElement> GetGuiBuilder() => (root) => root.Add(CreateGui(new GuiContext()));

        public void ToUIDocument(string assetPath)
        {
            // Matching the GameMastersCompanion stub pattern. 
            // Can be upgraded to GraphicalUserInterfaceBuilder.ConvertToUIDocument if utilizing OneGUI's serialization.
        }

        public void FromUIDocument(string assetPath)
        {
            // Stubbed for runtime hydration
        }
    }
}