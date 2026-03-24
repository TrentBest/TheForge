// File: Assets/Scripts/Workshop/Forge/Builders/Casting/CastBuilder.cs
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UIElements;
using TheSingularityWorkshop.Forge.Builders.GuiBuilders;
using TheSingularityWorkshop.Forge;
using TheSingularityWorkshop.Cast;

namespace TheSingularityWorkshop.Forge.Builders.Casting
{
    public class CastBuilder : MonoBehaviour, IForgeBuilder
    {
        // --- CONFIGURATION ---
        [SerializeField] private string _draftName = "New Actor";
        [SerializeField] private string _draftOccupation = "Citizen";

        // Removed [SerializeField] because Unity's inspector doesn't serialize static readonly class instances well.
        // It's perfectly fine as a private runtime variable for the Forge tool.
        private Role _draftRole = Role.Extra;

        // A helper list to replace Enum.GetValues
        private static readonly List<Role> _coreRoles = new List<Role>
        {
            Role.Protagonist, Role.Antagonist, Role.Supporting, Role.Extra
        };

        // --- RUNTIME STATE ---
        private List<Actor> _activeCast = new List<Actor>();

        // --- IBUILDER IMPLEMENTATION ---
        public string ToolName => "Cast & Crew";
        public Type GetProductType() => typeof(Actor);
        public IGuiProvider GetGuiProvider() => this as IGuiProvider;

        private void OnEnable() => BuilderRegistry.Register(this);

        // --- LOGIC ---
        public object Build()
        {
            var go = new GameObject();
            go.transform.SetParent(this.transform);

            var actor = go.AddComponent<Actor>();
            actor.Initialize(_draftName, _draftRole, _draftOccupation);

            // Visual Placeholder (Color Coded directly from the Role data!)
            var vis = GameObject.CreatePrimitive(PrimitiveType.Capsule);
            vis.transform.SetParent(go.transform);
            vis.transform.localPosition = Vector3.up;
            var r = vis.GetComponent<Renderer>();

            // We no longer need a massive switch statement; the Role knows its own color.
            if (r) r.material.color = _draftRole.NameplateColor;

            Destroy(vis.GetComponent<Collider>());

            _activeCast.Add(actor);
            return actor;
        }

        // --- GUI ---
        public string Title => ToolName;
        public Action<VisualElement> GetGuiBuilder() => null;

        public VisualElement CreateGui(GuiContext ctx)
        {
            var root = new GraphicalUserInterfaceBuilder("CastUI")
                .WithPadding(10)
                .WithFlexLayout(FlexDirection.Column, Justify.FlexStart, Align.Stretch);

            // 1. ROSTER (Grouped by Role)
            root.AddChild(CreateRosterPanel());

            // 2. SEPARATOR
            root.AddChild(new VisualElement { style = { height = 2, backgroundColor = new Color(1, 1, 1, 0.1f), marginTop = 10, marginBottom = 10 } });

            // 3. AUDITION (Creation)
            root.AddChild(CreateAuditionPanel());

            return root.Build();
        }

        private IGuiProvider CreateRosterPanel()
        {
            return new GraphicalUserInterfaceBuilder("Roster")
                .AddChild(ctx =>
                {
                    var scroll = new ScrollView { style = { height = 250 } };

                    // Replaced Enum.GetValues with our list of core roles
                    foreach (Role role in _coreRoles)
                    {
                        // Match by ID since they are class instances
                        var actors = _activeCast.Where(a => a != null && a.NarrativeRole.Id == role.Id).ToList();

                        var header = new Label(role.Name.ToUpper() + "S")
                        {
                            style = {
                                color = role.NameplateColor, // Dynamically styled by the data!
                                unityFontStyleAndWeight = FontStyle.Bold,
                                fontSize = 10, marginTop = 10, marginBottom = 2
                            }
                        };
                        scroll.Add(header);

                        if (actors.Count == 0)
                        {
                            scroll.Add(new Label("- Empty -") { style = { opacity = 0.3f, fontSize = 9, marginLeft = 10 } });
                        }
                        else
                        {
                            foreach (var actor in actors)
                            {
                                var row = new VisualElement { style = { flexDirection = FlexDirection.Row, marginLeft = 10, marginBottom = 2 } };
                                row.Add(new Label(actor.StageName) { style = { width = 120, unityFontStyleAndWeight = FontStyle.Bold } });
                                row.Add(new Label($"[{actor.Occupation}]") { style = { opacity = 0.6f, fontSize = 9 } });
                                scroll.Add(row);
                            }
                        }
                    }
                    return scroll;
                });
        }

        private IGuiProvider CreateAuditionPanel()
        {
            // Extract names and find the current index for the dropdown
            var roleNames = _coreRoles.Select(r => r.Name).ToList();
            int defaultRoleIndex = Mathf.Max(0, roleNames.IndexOf(_draftRole.Name));

            return new GraphicalUserInterfaceBuilder("Audition")
                .WithTitle("AUDITION ROOM")
                .WithHeaderFontSize(12)
                .WithBackgroundColor(new Color(0, 0, 0, 0.2f))
                .WithPadding(10)

                // Data Bindings
                .AddStringData("Name", _draftName, v => _draftName = v)
                .AddStringData("Occupation", _draftOccupation, v => _draftOccupation = v)

                // Use your built-in fluent dropdown builder!
                .AddDropdownData("Role", roleNames, defaultRoleIndex, newValue =>
                {
                    if (Role.TryParse(newValue, out Role selectedRole))
                    {
                        _draftRole = selectedRole;
                    }
                })

                // Keep the custom-styled Hire Button
                .AddChild(c => new Button(() => { Build(); })
                {
                    text = "HIRE ACTOR",
                    style = {
                marginTop = 15, height = 35,
                backgroundColor = new Color(0, 0.6f, 0.1f),
                unityFontStyleAndWeight = FontStyle.Bold, color = Color.white
                    }
                });
        }
    }
}