using TheSingularityWorkshop.Armada2525;
using TheSingularityWorkshop.Builders.GuiBuilders;
using TheSingularityWorkshop.Forge.Builders.GuiBuilders;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UIElements;

public class Armada2525_Gui_AdvisorsView : IGuiProvider
{
    public string Title => "IMPERIAL COUNCIL & ADVISORS";

    private Armada2525 _game;
    private GuiContext _lastCtx;

    private VisualElement _mainContentArea;
    private VisualElement _rosterContainer;

    // --- MOCK SIMULATION DATA ---
    public enum AdvisorRole { Empty, Locked, Espionage, Education, Finance, Military, Science, Logistics }

    private class Advisor
    {
        public int RankIndex; // 1st, 2nd, 3rd in command
        public string Name;
        public AdvisorRole Role;
        public string Title;
        public string Trait;
        public string TraitDescription;
        public Color ThemeColor;
        public int Salary;
    }

    private List<Advisor> _councilSlots;
    private Advisor _selectedAdvisor;

    // Education State
    private long _totalEducationBudget = 10000000; // 10 Billion
    private Dictionary<string, float> _eduAllocations = new Dictionary<string, float>
    {
        { "Children's Education", 0.2f }, { "Young Adult", 0.2f }, { "Adult", 0.15f },
        { "Extended", 0.1f }, { "Advanced", 0.15f }, { "Specialized", 0.1f }, { "Hyper-Focused", 0.1f }
    };

    // Espionage State (MOO2 Style)
    private int _totalSpies = 45;
    private int _spiesDefending = 30;
    private int _spiesHiding = 5;
    private int _spiesOffense = 10;
    private string _espionageTarget = "The Kraal Hegemony";

    public VisualElement CreateGui(GuiContext ctx)
    {
        _lastCtx = ctx;
        _game = UnityEngine.Object.FindAnyObjectByType<Armada2525>();
        if (_councilSlots == null) InitializeMockData();

        var builder = new GraphicalUserInterfaceBuilder("Advisors_Root")
            .WithBackgroundColor(new Color(0.01f, 0.02f, 0.03f, 0.98f))
            .WithPercentSize(100, 100)
            .WithFlexLayout(FlexDirection.Row, Justify.FlexStart, Align.Stretch);

        // --- LEFT SIDEBAR: THE COUNCIL ROSTER (30%) ---
        builder.AddChild(c => {
            var sidebar = new VisualElement { style = { width = Length.Percent(30), backgroundColor = new Color(0.05f, 0.07f, 0.1f), borderRightWidth = 2, borderRightColor = Color.cyan, paddingTop = 20, paddingBottom = 20, paddingLeft = 15, paddingRight = 15 } };

            sidebar.Add(new Button(() => _game?.SwitchGui("GalaxyView")) { text = "<< RETURN TO COMMAND", style = { backgroundColor = Color.clear, color = Color.gray, borderBottomWidth = 1, borderBottomColor = Color.gray, marginBottom = 20, height = 30 } });

            sidebar.Add(new Label("THE IMPERIAL COUNCIL") { style = { color = Color.cyan, fontSize = 22, unityFontStyleAndWeight = FontStyle.Bold, marginBottom = 5 } });
            sidebar.Add(new Label("Advisors automate empire management. Their Rank determines how heavily their personal traits influence autonomous decisions.") { style = { color = Color.gray, fontSize = 10, whiteSpace = WhiteSpace.Normal, marginBottom = 20 } });

            var scroll = new ScrollView { style = { flexGrow = 1 } };
            _rosterContainer = new VisualElement();
            RenderCouncilRoster();
            scroll.Add(_rosterContainer);
            sidebar.Add(scroll);

            return sidebar;
        });

        // --- RIGHT AREA: ADVISOR PORTFOLIO (70%) ---
        builder.AddChild(c => {
            _mainContentArea = new VisualElement { style = { width = Length.Percent(70), paddingLeft = 30, paddingTop = 30, paddingRight = 30, paddingBottom = 30 } };
            RenderAdvisorPortfolio();
            return _mainContentArea;
        });

        return builder.Build();
    }

    // --------------------------------------------------------------------------------
    // RENDERING
    // --------------------------------------------------------------------------------

    private void RenderCouncilRoster()
    {
        _rosterContainer.Clear();

        // Sort by Rank
        _councilSlots = _councilSlots.OrderBy(a => a.RankIndex).ToList();

        for (int i = 0; i < _councilSlots.Count; i++)
        {
            var adv = _councilSlots[i];
            bool isSelected = _selectedAdvisor == adv;

            var card = new VisualElement { style = { flexDirection = FlexDirection.Row, backgroundColor = isSelected ? new Color(0.1f, 0.2f, 0.3f) : new Color(0.1f, 0.1f, 0.15f), marginBottom = 10, paddingLeft = 10, paddingTop = 10, paddingRight = 10, paddingBottom = 10, borderLeftWidth = 4, borderLeftColor = adv.ThemeColor } };

            // Rank controls (Up/Down)
            var rankCol = new VisualElement { style = { width = 30, alignItems = Align.Center, justifyContent = Justify.Center, marginRight = 10 } };
            if (adv.Role != AdvisorRole.Locked && adv.Role != AdvisorRole.Empty)
            {
                rankCol.Add(new Label($"#{adv.RankIndex}") { style = { color = Color.yellow, fontSize = 14, unityFontStyleAndWeight = FontStyle.Bold, marginBottom = 5 } });
                if (i > 0) rankCol.Add(new Button(() => SwapRank(i, i - 1)) { text = "▲", style = { height = 20, width = 25, fontSize = 8, paddingLeft = 0, paddingRight = 0 } });
                if (i < _councilSlots.Count - 1 && _councilSlots[i + 1].Role != AdvisorRole.Locked) rankCol.Add(new Button(() => SwapRank(i, i + 1)) { text = "▼", style = { height = 20, width = 25, fontSize = 8, paddingLeft = 0, paddingRight = 0 } });
            }
            card.Add(rankCol);

            // Info
            var infoCol = new VisualElement { style = { flexGrow = 1, justifyContent = Justify.Center } };
            if (adv.Role == AdvisorRole.Locked)
            {
                infoCol.Add(new Label("SLOT LOCKED") { style = { color = Color.red, unityFontStyleAndWeight = FontStyle.Bold } });
                infoCol.Add(new Label("Requires further Imperial expansion.") { style = { color = Color.gray, fontSize = 9 } });
            }
            else if (adv.Role == AdvisorRole.Empty)
            {
                infoCol.Add(new Label("VACANT SEAT") { style = { color = Color.gray, unityFontStyleAndWeight = FontStyle.Bold } });
                infoCol.Add(new Label("Click to review candidates.") { style = { color = Color.cyan, fontSize = 9 } });
            }
            else
            {
                infoCol.Add(new Label(adv.Name) { style = { color = Color.white, fontSize = 16, unityFontStyleAndWeight = FontStyle.Bold } });
                infoCol.Add(new Label(adv.Title) { style = { color = adv.ThemeColor, fontSize = 10 } });
            }
            card.Add(infoCol);

            // Click to select
            card.RegisterCallback<ClickEvent>(e => { _selectedAdvisor = adv; RenderCouncilRoster(); RenderAdvisorPortfolio(); });
            _rosterContainer.Add(card);
        }
    }

    private void RenderAdvisorPortfolio()
    {
        _mainContentArea.Clear();

        if (_selectedAdvisor == null)
        {
            _mainContentArea.Add(new Label("SELECT A COUNCIL SEAT") { style = { color = Color.gray, fontSize = 24, alignSelf = Align.Center, marginTop = 100 } });
            return;
        }

        if (_selectedAdvisor.Role == AdvisorRole.Locked || _selectedAdvisor.Role == AdvisorRole.Empty)
        {
            RenderRecruitmentPanel();
            return;
        }
        if (_selectedAdvisor.Role == AdvisorRole.Logistics)
        {
            _mainContentArea.Add(new Label("DEPARTMENT OF IMPERIAL LOGISTICS") { style = { color = Color.white, fontSize = 18, unityFontStyleAndWeight = FontStyle.Bold, marginBottom = 20 } });

            var launchBtn = new Button(() => {
                if (!_game.GUI.ContainsKey("TransportTycoon"))
                    _game.GUI["TransportTycoon"] = new Armada2525_Gui_TransportTycoon();
                _game.SwitchGui("TransportTycoon");
            })
            {
                text = "ENTER LIVE DISPATCH SIMULATOR",
                style = { backgroundColor = Color.magenta, color = Color.white, height = 50, width = 300, unityFontStyleAndWeight = FontStyle.Bold }
            };
            _mainContentArea.Add(launchBtn);
        }
        // --- ADVISOR HEADER ---
        var header = new VisualElement { style = { flexDirection = FlexDirection.Row, marginBottom = 30, borderBottomWidth = 1, borderBottomColor = _selectedAdvisor.ThemeColor, paddingBottom = 20 } };

        var profileImg = new VisualElement { style = { width = 100, height = 100, backgroundColor = _selectedAdvisor.ThemeColor, marginRight = 20, borderTopWidth = 2, borderBottomWidth = 2, borderLeftWidth = 2, borderRightWidth = 2, borderTopColor = Color.white, borderBottomColor = Color.white, borderLeftColor = Color.white, borderRightColor = Color.white } };
        profileImg.Add(new Label("PORTRAIT") { style = { color = Color.black, alignSelf = Align.Center, marginTop = 40 } });
        header.Add(profileImg);

        var bio = new VisualElement { style = { flexGrow = 1 } };
        bio.Add(new Label(_selectedAdvisor.Name.ToUpper()) { style = { color = Color.white, fontSize = 28, unityFontStyleAndWeight = FontStyle.Bold } });
        bio.Add(new Label(_selectedAdvisor.Title) { style = { color = _selectedAdvisor.ThemeColor, fontSize = 14, marginBottom = 10 } });

        var traitBox = new VisualElement { style = { backgroundColor = new Color(0, 0, 0, 0.5f), paddingLeft = 10, paddingTop = 5, paddingBottom = 5, borderLeftWidth = 2, borderLeftColor = Color.yellow } };
        traitBox.Add(new Label($"Known Trait: {_selectedAdvisor.Trait}") { style = { color = Color.yellow, fontSize = 12, unityFontStyleAndWeight = FontStyle.Bold } });
        traitBox.Add(new Label(_selectedAdvisor.TraitDescription) { style = { color = Color.gray, fontSize = 10, whiteSpace = WhiteSpace.Normal, marginTop = 2 } });
        bio.Add(traitBox);

        header.Add(bio);

        var rankWeight = 1.0f - ((_selectedAdvisor.RankIndex - 1) * 0.2f); // 1st = 1.0, 2nd = 0.8, etc.
        var stats = new VisualElement { style = { width = 200, alignItems = Align.FlexEnd } };
        stats.Add(new Label($"COUNCIL RANK: #{_selectedAdvisor.RankIndex}") { style = { color = Color.cyan, fontSize = 14, unityFontStyleAndWeight = FontStyle.Bold } });
        stats.Add(new Label($"Decision Weight: {rankWeight:P0}") { style = { color = Color.white, fontSize = 12, marginBottom = 10 } });
        stats.Add(new Button(() => Debug.Log("Dismissing Advisor..")) { text = "DISMISS ADVISOR", style = { backgroundColor = new Color(0.4f, 0.1f, 0.1f), color = Color.white, height = 30 } });
        header.Add(stats);

        _mainContentArea.Add(header);

        // --- SPECIFIC PORTFOLIOS ---
        if (_selectedAdvisor.Role == AdvisorRole.Education) RenderEducationPortfolio();
        else if (_selectedAdvisor.Role == AdvisorRole.Espionage) RenderEspionagePortfolio();
        else if (_selectedAdvisor.Role == AdvisorRole.Finance) RenderFinancePortfolio();
    }

    // --- EDUCATION PORTFOLIO ---
    private void RenderEducationPortfolio()
    {
        _mainContentArea.Add(new Label("DEPARTMENT OF EDUCATION & CITIZEN DEVELOPMENT") { style = { color = Color.white, fontSize = 18, unityFontStyleAndWeight = FontStyle.Bold, marginBottom = 10 } });
        _mainContentArea.Add(new Label("Set the gross educational budget and apportion grants to facility types across the empire. Proper funding ensures rapid technological assimilation and leader generation.") { style = { color = Color.gray, fontSize = 12, marginBottom = 20, whiteSpace = WhiteSpace.Normal } });

        var topRow = new VisualElement { style = { flexDirection = FlexDirection.Row, marginBottom = 20, alignItems = Align.Center } };
        topRow.Add(new Label("GLOBAL EDUCATION BUDGET:") { style = { color = Color.cyan, fontSize = 14, marginRight = 10 } });

        // Mock input field for budget
        var budgetField = new TextField { value = _totalEducationBudget.ToString(), style = { width = 200, backgroundColor = Color.black, color = Color.white } };
        budgetField.RegisterValueChangedCallback(e => { if (long.TryParse(e.newValue, out long v)) { _totalEducationBudget = v; RenderAdvisorPortfolio(); } });
        topRow.Add(budgetField);
        topRow.Add(new Label("cr / Turn") { style = { color = Color.gray, marginLeft = 10 } });
        _mainContentArea.Add(topRow);

        var scroll = new ScrollView { style = { flexGrow = 1 } };
        foreach (var facility in _eduAllocations.Keys.ToList())
        {
            scroll.Add(CreateAutoBalancingSlider(facility, _eduAllocations, _totalEducationBudget, _selectedAdvisor.ThemeColor));
        }
        _mainContentArea.Add(scroll);
    }

    // --- ESPIONAGE PORTFOLIO ---
    private void RenderEspionagePortfolio()
    {
        _mainContentArea.Add(new Label("INTELLIGENCE & BLACK OPERATIONS (MOO2 STYLE)") { style = { color = Color.white, fontSize = 18, unityFontStyleAndWeight = FontStyle.Bold, marginBottom = 10 } });
        _mainContentArea.Add(new Label("Allocate your spy network. Defense catches enemy agents. Offense infiltrates the selected target. Hiding spies gather passive intel without risking capture.") { style = { color = Color.gray, fontSize = 12, marginBottom = 20, whiteSpace = WhiteSpace.Normal } });

        var dash = new VisualElement { style = { flexDirection = FlexDirection.Row, marginBottom = 30 } };

        var statsBox = new VisualElement { style = { width = 250, backgroundColor = new Color(0.05f, 0.05f, 0.05f), paddingLeft = 15, paddingTop = 15, paddingBottom = 15, borderLeftWidth = 4, borderLeftColor = Color.magenta, marginRight = 20 } };
        statsBox.Add(new Label("NETWORK STATUS") { style = { color = Color.magenta, unityFontStyleAndWeight = FontStyle.Bold, marginBottom = 10 } });
        statsBox.Add(new Label($"Total Active Agents: {_totalSpies}") { style = { color = Color.white, fontSize = 16, marginBottom = 10 } });
        statsBox.Add(new Button(() => { _totalSpies++; RenderAdvisorPortfolio(); }) { text = "TRAIN AGENT (5,000 cr)", style = { backgroundColor = new Color(0.2f, 0.1f, 0.3f), color = Color.white } });
        dash.Add(statsBox);

        var targetBox = new VisualElement { style = { flexGrow = 1, backgroundColor = new Color(0.1f, 0.05f, 0.05f), paddingLeft = 15, paddingTop = 15, paddingRight = 15 } };
        targetBox.Add(new Label("PRIMARY OFFENSIVE TARGET") { style = { color = Color.red, unityFontStyleAndWeight = FontStyle.Bold, marginBottom = 10 } });
        targetBox.Add(new DropdownField(new List<string> { "The Kraal Hegemony", "United Terran Front", "Cygnus Syndicate" }, _espionageTarget) { style = { width = 300, marginBottom = 10 } });
        targetBox.Add(new Label("Mission: STEAL TECHNOLOGY") { style = { color = Color.yellow, fontSize = 12 } });
        dash.Add(targetBox);

        _mainContentArea.Add(dash);

        // Spy Allocation (Manual +/-, MOO2 style)
        _mainContentArea.Add(new Label("AGENT ALLOCATION") { style = { color = Color.white, fontSize = 14, unityFontStyleAndWeight = FontStyle.Bold, borderBottomWidth = 1, borderBottomColor = Color.gray, marginBottom = 15 } });

        _mainContentArea.Add(CreateSpyRow("COUNTER-INTELLIGENCE (Defense)", ref _spiesDefending, Color.cyan));
        _mainContentArea.Add(CreateSpyRow("DEEP COVER (Hiding/Passive Intel)", ref _spiesHiding, Color.gray));
        _mainContentArea.Add(CreateSpyRow("INFILTRATION (Offense)", ref _spiesOffense, Color.red));
    }

    private void RenderFinancePortfolio()
    {
        _mainContentArea.Add(new Label("IMPERIAL TREASURY & ECONOMICS") { style = { color = Color.white, fontSize = 18, unityFontStyleAndWeight = FontStyle.Bold, marginBottom = 10 } });
        _mainContentArea.Add(new Label("Manage taxes, issue bonds, and receive economic advice.") { style = { color = Color.gray, fontSize = 12, marginBottom = 20 } });

        var adviceBox = new VisualElement { style = { backgroundColor = new Color(0.1f, 0.15f, 0.1f), paddingLeft = 15, paddingTop = 15, paddingRight = 15, paddingBottom = 15, borderLeftWidth = 4, borderLeftColor = Color.green, marginBottom = 20 } };
        adviceBox.Add(new Label($"ADVICE FROM {_selectedAdvisor.Name.ToUpper()}:") { style = { color = Color.green, unityFontStyleAndWeight = FontStyle.Bold, marginBottom = 5 } });
        adviceBox.Add(new Label("\"My Lord, maintaining a high tax rate while running deficit spending on habitats is a recipe for inflation. Consider issuing war bonds to cover the fleet expansion, or dial back the educational grants.\"") { style = { color = Color.white,  whiteSpace = WhiteSpace.Normal } });
        _mainContentArea.Add(adviceBox);
    }

    private void RenderRecruitmentPanel()
    {
        _mainContentArea.Add(new Label("RECRUITMENT PROTOCOL") { style = { color = Color.white, fontSize = 24, unityFontStyleAndWeight = FontStyle.Bold, marginBottom = 10 } });
        if (_selectedAdvisor.Role == AdvisorRole.Locked)
        {
            _mainContentArea.Add(new Label("This Council Seat is currently locked. Increase your Empire's population and bureaucratic infrastructure to unlock.") { style = { color = Color.red } });
            return;
        }

        _mainContentArea.Add(new Label("Select a candidate to fill this vacant seat. Be warned: their traits will influence empire policy.") { style = { color = Color.gray, marginBottom = 20 } });
        // (Mock Recruitment Grid would go here)
    }

    // --------------------------------------------------------------------------------
    // UI UTILS & LOGIC
    // --------------------------------------------------------------------------------

    private void SwapRank(int idxA, int idxB)
    {
        // Simple swap logic
        var tempRank = _councilSlots[idxA].RankIndex;
        _councilSlots[idxA].RankIndex = _councilSlots[idxB].RankIndex;
        _councilSlots[idxB].RankIndex = tempRank;

        RenderCouncilRoster();
        if (_selectedAdvisor != null) RenderAdvisorPortfolio();
    }

    private VisualElement CreateSpyRow(string title, ref int count, Color theme)
    {
        var row = new VisualElement { style = { flexDirection = FlexDirection.Row, alignItems = Align.Center, marginBottom = 10, backgroundColor = new Color(0.1f, 0.1f, 0.15f), paddingLeft = 10, paddingTop = 5, paddingBottom = 5, paddingRight = 10 } };
        row.Add(new Label(title) { style = { color = theme, width = 250, unityFontStyleAndWeight = FontStyle.Bold } });

        var c = count; // local copy for lambda
        row.Add(new Button(() => { if (c > 0) { c--; AdjustSpies(title, c); } }) { text = "-", style = { width = 30 } });
        row.Add(new Label(count.ToString()) { style = { color = Color.white, width = 50, unityTextAlign = TextAnchor.MiddleCenter, fontSize = 16 } });
        row.Add(new Button(() => { if (_spiesDefending + _spiesHiding + _spiesOffense < _totalSpies) { c++; AdjustSpies(title, c); } }) { text = "+", style = { width = 30 } });

        return row;
    }

    private void AdjustSpies(string category, int newVal)
    {
        if (category.Contains("Defense")) _spiesDefending = newVal;
        else if (category.Contains("Deep")) _spiesHiding = newVal;
        else if (category.Contains("Offense")) _spiesOffense = newVal;
        RenderAdvisorPortfolio();
    }

    // Auto-balancer from Research, adapted for Education Budget
    private VisualElement CreateAutoBalancingSlider(string key, Dictionary<string, float> dict, long totalBudget, Color theme)
    {
        var container = new VisualElement { style = { marginBottom = 15 } };
        var header = new VisualElement { style = { flexDirection = FlexDirection.Row, justifyContent = Justify.SpaceBetween } };
        header.Add(new Label(key) { style = { color = Color.white, fontSize = 12, unityFontStyleAndWeight = FontStyle.Bold } });
        header.Add(new Label($"{(dict[key] * totalBudget):N0} cr ({dict[key]:P1})") { style = { color = theme, fontSize = 12 } });
        container.Add(header);

        var slider = new Slider(0f, 1f) { value = dict[key] };
        slider.RegisterValueChangedCallback(evt => {
            float delta = evt.newValue - dict[key];
            dict[key] = evt.newValue;
            var otherKeys = dict.Keys.Where(k => k != key).ToList();
            float sumOthers = otherKeys.Sum(k => dict[k]);

            if (sumOthers == 0 && delta > 0) { foreach (var k in otherKeys) dict[k] = 0f; dict[key] = 1.0f; }
            else
            {
                foreach (var k in otherKeys)
                {
                    dict[k] -= delta * (dict[k] / sumOthers);
                    if (dict[k] < 0) dict[k] = 0;
                }
            }
            float total = dict.Values.Sum();
            if (total > 0) { foreach (var k in dict.Keys.ToList()) dict[k] /= total; }
            RenderAdvisorPortfolio();
        });

        container.Add(slider);
        return container;
    }

    private void InitializeMockData()
    {
        _councilSlots = new List<Advisor>
        {
            new Advisor { RankIndex = 1, Name = "Lord Varis", Role = AdvisorRole.Espionage, Title = "Minister of Intelligence", ThemeColor = Color.magenta, Trait = "Ruthless Pragmatist", TraitDescription = "Disturbingly comfortable with assassinations. +20% Offense success, but highly unpopular with pacifist factions." },
            new Advisor { RankIndex = 2, Name = "Dr. Elara Vance", Role = AdvisorRole.Education, Title = "Director of Citizen Development", ThemeColor = Color.cyan, Trait = "Draconian Pedagogy", TraitDescription = "Believes forced learning is effective learning. +15% assimilation speed, -5% Global Morale." },
            new Advisor { RankIndex = 3, Name = "Executor Grell", Role = AdvisorRole.Finance, Title = "Master of Coin", ThemeColor = Color.green, Trait = "Corrupt but Capable", TraitDescription = "Embezzlement rumors surround him, but he yields high returns. +10% Trade Income, but random Treasury 'discrepancies' occur." },
            new Advisor { RankIndex = 4, Name = "High Dispatcher Thorne", Role = AdvisorRole.Logistics, Title = "Minister of Trade", ThemeColor = Color.magenta, Trait = "Tycoon", TraitDescription = "Unlocks the Transport Tycoon live simulation." },
            new Advisor { RankIndex = 5, Role = AdvisorRole.Locked },
            new Advisor { RankIndex = 6, Role = AdvisorRole.Locked }
        };
        _selectedAdvisor = _councilSlots[0];
    }

    public Action<VisualElement> GetGuiBuilder() => (root) => root.Add(CreateGui(new GuiContext()));
    public void FromUIDocument(string assetPath) => _lastCtx?.OnBuilt?.Invoke(GraphicalUserInterfaceBuilder.ConvertFromUIDocument(assetPath));
#if UNITY_EDITOR
    public void ToUIDocument(string assetPath) => GraphicalUserInterfaceBuilder.ConvertToUIDocument(CreateGui(new GuiContext()), assetPath);
#endif
}