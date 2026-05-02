using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;
using Workshop.UI_And_Tools.Forge.Builders.GuiBuilders;
using Workshop.UI_And_Tools.Forge.IO;

namespace Workshop.GURPS
{
    /// <summary>
    /// Visualizer for GURPS statistical distributions.
    /// Maps 3d6 bell curves and scatter data to the Forge UI.
    /// </summary>
    public class ProbabilityGraphBuilder : IGuiProvider
    {
        private readonly string _title;
        private readonly string _xAxisLabel;
        private readonly string _yAxisLabel;
        private readonly float _maxX;
        private readonly float _maxYProb;

        private float _mu = 10.5f;
        private float _stdDev = 2.5f;
        private Color _curveColor = Color.cyan;

        private VisualElement _graphContainer;
        private VisualElement _hoverPopup;
        private Label _hoverTitle;
        private Label _hoverData;

        public ProbabilityGraphBuilder(
            string title = "Probability Analysis",
            string xAxisLabel = "Value ➔",
            string yAxisLabel = "Prob. (Curve) ➔",
            float maxX = 20f,
            float maxYProb = 0.15f)
        {
            _title = title;
            _xAxisLabel = xAxisLabel;
            _yAxisLabel = yAxisLabel;
            _maxX = maxX;
            _maxYProb = maxYProb;
        }

        public ProbabilityGraphBuilder WithMean(float mu) { _mu = mu; return this; }
        public ProbabilityGraphBuilder WithStandardDeviation(float stdDev) { _stdDev = stdDev; return this; }
        public ProbabilityGraphBuilder WithColor(Color color) { _curveColor = color; return this; }

        public string Title => _title;

        public VisualElement CreateGui(GuiContext ctx)
        {
            // Pillar 8: Context-First Initialization
            if (ctx == null || !ctx.IsValid) return new VisualElement();

            var rootBuilder = new ForgeContainerBuilder("ProbGraph_Root")
                .WithPadding(15f)
                .WithBackgroundColor(new Color(0.02f, 0.02f, 0.04f));

            rootBuilder.AddChild(new ForgeLabelBuilder(_title.ToUpper())
                .WithFontSize(14)
                .WithBold()
                .WithColor(_curveColor)
                .WithMarginBottom(10f));

            rootBuilder.AddChild(new ForgeContainerBuilder("Viewport")
                .WithHeight(180f)
                .WithBackgroundColor(new Color(0.05f, 0.05f, 0.07f))
                .WithBorderWidth(1f)
                .WithBorderColor(new Color(0.3f, 0.3f, 0.3f))
                .OnBuild(ve => {
                    _graphContainer = ve;
                    ve.style.overflow = Overflow.Hidden;
                    ve.Add(new ForgeLabelBuilder(_yAxisLabel).WithFontSize(10).WithColor(Color.gray)
                        .OnBuild(l => { l.style.position = Position.Absolute; l.style.left = 5; l.style.top = 5; }).Build());
                    ve.Add(new ForgeLabelBuilder(_xAxisLabel).WithFontSize(10).WithColor(Color.gray)
                        .OnBuild(l => { l.style.position = Position.Absolute; l.style.right = 5; l.style.bottom = 5; }).Build());
                }));

            _hoverPopup = new ForgeContainerBuilder("Tooltip")
                .WithBackgroundColor(new Color(0.1f, 0.1f, 0.15f, 0.95f))
                .WithPadding(10f)
                .WithBorderWidth(1f)
                .WithBorderColor(_curveColor)
                .WithBorderRadius(5f)
                .OnBuild(ve => {
                    ve.style.position = Position.Absolute;
                    ve.style.display = DisplayStyle.None;
                    ve.pickingMode = PickingMode.Ignore;
                })
                .AddChild(new ForgeLabelBuilder("DATA_KEY")
                    .WithBold().WithColor(Color.white)
                    .OnBuild(l => _hoverTitle = (Label)l))
                .AddChild(new ForgeLabelBuilder("DATA_VAL")
                    .WithFontSize(10).WithColor(Color.yellow)
                    .OnBuild(l => _hoverData = (Label)l))
                .Build();

            rootBuilder.AddChild(_hoverPopup);

            // FIX 1: Subscribing to the correct lifecycle event 'OnBuilt'
            // Note: OnBuilt is Action<VisualElement>, so we accept the parameter
            ctx.OnBuilt += (root) => Redraw(_mu, _stdDev * _stdDev, new List<ScatterDot>());

            return rootBuilder.Build();
        }

        public void Redraw(float mu, float variance, List<ScatterDot> scatterDots)
        {
            if (_graphContainer == null) return;

            _graphContainer.Clear();
            _graphContainer.Add(_hoverPopup);

            float stepX = _maxX / 10f;
            for (float i = 0; i <= _maxX; i += stepX)
            {
                float leftPct = (i / _maxX) * 100f;
                _graphContainer.Add(new ForgeContainerBuilder("Tick")
                    .WithPosition(Position.Absolute)
                    .WithWidth(1f).WithHeight(new StyleLength(Length.Percent(100f)))
                    .WithBackgroundColor(new Color(1f, 1f, 1f, 0.05f))
                    .OnBuild(ve => ve.style.left = Length.Percent(leftPct))
                    .Build());
            }

            if (variance > 0)
            {
                float sigma = Mathf.Sqrt(variance);
                for (int x = 0; x <= (int)_maxX; x++)
                {
                    float exponent = Mathf.Exp(-Mathf.Pow(x - mu, 2) / (2 * variance));
                    float prob = (1f / (sigma * Mathf.Sqrt(2 * Mathf.PI))) * exponent;

                    if (prob > 0.001f)
                    {
                        float leftPct = (x / _maxX) * 100f;
                        float heightPct = (prob / _maxYProb) * 100f;

                        // FIX 2: Downgraded 'with' expression to C# 9.0 compatible Color constructor
                        Color barColor = new Color(_curveColor.r, _curveColor.g, _curveColor.b, 0.4f);

                        var bar = new ForgeContainerBuilder($"Bar_{x}")
                            .WithPosition(Position.Absolute)
                            .WithWidth(4f)
                            .WithHeight(new StyleLength(Length.Percent(heightPct)))
                            .WithBackgroundColor(barColor)
                            .OnBuild(ve => {
                                ve.style.left = Length.Percent(leftPct);
                                ve.style.bottom = 0;
                                ve.RegisterCallback<MouseEnterEvent>(e => ShowTooltip($"Result: {x}", $"Prob: {prob * 100:F1}%", leftPct, heightPct));
                                ve.RegisterCallback<MouseLeaveEvent>(e => _hoverPopup.style.display = DisplayStyle.None);
                            })
                            .Build();
                        _graphContainer.Add(bar);
                    }
                }
            }

            _hoverPopup.BringToFront();
        }

        private void ShowTooltip(string title, string data, float xPct, float yPct)
        {
            _hoverTitle.text = title;
            _hoverData.text = data;
            _hoverPopup.style.display = DisplayStyle.Flex;
            _hoverPopup.style.left = Length.Percent(xPct);
            _hoverPopup.style.bottom = Length.Percent(yPct + 5f);
        }

        public Action<VisualElement> GetGuiBuilder() => root => root.Add(CreateGui(new GuiContext()));
        public void ToUIDocument(string path) => WorkshopUxmlBaker.Bake(CreateGui(new GuiContext()), "ProbGraph_Snapshot");
        public void FromUIDocument(string path) { }
    }
}