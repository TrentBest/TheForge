using Assets.Scripts.MastersOfOrionII.Math;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;
using Workshop.UI_And_Tools.Forge.Builders.GuiBuilders;
using Workshop.UI_And_Tools.Forge.IO;

namespace Assets.Scripts.MastersOfOrionII
{
    public class MastersOfOrionII_Gui_StarSystemView : IGuiProvider
    {
        public string Title => "System Command";

        private int _systemSeed;
        private Star _currentStar;
        private List<Planet> _planets = new List<Planet>();
        private int _selectedPlanetIndex = -1;

        private VisualElement _orbitContainer;
        private VisualElement _detailsPanel;
        private MastersOfOrionII_Game _game;

        private const float AU_SCALE = 60f;

        public void Initialize(int seed)
        {
            _systemSeed = seed;
            _currentStar = SystemFactory.GenerateStar(seed);
            _planets = SystemFactory.GeneratePlanets(_currentStar);
            _selectedPlanetIndex = -1;
        }

        public VisualElement CreateGui(GuiContext ctx)
        {
            _game = UnityEngine.Object.FindAnyObjectByType<MastersOfOrionII_Game>();

            var rootBuilder = new ForgeContainerBuilder("StarSystem_Root")
                .WithDirection(FlexDirection.Row)
                .WithFlexGrow(1f)
                .WithBackgroundColor(Color.black);

            var visPanel = new ForgeContainerBuilder("Visualization")
                .WithFlexGrow(0.65f)
                .WithBackgroundColor(new Color(0f, 0f, 0f, 0.95f))
                .WithBorderWidth(0, 1f, 0, 0)
                .WithBorderColor(new Color(0.3f, 0.3f, 0.4f))
                .OnBuild(ve => ve.style.overflow = Overflow.Hidden)
                .AddChild(new DynamicGuiProvider(c => {
                    var innerRoot = new ForgeContainerBuilder("Orrery_Inner")
                        .WithPosition(Position.Absolute)
                        .WithWidth(new StyleLength(Length.Percent(100f)))
                        .WithHeight(new StyleLength(Length.Percent(100f)));

                    innerRoot.AddChild(CreateGoldilocksZone(_currentStar));

                    var sunSize = 60f + (_currentStar.Mass * 10f);
                    innerRoot.AddChild(new ForgeContainerBuilder("CentralStar")
                        .WithWidth(sunSize).WithHeight(sunSize)
                        .WithBackgroundColor(GetStarColor(_currentStar.Class))
                        .WithPosition(Position.Absolute)
                        .WithBorderRadius(sunSize)
                        .OnBuild(ve => {
                            ve.style.left = Length.Percent(50f);
                            ve.style.top = Length.Percent(50f);
                            ve.style.marginLeft = -sunSize / 2f;
                            ve.style.marginTop = -sunSize / 2f;
                            ve.RegisterCallback<ClickEvent>(evt => SelectObject(-1));
                        }));

                    innerRoot.AddChild(new ForgeContainerBuilder("OrbitContainer")
                        .WithPosition(Position.Absolute)
                        .WithWidth(new StyleLength(Length.Percent(100f)))
                        .WithHeight(new StyleLength(Length.Percent(100f)))
                        .OnBuild(ve => {
                            _orbitContainer = ve;
                            ve.RegisterCallback<GeometryChangedEvent>(evt => RenderOrbits());
                        }));

                    return innerRoot.Build();
                }));

            var detailsPanel = new ForgeContainerBuilder("Inspector")
                .WithWidth(new Length(35f, LengthUnit.Percent))
                .WithBackgroundColor(new Color(0.1f, 0.12f, 0.15f))
                .OnBuild(ve => _detailsPanel = ve);

            rootBuilder.AddChild(visPanel);
            rootBuilder.AddChild(detailsPanel);

            var finalRoot = rootBuilder.Build();
            SelectObject(-1);
            return finalRoot;
        }

        private void RenderOrbits()
        {
            if (_orbitContainer == null || _orbitContainer.resolvedStyle.width < 1f) return;
            _orbitContainer.Clear();

            float centerX = _orbitContainer.resolvedStyle.width / 2f;
            float centerY = _orbitContainer.resolvedStyle.height / 2f;

            for (int i = 0; i < _planets.Count; i++)
            {
                Planet p = _planets[i];
                float distPx = Mathf.Clamp(p.SemiMajorAxisAU * AU_SCALE, 50f, centerX - 40f);

                var ring = new ForgeContainerBuilder($"OrbitRing_{i}")
                    .WithPosition(Position.Absolute)
                    .WithWidth(distPx * 2f).WithHeight(distPx * 2f)
                    .WithBorderWidth(1f) // FIX: Builder handles the 4-side distribution
                    .WithBorderColor(new Color(1f, 1f, 1f, 0.08f))
                    .WithBorderRadius(distPx)
                    .OnBuild(ve => {
                        ve.style.left = centerX - distPx;
                        ve.style.top = centerY - distPx;
                        ve.pickingMode = PickingMode.Ignore;
                    });

                _orbitContainer.Add(ring.Build());

                float size = p.Type == PlanetType.GasGiant ? 20f : 12f;
                Color pColor = GetPlanetColor(p.Type);
                float angle = (p.Seed % 360f) * Mathf.Deg2Rad;
                float px = centerX + Mathf.Cos(angle) * distPx;
                float py = centerY + Mathf.Sin(angle) * distPx;

                var planetBtn = new ForgeButtonBuilder("")
                    .WithBackgroundColor(pColor)
                    .WithBorderRadius(size)
                    .WithWidth(size).WithHeight(size)
                    .OnBuild(ve => {
                        ve.style.position = Position.Absolute;
                        ve.style.left = px - (size / 2f);
                        ve.style.top = py - (size / 2f);

                        // FIX: Explicitly setting all 4 sides because IStyle doesn't support shorthand
                        if (_selectedPlanetIndex == p.SystemIndex)
                        {
                            ve.style.borderTopWidth = ve.style.borderBottomWidth = 2f;
                            ve.style.borderLeftWidth = ve.style.borderRightWidth = 2f;
                            ve.style.borderTopColor = ve.style.borderBottomColor = Color.white;
                            ve.style.borderLeftColor = ve.style.borderRightColor = Color.white;
                        }
                    })
                    .OnClick(() => SelectObject(p.SystemIndex));

                _orbitContainer.Add(planetBtn.Build());
            }
        }

        private void SelectObject(int index)
        {
            _selectedPlanetIndex = index;
            RenderOrbits();

            if (_detailsPanel == null) return;
            _detailsPanel.Clear();

            if (index == -1)
            {
                var starInfo = new ForgeContainerBuilder("StarInfo")
                    .WithPadding(20f)
                    .AddChild(new ForgeLabelBuilder($"STAR: {_currentStar.Class}").WithFontSize(24).WithBold().WithColor(GetStarColor(_currentStar.Class)))
                    .AddChild(new ForgeLabelBuilder($"Luminosity: {_currentStar.Luminosity:F2} Sols").WithMarginTop(10f))
                    .AddChild(new ForgeLabelBuilder($"Mass: {_currentStar.Mass:F2} Sols"))
                    .AddChild(new ForgeLabelBuilder($"Planet Count: {_planets.Count}"));

                _detailsPanel.Add(starInfo.Build());
                return;
            }

            Planet p = _planets.Find(x => x.SystemIndex == index);
            var planetView = new MastersOfOrionII_Gui_PlanetView();
            // Using the game's centralized warehouse
            planetView.Initialize(p, new List<Moon>(), _game.Warehouse);
            _detailsPanel.Add(planetView.CreateGui(new GuiContext()));
        }

        private IGuiProvider CreateGoldilocksZone(Star star)
        {
            float centerDistAU = Mathf.Sqrt(star.Luminosity);
            float widthAU = 0.4f * centerDistAU;
            float pxRadius = centerDistAU * AU_SCALE;
            float pxWidth = widthAU * AU_SCALE;

            return new ForgeContainerBuilder("HabitableZone")
                .WithPosition(Position.Absolute)
                .WithWidth((pxRadius + pxWidth) * 2f)
                .WithHeight((pxRadius + pxWidth) * 2f)
                .WithBorderWidth(pxWidth)
                .WithBorderColor(new Color(0.2f, 1f, 0.4f, 0.15f))
                .WithBorderRadius(pxRadius + pxWidth)
                .OnBuild(ve => {
                    ve.style.left = Length.Percent(50f);
                    ve.style.top = Length.Percent(50f);
                    ve.style.marginLeft = -(pxRadius + pxWidth);
                    ve.style.marginTop = -(pxRadius + pxWidth);
                    ve.pickingMode = PickingMode.Ignore;
                });
        }

        private Color GetStarColor(StarSpectralClass spectralClass) => spectralClass switch
        {
            StarSpectralClass.O => new Color(0.6f, 0.8f, 1f),
            StarSpectralClass.G => new Color(1f, 0.9f, 0.4f),
            StarSpectralClass.M => new Color(1f, 0.4f, 0.4f),
            _ => Color.white
        };

        private Color GetPlanetColor(PlanetType type) => type switch
        {
            PlanetType.GasGiant => new Color(0.85f, 0.7f, 0.5f),
            PlanetType.Terrestrial => new Color(0.3f, 0.7f, 0.3f),
            _ => Color.gray
        };

        public Action<VisualElement> GetGuiBuilder() => root => root.Add(CreateGui(new GuiContext()));
        public void ToUIDocument(string path) => WorkshopUxmlBaker.Bake(CreateGui(new GuiContext()), "StarSystemView");
        public void FromUIDocument(string path) { }
    }
}