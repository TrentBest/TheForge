using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;
using Workshop.Core.Memory;
using Workshop.GURPS;
using Workshop.UI_And_Tools.Forge.Builders.GuiBuilders;
using Workshop.UI_And_Tools.Forge.IO;

namespace Assets.Scripts.MastersOfOrionII
{
    public class MastersOfOrionII_Gui_PlanetView : IGuiProvider
    {
        // Change: Planet is now a struct, so we use an 'initialized' flag or a nullable struct
        private Planet? _planet = null;
        private List<Moon> _moons = new List<Moon>();
        private DataWarehouse _warehouse;

        // Change: Resolve the Name through the Warehouse using NameId
        public string Title => _planet.HasValue
            ? $"ORBITAL SCAN: {_warehouse.ResolveString(_planet.Value.NameId).ToUpper()}"
            : "ORBITAL SCAN: NO SIGNAL";

        public void Initialize(Planet p, List<Moon> moons, DataWarehouse warehouse)
        {
            _planet = p;
            _moons = moons ?? new List<Moon>();
            _warehouse = warehouse;
        }

        public VisualElement CreateGui(GuiContext ctx)
        {
            var rootBuilder = new ForgeContainerBuilder("OrbitalConsole_Root")
                .WithFlexGrow(1f)
                .WithDirection(FlexDirection.Row)
                .WithBackgroundColor(new Color(0.01f, 0.02f, 0.05f));

            // Viewport
            rootBuilder.AddChild(new ForgeContainerBuilder("OrbitalViewport")
                .WithFlexGrow(1f)
                .WithBackgroundColor(Color.black)
                .WithBorderWidth(0, 2f, 0, 0)
                .WithBorderColor(Color.cyan));

            // Readout
            var readoutPane = new ForgeContainerBuilder("ReadoutPane").WithWidth(300f).WithPadding(20f);

            readoutPane.AddChild(new ForgeLabelBuilder("ASTROMETRIC READOUT").WithBold().WithColor(Color.cyan).WithMarginBottom(15f));

            // FIX: Use .Value because it's now a Nullable Struct
            if (_planet.HasValue)
            {
                var p = _planet.Value;
                readoutPane.AddChild(CreateStatRow("Mass:", $"{p.RadiusEarth:F2} M⊕"));
                readoutPane.AddChild(CreateStatRow("Gravity:", $"{p.GravityG:F2} G"));
                readoutPane.AddChild(CreateStatRow("Year:", $"{p.OrbitalPeriodDays:F0} Days"));
            }

            rootBuilder.AddChild(readoutPane);
            return rootBuilder.Build();
        }

        private VisualElement CreateStatRow(string label, string value)
        {
            return new ForgeContainerBuilder($"Row_{label}")
                .WithDirection(FlexDirection.Row)
                .WithJustifyContent(Justify.SpaceBetween)
                .AddChild(new ForgeLabelBuilder(label).WithColor(Color.gray))
                .AddChild(new ForgeLabelBuilder(value).WithColor(Color.white).WithBold())
                .Build();
        }

        public Action<VisualElement> GetGuiBuilder() => root => root.Add(CreateGui(new GuiContext()));
        public void ToUIDocument(string path) => WorkshopUxmlBaker.Bake(CreateGui(new GuiContext()), "PlanetView");
        public void FromUIDocument(string path) { }
    }
}