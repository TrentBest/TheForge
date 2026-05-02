// File: Assets/Scripts/Workshop/UI_And_Tools/Forge/Builders/GuiBuilders/REST/REST_EndpointActionCardProvider.cs
using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using UnityEngine;
using UnityEngine.UIElements;
using Workshop.UI_And_Tools.Forge.IO;

namespace Workshop.UI_And_Tools.Forge.Builders.GuiBuilders.REST
{
    public class REST_EndpointActionCardProvider : IGuiProvider
    {
        public string Title => $"Action Card: {_endpoint.Name}";

        private RestApiDefinition _api;
        private RestEndpoint _endpoint;

        // State for execution
        private Dictionary<string, string> _pathParamValues = new Dictionary<string, string>();
        private string _payloadValue;
        private Label _responseConsole;

        public REST_EndpointActionCardProvider(RestApiDefinition api, RestEndpoint endpoint)
        {
            _api = api;
            _endpoint = endpoint;
            _payloadValue = endpoint.DefaultPayload;

            // Extract path params into state dictionary
            var matches = Regex.Matches(_endpoint.Route, @"\{(.*?)\}");
            foreach (Match match in matches)
            {
                string varName = match.Groups[1].Value;
                if (!_pathParamValues.ContainsKey(varName))
                    _pathParamValues.Add(varName, "");
            }
        }

        public VisualElement CreateGui(GuiContext ctx)
        {
            Color methodColor = GetMethodColor(_endpoint.Method);

            // 1. Core Card Styling
            var cardBuilder = new GraphicalUserInterfaceBuilder($"Card_{_endpoint.Method}")
                .WithBackgroundColor(new Color(0.15f, 0.15f, 0.15f, 1f))
                .WithPadding(10)
                .WithMarginTop(10)
                .WithBorderTopWidth(1).WithBorderRightWidth(1).WithBorderBottomWidth(1)
                .WithBorderLeftWidth(6)
                .WithBorderLeftColor(methodColor)
                .WithBorderColor(new Color(0.3f, 0.3f, 0.3f))
                .WithBorderRadius(4);

            // 2. Header Row
            var headerRow = new ForgeContainerBuilder()
                .WithDirection(FlexDirection.Row)
                .WithAlignItems(Align.Center)
                .WithMarginBottom(10)
                .AddChild(new GraphicalUserInterfaceBuilder().AddLabel(_endpoint.Method, methodColor, 14, FontStyle.Bold, width: 65))
                .AddChild(new GraphicalUserInterfaceBuilder().AddLabel(_endpoint.Route, Color.white, 14, FontStyle.Bold).WithFlexGrow(1));

            cardBuilder.AddChild(headerRow);

            if (!string.IsNullOrEmpty(_endpoint.Name))
            {
                cardBuilder.AddLabel(_endpoint.Name, Color.gray, 12, marginBottom: 10, whiteSpace: WhiteSpace.Normal);
            }

            // 3. Path Parameters
            if (_pathParamValues.Count > 0)
            {
                cardBuilder.AddLabel("Path Parameters:", Color.cyan, 11, marginBottom: 5);

                foreach (var key in new List<string>(_pathParamValues.Keys))
                {
                    var paramRow = new ForgeContainerBuilder()
                        .WithDirection(FlexDirection.Row)
                        .WithAlignItems(Align.Center)
                        .WithMarginBottom(5)
                        .AddChild(new GraphicalUserInterfaceBuilder().AddLabel($"{{{key}}}", new Color(0.8f, 0.8f, 0.8f), width: 100));

                    // Use AddStringData for two-way state binding
                    var inputFieldBuilder = new GraphicalUserInterfaceBuilder()
                        .WithFlexGrow(1)
                        .AddStringData("", _pathParamValues[key], val => _pathParamValues[key] = val);

                    paramRow.AddChild(inputFieldBuilder);
                    cardBuilder.AddChild(paramRow);
                }
            }

            // 4. Payload Body
            if (_endpoint.Method == "POST" || _endpoint.Method == "PUT" || _endpoint.Method == "PATCH")
            {
                cardBuilder.AddLabel("Request Body (JSON):", Color.cyan, 11, marginBottom: 5)
                           .WithMarginTop(10);

                cardBuilder.AddChild(c => {
                    var payloadInput = new TextField { multiline = true, value = _payloadValue };
                    payloadInput.style.minHeight = 60;
                    payloadInput.style.backgroundColor = new Color(0.1f, 0.1f, 0.1f);
                    payloadInput.RegisterValueChangedCallback(evt => _payloadValue = evt.newValue);
                    return payloadInput;
                });
            }

            // 5. Execution Footer
            var footerRow = new ForgeContainerBuilder()
                .WithDirection(FlexDirection.Row)
                .WithJustifyContent(Justify.SpaceBetween)
                .WithAlignItems(Align.FlexEnd)
                .WithMarginTop(15);

            // Response Console Label injection
            footerRow.AddChild(new GraphicalUserInterfaceBuilder().OnBuild(ve => {
                _responseConsole = new Label("Ready.");
                _responseConsole.style.color = Color.gray;
                _responseConsole.style.fontSize = 10;
                _responseConsole.style.flexGrow = 1;
                _responseConsole.style.whiteSpace = WhiteSpace.Normal;
                _responseConsole.style.paddingRight = 10;
                ve.Add(_responseConsole);
            }));

            // Execute Button using GuiBuilderExtensions
            footerRow.AddChild(new GraphicalUserInterfaceBuilder()
                .AddStyledButton("Execute Action", new Color(0.2f, 0.5f, 0.8f), Color.white, ExecuteApiCall, 25));

            cardBuilder.AddChild(footerRow);

            return cardBuilder.CreateGui(ctx);
        }

        private void ExecuteApiCall()
        {
            if (_responseConsole != null)
            {
                _responseConsole.text = "Dispatching via Forge Bus...";
                _responseConsole.style.color = Color.yellow;
            }

            string resolvedRoute = _endpoint.Route;
            foreach (var kvp in _pathParamValues)
            {
                resolvedRoute = resolvedRoute.Replace($"{{{kvp.Key}}}", kvp.Value);
            }

            string fullUrl = $"{_api.BaseUrl.TrimEnd('/')}/{resolvedRoute.TrimStart('/')}";

            var intent = new NetworkIntent
            {
                FullUrl = fullUrl,
                Endpoint = _endpoint,
                Auth = _api.Authentication,
                Payload = _payloadValue,
                OnComplete = HandleResponse
            };

            ForgeNetworkBus.Instance.QueueRequest(intent);
        }

        private void HandleResponse(NetworkResult result)
        {
            if (_responseConsole == null) return;

            if (result.IsSuccess)
            {
                _responseConsole.text = $"Success ({result.StatusCode}): Payload Rx {result.Payload?.Length} bytes";
                _responseConsole.style.color = new Color(0.4f, 0.9f, 0.4f);
                Debug.Log($"[Forge Bus] Data received for {_endpoint.Route}:\n{result.Payload}");
            }
            else
            {
                _responseConsole.text = $"Error ({result.StatusCode}): Check console.";
                _responseConsole.style.color = new Color(0.9f, 0.4f, 0.4f);
            }
        }

        private Color GetMethodColor(string method)
        {
            string hex = method switch
            {
                "GET" => "#61affe",
                "POST" => "#49cc90",
                "PUT" => "#fca130",
                "DELETE" => "#f93e3e",
                "PATCH" => "#50e3c2",
                _ => "#888888"
            };
            ColorUtility.TryParseHtmlString(hex, out Color color);
            return color;
        }

        // --- IGuiProvider Contract ---
        public Action<VisualElement> GetGuiBuilder() => (root) => root.Add(CreateGui(new GuiContext()));
        public void ToUIDocument(string assetPath) { }
        public void FromUIDocument(string assetPath) { }
    }
}