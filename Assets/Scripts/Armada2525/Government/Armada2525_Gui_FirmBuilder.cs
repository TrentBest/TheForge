#if UNITY_EDITOR
using TheSingularityWorkshop.Armada2525.Economy;
using TheSingularityWorkshop.Forge.Builders.GuiBuilders;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace TheSingularityWorkshop.Armada2525.Editor
{
    public class Armada2525_Gui_FirmBuilder : IGuiProvider
    {
        public string Title => "CORPORATE FIRM REGISTRY";

        private GuiContext _lastCtx;
        private CRUD_Builder<ContractFirm> _crudInterface;

        // Master DB for explicitly authored Titan Firms
        public static List<ContractFirm> FirmDatabase = new List<ContractFirm>();

        public Armada2525_Gui_FirmBuilder()
        {
            if (FirmDatabase.Count == 0)
            {
                FirmDatabase.Add(new ContractFirm { Name = "Aegis Heavy Industries", TickerSymbol = "AEG", CEO = "Director Vance", EmployeeCount = 150000, LiquidCapital = 50000000, CurrentStockPrice = 45.20f, OutstandingShares = 5000000, MonthlyProductionCapacity = 100000f, IsPersistent = true });
            }
        }

        public VisualElement CreateGui(GuiContext ctx)
        {
            _lastCtx = ctx;

            _crudInterface = new CRUD_Builder<ContractFirm>(
                title: "REGISTERED CONTRACTORS",
                dataSource: () => FirmDatabase,
                getDisplayName: (firm) => string.IsNullOrEmpty(firm.Name) ? "Unknown Entity" : firm.Name,
                getGroupCategory: (firm) => firm.PrimaryIndustry != null ? firm.PrimaryIndustry.Name : "Uncategorized",

                buildEditorForm: (firm) =>
                {
                    var form = new VisualElement { style = { flexDirection = FlexDirection.Row, alignItems = Align.FlexStart } };

                    // --- LEFT SIDE: CORPORATE IDENTITY & ROLEPLAY ---
                    var identityCol = new VisualElement { style = { width = 350, marginRight = 20 } };
                    identityCol.Add(new Label("CORPORATE IDENTITY") { style = { color = new Color(0.8f, 0.4f, 0.1f), unityFontStyleAndWeight = FontStyle.Bold, marginBottom = 10 } });

                    var identityBox = new VisualElement { style = { backgroundColor = Color.black, paddingLeft = 15, paddingRight = 15, paddingTop = 15, paddingBottom = 15, borderLeftWidth = 2, borderLeftColor = new Color(0.8f, 0.4f, 0.1f) } };

                    var nameField = new TextField("Firm Name") { value = firm.Name };
                    nameField.RegisterValueChangedCallback(e => firm.Name = e.newValue);
                    identityBox.Add(nameField);

                    var tickerField = new TextField("Ticker Symbol") { value = firm.TickerSymbol, maxLength = 5 };
                    tickerField.RegisterValueChangedCallback(e => firm.TickerSymbol = e.newValue.ToUpper());
                    identityBox.Add(tickerField);

                    var repField = new TextField("CEO / Director") { value = firm.CEO };
                    repField.RegisterValueChangedCallback(e => firm.CEO = e.newValue);
                    identityBox.Add(repField);

                    var logoContainer = new VisualElement { style = { flexDirection = FlexDirection.Row, marginTop = 5, marginBottom = 10, alignItems = Align.Center } };

                    var logoField = new ObjectField("Corporate Logo") { objectType = typeof(Texture2D), value = firm.FirmLogo, style = { flexGrow = 1 } };
                    var logoPreview = new Image { image = firm.FirmLogo, style = { width = 64, height = 64, marginLeft = 15, backgroundColor = new Color(0.1f, 0.1f, 0.1f), borderLeftWidth = 1, borderRightWidth = 1, borderTopWidth = 1, borderBottomWidth = 1, borderLeftColor = Color.gray, borderRightColor = Color.gray, borderTopColor = Color.gray, borderBottomColor = Color.gray } };

                    // When the user drops an image in, update the data model AND the live preview picture!
                    logoField.RegisterValueChangedCallback(e => {
                        firm.FirmLogo = e.newValue as Texture2D;
                        logoPreview.image = firm.FirmLogo;
                    });

                    logoContainer.Add(logoField);
                    logoContainer.Add(logoPreview);
                    identityBox.Add(logoContainer);

                   

                    // Dropdown for Industry mapping (Checks the Industry Builder DB)
                    var industryNames = Armada2525_Gui_IndustryBuilder.IndustryDatabase.Select(i => i.Name).ToList();
                    industryNames.Insert(0, "None Assigned");

                    int defaultIndex = firm.PrimaryIndustry != null ? industryNames.IndexOf(firm.PrimaryIndustry.Name) : 0;
                    var indDropdown = new DropdownField("Primary Industry", industryNames, defaultIndex >= 0 ? defaultIndex : 0);
                    indDropdown.RegisterValueChangedCallback(e => {
                        if (e.newValue == "None Assigned") firm.PrimaryIndustry = null;
                        else firm.PrimaryIndustry = Armada2525_Gui_IndustryBuilder.IndustryDatabase.Find(i => i.Name == e.newValue);
                    });
                    identityBox.Add(indDropdown);

                    var originField = new EnumField("Origin Story", firm.Origin);
                    originField.RegisterValueChangedCallback(e => firm.Origin = (FirmOriginEvent)e.newValue);
                    identityBox.Add(originField);

                    var histField = new TextField("Founding History") { value = firm.FoundingHistory, multiline = true, style = { height = 60, marginTop = 10 } };
                    histField.RegisterValueChangedCallback(e => firm.FoundingHistory = e.newValue);
                    identityBox.Add(histField);

                    identityCol.Add(identityBox);
                    form.Add(identityCol);

                    // --- RIGHT SIDE: FINANCIALS & ENGINEERING ---
                    var metricsCol = new VisualElement { style = { flexGrow = 1 } };
                    metricsCol.Add(new Label("FINANCIALS & CAPABILITIES") { style = { color = Color.cyan, unityFontStyleAndWeight = FontStyle.Bold, marginBottom = 10 } });

                    var metricsBox = new VisualElement { style = { backgroundColor = Color.black, paddingLeft = 15, paddingRight = 15, paddingTop = 15, paddingBottom = 15, borderLeftWidth = 2, borderLeftColor = Color.cyan } };

                    var empField = new LongField("Employee Count") { value = firm.EmployeeCount };
                    empField.RegisterValueChangedCallback(e => firm.EmployeeCount = e.newValue);
                    metricsBox.Add(empField);

                    var capField = new DoubleField("Liquid Capital (cr)") { value = firm.LiquidCapital };
                    capField.RegisterValueChangedCallback(e => firm.LiquidCapital = e.newValue);
                    metricsBox.Add(capField);

                    var priceField = new FloatField("Current Stock Price") { value = firm.CurrentStockPrice };
                    priceField.RegisterValueChangedCallback(e => firm.CurrentStockPrice = e.newValue);
                    metricsBox.Add(priceField);

                    var sharesField = new LongField("Outstanding Shares") { value = firm.OutstandingShares };
                    sharesField.RegisterValueChangedCallback(e => firm.OutstandingShares = e.newValue);
                    metricsBox.Add(sharesField);

                    metricsBox.Add(new Label("--- ENGINEERING METRICS ---") { style = { color = Color.gray, marginTop = 15, marginBottom = 10, unityTextAlign = TextAnchor.MiddleCenter } });

                    var prodField = new FloatField("Monthly Production (Tons)") { value = firm.MonthlyProductionCapacity, tooltip = "How fast they can build ships/infrastructure." };
                    prodField.RegisterValueChangedCallback(e => firm.MonthlyProductionCapacity = e.newValue);
                    metricsBox.Add(prodField);

                    var perfField = new FloatField("Performance Multiplier") { value = firm.PerformanceMultiplier, tooltip = "1.0 is standard. >1.0 makes their parts fundamentally better." };
                    perfField.RegisterValueChangedCallback(e => firm.PerformanceMultiplier = e.newValue);
                    metricsBox.Add(perfField);

                    metricsCol.Add(metricsBox);
                    form.Add(metricsCol);

                    return form;
                },

                onSave: (firm) => {
                    firm.IsPersistent = true; // Authored firms are always persistent
                    if (!FirmDatabase.Contains(firm)) FirmDatabase.Add(firm);
                },
                onDelete: (firm) => { FirmDatabase.Remove(firm); },
                getSubtitle: (firm) => $"[ {firm.TickerSymbol} ] Cap: {firm.MarketCap:N0}"
            );

            return _crudInterface.CreateGui(ctx);
        }

        public Action<VisualElement> GetGuiBuilder() => (root) => root.Add(CreateGui(new GuiContext()));
        public void FromUIDocument(string assetPath) => _lastCtx?.OnBuilt?.Invoke(GraphicalUserInterfaceBuilder.ConvertFromUIDocument(assetPath));
        public void ToUIDocument(string assetPath) => GraphicalUserInterfaceBuilder.ConvertToUIDocument(CreateGui(new GuiContext()), assetPath);
    }
}
#endif