using System;
using UnityEngine;
using UnityEngine.UIElements;
using Workshop.UI_And_Tools.Forge.Builders.GuiBuilders;
using TheSingularityWorkshop.FSM_API;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Workshop.UI_And_Tools.Forge.Builders.GuiBuilders.Tools
{
    [ForgeInspector(typeof(DiegeticCartographerContext))]
    public class Workshop_Gui_DiegeticCartographer : IGuiProvider
    {
        public string Title => "X-Ray: Diegetic Cartographer";

        private DiegeticCartographerContext _context;
        private VisualElement _rightPanel;
        private VisualElement _scrollList;

        public Action<VisualElement> GetGuiBuilder() => root => root.Add(CreateGui(new GuiContext()));
        public void ToUIDocument(string assetPath) { }
        public void FromUIDocument(string assetPath) { }

        public VisualElement CreateGui(GuiContext ctx)
        {
            // [RULE 8] Context-First Initialization
            _context = new DiegeticCartographerContext();
            if (!_context.IsValid)
            {
                return new ForgeLabelBuilder("CRITICAL: Context Invalid. Awaiting Initialization...").WithColor(Color.red).Build();
            }

            var splitView = new ForgeSplitPanelBuilder(280, Side.Left);

            // --- LEFT SIDEBAR: THE MATRIX SCANNER ---
            var sidebar = new ForgeContainerBuilder("Cartographer_Sidebar")
                .WithPadding(10)
                .WithBackgroundColor(new Color(0.1f, 0.1f, 0.12f, 1f));

            sidebar.AddChild(new ForgeLabelBuilder("DIEGETIC MATRIX")
                .WithColor(new Color(0.0f, 0.9f, 1.0f)) // Cyan
                .WithFontStyle(FontStyle.Bold)
                .WithMarginBottom(10));

            sidebar.AddChild(new ForgeButtonBuilder("📡 Ping Ecosystem", () => RefreshScanner(ctx))
                .WithBackgroundColor(new Color(0.64f, 0.17f, 0.77f)) // Forge Purple
                .WithTextColor(Color.white)
                .WithMarginBottom(15));

            _scrollList = new ForgeScrollViewBuilder("EntityList").WithFlexGrow(1).Build();
            sidebar.AddChild(_scrollList);

            splitView.WithSidebar(sidebar);

            // --- RIGHT MAIN PANEL: TELEMETRY & CONTROL ---
            var mainArea = new ForgeContainerBuilder("Cartographer_Main")
                .WithPadding(20)
                .WithBackgroundColor(new Color(0.05f, 0.05f, 0.08f, 1f));

            _rightPanel = mainArea.Build();
            RenderEmptyState();

            // FIX: Using your SetRightPane wrapper which handles the DynamicGuiProvider for you!
            splitView.SetRightPane(_rightPanel);

            var root = splitView.CreateGui(ctx);

            // [RULE 4] Graceful UI Toolkit Scheduling for background telemetry ticking
            // Evaluates entity destruction safely without relying on MonoBehaviour.Update
            root.schedule.Execute(() =>
            {
                if (_context.SelectedEntity == null && _rightPanel.childCount > 0 && _context.SelectedType != null)
                {
                    RenderEmptyState();
                }
            }).Every(1000); // 1Hz safety polling

            RefreshScanner(ctx);

            return root;
        }

        private void RefreshScanner(GuiContext ctx)
        {
            _scrollList.Clear();

            // Scan for Terminals (InWorldGuiBuilders acting as OS Nodes)
            var terminals = GameObject.FindObjectsOfType<InWorldGuiBuilder>();
            if (terminals.Length > 0)
            {
                _scrollList.Add(new ForgeLabelBuilder("TERMINALS (OS Nodes)")
                    .WithColor(Color.gray).WithMarginTop(10).WithMarginBottom(5).Build());

                foreach (var t in terminals)
                {
                    _scrollList.Add(BuildListButton(t.Name ?? t.gameObject.name, "Terminal", t.transform, ctx));
                }
            }

            // Scan for Portals (Diegetic Spatial Transitions)
            var portals = GameObject.FindObjectsOfType<DiegeticPortalTrigger>();
            if (portals.Length > 0)
            {
                _scrollList.Add(new ForgeLabelBuilder("PORTALS (Spatial Transitions)")
                    .WithColor(Color.gray).WithMarginTop(10).WithMarginBottom(5).Build());

                foreach (var p in portals)
                {
                    _scrollList.Add(BuildListButton(p.gameObject.name, "Portal", p.transform, ctx));
                }
            }

            if (terminals.Length == 0 && portals.Length == 0)
            {
                _scrollList.Add(new ForgeLabelBuilder("No diegetic entities detected in the current reality.")
                    .WithColor(new Color(0.8f, 0.2f, 0.2f)).WithFontStyle(FontStyle.Italic).Build());
            }
        }

        private VisualElement BuildListButton(string displayName, string type, Transform target, GuiContext ctx)
        {
            return new ForgeButtonBuilder($"[ {type.Substring(0, 4).ToUpper()} ] {displayName}", () =>
            {
                _context.SelectedEntity = target;
                _context.SelectedType = type;
                RenderTelemetry(displayName, target, ctx);
            })
            .WithBackgroundColor(new Color(0.15f, 0.15f, 0.18f))
            .WithTextColor(Color.white)
            .WithMarginBottom(5)
            .Build();
        }

        private void RenderEmptyState()
        {
            _rightPanel.Clear();
            _context.SelectedType = null;
            _rightPanel.Add(new ForgeLabelBuilder("Select an entity from the matrix to view X-Ray telemetry.")
                .WithColor(Color.gray)
                .WithFontStyle(FontStyle.Italic)
                .Build());
        }

        private void RenderTelemetry(string displayName, Transform target, GuiContext ctx)
        {
            _rightPanel.Clear();

            // Header
            _rightPanel.Add(new ForgeLabelBuilder($"Telemetry: {displayName}")
                .WithColor(new Color(0.64f, 0.17f, 0.77f))
                .WithFontSize(22)
                .WithFontStyle(FontStyle.Bold)
                .WithMarginBottom(20)
                .Build());

            // Positional & Structural Data
            var dataBox = new ForgeContainerBuilder("DataBox")
                .WithBackgroundColor(new Color(0.1f, 0.1f, 0.12f))
                .WithPadding(15)
                .WithBorderRadius(5)
                .WithMarginBottom(20);

            dataBox.AddChild(new ForgeLabelBuilder($"Entity Protocol: {_context.SelectedType}").WithColor(Color.white).WithMarginBottom(5));
            dataBox.AddChild(new ForgeLabelBuilder($"World Position: {target.position}").WithColor(Color.white).WithMarginBottom(5));
            dataBox.AddChild(new ForgeLabelBuilder($"Rotation Matrix: {target.rotation.eulerAngles}").WithColor(Color.white));

            _rightPanel.Add(dataBox.Build());

            // Tactical Actions
            var actionsRow = new ForgeContainerBuilder("ActionsRow").WithFlexLayout(FlexDirection.Row, Justify.FlexStart, Align.Stretch).Build();

            actionsRow.Add(new ForgeButtonBuilder("👁 Focus Camera", () => FocusOnEntity(target))
                .WithBackgroundColor(new Color(0.0f, 0.6f, 0.8f)) // Forge Action Blue
                .WithTextColor(Color.white)
                .WithPadding(10, 10, 15, 15)
                .WithBorderRadius(4)
                .WithMarginRight(10)
                .Build());

            actionsRow.Add(new ForgeButtonBuilder("💥 De-Manifest", () =>
            {
                GameObject.DestroyImmediate(target.gameObject);
                RenderEmptyState();
                RefreshScanner(ctx);
            })
                .WithBackgroundColor(new Color(0.8f, 0.2f, 0.2f)) // Destructive Red
                .WithTextColor(Color.white)
                .WithPadding(10, 10, 15, 15)
                .WithBorderRadius(4)
                .Build());

            _rightPanel.Add(actionsRow);
        }

        private void FocusOnEntity(Transform target)
        {
            if (target == null) return;

#if UNITY_EDITOR
            if (!Application.isPlaying && SceneView.lastActiveSceneView != null)
            {
                Selection.activeGameObject = target.gameObject;
                SceneView.lastActiveSceneView.FrameSelected();
                Debug.Log($"[Cartographer] Editor Camera locked onto {target.name}.");
                return;
            }
#endif

            // Runtime fallback mapping
            if (Camera.main != null)
            {
                Camera.main.transform.position = target.position + (target.forward * -5f) + (Vector3.up * 2f);
                Camera.main.transform.LookAt(target);
                Debug.Log($"[Cartographer] Runtime Camera shifted to {target.name}.");
            }
        }
    }
}