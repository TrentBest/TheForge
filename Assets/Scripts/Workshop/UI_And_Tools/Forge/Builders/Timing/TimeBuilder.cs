using System;
using UnityEngine;
using UnityEngine.UIElements;
using Workshop.Core.Chronos;
using Workshop.UI_And_Tools.Forge.Builders.GuiBuilders;

namespace Workshop.UI_And_Tools.Forge.Builders.Timing
{
    public class TimeBuilder : MonoBehaviour, IForgeBuilder, IGuiProvider
    {
        [Header("Temporal Context")]
        [SerializeField] private TemporalContext _context = new TemporalContext();

        public string ToolName => "Chronos Builder";

        // Implemented Title for the Forge/Experience Model
        public string Title => "Chronos Control";

        /// <summary>
        /// Returns the TemporalContext which acts as the systemic anchor for Chronos Logic.
        /// </summary>
        public object Build()
        {
            return _context;
        }

        public Type GetProductType() => typeof(TemporalContext);

        public IGuiProvider GetGuiProvider() => this;

        /// <summary>
        /// Uses the GraphicalUserInterfaceBuilder to create a control panel for time manipulation.
        /// </summary>
        public VisualElement CreateGui(GuiContext context)
        {
            var gui = new GraphicalUserInterfaceBuilder("Chronos Controller")
                .WithTitle(Title)
                .WithPadding(8)
                .WithAutoGrow();

            // Direct binding to the TemporalContext vectors
            gui.AddSliderData("Chronos Scale", 0f, 5f, _context.TimeScale, val => {
                _context.TimeScale = val;
                _context.ApplyToUnity();
            });

            gui.AddSliderData("Target Scale", 0f, 5f, _context.TargetTimeScale, val => {
                _context.TargetTimeScale = val;
            });

            gui.AddFloatData("Dilation Factor", _context.DilationFactor, val => {
                _context.DilationFactor = val;
            });

            // FIXED AMBIGUITY: Specify '1' for int height or '1f' for float width
            gui.AddSeparator(Color.gray, 1);

            gui.AddButton("Reset Chronos", () => {
                _context.TimeScale = 1.0f;
                _context.TargetTimeScale = 1.0f;
                _context.ApplyToUnity();
            });

            return gui.Build();
        }

        // Explicit interface implementation to satisfy IBuilder
        IGuiProvider IBuilder.GetGuiProvider() => this;

        /// <summary>
        /// Provides an action to inject this UI into a parent root.
        /// </summary>
        public Action<VisualElement> GetGuiBuilder()
        {
            return (root) => root.Add(CreateGui(new GuiContext()));
        }

        #region --- Serialization & UIDocuments ---
#if UNITY_EDITOR
        public void ToUIDocument(string assetPath)
        {
            var root = CreateGui(new GuiContext());
            GraphicalUserInterfaceBuilder.ConvertToUIDocument(root, assetPath);
        }

        public void FromUIDocument(string assetPath)
        {
            var visualTree = UnityEditor.AssetDatabase.LoadAssetAtPath<VisualTreeAsset>(assetPath);
            if (visualTree != null)
            {
                // In this context, we can use the baker or builder to re-hydrate if needed
                Debug.Log($"[TimeBuilder] Hydrated from {assetPath}");
            }
        }
#else
        public void ToUIDocument(string assetPath) { }
        public void FromUIDocument(string assetPath) { }
#endif
        #endregion
    }
}