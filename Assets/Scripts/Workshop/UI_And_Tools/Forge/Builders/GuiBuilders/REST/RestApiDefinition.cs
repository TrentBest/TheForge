// File: Assets/Scripts/Workshop/Forge/Builders/GuiBuilders/REST/RestApiDefinition.cs
using System;
using System.Collections.Generic;

namespace Workshop.UI_And_Tools.Forge.Builders.GuiBuilders.REST
{
    [Serializable]
    public class RestApiDefinition
    {
        public string Id = Guid.NewGuid().ToString();
        public string SystemName = "New External System"; // e.g., "Autodesk ACC", "TopoSurvey_V2"
        public string BaseUrl = "https://developer.api.autodesk.com";

        public AuthConfiguration Authentication = new AuthConfiguration();
        public List<RestEndpoint> Endpoints = new List<RestEndpoint>();
    }

    [Serializable]
    public class AuthConfiguration
    {
        public enum AuthType { None, BearerToken, OAuth2, ApiKey }
        public AuthType Type = AuthType.BearerToken;
        public string TokenOrKey = "";
        public string AuthHeaderName = "Authorization"; // e.g., "Authorization" or "x-api-key"
        public string AuthHeaderPrefix = "Bearer "; // e.g., "Bearer " or ""
    }

    [Serializable]
    public class RestEndpoint
    {
        public string Id = Guid.NewGuid().ToString();
        public string Name = "Get Projects";
        public string Route = "/project/v1/hubs"; // Appended to BaseUrl
        public string Method = "GET";
        public string ExpectedJsonSchema = ""; // Useful for your AI agents to know what to expect!

        public List<Parameter> PathParameters = new List<Parameter>();
        public List<Parameter> QueryParameters = new List<Parameter>();
        public string DefaultPayload = ""; // For POST/PUT
    }

    [Serializable]
    public class Parameter
    {
        public string Key;
        public string DefaultValue;
        public bool IsRequired = true;
    }
}