using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;
using TheSingularityWorkshop.Forge.Builders.GuiBuilders;

namespace TheSingularityWorkshop.Forge.Runtime
{
    public enum ForgeScaleLimit { Micro, Personal, Tactical, Industrial, Orbital }

    /// <summary>
    /// The physical manifestation of a Forge in the game world.
    /// Bridges causality/physics with the IGuiProvider ecosystem.
    /// </summary>
    public class ForgeDiegeticTerminal : MonoBehaviour
    {
        [Header("Identity & Scope")]
        public string ForgeName = "Standard Fabrication Hub";
        public int TechnologyLevel = 5;
        public ForgeScaleLimit MaxScale = ForgeScaleLimit.Personal;

        [Header("Physical Anchors")]
        [Tooltip("The mesh/transform where the UI RenderTexture is applied.")]
        public Transform ScreenAnchor;

        [Tooltip("The physical location where LiveModelPreviews will be staged (e.g., a holographic table).")]
        public Transform FabricationPad;

        [Header("Capabilities")]
        [Tooltip("List of IGuiProvider Type Names this terminal can load.")]
        public List<string> AllowedToolProviders = new List<string>();

        // Internal UI mapping
        private UIDocument _diegeticDocument;
        private GuiContext _activeContext;
        private IGuiProvider _activeProvider;

        private void Awake()
        {
            // Set up the World-Space UI Document
            _diegeticDocument = gameObject.AddComponent<UIDocument>();

            // In a real setup, you assign a PanelSettings object here that outputs to a RenderTexture
            // _diegeticDocument.panelSettings = Resources.Load<PanelSettings>("DiegeticPanelSettings");
        }

        /// <summary>
        /// Triggered by player Interaction (e.g., via your FSM Interaction raycast).
        /// </summary>
        public void BootTerminal(IGuiProvider toolToLoad)
        {
            if (toolToLoad == null || !AllowedToolProviders.Contains(toolToLoad.GetType().Name))
            {
                Debug.LogWarning($"[Forge] {ForgeName} lacks the schematics/tech to load {toolToLoad?.Title}");
                return;
            }

            _activeProvider = toolToLoad;

            // 1. Create a fresh context
            _activeContext = new GuiContext();
            _activeContext.Name = $"Diegetic_{ForgeName}";

            // 2. DI INJECTION: Tell the ecosystem this is an In-World Forge!
            // This allows LMP and other builders to adapt their behavior.
            _activeContext.AddService(this);

            // 3. Build the UI and attach it to the physical document
            VisualElement rootUi = _activeProvider.CreateGui(_activeContext);

            _diegeticDocument.visualTreeAsset = null; // Clear existing
            _diegeticDocument.rootVisualElement.Clear();
            _diegeticDocument.rootVisualElement.Add(rootUi);

            Debug.Log($"[Forge] {ForgeName} booted schematic: {toolToLoad.Title}");
        }

        public void Shutdown()
        {
            // Triggers the DetachFromPanelEvent, cascading cleanup across the UI and FSMs
            _diegeticDocument.rootVisualElement.Clear();
            _activeContext = null;
        }
    }
}