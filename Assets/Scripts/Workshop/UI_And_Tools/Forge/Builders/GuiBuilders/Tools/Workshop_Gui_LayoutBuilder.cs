using System;
using System.Collections.Generic;
using System.Linq; // <--- THIS WAS MISSING
using UnityEngine;
using UnityEngine.UIElements;
using Workshop.UI_And_Tools.Forge.Builders.GuiBuilders;
using Workshop.Systems.MicroPackages;

namespace Assets.Scripts.GuiBuilders.Tools
{
    public class Workshop_Gui_LayoutBuilder : IGuiProvider
    {
        public string Title => "DIEGETIC LAYOUT SCULPTOR";

        private DiegeticLayoutManifest _activeManifest;

        public Workshop_Gui_LayoutBuilder(DiegeticLayoutManifest manifest = null)
        {
            _activeManifest = manifest ?? new DiegeticLayoutManifest { TargetToolId = "New_Tool_Manifest" };
        }

        public VisualElement CreateGui(GuiContext ctx)
        {
            var root = new ForgeContainerBuilder("LayoutSculptorRoot")
                .WithFlexGrow(1).WithPadding(20)
                .WithBackgroundColor(new Color(0.08f, 0.08f, 0.1f))
                .AddChild(new ForgeLabelBuilder("SPATIAL ARCHITECTURE DEFINITION")
                    .WithColor(Color.cyan).WithBold().WithFontSize(20).WithMarginBottom(20));

            root.OnBuild(ve =>
            {
                // 1. Spatial Orbit Selection
                var orbitDropdown = new DropdownField("Preferred Manifestation Ring", Enum.GetNames(typeof(SpatialOrbit)).ToList(), (int)_activeManifest.PreferredOrbit);
                orbitDropdown.RegisterValueChangedCallback(evt => {
                    _activeManifest.PreferredOrbit = (SpatialOrbit)Enum.Parse(typeof(SpatialOrbit), evt.newValue);
                });
                orbitDropdown.labelElement.style.color = Color.white;
                orbitDropdown.style.marginBottom = 15;
                ve.Add(orbitDropdown);

                // 2. Elevation Slider
                var elevationLabel = new Label($"Elevation (Float Height): {_activeManifest.ElevationOffset:F1}m") { style = { color = Color.cyan } };
                var elevationSlider = new Slider(0.0f, 3.0f) { value = _activeManifest.ElevationOffset };
                elevationSlider.RegisterValueChangedCallback(evt => {
                    _activeManifest.ElevationOffset = evt.newValue;
                    elevationLabel.text = $"Elevation (Float Height): {_activeManifest.ElevationOffset:F1}m";
                });
                ve.Add(elevationLabel); ve.Add(elevationSlider);

                // 3. Hardware Resolution
                ve.Add(new ForgeLabelBuilder("PANEL RESOLUTION (PIXELS)").WithColor(Color.white).WithMarginTop(15).WithMarginBottom(5).CreateGui(ctx));

                var resX = new IntegerField("Width") { value = (int)_activeManifest.PanelResolution.x };
                resX.RegisterValueChangedCallback(evt => _activeManifest.PanelResolution.x = evt.newValue);
                resX.labelElement.style.color = Color.gray;

                var resY = new IntegerField("Height") { value = (int)_activeManifest.PanelResolution.y };
                resY.RegisterValueChangedCallback(evt => _activeManifest.PanelResolution.y = evt.newValue);
                resY.labelElement.style.color = Color.gray;

                ve.Add(resX); ve.Add(resY);

                // 4. Behavioral Toggles
                ve.Add(new ForgeLabelBuilder("PHYSICS & BEHAVIOR").WithColor(new Color(0.64f, 0.17f, 0.77f)).WithMarginTop(20).WithBold().CreateGui(ctx));

                var billboardToggle = new Toggle("Billboards to User (Always faces camera)") { value = _activeManifest.BillboardsToUser };
                billboardToggle.RegisterValueChangedCallback(evt => _activeManifest.BillboardsToUser = evt.newValue);
                billboardToggle.labelElement.style.color = Color.white;
                ve.Add(billboardToggle);

                var summonToggle = new Toggle("Is Summonable (User can pull to hands)") { value = _activeManifest.IsSummonable };
                summonToggle.RegisterValueChangedCallback(evt => _activeManifest.IsSummonable = evt.newValue);
                summonToggle.labelElement.style.color = Color.white;
                ve.Add(summonToggle);

                var pinToggle = new Toggle("Is Pinned (Locks to world coordinates)") { value = _activeManifest.IsPinned };
                pinToggle.RegisterValueChangedCallback(evt => _activeManifest.IsPinned = evt.newValue);
                pinToggle.labelElement.style.color = Color.white;
                ve.Add(pinToggle);
            });

            // 5. Save/Export Button
            root.AddChild(new ForgeButtonBuilder("SAVE LAYOUT MANIFEST TO PACKAGE")
                .WithBackgroundColor(new Color(0.1f, 0.5f, 0.2f)).WithTextColor(Color.white)
                .WithMarginTop(30).WithPadding(15).WithBold()
                .WithOnClick(() =>
                {
                    Debug.Log($"[LayoutSculptor] Manifest saved for Tool: {_activeManifest.TargetToolId}. Orbit: {_activeManifest.PreferredOrbit}");
                }));

            return root.Build();
        }

        public Action<VisualElement> GetGuiBuilder() => root => root.Add(CreateGui(new GuiContext()));
        public void ToUIDocument(string path) { }
        public void FromUIDocument(string path) { }
    }
}