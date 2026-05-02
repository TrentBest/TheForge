using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;
using Workshop.UI_And_Tools.Forge.Builders;
using Workshop.UI_And_Tools.Forge.Builders.GuiBuilders;
using Workshop.UI_And_Tools.Forge.IO;

namespace Workshop.UI_And_Tools.Forge.Builders.GuiBuilders.REST
{
    /// <summary>
    /// The Architect for External System Integration.
    /// Manages the metadata and authentication for remote API uplinks.
    /// Reforged to resolve definition mismatches and adhere to Forge Protocols.
    /// </summary>
    public class RestForgeBuilder : IForgeBuilder
    {
        public string ToolName => "REST_API_Configurator";

        // --- ACTIVE CONFIGURATION ---
        public string SystemName { get; set; } = "Autodesk Construction Cloud";
        public string BaseUrl { get; set; } = "https://developer.api.autodesk.com";
        public AuthConfiguration Authentication { get; set; } = new AuthConfiguration();
        public List<RestEndpoint> Endpoints { get; set; } = new List<RestEndpoint>();

        public Type GetProductType() => typeof(RestApiDefinition);

        public IGuiProvider GetGuiProvider() => new REST_ConfiguratorGuiProvider(this);

        /// <summary>
        /// Manifests the finalized RestApiDefinition for DataWarehouse registration.
        /// Matches the class structure provided in Assets/Scripts/.../RestApiDefinition.cs.
        /// </summary>
        public object Build()
        {
            Debug.Log($"<color=#3498db>[RestForge]</color> Finalizing Uplink Definition: {SystemName}");
            return new RestApiDefinition
            {
                SystemName = this.SystemName,
                BaseUrl = this.BaseUrl,
                Authentication = this.Authentication,
                Endpoints = new List<RestEndpoint>(this.Endpoints)
            };
        }
    }

    /// <summary>
    /// High-Fidelity Configuration GUI for REST Integrations.
    /// Utilizes the Forge Protocol to eliminate raw VisualElement instantiations.
    /// </summary>
    public class REST_ConfiguratorGuiProvider : IGuiProvider
    {
        public string Title => "UPLINK ARCHITECT";
        private readonly RestForgeBuilder _builder;

        public REST_ConfiguratorGuiProvider(RestForgeBuilder builder)
        {
            _builder = builder;
        }

        public VisualElement CreateGui(GuiContext ctx)
        {
            var rootBuilder = new ForgeContainerBuilder("REST_Root")
                .WithPadding(20f)
                .WithBackgroundColor(new Color(0.05f, 0.05f, 0.08f))
                .WithBorderWidth(1f)
                .WithBorderColor(new Color(0.3f, 0.6f, 1.0f, 0.5f));

            // 1. Header & Identity
            rootBuilder.AddChild(new ForgeLabelBuilder("SYSTEM IDENTITY")
                .WithFontSize(14).WithBold().WithColor(Color.cyan).WithMarginBottom(10f));

            rootBuilder.AddChild(new ForgeTextFieldBuilder("SYSTEM NAME", _builder.SystemName)
                .WithMarginBottom(8f)
                .OnValueChanged(evt => _builder.SystemName = evt.newValue));

            rootBuilder.AddChild(new ForgeTextFieldBuilder("BASE URL", _builder.BaseUrl)
                .WithMarginBottom(20f)
                .OnValueChanged(evt => _builder.BaseUrl = evt.newValue));

            // 2. Authentication Protocol
            rootBuilder.AddChild(new ForgeLabelBuilder("AUTHENTICATION PROTOCOL")
                .WithFontSize(14).WithBold().WithColor(Color.cyan).WithMarginBottom(10f));

            var authPanel = new ForgeContainerBuilder("AuthSubPanel")
                .WithPadding(12f)
                .WithBackgroundColor(new Color(0.1f, 0.1f, 0.15f))
                .WithBorderRadius(5f)
                .WithMarginBottom(20f);

            authPanel.AddChild(new ForgeTextFieldBuilder("HEADER NAME", _builder.Authentication.AuthHeaderName)
                .WithMarginBottom(5f)
                .OnValueChanged(evt => _builder.Authentication.AuthHeaderName = evt.newValue));

            authPanel.AddChild(new ForgeTextFieldBuilder("PREFIX", _builder.Authentication.AuthHeaderPrefix)
                .WithMarginBottom(5f)
                .OnValueChanged(evt => _builder.Authentication.AuthHeaderPrefix = evt.newValue));

            authPanel.AddChild(new ForgeTextFieldBuilder("SECRET/TOKEN", "********") // Masked by default
                .OnValueChanged(evt => _builder.Authentication.TokenOrKey = evt.newValue));

            rootBuilder.AddChild(authPanel);

            // 3. Endpoint Registry Overview
            rootBuilder.AddChild(new ForgeLabelBuilder($"ENDPOINTS REGISTERED: {_builder.Endpoints.Count}")
                .WithFontSize(10).WithColor(Color.gray).WithMarginBottom(15f));

            // 4. Command Actions
            rootBuilder.AddChild(new ForgeButtonBuilder("🚀 REGISTER UPLINK")
                .WithHeight(40f)
                .WithBackgroundColor(new Color(0.1f, 0.4f, 0.2f))
                .WithBold()
                .OnClick(() => {
                    var definition = (RestApiDefinition)_builder.Build();
                    Debug.Log($"[RestForge] {definition.SystemName} successfully manifested to DataBus.");
                }));

            return rootBuilder.Build();
        }

        public Action<VisualElement> GetGuiBuilder() => root => root.Add(CreateGui(new GuiContext()));
        public void ToUIDocument(string path) => WorkshopUxmlBaker.Bake(CreateGui(new GuiContext()), "RestUplink_Snapshot");
        public void FromUIDocument(string path) { }
    }
}