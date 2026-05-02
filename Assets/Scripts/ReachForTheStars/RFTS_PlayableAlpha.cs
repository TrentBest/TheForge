using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;
using Workshop.UI_And_Tools.Forge.Builders.GuiBuilders;

namespace Workshop.ReachForTheStars
{
    public class StarSystem
    {
        public int X { get; set; }
        public int Y { get; set; }
        public string Name { get; set; }
        public int OwnerId { get; set; }

        public int Population { get; set; }
        public int MaxPopulation { get; set; }
        public int TechLevel { get; set; } = 1;
        public int LocalFunds { get; set; }

        public int Scouts { get; set; }
        public int Transports { get; set; }
        public int Warships { get; set; }

        public int FocusEnvironment { get; set; } = 25;
        public int FocusTech { get; set; } = 25;
        public int FocusShips { get; set; } = 50;

        public Color StarColor { get; set; }
        public float VisualSize { get; set; }
    }

    public class RftS_Playable_Alpha : IGuiProvider
    {
        public string Title => "RftS: Alpha Sector";

        private const int MAP_SIZE = 20;
        private StarSystem[,] _galaxyMap;
        private List<StarSystem> _activeStars;
        private StarSystem _selectedStar;

        private VisualElement[,] _uiGrid;
        private VisualElement _readoutPanel;
        private GuiContext _ctx;

        private int _globalCredits = 1000;
        private int _turn = 1;

        private readonly Color[] _stellarColors = new Color[] {
            new Color(0.6f, 0.8f, 1f), new Color(1f, 0.9f, 0.6f), new Color(1f, 0.5f, 0.2f), new Color(1f, 0.2f, 0.2f)
        };

        public RftS_Playable_Alpha() { GenerateGalaxy(); }

        private void GenerateGalaxy()
        {
            _galaxyMap = new StarSystem[MAP_SIZE, MAP_SIZE];
            _activeStars = new List<StarSystem>();
            System.Random rng = new System.Random();

            for (int i = 0; i < 25; i++)
            {
                int rx = rng.Next(0, MAP_SIZE);
                int ry = rng.Next(0, MAP_SIZE);

                if (_galaxyMap[rx, ry] == null)
                {
                    var star = new StarSystem
                    {
                        X = rx,
                        Y = ry,
                        Name = $"Sector {rx}-{ry}",
                        OwnerId = 0,
                        Population = rng.Next(10, 50),
                        MaxPopulation = rng.Next(50, 100),
                        StarColor = _stellarColors[rng.Next(0, _stellarColors.Length)],
                        VisualSize = (float)rng.NextDouble() * 12f + 12f
                    };
                    _galaxyMap[rx, ry] = star;
                    _activeStars.Add(star);
                }
            }

            var home = _activeStars[0];
            home.Name = "Terra Nova"; home.OwnerId = 1; home.Population = 100; home.MaxPopulation = 150;
            home.Scouts = 2; home.Transports = 1; home.Warships = 0; home.StarColor = _stellarColors[1]; home.VisualSize = 24f;

            var enemy = _activeStars[1];
            enemy.Name = "Zolarg Prime"; enemy.OwnerId = 2; enemy.Population = 80; enemy.MaxPopulation = 120; enemy.Warships = 5;
        }

        public VisualElement CreateGui(GuiContext ctx)
        {
            _ctx = ctx;
            var root = new GraphicalUserInterfaceBuilder("RftS_GameRoot")
                .WithFlexGrow(1).WithFlexLayout(FlexDirection.Row, Justify.FlexStart, Align.Stretch)
                .WithBackgroundColor(new Color(0.02f, 0.02f, 0.04f)).Build();

            // --- LAYOUT FIX: Map takes all remaining space ---
            var mapContainer = new GraphicalUserInterfaceBuilder("MapContainer")
                .WithFlexGrow(1).WithPadding(20).WithFlexLayout(FlexDirection.Column, Justify.Center, Align.Center).Build();

            var gridEl = new GraphicalUserInterfaceBuilder("StarGrid").WithFlexLayout(FlexDirection.Column).WithBorderColor(new Color(0.1f, 0.1f, 0.2f)).WithBorderWidth(1).Build();
            _uiGrid = new VisualElement[MAP_SIZE, MAP_SIZE];

            for (int y = 0; y < MAP_SIZE; y++)
            {
                var row = new GraphicalUserInterfaceBuilder($"Row_{y}").WithFlexLayout(FlexDirection.Row).Build();
                for (int x = 0; x < MAP_SIZE; x++)
                {
                    // Increased cell size to 35 to expand the galaxy
                    var cell = new GraphicalUserInterfaceBuilder($"Cell_{x}_{y}")
                        .WithWidth(35).WithHeight(35).WithBorderWidth(1).WithBorderColor(new Color(0.05f, 0.05f, 0.1f))
                        .WithFlexLayout(FlexDirection.Column, Justify.Center, Align.Center).Build();

                    int cx = x, cy = y;
                    cell.RegisterCallback<ClickEvent>(ev => OnCellClicked(cx, cy));
                    _uiGrid[x, y] = cell;
                    row.Add(cell);
                }
                gridEl.Add(row);
            }
            mapContainer.Add(gridEl);
            root.Add(mapContainer);

            // --- LAYOUT FIX: Strict Width for the Readout ---
            _readoutPanel = new GraphicalUserInterfaceBuilder("ReadoutPanel")
                .WithWidth(380) // Perfectly fits the text
                .OnBuild(v => v.style.flexShrink = 0) // Prevents it from being squashed
                .WithBackgroundColor(new Color(0.06f, 0.06f, 0.09f))
                .WithBorderLeftColor(new Color(0.3f, 0.5f, 0.8f)).WithBorderLeftWidth(2).WithPadding(20)
                .WithFlexLayout(FlexDirection.Column).Build();

            root.Add(_readoutPanel);
            RefreshMapUI();
            UpdateReadout();
            return root;
        }

        private void OnCellClicked(int x, int y) { _selectedStar = _galaxyMap[x, y]; UpdateReadout(); RefreshMapUI(); }

        private void RefreshMapUI()
        {
            for (int y = 0; y < MAP_SIZE; y++)
            {
                for (int x = 0; x < MAP_SIZE; x++)
                {
                    var cell = _uiGrid[x, y];
                    var star = _galaxyMap[x, y];
                    cell.Clear(); cell.style.backgroundColor = Color.clear;

                    if (star != null)
                    {
                        Color glowColor = Color.clear;
                        if (star.OwnerId == 1) glowColor = new Color(0f, 1f, 1f, 0.3f);
                        else if (star.OwnerId == 2) glowColor = new Color(1f, 0f, 0f, 0.3f);

                        var starContainer = new GraphicalUserInterfaceBuilder($"StarAura_{x}_{y}")
                            .WithWidth(30).WithHeight(30).WithBackgroundColor(glowColor).WithFlexLayout(FlexDirection.Column, Justify.Center, Align.Center)
                            .OnBuild(v => { v.style.borderTopLeftRadius = Length.Percent(50); v.style.borderTopRightRadius = Length.Percent(50); v.style.borderBottomLeftRadius = Length.Percent(50); v.style.borderBottomRightRadius = Length.Percent(50); }).Build();

                        var starCore = new GraphicalUserInterfaceBuilder($"StarCore_{x}_{y}")
                            .WithWidth(star.VisualSize).WithHeight(star.VisualSize).WithBackgroundColor(star.StarColor)
                            .OnBuild(v => { v.style.borderTopLeftRadius = Length.Percent(50); v.style.borderTopRightRadius = Length.Percent(50); v.style.borderBottomLeftRadius = Length.Percent(50); v.style.borderBottomRightRadius = Length.Percent(50); }).Build();

                        starContainer.Add(starCore); cell.Add(starContainer);
                        if (_selectedStar == star) cell.style.backgroundColor = new Color(1f, 1f, 1f, 0.15f);
                    }
                }
            }
        }

        private void UpdateReadout()
        {
            _readoutPanel.Clear();
            _readoutPanel.Add(new ForgeLabelBuilder($"TURN {_turn} | TREASURY: {_globalCredits} CR").WithFontSize(18).WithColor(Color.yellow).WithFontStyle(FontStyle.Bold).WithMarginBottom(20).Build());

            if (_selectedStar == null)
            {
                _readoutPanel.Add(new ForgeLabelBuilder("No Star System Selected. Endless Void.").WithColor(Color.gray).Build());
                AddEndTurnButton(); return;
            }

            _readoutPanel.Add(new ForgeLabelBuilder($"SYSTEM: {_selectedStar.Name.ToUpper()}").WithFontSize(24).WithColor(Color.white).WithFontStyle(FontStyle.Bold).Build());
            string owner = _selectedStar.OwnerId == 1 ? "PLAYER EMPIRE" : (_selectedStar.OwnerId == 0 ? "UNCLAIMED" : "HOSTILE");
            Color ownerCol = _selectedStar.OwnerId == 1 ? Color.cyan : (_selectedStar.OwnerId == 0 ? Color.gray : Color.red);
            _readoutPanel.Add(new ForgeLabelBuilder(owner).WithFontSize(14).WithColor(ownerCol).WithMarginBottom(20).Build());

            _readoutPanel.Add(new ForgeLabelBuilder("DEMOGRAPHICS & FLEET").WithFontSize(16).WithColor(new Color(0.8f, 0.5f, 0.2f)).WithMarginBottom(10).Build());
            _readoutPanel.Add(CreateStatBar("POPULATION", _selectedStar.Population, _selectedStar.MaxPopulation, Color.green));
            _readoutPanel.Add(new ForgeLabelBuilder($"TECH LEVEL: {_selectedStar.TechLevel}").WithColor(Color.white).WithMarginBottom(10).Build());
            _readoutPanel.Add(new ForgeLabelBuilder($"Scouts: {_selectedStar.Scouts} | Transports: {_selectedStar.Transports} | Warships: {_selectedStar.Warships}").WithColor(Color.cyan).WithMarginBottom(20).Build());

            if (_selectedStar.OwnerId == 1)
            {
                _readoutPanel.Add(new ForgeLabelBuilder("FUNDS ALLOCATION (%)").WithFontSize(16).WithColor(Color.cyan).WithMarginBottom(10).Build());
                _readoutPanel.Add(CreateAllocationControl("ENVIRONMENT (Boosts Pop Cap)", _selectedStar.FocusEnvironment, val => AdjustFocus(ref _selectedStar, 0, val)));
                _readoutPanel.Add(CreateAllocationControl("TECHNOLOGY (Upgrades Combat)", _selectedStar.FocusTech, val => AdjustFocus(ref _selectedStar, 1, val)));
                _readoutPanel.Add(CreateAllocationControl("SHIPYARDS (Builds Warships)", _selectedStar.FocusShips, val => AdjustFocus(ref _selectedStar, 2, val)));
            }
            AddEndTurnButton();
        }

        private void AddEndTurnButton()
        {
            _readoutPanel.Add(new ForgeButtonBuilder("END TURN (SIMULATE 1 YEAR)")
                .WithMarginTop(40).WithHeight(45).WithBackgroundColor(new Color(0.2f, 0.4f, 0.2f)).WithTextColor(Color.white).WithFontStyle(FontStyle.Bold)
                .OnClick(SimulateTurn).CreateGui(_ctx));
        }

        private void AdjustFocus(ref StarSystem star, int index, int delta)
        {
            int[] focuses = new int[] { star.FocusEnvironment, star.FocusTech, star.FocusShips };
            if (focuses[index] + delta >= 0 && focuses[index] + delta <= 100)
            {
                int other1 = (index + 1) % 3; int other2 = (index + 2) % 3;
                if (delta > 0)
                {
                    if (focuses[other1] > 0) focuses[other1] -= delta;
                    else if (focuses[other2] > 0) focuses[other2] -= delta;
                    else return;
                }
                else
                {
                    if (focuses[other1] > focuses[other2]) focuses[other1] -= delta;
                    else focuses[other2] -= delta;
                }
                focuses[index] += delta; star.FocusEnvironment = focuses[0]; star.FocusTech = focuses[1]; star.FocusShips = focuses[2];
                UpdateReadout();
            }
        }

        private VisualElement CreateAllocationControl(string label, int currentValue, Action<int> onAdjust)
        {
            var container = new GraphicalUserInterfaceBuilder("AllocContainer").WithFlexLayout(FlexDirection.Row, Justify.SpaceBetween, Align.Center).WithMarginBottom(5).Build();
            container.Add(new ForgeLabelBuilder($"{label}: {currentValue}%").WithFontSize(11).WithColor(Color.white).Build());
            var buttonGroup = new GraphicalUserInterfaceBuilder("BtnGroup").WithFlexLayout(FlexDirection.Row).Build();
            buttonGroup.Add(new ForgeButtonBuilder(" - ").WithWidth(30).WithBackgroundColor(new Color(0.3f, 0.1f, 0.1f)).WithTextColor(Color.white).OnClick(() => onAdjust(-5)).CreateGui(_ctx));
            buttonGroup.Add(new ForgeButtonBuilder(" + ").WithWidth(30).WithBackgroundColor(new Color(0.1f, 0.3f, 0.1f)).WithTextColor(Color.white).WithMarginLeft(5).OnClick(() => onAdjust(5)).CreateGui(_ctx));
            container.Add(buttonGroup); return container;
        }

        private VisualElement CreateStatBar(string label, int value, int max, Color col)
        {
            var container = new GraphicalUserInterfaceBuilder("StatContainer").WithMarginBottom(10).Build();
            container.Add(new ForgeLabelBuilder($"{label}: {value}/{max}").WithColor(Color.white).WithFontSize(12).Build());
            var fillAmt = Mathf.Clamp01((float)value / max) * 100f;
            Color barColor = value > max ? Color.red : col;
            var barBg = new GraphicalUserInterfaceBuilder("BarBackground").WithHeight(10).WithBackgroundColor(new Color(0.2f, 0.2f, 0.2f)).WithMarginTop(2).OnBuild(v => v.style.width = Length.Percent(100)).Build();
            var barFill = new GraphicalUserInterfaceBuilder("BarFill").WithHeight(10).WithBackgroundColor(barColor).OnBuild(v => v.style.width = Length.Percent(fillAmt)).Build();
            barBg.Add(barFill); container.Add(barBg); return container;
        }

        private void SimulateTurn()
        {
            foreach (var star in _activeStars)
            {
                if (star.OwnerId == 1)
                {
                    _globalCredits += star.Population * 2;
                    if (star.Population > star.MaxPopulation) _globalCredits -= (star.Population - star.MaxPopulation) * 10;
                }
            }

            foreach (var star in _activeStars)
            {
                if (star.OwnerId == 1 && _globalCredits > 0)
                {
                    int localBudget = Mathf.Min(_globalCredits, 50); _globalCredits -= localBudget;
                    int envSpend = (int)(localBudget * (star.FocusEnvironment / 100f));
                    int techSpend = (int)(localBudget * (star.FocusTech / 100f));
                    int shipSpend = (int)(localBudget * (star.FocusShips / 100f));

                    if (envSpend > 10) star.MaxPopulation += 2;
                    if (techSpend > 20 && UnityEngine.Random.value > 0.8f) star.TechLevel++;
                    if (shipSpend >= 25) star.Warships++;
                    star.Population += Mathf.CeilToInt(star.Population * 0.05f);
                }
            }
            _turn++; UpdateReadout(); RefreshMapUI();
        }

        public Action<VisualElement> GetGuiBuilder() => root => root.Add(CreateGui(new GuiContext()));
        public void ToUIDocument(string assetPath) { }
        public void FromUIDocument(string assetPath) { }
    }
}