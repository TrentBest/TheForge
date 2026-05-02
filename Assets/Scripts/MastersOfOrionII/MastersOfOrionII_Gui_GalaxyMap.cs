using System;
using UnityEngine;
using UnityEngine.UIElements;
using Workshop.Core.Diagnostics;
using Workshop.UI_And_Tools.Forge.Builders.GuiBuilders;

namespace Assets.Scripts.MastersOfOrionII
{
    public class MastersOfOrionII_Gui_GalaxyMap : IGuiProvider
    {
        public string Title => "GALAXY MAP";
        private GuiContext _lastCtx;

        // --- STATE ---
        // (In the future, this will be populated from GameData or UniverseGenerator)
        private bool _isDataHydrated = false;

        // REQUIRED: Parameterless Constructor for Reflection
        public MastersOfOrionII_Gui_GalaxyMap()
        {
            AllocateSafeState();
        }

        private void AllocateSafeState()
        {
            // Ensure any collections or map data configurations fall back to safe empty states
            _isDataHydrated = true;
        }

        public VisualElement CreateGui(GuiContext ctx)
        {
            _lastCtx = ctx;

            if (!_isDataHydrated) AllocateSafeState();

            var mapContainer = new GraphicalUserInterfaceBuilder("GalaxyMap_Canvas")
                .WithPercentSize(100, 100)
                .WithBackgroundColor(new Color(0.02f, 0.02f, 0.03f))
                .Build();

            mapContainer.style.position = Position.Absolute;
            mapContainer.style.overflow = Overflow.Hidden;

            // Generate simulated star systems
            mapContainer.Add(CreateStarSystem("Sol", 50, 50, Color.yellow, true));
            mapContainer.Add(CreateStarSystem("Altair", 200, 150, Color.cyan, false));
            mapContainer.Add(CreateStarSystem("Rigel", 350, 80, Color.red, false));
            mapContainer.Add(CreateStarSystem("Orion", 500, 300, Color.white, false));

            return mapContainer;
        }

        private VisualElement CreateStarSystem(string name, float x, float y, Color starColor, bool isPlayerOwned)
        {
            // Safe fallback for strings just in case
            name = string.IsNullOrEmpty(name) ? "Uncharted System" : name;

            var systemNode = new GraphicalUserInterfaceBuilder($"System_{name}")
                .WithFlexLayout(FlexDirection.Column, Justify.Center, Align.Center)
                .Build();

            systemNode.style.position = Position.Absolute;
            systemNode.style.left = x;
            systemNode.style.top = y;
            systemNode.style.width = 60;
            systemNode.style.height = 60;

            var starIcon = new GraphicalUserInterfaceBuilder("StarGraphic")
                .WithBackgroundColor(starColor)
                .Build();

            starIcon.style.width = 15;
            starIcon.style.height = 15;
            starIcon.style.borderTopLeftRadius = 10; starIcon.style.borderTopRightRadius = 10;
            starIcon.style.borderBottomLeftRadius = 10; starIcon.style.borderBottomRightRadius = 10;

            if (isPlayerOwned)
            {
                starIcon.style.borderTopWidth = 2; starIcon.style.borderRightWidth = 2;
                starIcon.style.borderBottomWidth = 2; starIcon.style.borderLeftWidth = 2;
                starIcon.style.borderTopColor = Color.green; starIcon.style.borderRightColor = Color.green;
                starIcon.style.borderBottomColor = Color.green; starIcon.style.borderLeftColor = Color.green;
            }

            systemNode.Add(starIcon);

            var nameLabel = new Label(name)
            {
                style = {
                    color = isPlayerOwned ? Color.green : Color.gray,
                    fontSize = 10,
                    marginTop = 5,
                    unityFontStyleAndWeight = isPlayerOwned ? FontStyle.Bold : FontStyle.Normal
                }
            };
            systemNode.Add(nameLabel);

            systemNode.RegisterCallback<ClickEvent>(e =>
            {
                ForgeLogger.Log($"[Galaxy Map] System Clicked: {name}");
            });

            systemNode.RegisterCallback<MouseEnterEvent>(e => starIcon.style.scale = new StyleScale(new Scale(new Vector3(1.5f, 1.5f, 1f))));
            systemNode.RegisterCallback<MouseLeaveEvent>(e => starIcon.style.scale = new StyleScale(new Scale(new Vector3(1f, 1f, 1f))));

            return systemNode;
        }

        public Action<VisualElement> GetGuiBuilder() => root => root.Add(CreateGui(new GuiContext()));
#if UNITY_EDITOR
        public void ToUIDocument(string assetPath) => GraphicalUserInterfaceBuilder.ConvertToUIDocument(CreateGui(new GuiContext()), assetPath);
#endif
        public void FromUIDocument(string assetPath) => _lastCtx?.OnBuilt?.Invoke(GraphicalUserInterfaceBuilder.ConvertFromUIDocument(assetPath));
    }
}