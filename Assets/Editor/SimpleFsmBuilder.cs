using TheSingularityWorkshop.Forge.Builders.GuiBuilders;
using System;
using UnityEngine;
using UnityEngine.UIElements;

namespace Singularity.Editor
{
    // This class is automatically discovered by the Hub via TypeCache
    public class SimpleFsmBuilder : IGuiProvider
    {
        // Domain Data
        public string FsmName = "New FSM";
        public bool IsActive = true;
        public float TickRate = 1.0f;

        public string Title => $"FSM: {FsmName}";

        public VisualElement CreateGui(GuiContext ctx)
        {
            throw new NotImplementedException();
        }

        public void FromUIDocument(string assetPath)
        {
            throw new NotImplementedException();
        }

        // The "Builder-Editor Duality"
        public Action<VisualElement> GetGuiBuilder()
        {
            return (root) =>
            {
                // Using the Fluent DSL

                // Header
                root.Embed(new Label("FSM Configuration").StyleHeader());

                // Name Field
                root.Embed(new TextField("FSM Name") { value = FsmName })
                    .OnChange<TextField, string>(newValue =>
                    {
                        FsmName = newValue;
                        // In a real scenario, call Undo.RecordObject here
                    });

                // Toggles and Sliders in a Row
                root.Embed(new VisualElement().Row().Padding(5).Background(new Color(0.2f, 0.2f, 0.2f)))
                    .Embed(new Toggle("Is Active") { value = IsActive })
                        .OnChange<Toggle, bool>(v => IsActive = v)
                    .parent // Go back to Row
                    .Embed(new Slider("Tick Rate", 0, 10) { value = TickRate }.Grow())
                        .OnChange<Slider, float>(v => TickRate = v);

                // Example of a Button doing work
                root.Embed(new Button { text = "Compile FSM" })
                    .OnClick(() => Debug.Log($"Compiling {FsmName} at rate {TickRate}.."))
                    .Height(40);
            };
        }

        public void ToUIDocument(string assetPath)
        {
            throw new NotImplementedException();
        }
    }

    // A tiny helper for local styling just for this example
    public static class LocalExtensions
    {
        public static T StyleHeader<T>(this T e) where T : VisualElement
        {
            e.style.fontSize = 20;
            e.style.unityFontStyleAndWeight = FontStyle.Bold;
            e.style.marginBottom = 10;
            return e;
        }
    }
}