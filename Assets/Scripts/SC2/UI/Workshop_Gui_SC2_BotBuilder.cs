using UnityEngine;
using UnityEngine.UIElements;
using TheSingularityWorkshop.Forge.Builders.GuiBuilders;
using TheSingularityWorkshop.Forge.SC2.Data;
using System.Linq;
using System;

namespace TheSingularityWorkshop.Forge.SC2.UI
{
    public class Workshop_Gui_SC2_BotBuilder : IGuiProvider
    {
        private SC2BotBlueprint _activeBlueprint;
        private VisualElement _behaviorContainer;

        public Workshop_Gui_SC2_BotBuilder()
        {
            _activeBlueprint = new SC2BotBlueprint();
        }

        public string Title => throw new NotImplementedException();

        public VisualElement CreateGui(GuiContext context)
        {
            var root = new VisualElement { style = { flexGrow = 1, flexDirection = FlexDirection.Row, backgroundColor = new Color(0.15f, 0.15f, 0.15f) } };

            // --- LEFT PANEL: Base Configuration ---
            var configPanel = new VisualElement { style = { width = 250, paddingTop = 10,paddingBottom = 10, paddingLeft = 10, paddingRight = 10, borderRightWidth = 1, borderRightColor = Color.gray } };

            var nameField = new TextField("Bot Name") { value = _activeBlueprint.BotName };
            nameField.RegisterValueChangedCallback(evt => _activeBlueprint.BotName = evt.newValue);
            configPanel.Add(nameField);

            var raceDropdown = new DropdownField("Race", System.Enum.GetNames(typeof(SC2Race)).ToList(), _activeBlueprint.PlayableRace.ToString());
            raceDropdown.RegisterValueChangedCallback(evt => _activeBlueprint.PlayableRace = (SC2Race)System.Enum.Parse(typeof(SC2Race), evt.newValue));
            configPanel.Add(raceDropdown);

            var addBehaviorBtn = new Button(() => AddBehaviorProvider(context, new CartographyBehaviorData())) { text = "Add Cartography Behavior" };
            addBehaviorBtn.style.marginTop = 20;
            configPanel.Add(addBehaviorBtn);

            // --- RIGHT PANEL: Recursive Behavior Container ---
            var stagingPanel = new VisualElement { style = { flexGrow = 1, paddingTop = 10, paddingRight = 10, paddingLeft = 10, paddingBottom = 10 } };
            stagingPanel.Add(new Label("Behavior Modules") { style = { fontSize = 18, color = Color.white, unityFontStyleAndWeight = FontStyle.Bold } });

            _behaviorContainer = new ScrollView(ScrollViewMode.Vertical);
            stagingPanel.Add(_behaviorContainer);

            root.Add(configPanel);
            root.Add(stagingPanel);

            return root;
        }

        public void FromUIDocument(string assetPath)
        {
            throw new NotImplementedException();
        }

        public Action<VisualElement> GetGuiBuilder()
        {
            throw new NotImplementedException();
        }

        public void ToUIDocument(string assetPath)
        {
            throw new NotImplementedException();
        }

        private void AddBehaviorProvider(GuiContext context, IBotBehaviorData behaviorData)
        {
            _activeBlueprint.Behaviors.Add(behaviorData);

            // Here we leverage the recursive nature: we spin up a new specific IGuiProvider 
            // to edit this specific piece of data, and inject its VisualElement into our container.

            IGuiProvider specificBuilder = null;
            if (behaviorData is CartographyBehaviorData cartoData)
            {
                specificBuilder = new Workshop_Gui_CartographyBehaviorBuilder(cartoData);
            }

            if (specificBuilder != null)
            {
                // Wrap the resulting GUI in our Space-Aware container
                var wrappedElement = CreateSpaceAwareWrapper(specificBuilder.CreateGui(context), behaviorData.ModuleName);
                _behaviorContainer.Add(wrappedElement);
            }
        }

        // --- The Space-Awareness / Expandable Wrapper ---
        private VisualElement CreateSpaceAwareWrapper(VisualElement content, string title)
        {
            var wrapper = new VisualElement { style = {  marginTop = 10, paddingTop = 5, paddingBottom = 5, paddingLeft = 5, paddingRight = 5, backgroundColor = new Color(0.2f, 0.2f, 0.2f) } };

            var header = new VisualElement { style = { flexDirection = FlexDirection.Row, justifyContent = Justify.SpaceBetween } };
            header.Add(new Label(title) { style = { color = Color.cyan, unityFontStyleAndWeight = FontStyle.Bold } });

            var expandBtn = new Button { text = "Expand/Pop-out" };
            // In a real implementation, this button would trigger a ModalOverlayBuilder or DialogBuilder
            // passing the 'content' VisualElement into a larger overlay, and leaving a graphic placeholder here.

            header.Add(expandBtn);

            wrapper.Add(header);
            wrapper.Add(content); // Add the recursive builder's UI inside the wrapper

            // Geometry event to check space (The "If space isn't big enough" logic)
            wrapper.RegisterCallback<GeometryChangedEvent>(evt =>
            {
                if (evt.newRect.width < 150 || evt.newRect.height < 50)
                {
                    content.style.display = DisplayStyle.None; // Hide complex UI
                    // Show compact graphic/icon here
                }
                else
                {
                    content.style.display = DisplayStyle.Flex; // Show full recursive UI
                }
            });

            return wrapper;
        }
    }
}