#if UNITY_EDITOR
using Assets.Scripts.MastersOfOrionII.Economy;
using Assets.Scripts.MastersOfOrionII.Government;
using System;
using UnityEngine;
using UnityEngine.UIElements;
using Workshop.UI_And_Tools.Forge.Builders.GuiBuilders;

namespace Assets.Scripts.MastersOfOrionII
{
    public class MastersOfOrionII_Gui_GovernmentView : IGuiProvider
    {
        public string Title => "IMPERIAL COMMAND NEXUS";

        private GuiContext _guiContext;
        private MastersOfOrionII_Game _game;
        private VisualElement _rootContainer;
        private VisualElement _briefingPanel;

        // The current state of the player's empire
        private GovernmentContext _govContext;
        private CabinetMember _selectedAdvisor;

        // We will pass an Action down so the UI can hot-swap to the Builders when the "+" is clicked
        private Action<string> _onNavigateToBuilder;

        public MastersOfOrionII_Gui_GovernmentView() : this(null, null)
        {
        }

        public MastersOfOrionII_Gui_GovernmentView(GovernmentContext context, Action<string> onNavigate)
        {
            _govContext = context ?? GenerateMockGovernment();
            _onNavigateToBuilder = onNavigate;
        }

        public VisualElement CreateGui(GuiContext context)
        {
            _guiContext = context;
            _game = UnityEngine.Object.FindAnyObjectByType<MastersOfOrionII_Game>();

            var builder = new GraphicalUserInterfaceBuilder("GovView_Root")
                .WithBackgroundColor(new Color(0.02f, 0.02f, 0.03f))
                .WithFlexLayout(FlexDirection.Column, Justify.FlexStart, Align.Stretch)
                .WithPercentSize(100, 100);

            // --- HEADER: THE THRONE ---
            builder.AddChild(c =>
            {
                var header = new VisualElement { style = { height = 70, flexDirection = FlexDirection.Row, backgroundColor = new Color(0.05f, 0.05f, 0.08f), borderBottomWidth = 2, borderBottomColor = new Color(0.8f, 0.6f, 0.1f), paddingLeft = 20, paddingRight = 20, alignItems = Align.Center } };

                header.Add(new Label("IMPERIAL COMMAND NEXUS") { style = { color = new Color(0.8f, 0.6f, 0.1f), fontSize = 24, unityFontStyleAndWeight = FontStyle.Bold, flexGrow = 1 } });

                var treasuryBox = new VisualElement { style = { alignItems = Align.FlexEnd } };
                treasuryBox.Add(new Label("IMPERIAL TREASURY") { style = { color = Color.gray, fontSize = 10 } });
                treasuryBox.Add(new Label($"{_govContext.ImperialTreasury:N0} cr") { style = { color = Color.green, fontSize = 18, unityFontStyleAndWeight = FontStyle.Bold } });
                header.Add(treasuryBox);

                header.Add(new VisualElement { style = { width = 20 } });
                header.Add(new Button(() => _game?.SwitchGui("MainMenu")) { text = "RETURN TO SANCTUM", style = { height = 35, width = 150, backgroundColor = new Color(0.2f, 0.2f, 0.2f), color = Color.white } });

                return header;
            });

            // --- MAIN DASHBOARD LAYOUT ---
            builder.AddChild(c =>
            {
                var mainLayout = new VisualElement { style = { flexGrow = 1, flexDirection = FlexDirection.Row } };

                // LEFT: THE CABINET
                mainLayout.Add(CreateCabinetSidebar());

                // CENTER: THE BRIEFING ROOM (Dynamic Content)
                _briefingPanel = new VisualElement { style = { flexGrow = 1, backgroundColor = new Color(0.01f, 0.01f, 0.02f), paddingLeft = 20, paddingRight = 20, paddingTop = 20, paddingBottom = 20 } };
                RenderBriefingRoom();
                mainLayout.Add(_briefingPanel);

                // RIGHT: EDICTS & LAWS
                mainLayout.Add(CreateEdictsSidebar());

                return mainLayout;
            });

            _rootContainer = builder.Build();

            // Setup a recurring UI tick so we can watch the economy fluctuate live!
            _rootContainer.RegisterCallback<AttachToPanelEvent>(e =>
            {
                _rootContainer.schedule.Execute(() =>
                {
                    // In a full game, your FSM is ticking this data. We just refresh the visual numbers here.
                    if (_selectedAdvisor != null && _selectedAdvisor.Department.Name.Contains("Economy"))
                    {
                        RenderBriefingRoom(); // Redraws the stock ticker
                    }
                }).Every(1000); // 1 second refresh for the dashboard
            });

            return _rootContainer;
        }

        // --- THE CABINET (ADVISORS) ---
        private VisualElement CreateCabinetSidebar()
        {
            var sidebar = new VisualElement { style = { width = 250, backgroundColor = new Color(0.04f, 0.04f, 0.06f), borderRightWidth = 2, borderRightColor = new Color(0.8f, 0.6f, 0.1f), paddingLeft = 10, paddingRight = 10, paddingTop = 15 } };

            sidebar.Add(new Label("THE CABINET") { style = { color = new Color(0.8f, 0.6f, 0.1f), fontSize = 16, unityFontStyleAndWeight = FontStyle.Bold, marginBottom = 15 } });

            var scroll = new ScrollView(ScrollViewMode.Vertical) { style = { flexGrow = 1 } };

            foreach (var advisor in _govContext.ActiveCabinet)
            {
                bool isSelected = _selectedAdvisor == advisor;
                var btn = new Button(() => { _selectedAdvisor = advisor; RenderBriefingRoom(); })
                {
                    style = {
                        height = 60, marginBottom = 5, paddingLeft = 10, justifyContent = Justify.Center, alignItems = Align.FlexStart,
                        backgroundColor = isSelected ? new Color(0.2f, 0.2f, 0.3f) : new Color(0.1f, 0.1f, 0.15f),
                        borderLeftWidth = isSelected ? 4 : 0, borderLeftColor = new Color(0.8f, 0.6f, 0.1f)
                    }
                };

                string deptName = advisor.Department != null ? advisor.Department.Name : "Unassigned";
                btn.Add(new Label(deptName.ToUpper()) { style = { color = Color.gray, fontSize = 9, unityFontStyleAndWeight = FontStyle.Bold } });
                btn.Add(new Label(advisor.Name) { style = { color = Color.white, fontSize = 14 } });

                scroll.Add(btn);
            }

            sidebar.Add(scroll);

            // THE "+" BUTTON: Create a new Ministry or Hire a new Advisor
            var appointBtn = new Button(() => _onNavigateToBuilder?.Invoke("CabinetBuilder")) { text = "+ APPOINT NEW MINISTRY", style = { height = 40, marginTop = 10, marginBottom = 10, backgroundColor = new Color(0.1f, 0.3f, 0.1f), color = Color.white, unityFontStyleAndWeight = FontStyle.Bold } };
            sidebar.Add(appointBtn);

            return sidebar;
        }

        // --- THE BRIEFING ROOM (CENTER PANEL) ---
        private void RenderBriefingRoom()
        {
            _briefingPanel.Clear();

            if (_selectedAdvisor == null)
            {
                _briefingPanel.Add(new Label("Awaiting Advisory Briefing..") { style = { color = Color.gray, fontSize = 24, unityTextAlign = TextAnchor.MiddleCenter, flexGrow = 1 } });
                return;
            }

            string deptName = _selectedAdvisor.Department != null ? _selectedAdvisor.Department.Name : "Unassigned Portfolio";

            // Header
            var headerRow = new VisualElement { style = { flexDirection = FlexDirection.Row, borderBottomWidth = 1, borderBottomColor = Color.gray, paddingBottom = 10, marginBottom = 20 } };
            headerRow.Add(new Label($"MINISTRY REPORT: {deptName.ToUpper()}") { style = { color = Color.white, fontSize = 22, unityFontStyleAndWeight = FontStyle.Bold, flexGrow = 1 } });
            headerRow.Add(new Label($"Rep: {_selectedAdvisor.Name} | Loyalty: {_selectedAdvisor.Loyalty * 100f}%") { style = { color = Color.cyan, unityTextAlign = TextAnchor.LowerRight } });
            _briefingPanel.Add(headerRow);

            // Contextual Dashboard Rendering
            if (deptName.Contains("Economy") || deptName.Contains("Treasury"))
            {
                RenderEconomicDashboard();
            }
            else
            {
                // Generic fallback for other ministries (Science, Military, etc.)
                _briefingPanel.Add(new Label("No pressing crises in this sector.") { style = { color = Color.gray } });
            }
        }

        private void RenderEconomicDashboard()
        {
            if (_govContext.Exchange == null)
            {
                _briefingPanel.Add(new Label("Galactic Stock Exchange module not initialized.") { style = { color = Color.red } });
                return;
            }

            var exchange = _govContext.Exchange;

            // Macro Stats
            var statsRow = new VisualElement { style = { flexDirection = FlexDirection.Row, marginBottom = 20 } };
            statsRow.Add(CreateStatCard("TOTAL MARKET CAP", $"{exchange.TotalMarketCap:N0} cr", Color.cyan));

            string trendColor = exchange.GalacticGDP_Trend >= 0 ? "green" : "red";
            string trendSign = exchange.GalacticGDP_Trend >= 0 ? "+" : "";
            statsRow.Add(CreateStatCard("GDP TREND (TICK)", $"<color={trendColor}>{trendSign}{exchange.GalacticGDP_Trend:N0}</color>", Color.white));

            _briefingPanel.Add(statsRow);

            // Active Firms / Ticker
            _briefingPanel.Add(new Label("ACTIVE CORPORATE CONTRACTORS") { style = { color = new Color(0.8f, 0.6f, 0.1f), unityFontStyleAndWeight = FontStyle.Bold, marginBottom = 10 } });

            var tickerScroll = new ScrollView(ScrollViewMode.Vertical) { style = { flexGrow = 1, backgroundColor = new Color(0.05f, 0.05f, 0.05f), borderTopWidth = 1, borderBottomWidth = 1, borderTopColor = Color.gray, borderBottomColor = Color.gray } };

            foreach (var firm in exchange.PersistentFirms)
            {
                var row = new VisualElement { style = { flexDirection = FlexDirection.Row, height = 40, alignItems = Align.Center, borderBottomWidth = 1, borderBottomColor = new Color(0.2f, 0.2f, 0.2f), paddingLeft = 10, paddingRight = 10 } };

                row.Add(new Label(firm.TickerSymbol) { style = { width = 50, color = Color.yellow, unityFontStyleAndWeight = FontStyle.Bold } });
                row.Add(new Label(firm.Name) { style = { width = 200, color = Color.white } });
                row.Add(new Label($"Cap: {firm.LiquidCapital:N0}") { style = { width = 150, color = Color.gray } });
                row.Add(new Label($"{firm.CurrentStockPrice:C2} / sh") { style = { flexGrow = 1, color = Color.cyan, unityTextAlign = TextAnchor.MiddleRight } });

                // The Interactive Element: Subsidize the firm!
                var subsidizeBtn = new Button(() =>
                {
                    long subsidy = 1000000;
                    if (_govContext.ImperialTreasury >= subsidy)
                    {
                        _govContext.ImperialTreasury -= subsidy;
                        firm.LiquidCapital += subsidy;
                        exchange.GalacticGDP_Trend += subsidy * 0.1f; // Subsidies boost the overall market confidence
                        RenderBriefingRoom();
                    }
                })
                { text = "GRANT 1M SUBSIDY", style = { height = 25, backgroundColor = new Color(0.1f, 0.3f, 0.1f), color = Color.white } };

                row.Add(subsidizeBtn);
                tickerScroll.Add(row);
            }
            _briefingPanel.Add(tickerScroll);
        }

        private VisualElement CreateStatCard(string title, string valueHtml, Color accent)
        {
            var card = new VisualElement { style = { flexGrow = 1, backgroundColor = new Color(0.1f, 0.1f, 0.15f), marginRight = 10, paddingLeft = 15, paddingTop = 15, paddingBottom = 15, borderLeftWidth = 4, borderLeftColor = accent } };
            card.Add(new Label(title) { style = { color = Color.gray, fontSize = 10, unityFontStyleAndWeight = FontStyle.Bold, marginBottom = 5 } });
            // Using a simple label, but relying on Unity's rich text parsing if enabled
            card.Add(new Label(valueHtml) { style = { color = Color.white, fontSize = 20 } });
            return card;
        }

        // --- EDICTS AND POLICIES (RIGHT SIDEBAR) ---
        private VisualElement CreateEdictsSidebar()
        {
            var sidebar = new VisualElement { style = { width = 280, backgroundColor = new Color(0.04f, 0.04f, 0.06f), borderLeftWidth = 2, borderLeftColor = new Color(0.8f, 0.6f, 0.1f), paddingLeft = 10, paddingRight = 10, paddingTop = 15 } };

            sidebar.Add(new Label("IMPERIAL EDICTS") { style = { color = new Color(0.8f, 0.6f, 0.1f), fontSize = 16, unityFontStyleAndWeight = FontStyle.Bold, marginBottom = 15 } });

            var scroll = new ScrollView(ScrollViewMode.Vertical) { style = { flexGrow = 1 } };

            foreach (var edict in _govContext.ActiveEdicts)
            {
                var card = new VisualElement { style = { backgroundColor = new Color(0.1f, 0.1f, 0.1f), marginBottom = 10, paddingLeft = 10, paddingRight = 10, paddingTop = 10, paddingBottom = 10, borderLeftWidth = 2, borderLeftColor = Color.red } };
                card.Add(new Label(edict.Name) { style = { color = Color.white, unityFontStyleAndWeight = FontStyle.Bold, marginBottom = 5 } });
                card.Add(new Label(edict.Description) { style = { color = Color.gray, fontSize = 10, whiteSpace = WhiteSpace.Normal, marginBottom = 5 } });
                card.Add(new Label($"Upkeep: {edict.UpkeepCost:N0} cr/mo") { style = { color = new Color(0.8f, 0.4f, 0.4f), fontSize = 10, unityTextAlign = TextAnchor.MiddleRight } });
                scroll.Add(card);
            }

            sidebar.Add(scroll);

            // THE "+" BUTTON: Draft new legislation
            var passEdictBtn = new Button(() => _onNavigateToBuilder?.Invoke("EdictBuilder")) { text = "+ DRAFT NEW EDICT", style = { height = 40, marginTop = 10, marginBottom = 10, backgroundColor = new Color(0.4f, 0.1f, 0.1f), color = Color.white, unityFontStyleAndWeight = FontStyle.Bold } };
            sidebar.Add(passEdictBtn);

            return sidebar;
        }

        // Utility to bootstrap the view if no context is provided
        private GovernmentContext GenerateMockGovernment()
        {
            var ctx = new GovernmentContext();

            var econDept = new MinistryDepartment { Name = "Ministry of the Economy", Description = "Oversight of galactic markets and industrial output." };
            // A nod to recent shifts in leadership: a pragmatist focusing on heavy industry.
            ctx.ActiveCabinet.Add(new CabinetMember { Name = "High Chancellor Moritz", Department = econDept, Loyalty = 0.85f });

            var warDept = new MinistryDepartment { Name = "Ministry of War", Description = "Naval intelligence and fleet logistics." };
            ctx.ActiveCabinet.Add(new CabinetMember { Name = "Lord Admiral Thorne", Department = warDept, Loyalty = 0.95f });

            ctx.ActiveEdicts.Add(new ImperialEdict { Name = "Wartime Conscription", Description = "Increases fleet personnel limits but drains treasury.", UpkeepCost = 250000 });
            ctx.ActiveEdicts.Add(new ImperialEdict { Name = "Industrial Subsidization", Description = "Redirects taxes to failing heavy-metallurgy sectors.", UpkeepCost = 500000 });

            // Mock the exchange
            ctx.Exchange = new GalacticStockExchange();

            // Ensure we mark them as Persistent so the UI logic we wrote earlier processes them!
            ctx.Exchange.PersistentFirms.Add(new ContractFirm { Name = "Aegis Heavy Industries", TickerSymbol = "AEG", LiquidCapital = 15000000, CurrentStockPrice = 45.20f, IsPersistent = true });
            ctx.Exchange.PersistentFirms.Add(new ContractFirm { Name = "Nova-Core Dynamics", TickerSymbol = "NVC", LiquidCapital = 8500000, CurrentStockPrice = 18.50f, IsPersistent = true });

            return ctx;
        }

        public Action<VisualElement> GetGuiBuilder() => (root) => root.Add(CreateGui(new GuiContext()));
        public void FromUIDocument(string assetPath) => _guiContext?.OnBuilt?.Invoke(GraphicalUserInterfaceBuilder.ConvertFromUIDocument(assetPath));
        public void ToUIDocument(string assetPath) => GraphicalUserInterfaceBuilder.ConvertToUIDocument(CreateGui(new GuiContext()), assetPath);
    }
}

#endif