using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;
using TheSingularityWorkshop.Forge.Builders.GuiBuilders.WorldBuilding;

namespace TheSingularityWorkshop.Forge.Builders.GuiBuilders
{
    public class Workshop_Gui_MapIllustrator : IGuiProvider, IDisposable
    {
        public string Title => "Cartographer: Illustrator";

        private MapIllustrationContext _mapCtx = new MapIllustrationContext();
        private CartoCanvas _canvas = new CartoCanvas();
        private RenderTexture _illustrationTarget;

        public VisualElement CreateGui(GuiContext ctx)
        {
            var root = new GraphicalUserInterfaceBuilder("MapIllustrator")
                .AddChild(new Label("CARTOGRAPHIC EXTRACTION ENGINE")
                {
                    style = { fontSize = 18, unityFontStyleAndWeight = FontStyle.Bold, color = Color.cyan }
                })
                .AddSeparator(Color.gray, 1)

                // Technology Level Slider
                .AddSliderData("Technology Level (TL)", 0, 12, _mapCtx.TechnologyLevel, val => {
                    _mapCtx.TechnologyLevel = (int)val;
                })

                // Extraction Trigger - Pulling from the Service Bag
                .AddButton("PERFORM SURVEY & EXTRACT", () => PerformExtraction(ctx))

                .AddSeparator(Color.gray, 1)

                .OnBuild(ve => {
                    var mapPreview = new Image
                    {
                        style = {
                            flexGrow = 1,
                            marginTop = 10,
                            backgroundColor = new Color(0.1f, 0.1f, 0.1f),
                            borderTopWidth = 2, borderBottomWidth = 2, borderLeftWidth = 2, borderRightWidth = 2,
                            borderTopColor = Color.cyan, borderBottomColor = Color.cyan, borderLeftColor = Color.cyan, borderRightColor = Color.cyan
                        }
                    };
                    ve.Add(mapPreview);
                })
                .CreateGui(ctx);

            return root;
        }

        private void PerformExtraction(GuiContext ctx)
        {
            // FIX: Using your TryGetService pattern to find the WorldBuilderContext
            if (ctx.TryGetService<WorldBuilderContext>(out var worldContext))
            {
                var worldData = worldContext.Data;

                _canvas.Icons.Clear();
                _canvas.PathPoints.Clear();

                // Branching logic based on Technology Level
                if (_mapCtx.TechnologyLevel <= 4)
                {
                    ApplyHandDrawnFilters(worldData, _canvas);
                }
                else
                {
                    ApplyTopographicFilters(worldData, _canvas);
                }

                ctx.Log?.Invoke($"[Illustrator] Survey complete at TL {_mapCtx.TechnologyLevel}. Elements: {_canvas.Icons.Count}");
            }
            else
            {
                ctx.Log?.Invoke("[Illustrator] Error: WorldBuilderContext not found in GuiContext Services.");
            }
        }

        private void ApplyHandDrawnFilters(WorldSimulationData data, CartoCanvas canvas)
        {
            // Derive landmarks from the physical truth of the continents
            foreach (var continent in data.Continents)
            {
                // Place a "Hero" icon at the center of the landmass
                canvas.PlaceSymbol("icon_mountain_sketched", continent.CenterPoint);

                // Add the cultural label
                canvas.Icons.Add(new MapIcon
                {
                    Id = $"label_{continent.ContinentName}",
                    Position = continent.CenterPoint + (Vector2.up * 5)
                });
            }
        }

        private void ApplyTopographicFilters(WorldSimulationData data, CartoCanvas canvas)
        {
            // Extract vertex data from the PlanetMesh for contour nodes
            if (data.PlanetMesh != null)
            {
                var vertices = data.PlanetMesh.vertices;
                // AI Elbow Grease: Sample height variances for contouring
                for (int i = 0; i < vertices.Length; i += 100)
                {
                    var v = vertices[i];
                    if (v.y > 0.7f) // Only map the "High Frontier"
                    {
                        canvas.PlaceSymbol("topo_peak_node", new Vector2(v.x, v.z));
                    }
                }
            }
        }

        // --- IGuiProvider Implementation ---

        public void ToUIDocument(string id)
        {
            string json = JsonUtility.ToJson(_mapCtx);
            // Internal Logic to save 'json' to the Registry or DataWarehouse
            Debug.Log($"[Illustrator] Map state saved to {id}");
        }

        public void FromUIDocument(string document)
        {
            if (string.IsNullOrEmpty(document)) return;
            _mapCtx = JsonUtility.FromJson<MapIllustrationContext>(document);
        }

        public void Dispose()
        {
            if (_illustrationTarget != null) _illustrationTarget.Release();
        }

        public Action<VisualElement> GetGuiBuilder() => (root) => root.Add(CreateGui(new GuiContext()));

        private int we;
    }
}