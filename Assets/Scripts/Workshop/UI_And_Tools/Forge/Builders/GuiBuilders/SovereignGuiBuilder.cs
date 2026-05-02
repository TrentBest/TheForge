// File: Assets/Scripts/Workshop/UI_And_Tools/Forge/Builders/GuiBuilders/SovereignGuiBuilder.cs
using System;
using TheSingularityWorkshop.FSM_API;
using UnityEngine;
using UnityEngine.UIElements;
using Workshop.Core;
using Workshop.Core.Memory;
using Workshop.UI_And_Tools.Forge.Core;
using Workshop.UI_And_Tools.Forge.IO;

namespace Workshop.UI_And_Tools.Forge.Builders.GuiBuilders
{
    /// <summary>
    /// A pure C# UI Builder. Lobotomized from MonoBehaviour.
    /// Manages a Panel and VisualElement tree while stationing physical data in the Warehouse.
    /// </summary>
    public class SovereignGuiBuilder : IStateContext, IDomainPassenger
    {
        public string Name { get; set; }
        public bool IsValid { get; set; } = true;

        private IGuiProvider _provider;
        private PanelSettings _panelSettings;
        private RenderTexture _renderTexture;
        private VisualElement _root;

        private int _warehouseIndex = -1;
        private int _colliderIndex = -1;

        public SovereignGuiBuilder(string name, IGuiProvider provider)
        {
            Name = name;
            _provider = provider;

            // Hop aboard the reload sequence immediately
            ForgeDomainConductor.IssueTicket(this);
            Ignite();
        }

        public void Ignite()
        {
            // 1. Setup the UI Toolkit Panel manually (The "Vessel" for VisualElements)
            _renderTexture = new RenderTexture(1024, 1024, 24);
            _renderTexture.Create();

            // We still use PanelSettings as a ScriptableObject "Template"
            _panelSettings = ScriptableObject.CreateInstance<PanelSettings>();
            _panelSettings.targetTexture = _renderTexture;
            _panelSettings.clearColor = true;

            // 2. Build the Tree
            var ctx = new GuiContext { Name = Name, Controls = new RuntimeControlFactory() };
            _root = _provider.CreateGui(ctx);

            // 3. Station in the Warehouse
            StationInWarehouse();
        }

        private void StationInWarehouse()
        {
            var warehouse = SingularityBootloader.MainWarehouse;
            var guiShelf = warehouse.GetOrCreateShelf<SovereignGuiSurface>();
            var colShelf = warehouse.GetOrCreateShelf<SovereignCollider>();

            // Store physical data for the Render Pipeline
            _warehouseIndex = guiShelf.Store(new SovereignGuiSurface
            {
                Position = Vector3.zero,
                Rotation = Quaternion.identity,
                PhysicalSize = new Vector2(2, 2),
                TextureId = warehouse.RegisterTexture(_renderTexture),
                IsVisible = true
            });

            // Store collider data for the custom Interaction System
            _colliderIndex = colShelf.Store(new SovereignCollider
            {
                Center = Vector3.zero,
                HalfExtents = new Vector3(1, 1, 0.01f),
                Rotation = Quaternion.identity,
                OwnerEntityId = _warehouseIndex,
                IsActive = true
            });
        }

        // --- IDOMAINPASSENGER IMPLEMENTATION ---

        public void OnSuspend()
        {
            // Release hardware handles before the blink
            _renderTexture?.Release();
            _root?.Clear();
            IsValid = false;
        }

        public void OnResume()
        {
            // Re-ignite on the other side
            ForgeDomainConductor.IssueTicket(this);
            Ignite();
        }
    }
}