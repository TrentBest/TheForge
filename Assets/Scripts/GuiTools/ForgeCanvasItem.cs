using System;
using UnityEngine;
using UnityEngine.UIElements;

namespace Workshop.UI_And_Tools.Forge.Builders.GuiBuilders.Tools
{
    public class ForgeCanvasItem
    {
        public string Id = Guid.NewGuid().ToString();
        public string Name = "New Element";
        public string ElementType = "Container"; // Container, Button, Label

        // Layout & Styling
        public float Width = 200f;
        public float Height = 100f;
        public float FlexGrow = 0f;
        public Color BgColor = new Color(0.15f, 0.15f, 0.18f);
        public FlexDirection FlowDirection = FlexDirection.Column;

        // Sensory Bindings
        public string BoundEffectId = "None";
        public string BoundClickSfx = "None";

        // REST Bindings
        public string BoundEndpoint = "None";
        public string MockPayload = "{\n  \"status\": \"success\"\n}";
    }
}