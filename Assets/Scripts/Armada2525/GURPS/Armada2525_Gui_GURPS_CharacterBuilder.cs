using TheSingularityWorkshop.Armada2525;
using TheSingularityWorkshop.Builders.GuiBuilders;
using TheSingularityWorkshop.Forge.Builders.GuiBuilders;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UIElements;

public class Armada2525_Gui_GURPS_CharacterSheet : IGuiProvider
{
    public string Title => "GURPS 3E CHARACTER SHEET (DARK)";

    private GURPS3eCharacterData _character;

    // UI Elements that need refreshing
    private Label _pointsLabel;
    private VisualElement _skillsTableContainer;
    private DropdownField _skillSelector;

    // --- MOCK MASTER SKILL LIST ---
    private List<GURPSSkillDefinition> _masterSkills = new List<GURPSSkillDefinition>
    {
        new GURPSSkillDefinition { Name = "Astrogation", BaseAttribute = GURPSAttributeType.IQ, Difficulty = GURPSDifficulty.Average },
        new GURPSSkillDefinition { Name = "Piloting (Starship)", BaseAttribute = GURPSAttributeType.DX, Difficulty = GURPSDifficulty.Average },
        new GURPSSkillDefinition { Name = "Beam Weapons (Pistol)", BaseAttribute = GURPSAttributeType.DX, Difficulty = GURPSDifficulty.Easy },
        new GURPSSkillDefinition { Name = "Computer Programming", BaseAttribute = GURPSAttributeType.IQ, Difficulty = GURPSDifficulty.Hard },
        new GURPSSkillDefinition { Name = "Free Fall", BaseAttribute = GURPSAttributeType.DX, Difficulty = GURPSDifficulty.Average }
    };
    private GuiContext _lastCtx;

    public VisualElement CreateGui(GuiContext ctx)
    {
        _lastCtx = ctx;
        _character = new GURPS3eCharacterData();

        var builder = new GraphicalUserInterfaceBuilder("GURPS_Dark_Sheet")
            .WithBackgroundColor(new Color(0.1f, 0.1f, 0.12f)) // Dark Slate Background
            .WithFlexLayout(FlexDirection.Column, Justify.FlexStart, Align.Stretch)
            .WithPercentSize(100, 100);

        // --- HEADER ---
        builder.AddChild(c => {
            var header = new VisualElement
            {
                style = {
                flexDirection = FlexDirection.Row, backgroundColor = new Color(0.15f, 0.15f, 0.18f),
                paddingTop = 15, paddingBottom = 15, paddingLeft = 15, paddingRight =  15, borderBottomWidth = 2, borderBottomColor = Color.cyan
            }
            };

            header.Add(new Label("GURPS 3e ROSTER") { style = { color = Color.cyan, fontSize = 20, unityFontStyleAndWeight = FontStyle.Bold, width = 250 } });

            _pointsLabel = new Label("TOTAL POINTS: 0 / 100")
            {
                style = {
                color = Color.yellow, fontSize = 16, unityFontStyleAndWeight = FontStyle.Bold, marginLeft = 50, alignSelf = Align.Center
            }
            };
            header.Add(_pointsLabel);

            return header;
        });

        // --- BODY ---
        builder.AddChild(c => {
            var body = new VisualElement { style = { flexGrow = 1, flexDirection = FlexDirection.Row, paddingTop = 20, paddingRight = 20, paddingLeft = 20, paddingBottom = 20 } };

            // LEFT COLUMN: Attributes (Simplified for layout)
            var col1 = new VisualElement { style = { width = 250, paddingRight = 20, borderRightWidth = 1, borderRightColor = Color.gray } };
            col1.Add(CreateSectionHeader("ATTRIBUTES"));
            col1.Add(CreateAttributeRow("ST", _character.ST, v => { _character.ST = v; UpdateAll(); }));
            col1.Add(CreateAttributeRow("DX", _character.DX, v => { _character.DX = v; UpdateAll(); }));
            col1.Add(CreateAttributeRow("IQ", _character.IQ, v => { _character.IQ = v; UpdateAll(); }));
            col1.Add(CreateAttributeRow("HT", _character.HT, v => { _character.HT = v; UpdateAll(); }));
            body.Add(col1);

            // RIGHT COLUMN: SKILLS SYSTEM
            var col2 = new VisualElement { style = { flexGrow = 1, paddingLeft = 20 } };
            col2.Add(CreateSectionHeader("SKILLS & TRAINING"));

            // Master Skill Adder
            var adderRow = new VisualElement { style = { flexDirection = FlexDirection.Row, marginBottom = 15, alignItems = Align.Center } };

            _skillSelector = new DropdownField(_masterSkills.Select(s => s.Name).ToList(), 0) { style = { width = 250, backgroundColor = Color.black, color = Color.white } };
            adderRow.Add(_skillSelector);

            var addBtn = new Button(AddSelectedSkill)
            {
                text = "ADD SKILL",
                style = {
                backgroundColor = new Color(0, 0.4f, 0.2f), color = Color.white, height = 30, marginLeft = 10
            }
            };
            adderRow.Add(addBtn);
            col2.Add(adderRow);

            // The Skills Table
            _skillsTableContainer = new VisualElement { style = { flexGrow = 1, backgroundColor = new Color(0.08f, 0.08f, 0.1f), paddingTop = 10, paddingBottom = 10, paddingLeft = 10, paddingRight = 10, borderTopWidth = 1, borderTopColor = Color.gray } };
            col2.Add(_skillsTableContainer);

            body.Add(col2);
            return body;
        });
        var root = builder.Build();
        UpdateAll();
        return root;
    }

    // --- SKILLS LOGIC ---

    private void AddSelectedSkill()
    {
        string selectedName = _skillSelector.value;
        var definition = _masterSkills.FirstOrDefault(s => s.Name == selectedName);

        if (definition != null && !_character.Skills.Any(s => s.Definition.Name == definition.Name))
        {
            _character.Skills.Add(new GURPSCharacterSkill { Definition = definition, PointsInvested = 0.5f });
            UpdateAll();
        }
    }

    private void RenderSkillsTable()
    {
        _skillsTableContainer.Clear();

        // Table Header
        var headerRow = new VisualElement { style = { flexDirection = FlexDirection.Row, borderBottomWidth = 1, borderBottomColor = Color.gray, paddingBottom = 5, marginBottom = 5 } };
        headerRow.Add(new Label("SKILL NAME") { style = { width = 200, color = Color.gray, unityFontStyleAndWeight = FontStyle.Bold } });
        headerRow.Add(new Label("TYPE") { style = { width = 60, color = Color.gray, unityFontStyleAndWeight = FontStyle.Bold } });
        headerRow.Add(new Label("R-LEVEL") { style = { width = 80, color = Color.gray, unityFontStyleAndWeight = FontStyle.Bold } });
        headerRow.Add(new Label("LEVEL") { style = { width = 60, color = Color.white, unityFontStyleAndWeight = FontStyle.Bold } });
        headerRow.Add(new Label("PTS") { style = { width = 100, color = Color.yellow, unityFontStyleAndWeight = FontStyle.Bold, unityTextAlign = TextAnchor.MiddleCenter } });
        _skillsTableContainer.Add(headerRow);

        // Table Rows
        foreach (var skill in _character.Skills)
        {
            var row = new VisualElement { style = { flexDirection = FlexDirection.Row, alignItems = Align.Center, marginBottom = 5, backgroundColor = new Color(0.15f, 0.15f, 0.18f), 
                    paddingTop = 5, paddingRight = 5, paddingLeft = 5, paddingBottom = 5 } };

            row.Add(new Label(skill.Definition.Name) { style = { width = 200, color = Color.cyan } });
            row.Add(new Label(skill.Definition.TypeString) { style = { width = 60, color = Color.gray } });
            row.Add(new Label(skill.RelativeLevelString) { style = { width = 80, color = Color.gray } });
            row.Add(new Label(skill.GetActualLevel(_character).ToString()) { style = { width = 60, color = Color.white, fontSize = 16, unityFontStyleAndWeight = FontStyle.Bold } });

            // Points Controls
            var ptContainer = new VisualElement { style = { flexDirection = FlexDirection.Row, width = 100, justifyContent = Justify.Center } };
            ptContainer.Add(new Button(() => { AdjustSkillPoints(skill, false); }) { text = "-", style = { width = 25, backgroundColor = new Color(0.3f, 0.1f, 0.1f), color = Color.white } });
            ptContainer.Add(new Label(skill.PointsInvested.ToString("0.#")) { style = { width = 40, color = Color.yellow, unityTextAlign = TextAnchor.MiddleCenter } });
            ptContainer.Add(new Button(() => { AdjustSkillPoints(skill, true); }) { text = "+", style = { width = 25, backgroundColor = new Color(0.1f, 0.3f, 0.1f), color = Color.white } });
            row.Add(ptContainer);

            // Remove Button
            row.Add(new Button(() => { _character.Skills.Remove(skill); UpdateAll(); }) { text = "X", style = { width = 30, backgroundColor = Color.black, color = Color.red } });

            _skillsTableContainer.Add(row);
        }
    }

    private void AdjustSkillPoints(GURPSCharacterSkill skill, bool increase)
    {
        float[] steps = { 0.5f, 1f, 2f, 4f, 6f, 8f, 10f, 12f }; // standard 3e progression
        int currentIndex = System.Array.IndexOf(steps, skill.PointsInvested);
        if (currentIndex == -1) currentIndex = 0; // Fallback

        if (increase && currentIndex < steps.Length - 1)
            skill.PointsInvested = steps[currentIndex + 1];
        else if (!increase && currentIndex > 0)
            skill.PointsInvested = steps[currentIndex - 1];

        UpdateAll();
    }

    // --- UI HELPERS ---

    private void UpdateAll()
    {
        _pointsLabel.text = $"TOTAL POINTS: {(_character.TotalAttributePoints + _character.TotalSkillPoints)} / {_character.StartingPoints}";
        RenderSkillsTable();
    }

    private VisualElement CreateSectionHeader(string text)
    {
        return new Label(text)
        {
            style = {
            backgroundColor = new Color(0.2f, 0.2f, 0.25f), color = Color.white, unityFontStyleAndWeight = FontStyle.Bold,
            paddingTop = 8, paddingBottom = 8, paddingLeft = 8, paddingRight = 8, marginTop = 15, marginBottom = 10
        }
        };
    }

    private VisualElement CreateAttributeRow(string attrName, int startValue, System.Action<int> onChange)
    {
        var row = new VisualElement { style = { flexDirection = FlexDirection.Row, alignItems = Align.Center, marginBottom = 8 } };
        row.Add(new Label(attrName) { style = { color = Color.cyan, fontSize = 20, unityFontStyleAndWeight = FontStyle.Bold, width = 50 } });

        var valLabel = new Label(startValue.ToString()) { style = { color = Color.white, fontSize = 18, width = 30, unityTextAlign = TextAnchor.MiddleCenter } };

        row.Add(new Button(() => { int v = int.Parse(valLabel.text) - 1; valLabel.text = v.ToString(); onChange(v); }) { text = "-", style = { width = 30, backgroundColor = Color.black, color = Color.white } });
        row.Add(valLabel);
        row.Add(new Button(() => { int v = int.Parse(valLabel.text) + 1; valLabel.text = v.ToString(); onChange(v); }) { text = "+", style = { width = 30, backgroundColor = Color.black, color = Color.white } });

        return row;
    }

    public System.Action<VisualElement> GetGuiBuilder() => (root) => root.Add(CreateGui(new GuiContext()));
    public void FromUIDocument(string assetPath) => _lastCtx?.OnBuilt?.Invoke(GraphicalUserInterfaceBuilder.ConvertFromUIDocument(assetPath));
#if UNITY_EDITOR
    public void ToUIDocument(string assetPath) => GraphicalUserInterfaceBuilder.ConvertToUIDocument(CreateGui(new GuiContext()), assetPath);
#endif
}