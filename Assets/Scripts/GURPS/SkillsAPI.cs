using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UIElements;
using Workshop.Core.Memory;
using Workshop.Systems.MicroPackages;
using Workshop.UI_And_Tools.Forge.Builders.GuiBuilders;
using Workshop.UI_And_Tools.Forge.IO;

namespace Workshop.GURPS
{
    /// <summary>
    /// The Digital GURPS Skills Library.
    /// Manages skill definitions and provides the 3e mathematical costing engine.
    /// Reforged to follow the Forge Protocol and natively implement Development GUIDs.
    /// </summary>
    public class SkillsAPI : IGurpsApiProvider, IFilterWindowProvider, IGuiProvider
    {
        // --- IDENTIFICATION ---
        // During MetaDev we use a deterministic "Low Number" GUID. 
        // When published, the Forge network bus will override this with the official backend GUID.
        private Guid _id = Guid.Parse("00000000-0000-0000-0000-000000001006");
        public Guid Id { get => _id; set => _id = value; }

        public ProviderType ProviderType => ProviderType.GurpsApi;
        public string ModuleName => "Skills Library";
        public string Title => "SKILLS REGISTRY";
        public Vector2 position { get; set; }

        // --- STATE ---
        private List<GURPSSkillDefinition> _skills = new List<GURPSSkillDefinition>();
        private DataWarehouse _warehouse;
        private const string CacheKey = "GURPS_SKILLS_DATABASE";

        // --- UI CACHE ---
        private GURPSSkillDefinition _selectedSkill;
        private VisualElement _detailPanel;

        public SkillsAPI() { }

        public SkillsAPI(DataWarehouse warehouse)
        {
            Bind(warehouse);
        }

        public void Initialize() => LoadFromCache();

        // --- LIFECYCLE & BINDING ---

        public void Bind(DataWarehouse warehouse)
        {
            if (warehouse == null)
            {
                Debug.LogWarning("[SkillsAPI] Attempted to bind a null DataWarehouse. Aborting.");
                return;
            }

            _warehouse = warehouse;
            LoadFromCache();
            Debug.Log($"[SkillsAPI] Bound to Sovereign Memory. {_skills.Count} skills loaded.");
        }

        // --- CORE RULE ENGINE: GURPS 3e COSTING ---

        public int GetPointCost(GURPSSkillDefinition skill, int levelRelativeToAttribute)
        {
            if (skill == null) return 0;

            return skill.Type == SkillType.Physical
                ? CalculatePhysicalCost(skill.Difficulty, levelRelativeToAttribute)
                : CalculateMentalCost(skill.Difficulty, levelRelativeToAttribute);
        }

        private int CalculatePhysicalCost(GURPSDifficulty diff, int rel)
        {
            int startPoint = diff switch { GURPSDifficulty.Easy => 0, GURPSDifficulty.Average => -1, _ => -2 };
            int steps = rel - startPoint;

            if (steps < 0) return 0;
            if (steps == 0) return 1;
            if (steps == 1) return 2;
            if (steps == 2) return 4;

            return 4 + ((steps - 2) * 8); // 3e pattern: 1, 2, 4, 8, 16...
        }

        private int CalculateMentalCost(GURPSDifficulty diff, int rel)
        {
            int startPoint = diff switch { GURPSDifficulty.Easy => -1, GURPSDifficulty.Average => -2, _ => -3 };
            int steps = rel - startPoint;

            if (steps < 0) return 0;
            if (steps == 0) return 1;
            if (steps == 1) return 1; // Mental starts at 1/2 (rounded to 1 for digital)
            if (steps == 2) return 2;

            return 2 + ((steps - 2) * 2); // Mental is more linear in 3e
        }

        // --- DATA MANIPULATION (CRUD) ---

        public void Register(GURPSSkillDefinition skill)
        {
            if (skill == null || _skills.Contains(skill)) return;
            _skills.Add(skill);
            SaveToCache();
        }

        internal void UnRegister(GURPSSkillDefinition skill)
        {
            if (skill == null) return;

            if (_skills.Remove(skill))
            {
                SaveToCache();

                // Clear the UI panel if the user deleted what they were looking at
                if (_selectedSkill == skill && _detailPanel != null)
                {
                    _selectedSkill = null;
                    _detailPanel.Clear();
                }
            }
        }

        public List<GURPSSkillDefinition> GetAll() => _skills.OrderBy(s => s.Name).ToList();

        // --- PERSISTENCE ---

        public void SaveToCache()
        {
            if (_warehouse == null) return;
            string json = JsonUtility.ToJson(new CacheWrapper { Skills = _skills }, true);
            _warehouse.StoreTemporary(CacheKey, json);
        }

        public void LoadFromCache()
        {
            if (_warehouse != null && _warehouse.TryRetrieveTemporary(CacheKey, out string json))
            {
                var cache = JsonUtility.FromJson<CacheWrapper>(json);
                if (cache?.Skills != null && cache.Skills.Count > 0)
                {
                    _skills = cache.Skills;
                    return;
                }
            }
            SeedStandardSkills();
        }

        private void SeedStandardSkills()
        {
            _skills.Clear();
            // Added directly to avoid triggering SaveToCache() 3 times in a row during boot
            _skills.Add(new GURPSSkillDefinition { Name = "Astrogation", Type = SkillType.Mental, BaseAttribute = GURPSAttributeType.IQ, Difficulty = GURPSDifficulty.Average });
            _skills.Add(new GURPSSkillDefinition { Name = "Beam Weapons", Type = SkillType.Physical, BaseAttribute = GURPSAttributeType.DX, Difficulty = GURPSDifficulty.Easy });
            _skills.Add(new GURPSSkillDefinition { Name = "Piloting", Type = SkillType.Physical, BaseAttribute = GURPSAttributeType.DX, Difficulty = GURPSDifficulty.Average });

            SaveToCache(); // Save once at the end
        }

        // --- GUI MANIFESTATION (Forge Protocol) ---

        public VisualElement CreateGui(GuiContext ctx)
        {
            var rootBuilder = new ForgeContainerBuilder("Skills_Root")
                .WithDirection(FlexDirection.Row)
                .WithFlexGrow(1f)
                .WithBackgroundColor(new Color(0.02f, 0.02f, 0.05f));

            // LEFT: Skill List (30%)
            var listPanel = new ForgeContainerBuilder("SkillList")
                .WithWidth(new StyleLength(Length.Percent(30f)))
                .WithPadding(10f)
                .WithBorderWidth(0, 1f, 0, 0)
                .WithBorderColor(new Color(0.3f, 0.3f, 0.4f))
                .AddChild(new ForgeLabelBuilder("REGISTRY").WithBold().WithColor(Color.cyan).WithMarginBottom(10f));

            listPanel.AddChild(new DynamicGuiProvider(c => {
                var scroll = new ScrollView();
                foreach (var skill in _skills.OrderBy(s => s.Name))
                {
                    var capturedSkill = skill;
                    scroll.Add(new ForgeButtonBuilder(skill.Name)
                        .WithMarginBottom(2f)
                        .WithBackgroundColor(new Color(0.1f, 0.1f, 0.15f))
                        .OnClick(() => SelectSkill(capturedSkill, ctx))
                        .Build());
                }
                return scroll;
            }));

            // RIGHT: Inspector (70%)
            var inspectorPane = new ForgeContainerBuilder("SkillInspector")
                .WithFlexGrow(1f)
                .WithPadding(20f)
                .OnBuild(ve => _detailPanel = ve);

            rootBuilder.AddChild(listPanel).AddChild(inspectorPane);

            var root = rootBuilder.Build();
            if (_skills.Count > 0) SelectSkill(_skills[0], ctx);

            return root;
        }

        private void SelectSkill(GURPSSkillDefinition skill, GuiContext ctx)
        {
            // Roach Spray: Null safety for DOM race conditions
            if (skill == null || _detailPanel == null) return;

            _selectedSkill = skill;
            _detailPanel.Clear();

            var detailBuilder = new ForgeContainerBuilder("DetailContent")
                .AddChild(new ForgeLabelBuilder(skill.Name.ToUpper()).WithFontSize(24).WithBold().WithColor(Color.cyan))
                .AddChild(new ForgeLabelBuilder($"{skill.Type} | {skill.Difficulty} | {skill.BaseAttribute}").WithColor(Color.gray).WithMarginBottom(15f))
                .AddChild(new ForgeLabelBuilder(skill.Description).WithMarginBottom(20f));

            // Costing Table
            var table = new ForgeContainerBuilder("CostTable")
                .WithPadding(10f).WithBackgroundColor(new Color(0, 0, 0, 0.3f)).WithBorderRadius(5f);

            table.AddChild(new ForgeLabelBuilder("POINT COST PROGRESSION").WithFontSize(10).WithColor(Color.gray).WithMarginBottom(5f));

            for (int i = -3; i <= 3; i++)
            {
                int cost = GetPointCost(skill, i);
                string levelLabel = i == 0 ? $"{skill.BaseAttribute}" : $"{skill.BaseAttribute}{(i > 0 ? "+" : "")}{i}";

                table.AddChild(new ForgeContainerBuilder($"Row_{i}")
                    .WithDirection(FlexDirection.Row).WithJustifyContent(Justify.SpaceBetween)
                    .AddChild(new ForgeLabelBuilder(levelLabel))
                    .AddChild(new ForgeLabelBuilder($"{cost} pts").WithBold().WithColor(cost > 0 ? Color.white : Color.gray)));
            }

            detailBuilder.AddChild(table);
            _detailPanel.Add(detailBuilder.Build());
        }

        // --- FORGE GUI INTEGRATION STUBS ---

        public Action<VisualElement> GetGuiBuilder() => root => root.Add(CreateGui(new GuiContext()));
        public void ToUIDocument(string path) => WorkshopUxmlBaker.Bake(CreateGui(new GuiContext()), "SkillsRegistry");
        public void FromUIDocument(string path) { /* Ready for future runtime deserialization */ }

        // --- IFILTERWINDOWPROVIDER ---

        public void CreateComponentTree(List<ForgeFilterWindow.Element> tree)
        {
            tree.Add(new ForgeFilterWindow.GroupElement(0, "Skills Library"));
            foreach (var skill in _skills)
            {
                tree.Add(new ForgeFilterWindow.Element(1, skill.Name) { userData = skill });
            }
        }

        public bool GoToChild(ForgeFilterWindow.Element element, bool add) => element.userData is GURPSSkillDefinition;

        // Internal wrapper required for flat JSON array serialization in Unity
        [Serializable]
        private class CacheWrapper { public List<GURPSSkillDefinition> Skills; }
    }
}