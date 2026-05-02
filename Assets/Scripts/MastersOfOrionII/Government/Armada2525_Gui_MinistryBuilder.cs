#if UNITY_EDITOR
using Assets.Scripts.MastersOfOrionII.Economy;
using Assets.Scripts.MastersOfOrionII.Government;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UIElements;
using Workshop.UI_And_Tools.Forge.Builders.GuiBuilders;

namespace Assets.Scripts.MastersOfOrionII
{
    public class MastersOfOrionII_Gui_MinistryBuilder : IGuiProvider
    {
        public string Title => "MINISTERIAL CHARTER BUREAU";

        private GuiContext _lastCtx;
        private CRUD_Builder<MinistryDepartment> _crudInterface;

        // Mock DB for the editor
        public static List<MinistryDepartment> MinistryDatabase = new List<MinistryDepartment>
        {
            new MinistryDepartment { Name = "Ministry of the Economy", Description = "Oversight of galactic markets." },
            new MinistryDepartment { Name = "Ministry of Defense", Description = "Fleet logistics and border security." }
        };

        public VisualElement CreateGui(GuiContext ctx)
        {
            _lastCtx = ctx;

            _crudInterface = new CRUD_Builder<MinistryDepartment>(
                title: "GOVERNMENT PORTFOLIOS",
                dataSource: () => MinistryDatabase,
                getDisplayName: (min) => string.IsNullOrEmpty(min.Name) ? "Unchartered Department" : min.Name,
                getGroupCategory: (min) => "Active Ministries",

                buildEditorForm: (min) =>
                {
                    var form = new VisualElement { style = { flexDirection = FlexDirection.Column, paddingLeft = 10, paddingRight = 10 } };

                    form.Add(new Label("MINISTRY DEFINITION") { style = { color = new Color(0.8f, 0.6f, 0.1f), unityFontStyleAndWeight = FontStyle.Bold, marginBottom = 10 } });

                    var box = new VisualElement { style = { backgroundColor = Color.black, paddingLeft = 15, paddingRight = 15, paddingTop = 15, paddingBottom = 15, borderLeftWidth = 2, borderLeftColor = new Color(0.8f, 0.6f, 0.1f) } };

                    var nameField = new TextField("Portfolio Name") { value = min.Name };
                    nameField.RegisterValueChangedCallback(e => min.Name = e.newValue);
                    box.Add(nameField);

                    var descField = new TextField("Jurisdiction") { value = min.Description, multiline = true, style = { height = 60, marginTop = 10, marginBottom = 15 } };
                    descField.RegisterValueChangedCallback(e => min.Description = e.newValue);
                    box.Add(descField);

                    // Connect the Government directly to the FSM Economy!
                    // This dropdown lets the player assign a specific industry to this ministry's oversight.
                    var industryNames = MastersOfOrionII_Gui_IndustryBuilder.IndustryDatabase.Select(i => i.Name).ToList();
                    industryNames.Insert(0, "General Governance (No Specific Sector)");

                    int defaultIndex = min.OversightSector != null ? industryNames.IndexOf(min.OversightSector.Name) : 0;
                    if (defaultIndex < 0) defaultIndex = 0;

                    var indDropdown = new DropdownField("Economic Oversight", industryNames, defaultIndex);
                    indDropdown.RegisterValueChangedCallback(e => {
                        if (e.newValue.Contains("General")) min.OversightSector = null;
                        else min.OversightSector = MastersOfOrionII_Gui_IndustryBuilder.IndustryDatabase.Find(i => i.Name == e.newValue);
                    });

                    box.Add(indDropdown);
                    form.Add(box);

                    return form;
                },

                onSave: (min) => { if (!MinistryDatabase.Contains(min)) MinistryDatabase.Add(min); },
                onDelete: (min) => { MinistryDatabase.Remove(min); },
                getSubtitle: (min) => min.OversightSector != null ? $"Oversight: {min.OversightSector.Name}" : "General Governance"
            );

            return _crudInterface.CreateGui(ctx);
        }

        public Action<VisualElement> GetGuiBuilder() => (root) => root.Add(CreateGui(new GuiContext()));
        public void FromUIDocument(string assetPath) => _lastCtx?.OnBuilt?.Invoke(GraphicalUserInterfaceBuilder.ConvertFromUIDocument(assetPath));
        public void ToUIDocument(string assetPath) => GraphicalUserInterfaceBuilder.ConvertToUIDocument(CreateGui(new GuiContext()), assetPath);
    }
}
#endif