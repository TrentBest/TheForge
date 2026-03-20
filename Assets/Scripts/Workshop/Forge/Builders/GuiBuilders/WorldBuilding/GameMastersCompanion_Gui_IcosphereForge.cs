using TheSingularityWorkshop.Forge.Builders.GuiBuilders;
using TheSingularityWorkshop.FSM_API;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

namespace TheSingularityWorkshop.Forge.Builders.GuiBuilders.WorldBuilding
{
    public class GameMastersCompanion_Gui_IcosphereForge : IGuiProvider
    {
        public string Title => "GM COMPANION: HOLOGRAPHIC PLANET FORGE";
        private GuiContext _lastCtx;

        private IcosphereForgeContext _context;
        private VisualElement _previewContainer;

        public VisualElement CreateGui(GuiContext ctx)
        {
            _lastCtx = ctx;

            if (_context == null)
            {
                _context = new IcosphereForgeContext();
                InitializeFSM();
            }

            var splitPanel = new SplitPanelBuilder(sidebarWidth: 350)
                .WithSidebar(CreateSidebar())
                .WithMain(CreateMainViewport());

            var root = splitPanel.CreateGui(ctx);

            root.schedule.Execute(() => {
                FSM_API.FSM_API.Interaction.Update("IcosphereBuilder");
            }).Every(50);

            return root;
        }

        private IGuiProvider CreateSidebar()
        {
            return new GraphicalUserInterfaceBuilder("IcoControls")
                .WithPadding(15).WithBackgroundColor(new Color(0.1f, 0.1f, 0.12f))
                .WithFlexLayout(FlexDirection.Column, Justify.FlexStart, Align.Stretch)

                .AddChild(new Label("PLANETARY MESH GENERATOR") { style = { color = Color.cyan, fontSize = 16, unityFontStyleAndWeight = FontStyle.Bold, marginBottom = 15 } })

                .AddChild(new Label("Subdivision (Resolution):") { style = { color = Color.yellow, marginTop = 10 } })
                .AddChild(new Label("Level 0 = 30 Edges\nLevel 5 = 30,720 Edges!") { style = { color = Color.gray, fontSize = 10, marginBottom = 5 } })
                .AddIntSliderData("Recursion Level", 0, 5, _context.SubdivisionLevel, v => {
                    if (_context.SubdivisionLevel != v)
                    {
                        _context.SubdivisionLevel = v;
                        _context.NeedsGeometryRebuild = true;
                    }
                })

                .AddSeparator()
                .AddChild(new Label("Hologram Settings:") { style = { color = Color.white, marginTop = 5 } })
                .AddSliderData("Line Thickness", 0.001f, 0.05f, _context.LineThickness, v => {
                    if (_context.LineThickness != v)
                    {
                        _context.LineThickness = v;
                        _context.NeedsThicknessUpdate = true;
                    }
                });
        }

        private IGuiProvider CreateMainViewport()
        {
            return new GraphicalUserInterfaceBuilder("IcoViewport")
                .WithBackgroundColor(Color.black)
                .WithFlexGrow(1).WithFlexShrink(1)
                .OnBuild(ve => {
                    _previewContainer = ve;
                    RefreshPreviewViewport();
                });
        }

        private void InitializeFSM()
        {
            if ( !FSM_API.FSM_API.Interaction.Exists("IcosphereFSM"))
            {
                FSM_API.FSM_API.Create.CreateFiniteStateMachine("IcosphereFSM", -1, "IcosphereBuilder")
                    .State("Idle", null, null, null)
                    .State("Rebuilding", null, RebuildMesh, null)
                    // Trigger if EITHER flag is true
                    .Transition("Idle", "Rebuilding", ctx => {
                        var c = (IcosphereForgeContext)ctx;
                        return c.NeedsGeometryRebuild || c.NeedsThicknessUpdate;
                    })
                    // Return to Idle when both are handled
                    .Transition("Rebuilding", "Idle", ctx => {
                        var c = (IcosphereForgeContext)ctx;
                        return !c.NeedsGeometryRebuild && !c.NeedsThicknessUpdate;
                    })
                    .BuildDefinition();
            }

            FSM_API.FSM_API.Create.CreateInstance("IcosphereFSM", _context, "IcosphereBuilder");
        }

        private void RebuildMesh(IStateContext ctx)
        {
            var icoCtx = (IcosphereForgeContext)ctx;

            // SCENARIO 1: We changed the Subdivision slider (Heavy Operation)
            if (icoCtx.NeedsGeometryRebuild)
            {
                if (icoCtx.GeneratedPlanetPrefab != null)
                {
                    UnityEngine.Object.DestroyImmediate(icoCtx.GeneratedPlanetPrefab);
                }

                icoCtx.GeneratedPlanetPrefab = new GameObject($"Temp_Icosphere_Level{icoCtx.SubdivisionLevel}");
                icoCtx.GeneratedPlanetPrefab.hideFlags = HideFlags.HideAndDontSave;
                icoCtx.ActiveLines.Clear(); // Clear the cache

                Mesh rawMesh = IcosphereGenerator.Create(icoCtx.SubdivisionLevel, 1f);
                Vector3[] verts = rawMesh.vertices;
                int[] tris = rawMesh.triangles;

                HashSet<long> uniqueEdges = new HashSet<long>();
                for (int i = 0; i < tris.Length; i += 3)
                {
                    AddUniqueEdge(uniqueEdges, tris[i], tris[i + 1]);
                    AddUniqueEdge(uniqueEdges, tris[i + 1], tris[i + 2]);
                    AddUniqueEdge(uniqueEdges, tris[i + 2], tris[i]);
                }

                Material lineMat = new Material(Shader.Find("Sprites/Default"));
                lineMat.color = Color.cyan;

                foreach (long edge in uniqueEdges)
                {
                    int v1 = (int)(edge >> 32);
                    int v2 = (int)(edge & 0xFFFFFFFF);

                    GameObject edgeObj = new GameObject("Edge");
                    edgeObj.transform.SetParent(icoCtx.GeneratedPlanetPrefab.transform, false);

                    LineRenderer lr = edgeObj.AddComponent<LineRenderer>();
                    lr.sharedMaterial = lineMat;
                    lr.startWidth = icoCtx.LineThickness;
                    lr.endWidth = icoCtx.LineThickness;
                    lr.positionCount = 2;
                    lr.SetPosition(0, verts[v1]);
                    lr.SetPosition(1, verts[v2]);
                    lr.useWorldSpace = false;

                    icoCtx.ActiveLines.Add(lr); // Cache it!
                }

                RefreshPreviewViewport(); // Only recreate the UI viewport if the whole prefab changed
            }
            // SCENARIO 2: We ONLY changed the Thickness slider (Ultra-Fast Operation)
            else if (icoCtx.NeedsThicknessUpdate)
            {
                foreach (var lr in icoCtx.ActiveLines)
                {
                    if (lr != null)
                    {
                        lr.startWidth = icoCtx.LineThickness;
                        lr.endWidth = icoCtx.LineThickness;
                    }
                }
                // We DO NOT call RefreshPreviewViewport() here. The LiveModelPreviewBuilder 
                // is already rotating the prefab, and we just updated the lines in memory!
            }

            // Acknowledge the work is done
            icoCtx.NeedsGeometryRebuild = false;
            icoCtx.NeedsThicknessUpdate = false;
        }

        private void AddUniqueEdge(HashSet<long> edges, int v1, int v2)
        {
            long min = Mathf.Min(v1, v2);
            long max = Mathf.Max(v1, v2);
            long key = (min << 32) | (uint)max;
            edges.Add(key);
        }

        private void RefreshPreviewViewport()
        {
            if (_previewContainer == null) return;
            _previewContainer.Clear();

            if (_context.GeneratedPlanetPrefab == null)
            {
                _previewContainer.Add(new Label("Waiting for generator..") { style = { color = Color.gray, alignSelf = Align.Center, marginTop = 100 } });
                return;
            }

            var previewBuilder = new LiveModelPreviewBuilder(_context.GeneratedPlanetPrefab);

            _previewContainer.Add(new GraphicalUserInterfaceBuilder("PreviewWrapper")
                .WithFlexGrow(1).WithFlexShrink(1)
                .AddChild(previewBuilder)
                .Build());
        }

        public Action<VisualElement> GetGuiBuilder() => (root) => root.Add(CreateGui(new GuiContext()));
        public void ToUIDocument(string assetPath) { }
        public void FromUIDocument(string assetPath) { }
    }
}