using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;
using Workshop.Core.Memory;
using Workshop.UI_And_Tools.Forge.Builders.GuiBuilders;

namespace Workshop.GURPS
{
    [Serializable]
    public class PlotHook { public string Title; public string Hook; public int Difficulty; }

    public class NarrativeAPI : IGurpsApiProvider
    {
        public string ModuleName => "Quest & Narrative Ledger";
        public string Title => "CHRONICLE ENGINE";
        public Guid Id { get; set; } = Guid.NewGuid();

        private List<PlotHook> _hooks = new List<PlotHook>();
        private readonly DataWarehouse _warehouse;

        public NarrativeAPI() { _warehouse = new DataWarehouse(); }
        public NarrativeAPI(DataWarehouse warehouse) => _warehouse = warehouse;

        public void Initialize() => SeedStandardHooks();

        private void SeedStandardHooks()
        {
            _hooks.Add(new PlotHook { Title = "The Broken Anchor", Hook = "AEC Extraction point 42 has gone silent.", Difficulty = 3 });
        }

        public VisualElement CreateGui(GuiContext ctx)
        {
            var gui = new GraphicalUserInterfaceBuilder(ModuleName)
                .WithPadding(15)
                .AddHeader("ACTIVE NARRATIVE THREADS", Color.cyan);

            foreach (var hook in _hooks)
            {
                gui.WithPanel($"Hook_{hook.Title}")
                    .WithBorderWidth(1).WithBorderColor(Color.gray)
                    .AddChild(new ForgeLabelBuilder(hook.Title).WithBold())
                    .AddChild(new ForgeLabelBuilder(hook.Hook).WithFontSize(11))
                    .EndPanel();
            }
            return gui.Build();
        }

        public Action<VisualElement> GetGuiBuilder() => root => root.Add(CreateGui(new GuiContext()));
        public void Bind(DataWarehouse w) { }
        public void SaveToCache() { }
        public void LoadFromCache() { }
        public void ToUIDocument(string p) { }
        public void FromUIDocument(string p) { }
    }
}