using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

namespace TheSingularityWorkshop.Builders.GuiBuilders
{
    public struct ScatterDot
    {
        public float X_Value;
        public float Y_Percent; // 0 to 100 (Height on the graph)
        public Color DotColor;
        public string TooltipTitle;
        public string TooltipData;
    }

    public class ProbabilityGraphBuilder
    {
        private string _title;
        private string _xAxisLabel;
        private string _yAxisLabel;
        private float _maxX;
        private float _maxYProb;

        private VisualElement _graphContainer;
        private VisualElement _hoverPopup;
        private Label _hoverTitle;
        private Label _hoverData;

        public ProbabilityGraphBuilder(
            string title,
            string xAxisLabel = "Value ➔",
            string yAxisLabel = "Category (Dots) / Prob. (Curve) ➔",
            float maxX = 100f,
            float maxYProb = 0.20f)
        {
            _title = title;
            _xAxisLabel = xAxisLabel;
            _yAxisLabel = yAxisLabel;
            _maxX = maxX;
            _maxYProb = maxYProb;
        }

        public VisualElement Build()
        {
            var root = new VisualElement();

            if (!string.IsNullOrEmpty(_title))
            {
                root.Add(new Label(_title) { style = { color = Color.cyan, marginTop = 15, marginBottom = 5, unityFontStyleAndWeight = FontStyle.Bold } });
            }

            _graphContainer = new VisualElement
            {
                style = {
                    height = 180, backgroundColor = new Color(0.05f, 0.05f, 0.07f),
                    borderTopWidth = 1, borderRightWidth = 1, borderLeftWidth = 1, borderBottomWidth = 1, borderTopColor = Color.gray, 
                    borderRightColor = Color.gray, borderLeftColor=Color.gray, borderBottomColor = Color.gray, marginBottom = 5,
                    overflow = Overflow.Hidden
                }
            };
            root.Add(_graphContainer);

            // --- CUSTOM HOVER TOOLTIP ---
            _hoverPopup = new VisualElement
            {
                pickingMode = PickingMode.Ignore,
                style = {
                    position = Position.Absolute, display = DisplayStyle.None,
                    backgroundColor = new Color(0.1f, 0.1f, 0.15f, 0.95f),
                    borderTopWidth = 1, borderBottomWidth = 1, borderLeftWidth =1, borderRightWidth =1, borderTopColor = Color.cyan,borderBottomColor = Color.cyan,borderLeftColor = Color.cyan,borderRightColor = Color.cyan,
                    paddingTop = 5, paddingBottom = 5, paddingLeft = 10, paddingRight = 10,
                    borderTopLeftRadius = 5, borderTopRightRadius = 5, borderBottomRightRadius = 5
                }
            };
            _hoverTitle = new Label() { style = { color = Color.white, unityFontStyleAndWeight = FontStyle.Bold } };
            _hoverData = new Label() { style = { color = Color.yellow, fontSize = 10 } };
            _hoverPopup.Add(_hoverTitle);
            _hoverPopup.Add(_hoverData);

            return root;
        }

        /// <summary>
        /// Clears and redraws the graph with the provided statistical data.
        /// </summary>
        /// <param name="mu">The Mean (Average Expected Value)</param>
        /// <param name="variance">The Variance (Spread of the curve)</param>
        /// <param name="scatterDots">List of data points to plot as reference dots</param>
        public void Redraw(float mu, float variance, List<ScatterDot> scatterDots)
        {
            _graphContainer.Clear();

            // 1. Re-add Axis Labels & Popup
            _graphContainer.Add(new Label(_yAxisLabel) { style = { position = Position.Absolute, left = 5, top = 5, color = new Color(0.5f, 0.5f, 0.5f), fontSize = 10 } });
            _graphContainer.Add(new Label(_xAxisLabel) { style = { position = Position.Absolute, right = 5, bottom = 5, color = new Color(0.5f, 0.5f, 0.5f), fontSize = 10 } });
            _graphContainer.Add(_hoverPopup);

            // 2. Draw Vertical Grid (Ticks every 10% of MaxX)
            float stepX = _maxX / 10f;
            for (float i = stepX; i <= _maxX; i += stepX)
            {
                float leftPct = (i / _maxX) * 100f;
                var tick = new VisualElement { style = { position = Position.Absolute, left = Length.Percent(leftPct), bottom = 0, top = 0, width = 1, backgroundColor = new Color(0.2f, 0.2f, 0.2f) } };
                _graphContainer.Add(tick);
            }

            // 3. Draw Horizontal Grid (Faint Magenta at 25%, 50%, 75%, 100%)
            for (int i = 25; i <= 100; i += 25)
            {
                var hLine = new VisualElement { style = { position = Position.Absolute, left = 0, right = 0, bottom = Length.Percent(i), height = 1, backgroundColor = new Color(0.6f, 0.1f, 0.6f, 0.3f) } };
                _graphContainer.Add(hLine);
            }

            // 4. Scatter Plot: Background Data
            if (scatterDots != null)
            {
                foreach (var dotData in scatterDots)
                {
                    if (dotData.X_Value > 0 && dotData.X_Value <= _maxX)
                    {
                        float leftPct = (dotData.X_Value / _maxX) * 100f;
                        float bottomPct = Mathf.Clamp(dotData.Y_Percent, 5f, 95f);

                        var dot = new VisualElement
                        {
                            style = {
                                position = Position.Absolute, left = Length.Percent(leftPct), bottom = Length.Percent(bottomPct),
                                width = 8, height = 8, backgroundColor = dotData.DotColor,
                                borderTopLeftRadius = 4, borderTopRightRadius = 4, borderBottomLeftRadius = 4, borderBottomRightRadius = 4,
                                borderTopWidth = 1, borderTopColor = Color.black,
                                borderBottomWidth = 1, borderBottomColor = Color.black,
                                borderLeftWidth = 1, borderLeftColor = Color.black,
                                borderRightWidth = 1, borderRightColor = Color.black,
                            }
                        };

                        dot.RegisterCallback<MouseEnterEvent>(e => {
                            _hoverPopup.style.display = DisplayStyle.Flex;
                            _hoverTitle.text = dotData.TooltipTitle;
                            _hoverData.text = dotData.TooltipData;
                            _hoverPopup.style.left = Length.Percent(leftPct);
                            _hoverPopup.style.bottom = Length.Percent(bottomPct + 5f);
                            dot.style.backgroundColor = Color.cyan;
                        });
                        dot.RegisterCallback<MouseLeaveEvent>(e => {
                            _hoverPopup.style.display = DisplayStyle.None;
                            dot.style.backgroundColor = dotData.DotColor;
                        });

                        _graphContainer.Add(dot);
                    }
                }
            }

            // 5. Bell Curve: Normal Distribution PMF
            if (variance > 0)
            {
                float sigma = Mathf.Sqrt(variance);
                int minX = Mathf.Max(1, Mathf.FloorToInt(mu - (sigma * 3)));
                int maxX = Mathf.Min((int)_maxX, Mathf.CeilToInt(mu + (sigma * 3)));

                for (int x = minX; x <= maxX; x++)
                {
                    float exponent = Mathf.Exp(-Mathf.Pow(x - mu, 2) / (2 * variance));
                    float prob = (1f / (sigma * Mathf.Sqrt(2 * Mathf.PI))) * exponent;

                    if (prob > 0.005f)
                    {
                        float leftPct = (x / _maxX) * 100f;
                        float heightPct = Mathf.Clamp01(prob / _maxYProb) * 100f;

                        var bar = new VisualElement
                        {
                            style = {
                                position = Position.Absolute, left = Length.Percent(leftPct), bottom = 0,
                                width = 4, height = Length.Percent(heightPct), backgroundColor = new Color(0, 1f, 1f, 0.5f)
                            }
                        };

                        bar.RegisterCallback<MouseEnterEvent>(e => {
                            _hoverPopup.style.display = DisplayStyle.Flex;
                            _hoverTitle.text = $"Value: {x}";
                            _hoverData.text = $"Probability: {(prob * 100):F1}%";
                            _hoverPopup.style.left = Length.Percent(leftPct);
                            _hoverPopup.style.bottom = Length.Percent(heightPct);
                            bar.style.backgroundColor = new Color(1f, 1f, 1f, 0.9f);
                        });
                        bar.RegisterCallback<MouseLeaveEvent>(e => {
                            _hoverPopup.style.display = DisplayStyle.None;
                            bar.style.backgroundColor = new Color(0, 1f, 1f, 0.5f);
                        });

                        _graphContainer.Add(bar);
                    }
                }
            }

            _hoverPopup.BringToFront();
        }
    }
}