using TheSingularityWorkshop.Armada2525.GURPS;
using TheSingularityWorkshop.Builders.GuiBuilders;
using TheSingularityWorkshop.Forge.Builders.GuiBuilders;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class Armada2525_Gui_GURPS_WeaponEditor : IGuiProvider
{
    public string Title => "WEAPON ARSENAL EDITOR";

    private GuiContext _lastCtx;
    private GURPS_CRUD_Builder<GURPSWeapon> _crudInterface;

    public VisualElement CreateGui(GuiContext ctx)
    {
        _lastCtx = ctx;
        DigitalGenericUniversalRolePlayingSystem.InitializeCoreSystem();

        _crudInterface = new GURPS_CRUD_Builder<GURPSWeapon>(
            title: "ARMORY DATABASE",
            getGlobalData: () => DigitalGenericUniversalRolePlayingSystem.Weapons.GetAll(),
            getDisplayName: (w) => w.Name,
            getSourceBook: (w) => w.SourceBookName,
            setSourceBook: (w, b) => w.SourceBookName = b,
            buildEditorForm: (w) => BuildWeaponForm(w),
            onSaveGlobal: (w) => DigitalGenericUniversalRolePlayingSystem.Weapons.Register(w),
            onDeleteGlobal: (w) => DigitalGenericUniversalRolePlayingSystem.Weapons.UnRegister(w),
            getSubtitle: (w) => $"TL {w.TechLevel} | {w.DamageString}",
            getSortKey: (w) => w.TechLevel,
            showAllBooksFilter: true
        );

        return _crudInterface.CreateGui(ctx);
    }

    private VisualElement BuildWeaponForm(GURPSWeapon weapon)
    {
        var form = new VisualElement();

        // --- 1. GENERAL INFO ---
        var bookTag = new Label($"Source Book: {weapon.SourceBookName}") { style = { color = Color.yellow, fontSize = 14, marginBottom = 15, unityFontStyleAndWeight = FontStyle.Bold } };
        form.Add(bookTag);

        var nameRow = new VisualElement { style = { flexDirection = FlexDirection.Row, marginBottom = 10 } };
        var nameField = new TextField("Weapon Name") { value = weapon.Name, style = { flexGrow = 1 } };
        nameField.Q<Label>().style.minWidth = 100;
        nameField.RegisterValueChangedCallback(e => weapon.Name = e.newValue);

        var tlField = new IntegerField("TL") { value = weapon.TechLevel, style = { width = 60, marginLeft = 10 } };
        tlField.RegisterValueChangedCallback(e => weapon.TechLevel = e.newValue);

        nameRow.Add(nameField); nameRow.Add(tlField);
        form.Add(nameRow);

        var descField = new TextField("Description") { value = weapon.Description, multiline = true, style = { marginBottom = 15, height = 50 } };
        descField.Q<VisualElement>("unity-text-input").style.whiteSpace = WhiteSpace.Normal;
        descField.RegisterValueChangedCallback(e => weapon.Description = e.newValue);
        form.Add(descField);

        // --- 2. THE STATISTICAL PROBABILITY GRAPH ---
        var graphHeaderRow = new VisualElement { style = { flexDirection = FlexDirection.Row, justifyContent = Justify.SpaceBetween, alignItems = Align.FlexEnd, marginTop = 15, marginBottom = 5 } };
        graphHeaderRow.Add(new Label("DAMAGE PROBABILITY DISTRIBUTION") { style = { color = Color.cyan, unityFontStyleAndWeight = FontStyle.Bold } });

        var showAllToggle = new Toggle("Compare vs All Books") { value = false };
        showAllToggle.Q<Label>().style.color = Color.gray;
        graphHeaderRow.Add(showAllToggle);

        form.Add(graphHeaderRow);

        var probGraph = new ProbabilityGraphBuilder(
            title: "",
            xAxisLabel: "Expected Damage ➔",
            yAxisLabel: "Tech Level (Dots) / Prob. (Curve) ➔",
            maxX: 100f,
            maxYProb: 0.20f
        );
        form.Add(probGraph.Build());

        // --- 3. DAMAGE BUILDER ---
        var sliderRow = new VisualElement { style = { flexDirection = FlexDirection.Row, alignItems = Align.Center, marginBottom = 20 } };
        var evSlider = new Slider("Expected Dmg", 1f, 100f) { style = { flexGrow = 1 } };

        var dmgMathRow = new VisualElement { style = { flexDirection = FlexDirection.Row, marginBottom = 20, alignItems = Align.Center, paddingBottom = 15, borderBottomWidth = 1, borderBottomColor = Color.gray } };
        var diceField = new IntegerField() { style = { width = 40 } };
        var addsField = new IntegerField() { style = { width = 40 } };
        var armorDivField = new TextField() { style = { width = 60 } };
        var dmgTypeField = new TextField() { style = { width = 60 } };
        var finalDmgLabel = new Label() { style = { color = Color.yellow, fontSize = 16, marginLeft = 15, unityFontStyleAndWeight = FontStyle.Bold } };

        int dice = 1, adds = 0; string armorDiv = "", dmgType = "pi";
        ParseDamageString(weapon.DamageString, ref dice, ref adds, ref armorDiv, ref dmgType);

        diceField.value = dice; addsField.value = adds;
        armorDivField.value = armorDiv; dmgTypeField.value = dmgType;
        evSlider.value = (dice * 3.5f) + adds;

        // --- 4. DATA DISPATCHER ---
        Action dispatchDataToGraph = () => {
            float mu = (diceField.value * 3.5f) + addsField.value;
            float variance = Mathf.Max(0.5f, diceField.value * 2.9167f);

            var dots = new List<ScatterDot>();
            var allWeapons = DigitalGenericUniversalRolePlayingSystem.Weapons.GetAll();

            foreach (var w in allWeapons)
            {
                if (!showAllToggle.value && w.SourceBookName != weapon.SourceBookName) continue;

                int wD = 0, wA = 0; string dump1 = "", dump2 = "";
                ParseDamageString(w.DamageString, ref wD, ref wA, ref dump1, ref dump2);
                float wEV = (wD * 3.5f) + wA;

                if (wEV > 0)
                {
                    dots.Add(new ScatterDot
                    {
                        X_Value = wEV,
                        Y_Percent = Mathf.Clamp((w.TechLevel / 12f) * 100f, 5f, 95f),
                        DotColor = w == weapon ? Color.yellow : new Color(0.7f, 0.7f, 0.7f, 0.8f),
                        TooltipTitle = w.Name,
                        TooltipData = $"TL {w.TechLevel} | Book: {w.SourceBookName}\nDmg: {w.DamageString} (EV: {wEV:F1})"
                    });
                }
            }
            probGraph.Redraw(mu, variance, dots);
        };

        showAllToggle.RegisterValueChangedCallback(e => dispatchDataToGraph());

        Action updateString = () => {
            string addStr = addsField.value == 0 ? "" : (addsField.value > 0 ? $"+{addsField.value}" : $"{addsField.value}");
            weapon.DamageString = $"{Mathf.Max(0, diceField.value)}d{addStr}{armorDivField.value} {dmgTypeField.value}".Trim();
            finalDmgLabel.text = weapon.DamageString;
            dispatchDataToGraph();
        };

        evSlider.RegisterValueChangedCallback(e => {
            float targetEV = e.newValue;
            int newDice = Mathf.Max(1, Mathf.RoundToInt(targetEV / 3.5f));
            int newAdds = Mathf.RoundToInt(targetEV - (newDice * 3.5f));
            diceField.SetValueWithoutNotify(newDice);
            addsField.SetValueWithoutNotify(newAdds);
            updateString();
        });

        diceField.RegisterValueChangedCallback(e => { evSlider.SetValueWithoutNotify((e.newValue * 3.5f) + addsField.value); updateString(); });
        addsField.RegisterValueChangedCallback(e => { evSlider.SetValueWithoutNotify((diceField.value * 3.5f) + e.newValue); updateString(); });
        armorDivField.RegisterValueChangedCallback(e => updateString());
        dmgTypeField.RegisterValueChangedCallback(e => updateString());

        sliderRow.Add(evSlider);
        form.Add(sliderRow);

        dmgMathRow.Add(diceField); dmgMathRow.Add(new Label(" d "));
        dmgMathRow.Add(addsField); dmgMathRow.Add(new Label(" + "));
        dmgMathRow.Add(armorDivField); dmgMathRow.Add(new Label(" "));
        dmgMathRow.Add(dmgTypeField);
        dmgMathRow.Add(finalDmgLabel);
        form.Add(dmgMathRow);

        // --- 5. GURPS 3e ULTRA-TECH STAT BLOCK ---
        form.Add(new Label("ULTRA-TECH STATS (3rd Edition Format)") { style = { color = Color.cyan, unityFontStyleAndWeight = FontStyle.Bold, marginBottom = 10 } });

        var statRow = new VisualElement { style = { flexDirection = FlexDirection.Row, flexWrap = Wrap.Wrap } };

        // Column A: Operational / Firing Mechanics
        var colA = new VisualElement { style = { width = 220, marginRight = 20 } };
        var malfField = new TextField("Malfunction") { value = weapon.Malfunction, style = { marginBottom = 5 } }; malfField.RegisterValueChangedCallback(e => weapon.Malfunction = e.newValue); colA.Add(malfField);
        var ssField = new TextField("SnapShot (SS)") { value = weapon.SnapShot, style = { marginBottom = 5 } }; ssField.RegisterValueChangedCallback(e => weapon.SnapShot = e.newValue); colA.Add(ssField);
        var accField = new TextField("Accuracy (Acc)") { value = weapon.Accuracy, style = { marginBottom = 5 } }; accField.RegisterValueChangedCallback(e => weapon.Accuracy = e.newValue); colA.Add(accField);
        var halfDField = new TextField("1/2D Range") { value = weapon.HalfDamageRange, style = { marginBottom = 5 } }; halfDField.RegisterValueChangedCallback(e => weapon.HalfDamageRange = e.newValue); colA.Add(halfDField);
        var maxField = new TextField("Max Range") { value = weapon.MaxRange, style = { marginBottom = 5 } }; maxField.RegisterValueChangedCallback(e => weapon.MaxRange = e.newValue); colA.Add(maxField);
        var rofField = new TextField("Rate of Fire") { value = weapon.RateOfFire, style = { marginBottom = 5 } }; rofField.RegisterValueChangedCallback(e => weapon.RateOfFire = e.newValue); colA.Add(rofField);
        var rclField = new TextField("Recoil (Rcl)") { value = weapon.Recoil, style = { marginBottom = 5 } }; rclField.RegisterValueChangedCallback(e => weapon.Recoil = e.newValue); colA.Add(rclField);
        statRow.Add(colA);

        // Column B: Physical / Logistical / Requirements
        var colB = new VisualElement { style = { width = 220 } };
        var stField = new IntegerField("Strength (ST)") { value = weapon.MinimumStrength, style = { marginBottom = 5 } }; stField.RegisterValueChangedCallback(e => weapon.MinimumStrength = e.newValue); colB.Add(stField);
        var weightField = new FloatField("Weight (Wt)") { value = weapon.Weight, style = { marginBottom = 5 } }; weightField.RegisterValueChangedCallback(e => weapon.Weight = e.newValue); colB.Add(weightField);

        var ammoRow = new VisualElement { style = { flexDirection = FlexDirection.Row, justifyContent = Justify.SpaceBetween, marginBottom = 5 } };
        var shotField = new TextField("Shots") { value = weapon.Shots, style = { flexGrow = 1, marginRight = 5 } }; shotField.RegisterValueChangedCallback(e => weapon.Shots = e.newValue); ammoRow.Add(shotField);
        var reloadField = new TextField("Reload") { value = weapon.ReloadTime, style = { width = 80 } }; reloadField.Q<Label>().style.minWidth = 50; reloadField.RegisterValueChangedCallback(e => weapon.ReloadTime = e.newValue); ammoRow.Add(reloadField);
        colB.Add(ammoRow);

        var costField = new IntegerField("Cost ($)") { value = weapon.Cost, style = { marginBottom = 5 } }; costField.RegisterValueChangedCallback(e => weapon.Cost = e.newValue); colB.Add(costField);
        var lcField = new IntegerField("Legality (LC)") { value = weapon.LegalityClass, style = { marginBottom = 5 } }; lcField.RegisterValueChangedCallback(e => weapon.LegalityClass = e.newValue); colB.Add(lcField);
        var usageField = new TextField("Usage") { value = weapon.UsageBehavior, style = { marginBottom = 5 } }; usageField.RegisterValueChangedCallback(e => weapon.UsageBehavior = e.newValue); colB.Add(usageField);
        statRow.Add(colB);

        form.Add(statRow);

        dispatchDataToGraph();
        finalDmgLabel.text = weapon.DamageString;

        return form;
    }

    private void ParseDamageString(string raw, ref int dice, ref int adds, ref string armorDiv, ref string dmgType)
    {
        try
        {
            if (string.IsNullOrEmpty(raw)) return;
            string r = raw.ToLower().Trim();

            int spaceIdx = r.IndexOf(' ');
            if (spaceIdx > 0)
            {
                dmgType = raw.Substring(spaceIdx + 1).Trim();
                r = r.Substring(0, spaceIdx);
            }

            int pStart = r.IndexOf('(');
            int pEnd = r.IndexOf(')');
            if (pStart > 0 && pEnd > pStart)
            {
                armorDiv = r.Substring(pStart, pEnd - pStart + 1);
                r = r.Remove(pStart, pEnd - pStart + 1);
            }

            string[] dSplit = r.Split('d');
            if (dSplit.Length > 0 && int.TryParse(dSplit[0], out int d)) dice = d;
            if (dSplit.Length > 1 && !string.IsNullOrEmpty(dSplit[1]))
            {
                if (dSplit[1].StartsWith("+")) int.TryParse(dSplit[1].Substring(1), out adds);
                else if (dSplit[1].StartsWith("-")) { int.TryParse(dSplit[1].Substring(1), out int sub); adds = -sub; }
            }
        }
        catch { Debug.LogWarning($"[GURPS] Failed to parse damage string: {raw}"); }
    }

    public Action<VisualElement> GetGuiBuilder() => (root) => root.Add(CreateGui(new GuiContext()));
    public void FromUIDocument(string assetPath) => _lastCtx?.OnBuilt?.Invoke(GraphicalUserInterfaceBuilder.ConvertFromUIDocument(assetPath));
#if UNITY_EDITOR
    public void ToUIDocument(string assetPath) => GraphicalUserInterfaceBuilder.ConvertToUIDocument(CreateGui(new GuiContext()), assetPath);
#endif
}