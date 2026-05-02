#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;
using Workshop.UI_And_Tools.Forge.Builders.GuiBuilders;

namespace Workshop.Asteroids
{
    public class AsteroidFighterBuilderGui : IGuiProvider
    {
        public string Title => "STAR SPARROW HANGAR";

        private GuiContext _lastCtx;
        private CRUD_Builder<ModularFighterDesign> _crudInterface;

        private static List<ModularFighterDesign> _hangarDatabase = new List<ModularFighterDesign>
        {
            new ModularFighterDesign { Name = "Alpha Strike (Stock)", MaxSpeed = 12f, ThrustPower = 8f, UsePreAssembled = true, PreAssembledName = "StarSparrow_Example_01" },
            new ModularFighterDesign { Name = "Scrap-Built Interceptor", MaxSpeed = 18f, ThrustPower = 10f, UsePreAssembled = false, CoreStyle = "Core_04", WingStyle = "Wing_07", WeaponStyle = "Weapon_02" }
        };


        public AsteroidFighterBuilderGui()
        {

        }

        public VisualElement CreateGui(GuiContext ctx)
        {
            _lastCtx = ctx;

            _crudInterface = new CRUD_Builder<ModularFighterDesign>(
                title: "FLEET REGISTRY",
                dataSource: () => _hangarDatabase,
                getDisplayName: (ship) => string.IsNullOrEmpty(ship.Name) ? "Unregistered Hull" : ship.Name,
                getGroupCategory: (ship) => ship.UsePreAssembled ? "Factory Models" : "Custom Forge",

                buildEditorForm: (ship) =>
                {
                    var form = new VisualElement { style = { flexDirection = FlexDirection.Row, alignItems = Align.FlexStart } };

                    // --- LEFT SIDE: 3D CHASSIS ASSEMBLY ---
                    var chassisCol = new VisualElement { style = { width = 280, marginRight = 20 } };
                    chassisCol.Add(new Label("CHASSIS ASSEMBLY") { style = { color = new Color(0.85f, 0.40f, 0.10f), unityFontStyleAndWeight = FontStyle.Bold, marginBottom = 10 } });

                    var chassisBox = new VisualElement { style = { backgroundColor = Color.black, paddingLeft = 10, paddingRight = 10, paddingTop = 10, paddingBottom = 10, borderLeftWidth = 2, borderLeftColor = new Color(0.85f, 0.40f, 0.10f) } };

                    // Toggle for Assembly Mode
                    var modeToggle = new Toggle("Use Factory Prefab") { value = ship.UsePreAssembled };
                    var factoryField = new TextField("Factory Name") { value = ship.PreAssembledName };

                    // Container for custom parts so we can hide/show it based on the toggle
                    var customPartsBox = new VisualElement { style = { marginTop = 10, paddingTop = 10, borderTopWidth = 1, borderTopColor = Color.gray } };

                    customPartsBox.Add(CreatePartField("Core", ship.CoreStyle, val => ship.CoreStyle = val));
                    customPartsBox.Add(CreatePartField("Engine", ship.EngineStyle, val => ship.EngineStyle = val));
                    customPartsBox.Add(CreatePartField("Wing", ship.WingStyle, val => ship.WingStyle = val));
                    customPartsBox.Add(CreatePartField("Weapon", ship.WeaponStyle, val => ship.WeaponStyle = val));
                    customPartsBox.Add(CreatePartField("Tail", ship.TailStyle, val => ship.TailStyle = val));
                    customPartsBox.Add(CreatePartField("Fin", ship.FinStyle, val => ship.FinStyle = val));
                    customPartsBox.Add(CreatePartField("Thruster", ship.ThrusterStyle, val => ship.ThrusterStyle = val));
                    customPartsBox.Add(CreatePartField("Plasma", ship.PlasmaStyle, val => ship.PlasmaStyle = val));

                    // Logic to swap UI based on mode
                    Action updateAssemblyVisibility = () =>
                    {
                        ship.UsePreAssembled = modeToggle.value;
                        factoryField.style.display = ship.UsePreAssembled ? DisplayStyle.Flex : DisplayStyle.None;
                        customPartsBox.style.display = ship.UsePreAssembled ? DisplayStyle.None : DisplayStyle.Flex;
                    };

                    modeToggle.RegisterValueChangedCallback(e => updateAssemblyVisibility());
                    factoryField.RegisterValueChangedCallback(e => ship.PreAssembledName = e.newValue);

                    updateAssemblyVisibility(); // Initial setup

                    chassisBox.Add(modeToggle);
                    chassisBox.Add(factoryField);
                    chassisBox.Add(customPartsBox);

                    // Colors
                    var colorsBox = new VisualElement { style = { marginTop = 10, paddingTop = 10, borderTopWidth = 1, borderTopColor = Color.gray } };
                    var paintField = new ColorField("Hull Paint") { value = ship.HullPaint };
                    paintField.RegisterValueChangedCallback(e => ship.HullPaint = e.newValue);
                    var glowField = new ColorField("Emission Glow") { value = ship.EmissionColor };
                    glowField.RegisterValueChangedCallback(e => ship.EmissionColor = e.newValue);
                    colorsBox.Add(paintField);
                    colorsBox.Add(glowField);
                    chassisBox.Add(colorsBox);

                    chassisCol.Add(chassisBox);
                    form.Add(chassisCol);

                    // --- RIGHT SIDE: FLIGHT DYNAMICS ---
                    var physicsCol = new VisualElement { style = { flexGrow = 1 } };
                    physicsCol.Add(new Label("FLIGHT DYNAMICS") { style = { color = Color.cyan, unityFontStyleAndWeight = FontStyle.Bold, marginBottom = 10 } });

                    var physicsBox = new VisualElement { style = { backgroundColor = Color.black, paddingLeft = 10, paddingRight = 10, paddingTop = 10, paddingBottom = 10, borderLeftWidth = 2, borderLeftColor = Color.cyan } };

                    var nameField = new TextField("Callsign") { value = ship.Name };
                    nameField.RegisterValueChangedCallback(e => ship.Name = e.newValue);
                    physicsBox.Add(nameField);

                    var maxSpeedField = new FloatField("Max Speed") { value = ship.MaxSpeed };
                    maxSpeedField.RegisterValueChangedCallback(e => ship.MaxSpeed = e.newValue);
                    physicsBox.Add(maxSpeedField);

                    var thrustField = new FloatField("Thrust Power") { value = ship.ThrustPower };
                    thrustField.RegisterValueChangedCallback(e => ship.ThrustPower = e.newValue);
                    physicsBox.Add(thrustField);

                    var dragField = new FloatField("Inertial Dampening") { value = ship.Drag };
                    dragField.RegisterValueChangedCallback(e => ship.Drag = e.newValue);
                    physicsBox.Add(dragField);

                    physicsCol.Add(physicsBox);
                    form.Add(physicsCol);

                    return form;
                },

                onSave: (ship) =>
                {
                    if (!_hangarDatabase.Contains(ship)) _hangarDatabase.Add(ship);
                },

                onDelete: (ship) =>
                {
                    _hangarDatabase.Remove(ship);
                },

                getSubtitle: (ship) => ship.UsePreAssembled ? $"Factory: {ship.PreAssembledName}" : $"Custom: {ship.CoreStyle} + {ship.WingStyle}"
            );

            return _crudInterface.CreateGui(ctx);
        }

        private TextField CreatePartField(string label, string currentValue, Action<string> onValueChanged)
        {
            var field = new TextField(label) { value = currentValue };
            field.Q<Label>().style.minWidth = 70;
            field.Q<Label>().style.color = Color.gray;
            field.RegisterValueChangedCallback(e => onValueChanged(e.newValue));
            return field;
        }

        public Action<VisualElement> GetGuiBuilder() => (root) => root.Add(CreateGui(new GuiContext()));
        public void FromUIDocument(string assetPath) => _lastCtx?.OnBuilt?.Invoke(GraphicalUserInterfaceBuilder.ConvertFromUIDocument(assetPath));
        public void ToUIDocument(string assetPath) => GraphicalUserInterfaceBuilder.ConvertToUIDocument(CreateGui(new GuiContext()), assetPath);
    }
}

#endif