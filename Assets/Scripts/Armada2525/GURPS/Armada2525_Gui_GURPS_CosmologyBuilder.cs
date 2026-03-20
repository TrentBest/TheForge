using TheSingularityWorkshop.Armada2525.GURPS;
using System;
using UnityEngine;
using UnityEngine.UIElements;

internal class Armada2525_Gui_GURPS_CosmologyBuilder
{
    public Armada2525_Gui_GURPS_CosmologyBuilder()
    {
    }

    internal VisualElement CreateGui(GURPSUniverse uni)
    {
        var root = new VisualElement { style = { paddingTop = 10, paddingBottom = 10, paddingLeft = 10, paddingRight = 10 } };

        // --- SECTION: IDENTITY & REALITY ANCHORS ---
        var identityHeader = CreateHeader("COSMOLOGY ANCHORS (LAYER 1 & 7)");
        root.Add(identityHeader);

        // Universe Seed: 42 triggers Real-World AEC mode
        var seedField = new IntegerField("Universe Seed") { value = uni.UniverseSeed };
        seedField.tooltip = "Seed 42 anchors to Real-World AEC data. Others are Simulated.";
        seedField.RegisterValueChangedCallback(e => {
            uni.UniverseSeed = e.newValue;
            // The IsRealWorld property in GURPSUniverse automatically updates based on this value
        });
        root.Add(seedField);

        var nameField = new TextField("Universe Name") { value = uni.Name };
        nameField.RegisterValueChangedCallback(e => uni.Name = e.newValue);
        root.Add(nameField);

        // --- SECTION: CHRONOLOGY & TECH (LAYER 6) ---
        root.Add(new VisualElement { style = { height = 10 } }); // Spacer
        root.Add(CreateHeader("TEMPORAL STATE (LAYER 6)"));

        var tlSlider = new SliderInt("Base Tech Level (TL)", 0, 15) { value = uni.DefaultTechLevel };
        tlSlider.showInputField = true;
        tlSlider.RegisterValueChangedCallback(e => uni.DefaultTechLevel = e.newValue);
        root.Add(tlSlider);

        // --- SECTION: PHYSICS & DOMAIN (LAYER 3) ---
        root.Add(new VisualElement { style = { height = 10 } }); // Spacer
        root.Add(CreateHeader("PHYSICS PROFILE"));

        var physicsField = new TextField("Physics Profile") { value = uni.PhysicsProfile };
        physicsField.tooltip = "Defines universal constants like Mana levels or Gravity.";
        physicsField.RegisterValueChangedCallback(e => uni.PhysicsProfile = e.newValue);
        root.Add(physicsField);

        var descField = new TextField("Cosmology Description")
        {
            value = uni.Description,
            multiline = true,
            style = { height = 60, marginTop = 10 }
        };
        descField.RegisterValueChangedCallback(e => uni.Description = e.newValue);
        root.Add(descField);

        return root;
    }

    private Label CreateHeader(string text)
    {
        return new Label(text)
        {
            style = {
                unityFontStyleAndWeight = FontStyle.Bold,
                color = new Color(0, 0.8f, 1f), // Cyan highlight for the Forge aesthetic
                marginBottom = 5,
                marginTop = 5
            }
        };
    }
}