// File: Assets/Editor/ActionGuiProvider.cs
using System;
using UnityEngine.UIElements;
using TheSingularityWorkshop.Forge.Builders.GuiBuilders;

namespace Assets.Editor
{
    public class ActionGuiProvider : IGuiProvider
    {
        public string Title { get; set; }
        private Func<GuiContext, VisualElement> _factory;
        public ActionGuiProvider(Func<GuiContext, VisualElement> factory) => _factory = factory;
        public VisualElement CreateGui(GuiContext ctx) => _factory?.Invoke(ctx);
        public Action<VisualElement> GetGuiBuilder() => (root) => root.Add(CreateGui(new GuiContext()));
        public void ToUIDocument(string path) { }
        public void FromUIDocument(string path) { }
    }
}