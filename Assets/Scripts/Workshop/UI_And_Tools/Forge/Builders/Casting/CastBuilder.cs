using Assets.Scripts.Workshop.UI_And_Tools.Forge.Builders.GuiBuilders;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UIElements;
using Workshop.Gameplay.Cast;
using Workshop.UI_And_Tools.Forge.Builders.GuiBuilders;
using Workshop.UI_And_Tools.Forge.IO;

namespace Workshop.UI_And_Tools.Forge.Builders.Casting
{
    public class CastBuilder : MonoBehaviour, IForgeBuilder, IGuiProvider
    {
        // --- CONFIGURATION ---
        [SerializeField] private string _draftName = "New Actor";
        [SerializeField] private string _draftOccupation = "Citizen";

        private Role _draftRole = Role.Extra;

        // A helper list to replace Enum.GetValues
        private static readonly List<Role> _coreRoles = new List<Role>
        {
            Role.Protagonist, Role.Antagonist, Role.Supporting, Role.Extra
        };

        // --- RUNTIME STATE ---
        private List<Actor> _activeCast = new List<Actor>();
        private ScrollView _rosterView;

        // --- IBUILDER IMPLEMENTATION ---
        public string ToolName => "Cast & Crew";
        public Type GetProductType() => typeof(Actor);

        public IGuiProvider GetGuiProvider() => this;

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

            if (r) r.material.color = _draftRole.NameplateColor;

            Destroy(vis.GetComponent<Collider>());

            _activeCast.Add(actor);

            // Instantly update the UI to reflect the new actor
            RefreshRoster();

            return actor;
        }

        // --- GUI LAYER ---
        public string Title => ToolName;

        public Action<VisualElement> GetGuiBuilder() => root => root.Add(CreateGui(new GuiContext()));

        public VisualElement CreateGui(GuiContext ctx)
        {
            var rootBuilder = new ForgeContainerBuilder("CastUI_Root")
                .WithPadding(10)
                .WithFlexLayout(FlexDirection.Column, Justify.FlexStart, Align.Stretch)

                // 1. ROSTER (Grouped by Role)
                .AddChild(CreateRosterPanel())

                // 2. SEPARATOR
              //  .AddSeparator(new Color(1f, 1f, 1f, 0.1f), 2f)

                // 3. AUDITION (Creation)
                .AddChild(CreateAuditionPanel());

            return rootBuilder.Build();
        }

        private IGuiProvider CreateRosterPanel()
        {
            return new ForgeScrollViewBuilder()
                .OnBuild(ve =>
                {
                    ve.style.height = 250;
                    _rosterView = ve as ScrollView;
                    RefreshRoster();
                });
        }

        private void RefreshRoster()
        {
            if (_rosterView == null) return;
            _rosterView.Clear();

            foreach (Role role in _coreRoles)
            {
                var actors = _activeCast.Where(a => a != null && a.NarrativeRole.Id == role.Id).ToList();

                _rosterView.Add(new ForgeLabelBuilder(role.Name.ToUpper() + "S")
                    .WithColor(role.NameplateColor)
                    .WithFontStyle(FontStyle.Bold)
                    .WithFontSize(10)
                    .WithMarginTop(10)
                    .WithMarginBottom(2)
                    .Build());

                if (actors.Count == 0)
                {
                    _rosterView.Add(new ForgeLabelBuilder("- Empty -")
                        .WithFontSize(9)
                        .WithMarginLeft(10)
                        .OnBuild(ve => ve.style.opacity = 0.3f)
                        .Build());
                }
                else
                {
                    foreach (var actor in actors)
                    {
                        var row = new ForgeContainerBuilder()
                            .WithFlexLayout(FlexDirection.Row, Justify.FlexStart, Align.FlexStart)
                            .WithMarginLeft(10)
                            .WithMarginBottom(2)
                            .AddChild(new ForgeLabelBuilder(actor.StageName)
                                .WithWidth(120)
                                .WithFontStyle(FontStyle.Bold))
                            .AddChild(new ForgeLabelBuilder($"[{actor.Occupation}]")
                                .WithFontSize(9)
                                .OnBuild(ve => ve.style.opacity = 0.6f));

                        _rosterView.Add(row.Build());
                    }
                }
            }
        }

        private IGuiProvider CreateAuditionPanel()
        {
            // Extract names and find the current index for the dropdown
            var roleNames = _coreRoles.Select(r => r.Name).ToList();
            int defaultRoleIndex = Mathf.Max(0, roleNames.IndexOf(_draftRole.Name));

            return new ForgeContainerBuilder("AuditionPanel")
                .WithBackgroundColor(new Color(0f, 0f, 0f, 0.2f))
                .WithPadding(10)

                .AddChild(new ForgeLabelBuilder("AUDITION ROOM")
                    .WithFontSize(12)
                    .WithFontStyle(FontStyle.Bold)
                    .WithMarginBottom(15))

                .AddChild(new ForgeTextFieldBuilder("Name", _draftName)
                    .OnValueChanged(evt => _draftName = evt.newValue)
                    .WithMarginBottom(5))

                .AddChild(new ForgeTextFieldBuilder("Occupation", _draftOccupation)
                    .OnValueChanged(evt => _draftOccupation = evt.newValue)
                    .WithMarginBottom(5))

                .AddChild(new ForgeDropdownBuilder("Role", roleNames)
                    .OnBuild(ve => {
                        var dropdown = ve as DropdownField;
                        if (dropdown != null)
                        {
                            dropdown.index = defaultRoleIndex;
                            dropdown.RegisterValueChangedCallback(evt => {
                                if (Role.TryParse(evt.newValue, out Role selectedRole))
                                {
                                    _draftRole = selectedRole;
                                }
                            });
                        }
                    })
                    .WithMarginBottom(15))

                .AddChild(new ForgeButtonBuilder("HIRE ACTOR")
                    .WithBackgroundColor(new Color(0f, 0.6f, 0.1f))
                    .WithColor(Color.white)
                    .WithFontStyle(FontStyle.Bold)
                    .WithHeight(35)
                    .WithMarginTop(15)
                    .OnClick(() => Build()));
        }

        public void ToUIDocument(string assetPath)
        {
            var snapshotRoot = CreateGui(new GuiContext());
            string fileName = string.IsNullOrEmpty(assetPath) ? "CastBuilder_Snapshot" : System.IO.Path.GetFileNameWithoutExtension(assetPath);
            WorkshopUxmlBaker.Bake(snapshotRoot, fileName);
        }

        public void FromUIDocument(string assetPath)
        {
            Debug.LogWarning("[CastBuilder] FromUIDocument is not supported. This UI is dynamically generated based on runtime Actor instances.");
        }
    }
}