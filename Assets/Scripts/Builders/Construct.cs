using Assets.Scripts.MastersOfOrionII;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;
using Workshop.UI_And_Tools.Forge.Builders;
using Workshop.UI_And_Tools.Forge.Builders.GuiBuilders;

namespace Assets.Scripts.Asteroids
{
    // --- RUNTIME DATA INSTANCE ---
    public class Construct
    {
        public string ClassificationId { get; private set; }
        // Universe Scale Logic: Addresses the 'where' in infinite space
        public UniversePosition Position;

        private Dictionary<int, string> _data;

        public Construct(string classificationId, Dictionary<int, string> data)
        {
            this.ClassificationId = classificationId;
            this._data = data ?? new Dictionary<int, string>();
        }

        static public ConstructBuilder Create(string constructClassification)
        {
            return new ConstructBuilder(constructClassification);
        }
    }

    // --- INSTANCE BUILDER ---
    public class ConstructBuilder : IForgeBuilder, IGuiProvider
    {
        private string _classificationTarget;
        public string Name => $"Construct_Builder_{_classificationTarget}";
        public string ToolName => "Construct Instance Spawner";
        public Type GetProductType() => typeof(Construct);

        public ConstructBuilder(string constructClassification) { _classificationTarget = constructClassification; }

        public IGuiProvider GetGuiProvider() => this;

        public object Build()
        {
            Debug.Log($"[ConstructBuilder] Spawning instance of {_classificationTarget} in sector zero.");
            return new Construct(_classificationTarget, new Dictionary<int, string>());
        }

        // --- GUI GENERATION (Forge Protocol) ---
        public string Title => ToolName;

        public VisualElement CreateGui(GuiContext ctx)
        {
            var rootBuilder = new ForgeContainerBuilder("HeadlessBuilderWarning")
                .WithPadding(20f)
                .WithBackgroundColor(new Color(0.12f, 0.12f, 0.15f))
                .AddChild(new ForgeLabelBuilder("HEADLESS API COMPONENT").WithColor(new Color(0.9f, 0.6f, 0.2f)).WithBold().WithFontSize(16))
                .AddChild(new ForgeLabelBuilder($"Spawning blueprint: '{_classificationTarget}'").WithColor(Color.gray).WithMarginBottom(20f))
                .AddChild(new ForgeButtonBuilder($"⚙️ Force Spawn Instance")
                    .WithBackgroundColor(new Color(0.2f, 0.5f, 0.8f)).WithPadding(10f)
                    .OnClick(() => Build()));

            return rootBuilder.Build();
        }

        public Action<VisualElement> GetGuiBuilder() => root => root.Add(CreateGui(new GuiContext()));
        public void ToUIDocument(string path) => WorkshopUxmlBaker.Bake(CreateGui(new GuiContext()), "SpawnerSnapshot");
        public void FromUIDocument(string path) { }
    }
}