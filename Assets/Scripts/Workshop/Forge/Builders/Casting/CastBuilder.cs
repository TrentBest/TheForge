// File: Assets/Scripts/Workshop/Forge/Builders/Casting/CastBuilder.cs
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UIElements;
using TheSingularityWorkshop.Builders.GuiBuilders;
using TheSingularityWorkshop.Forge.Builders.GuiBuilders;
using TheSingularityWorkshop.Forge;
using TheSingularityWorkshop.Cast;

namespace TheSingularityWorkshop.Forge.Builders.Casting
{
    public class CastBuilder : MonoBehaviour, IForgeBuilder
    {
        // --- CONFIGURATION ---
        [SerializeField] private string _draftName = "New Actor";
        [SerializeField] private Role _draftRole = Role.Extra;
        [SerializeField] private string _draftOccupation = "Citizen";

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

            // Visual Placeholder (Color Coded)
            var vis = GameObject.CreatePrimitive(PrimitiveType.Capsule);
            vis.transform.SetParent(go.transform);
            vis.transform.localPosition = Vector3.up;
            var r = vis.GetComponent<Renderer>();
            if (r) r.material.color = GetRoleColor(_draftRole);
            Destroy(vis.GetComponent<Collider>());

            _activeCast.Add(actor);
            return actor;
        }

        private Color GetRoleColor(Role role)
        {
            switch (role)
            {
                case Role.Protagonist: return new Color(0f, 0.8f, 1f); // Cyan
                case Role.Antagonist: return new Color(1f, 0.2f, 0.2f); // Red
                case Role.Supporting: return new Color(0.4f, 1f, 0.4f); // Green
                default: return Color.gray;
            }
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

                    foreach (Role role in Enum.GetValues(typeof(Role)))
                    {
                        var actors = _activeCast.Where(a => a != null && a.NarrativeRole == role).ToList();

                        var header = new Label(role.ToString().ToUpper() + "S")
                        {
                            style = {
                                color = GetRoleColor(role),
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
            return new GraphicalUserInterfaceBuilder("Audition")
                .WithTitle("AUDITION ROOM")
                .WithHeaderFontSize(12)
                .WithBackgroundColor(new Color(0, 0, 0, 0.2f))
                .WithPadding(10)
                .AddStringData("Name", _draftName, v => _draftName = v)
                .AddStringData("Occupation", _draftOccupation, v => _draftOccupation = v)
                .AddChild(ctx =>
                {
                    var roles = Enum.GetNames(typeof(Role)).ToList();
                    var dd = new PopupField<string>("Role", roles, _draftRole.ToString());
                    dd.RegisterValueChangedCallback(e => Enum.TryParse(e.newValue, out _draftRole));
                    return dd;
                })
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