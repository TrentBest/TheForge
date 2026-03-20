#if UNITY_EDITOR
using TheSingularityWorkshop.Armada2525.Shipyard;
using TheSingularityWorkshop.Forge.Builders.GuiBuilders;
using System;
using UnityEngine;
using UnityEngine.UIElements;

namespace TheSingularityWorkshop.Armada2525.Editor
{
    public class Armada2525_Gui_HullRoleBuilder : IGuiProvider
    {
        public string Title => "NAVAL DOCTRINE & ROLES";

        private GuiContext _lastCtx;
        private CRUD_Builder<HullRole> _crudInterface;

        public VisualElement CreateGui(GuiContext ctx)
        {
            _lastCtx = ctx;

            _crudInterface = new CRUD_Builder<HullRole>(
                title: "STRATEGIC DOCTRINES",
                dataSource: () => HullRole.Database,
                getDisplayName: (role) => string.IsNullOrEmpty(role.Name) ? "Unregistered Role" : role.Name,
                getGroupCategory: (role) => "Vessel Roles",

                buildEditorForm: (role) =>
                {
                    var form = new VisualElement { style = { flexDirection = FlexDirection.Column } };

                    form.Add(new Label("DOCTRINE DEFINITION") { style = { color = Color.cyan, unityFontStyleAndWeight = FontStyle.Bold, marginBottom = 10 } });

                    var box = new VisualElement { style = { backgroundColor = Color.black, paddingLeft = 10, paddingRight = 10, paddingTop = 10, paddingBottom = 10, borderLeftWidth = 2, borderLeftColor = Color.cyan } };

                    var nameField = new TextField("Role Designation") { value = role.Name };
                    nameField.RegisterValueChangedCallback(e => role.Name = e.newValue);
                    box.Add(nameField);

                    var descField = new TextField("Operational Profile") { value = role.Description, multiline = true };
                    descField.style.height = 60;
                    descField.RegisterValueChangedCallback(e => role.Description = e.newValue);
                    box.Add(descField);

                    // FUTURE EXPANSION IDEA:
                    // You would add your Advisor mappings here! 
                    // e.g., Dropdown for "Commanding Advisor: [Military / Science / Logistics]"

                    form.Add(box);
                    return form;
                },

                onSave: (role) => {
                    if (!HullRole.Database.Contains(role)) HullRole.Database.Add(role);
                },
                onDelete: (role) => {
                    HullRole.Database.Remove(role);
                },
                getSubtitle: (role) => "Active Doctrine"
            );

            return _crudInterface.CreateGui(ctx);
        }

        public Action<VisualElement> GetGuiBuilder() => (root) => root.Add(CreateGui(new GuiContext()));
        public void FromUIDocument(string assetPath) => _lastCtx?.OnBuilt?.Invoke(GraphicalUserInterfaceBuilder.ConvertFromUIDocument(assetPath));
        public void ToUIDocument(string assetPath) => GraphicalUserInterfaceBuilder.ConvertToUIDocument(CreateGui(new GuiContext()), assetPath);
    }
}
#endif