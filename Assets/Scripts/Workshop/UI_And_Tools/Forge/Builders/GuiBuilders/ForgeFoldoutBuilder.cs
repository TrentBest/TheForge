using System;
using UnityEngine;
using UnityEngine.UIElements;

namespace Workshop.UI_And_Tools.Forge.Builders.GuiBuilders
{
    public class ForgeFoldoutBuilder : IGuiProvider
    {
        private Foldout _foldout;
        public string Title => "Forge Foldout";

        public ForgeFoldoutBuilder(string title, bool isExpanded = false)
        {
            _foldout = new Foldout
            {
                text = title,
                value = isExpanded
            };
        }

        public ForgeFoldoutBuilder WithValue(bool isExpanded)
        {
            _foldout.value = isExpanded;
            return this;
        }

        public ForgeFoldoutBuilder WithMarginTop(float margin)
        {
            _foldout.style.marginTop = margin;
            return this;
        }

        public ForgeFoldoutBuilder WithMarginBottom(float margin)
        {
            _foldout.style.marginBottom = margin;
            return this;
        }

        public ForgeFoldoutBuilder AddChild(VisualElement child)
        {
            if (child != null)
                _foldout.Add(child);
            return this;
        }

        // --- IGuiProvider Implementation ---
        public VisualElement CreateGui(GuiContext ctx)
        {
            ctx?.OnBuilt?.Invoke(_foldout);
            return _foldout;
        }

        public Action<VisualElement> GetGuiBuilder() => root => root.Add(CreateGui(new GuiContext()));

        public void ToUIDocument(string assetPath)
        {
            // Implementation matching SkillsAPI/BooksAPI pattern
            WorkshopUxmlBaker.Bake(CreateGui(new GuiContext()), "FoldoutRegistry");
        }

        public void FromUIDocument(string assetPath) { }

        public VisualElement Build() => _foldout;
    }
}