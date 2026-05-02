// File: Assets/Scripts/Workshop/Forge/Builders/GuiBuilders/Casting/Workshop_Gui_ActorBuilder.cs
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UIElements;
using Workshop.Gameplay.Cast;


namespace Workshop.UI_And_Tools.Forge.Builders.GuiBuilders
{
    public class Workshop_Gui_ActorBuilder : CRUD_Builder<ActorDefinition>
    {
        // public cache for the Forge session
        private static List<ActorDefinition> _roster = new List<ActorDefinition>();
        private static List<Role> _availableRoles = new List<Role> { Role.Protagonist, Role.Antagonist, Role.Supporting, Role.Extra };

        // We pass all the logic into your powerful base constructor
        public Workshop_Gui_ActorBuilder() : base(
            title: "🎭 Casting Office",
            dataSource: () => _roster,
            getDisplayName: actor => string.IsNullOrEmpty(actor.DisplayName) ? "New Actor" : actor.DisplayName,
            buildEditorForm: BuildEditor,
            onSave: SaveActor,
            onDelete: DeleteActor,
            getSubtitle: actor => $"Armor: {actor.Armor} | Speed: {actor.MovementSpeed}",
            getGroupCategory: actor => Role.GetById(actor.RoleId).Name // This seamlessly enables your Tree view!
        )
        {
            if (_roster.Count == 0) LoadMockData();
        }

        private static VisualElement BuildEditor(ActorDefinition actor)
        {
            // Using your declarative wrapper instead of raw UI Toolkit
            var builder = new GraphicalUserInterfaceBuilder("ActorEditorForm")
                .WithFlexLayout(FlexDirection.Column, Justify.FlexStart, Align.Stretch);

            var nameField = new TextField("Stage Name") { value = actor.DisplayName };
            nameField.RegisterValueChangedCallback(evt => actor.DisplayName = evt.newValue);
            builder.AddChild(nameField);

            var prefabField = new TextField("Prefab Path") { value = actor.PrefabResourcePath };
            prefabField.RegisterValueChangedCallback(evt => actor.PrefabResourcePath = evt.newValue);
            builder.AddChild(prefabField);

            // Role Assignment Dropdown
            var currentRole = Role.GetById(actor.RoleId);
            var roleDropdown = new DropdownField("Narrative Role", _availableRoles.Select(r => r.Name).ToList(), currentRole.Name);
            roleDropdown.RegisterValueChangedCallback(evt =>
            {
                var selected = _availableRoles.FirstOrDefault(r => r.Name == evt.newValue);
                if (selected != null) actor.RoleId = selected.Id;
            });
            builder.AddChild(roleDropdown);

            // Stats
            var speedSlider = new Slider("Movement Speed", 1f, 20f) { value = actor.MovementSpeed };
            speedSlider.RegisterValueChangedCallback(evt => actor.MovementSpeed = evt.newValue);
            builder.AddChild(speedSlider);

            var armorSlider = new Slider("Base Armor", 0f, 500f) { value = actor.Armor };
            armorSlider.RegisterValueChangedCallback(evt => actor.Armor = evt.newValue);
            builder.AddChild(armorSlider);

            return builder.Build();
        }

        private static void SaveActor(ActorDefinition actor)
        {
            if (!_roster.Contains(actor)) _roster.Add(actor);
            Debug.Log($"[Forge] Saved Blueprint: {actor.DisplayName}");
        }

        private static void DeleteActor(ActorDefinition actor)
        {
            if (_roster.Contains(actor)) _roster.Remove(actor);
        }

        private void LoadMockData()
        {
            var mock = ScriptableObject.CreateInstance<ActorDefinition>();
            mock.DisplayName = "Space Marine";
            mock.RoleId = Role.Protagonist.Id; // Ensure ActorDefinition has a public byte RoleId;
            _roster.Add(mock);
        }
    }
}