#if UNITY_EDITOR
using System;
using UnityEngine;
using UnityEngine.UIElements;
using Workshop.UI_And_Tools.Forge.Builders.GuiBuilders;

namespace Assets.Scripts.Unity
{
    public class Forge_Gui_CloudSaveModule : IGuiProvider
    {
        public string Title => "CLOUD SAVE";
        public VisualElement CreateGui(GuiContext context)
        {
            return new GraphicalUserInterfaceBuilder("CloudSave_Root")
                .WithPadding(20)
                .AddChild(new Label("CLOUD SAVE MANAGEMENT") { style = { color = Color.cyan, fontSize = 20, unityFontStyleAndWeight = FontStyle.Bold } })
                .AddChild(new Label("Database link established. Awaiting schema definitions for the DataWarehouse...") { style = { color = Color.gray, marginTop = 10, whiteSpace = WhiteSpace.Normal } })
                .OnBuild(ve => {
                    // Logic to automatically generate fields from DataShelf keys
                })
                .Build();
        }
        public Action<VisualElement> GetGuiBuilder() => (root) => root.Add(CreateGui(new GuiContext()));
        public void FromUIDocument(string assetPath) { }
        public void ToUIDocument(string assetPath) { }
    }
}
#endif