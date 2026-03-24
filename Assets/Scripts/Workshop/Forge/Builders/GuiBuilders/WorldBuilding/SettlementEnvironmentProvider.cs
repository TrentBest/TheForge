using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;
using TheSingularityWorkshop.Forge.Builders.GuiBuilders;

namespace TheSingularityWorkshop.Forge.Builders.GuiBuilders.WorldBuilding
{
    public class SettlementEnvironmentProvider : IGuiProvider
    {
        public string Title => "ENVIRONMENT & TECHNOLOGY";

        private SettlementEnvironmentData _envData;
        private Action _onDataChanged;

        // Reactive Labels
        private Label _tlDescriptionLabel;
        private Label _adaptationWarningLabel;

        public SettlementEnvironmentProvider(SettlementEnvironmentData envData, Action onDataChanged = null)
        {
            _envData = envData;
            _onDataChanged = onDataChanged;
        }

        public VisualElement CreateGui(GuiContext ctx)
        {
            return new GraphicalUserInterfaceBuilder("EnvironmentPanel")
                .WithBackgroundColor(new Color(0.1f, 0.12f, 0.1f)) // Subtle greenish tint for environment
                .WithPadding(15)
                .WithBorderTopWidth(2).WithBorderTopColor(new Color(0.2f, 0.8f, 0.4f))
                .WithBorderBottomLeftRadius(8).WithBorderBottomRightRadius(8)

                .AddChild(new Label(Title) { style = { color = new Color(0.4f, 0.9f, 0.6f), unityFontStyleAndWeight = FontStyle.Bold, marginBottom = 10 } })

                // --- LOCAL PLANETARY READOUT (Read-Only Context) ---
                .AddChild(new Label($"Local Biome: {_envData.LocalBiomeName}  |  Gravity: {_envData.LocalGravityG}G")
                { style = { color = Color.gray, fontSize = 11, marginBottom = 15, unityFontStyleAndWeight = FontStyle.Italic } })

                // --- TECHNOLOGY LEVEL ---
                .OnBuild(ve => {
                    var tlContainer = new VisualElement();

                    var slider = new SliderInt("Technology Level (TL)", 0, 12, SliderDirection.Horizontal) { value = _envData.TechLevel };
                    slider.style.color = Color.white;

                    _tlDescriptionLabel = new Label(GetTLDescription(_envData.TechLevel))
                    { style = { color = new Color(0.6f, 0.8f, 1f), fontSize = 11, marginLeft = 150, marginBottom = 10 } };

                    slider.RegisterValueChangedCallback(evt => {
                        _envData.TechLevel = evt.newValue;
                        _tlDescriptionLabel.text = GetTLDescription(evt.newValue);
                        EvaluateAdaptationViability();
                        _onDataChanged?.Invoke();
                    });

                    tlContainer.Add(slider);
                    tlContainer.Add(_tlDescriptionLabel);
                    ve.Add(tlContainer);
                })

                // --- ADAPTATION STRATEGY ---
                .OnBuild(ve => {
                    var enumField = new EnumField("Adaptation Strategy", _envData.Adaptation);
                    enumField.style.color = Color.white;
                    enumField.style.marginTop = 10;

                    enumField.RegisterValueChangedCallback(evt => {
                        _envData.Adaptation = (SettlementAdaptation)evt.newValue;
                        EvaluateAdaptationViability();
                        _onDataChanged?.Invoke();
                    });

                    ve.Add(enumField);

                    // Warning label for impossible combinations
                    _adaptationWarningLabel = new Label("")
                    { style = { color = new Color(0.9f, 0.3f, 0.3f), fontSize = 11, marginLeft = 150, marginTop = 2, whiteSpace = WhiteSpace.Normal } };
                    ve.Add(_adaptationWarningLabel);

                    // Run initial check
                    EvaluateAdaptationViability();
                })

                // --- ECOLOGICAL IMPACT ---
                .AddChild(new Label("ECOLOGICAL FOOTPRINT") { style = { color = Color.white, marginTop = 20, marginBottom = 5, fontSize = 12 } })

                // Pollution / Footprint
                .AddSliderData("Pollution Index", 0f, 100f, _envData.EcologicalFootprint, v => {
                    _envData.EcologicalFootprint = v;
                    _onDataChanged?.Invoke();
                })
                .AddChild(new Label("High values increase unrest and mortality, but may boost industrial output.") { style = { color = Color.gray, fontSize = 10, marginLeft = 150, marginBottom = 10 } })

                // Terraforming
                .AddSliderData("Terraforming", 0f, 100f, _envData.TerraformingProgress, v => {
                    _envData.TerraformingProgress = v;
                    _onDataChanged?.Invoke();
                })
                .AddChild(new Label("100% matches the founder species' native homeworld conditions.") { style = { color = Color.gray, fontSize = 10, marginLeft = 150 } })

                .Build();
        }

        // ==========================================
        // DIEGETIC LOGIC & FEEDBACK
        // ==========================================

        private string GetTLDescription(int tl)
        {
            switch (tl)
            {
                case 0: return "[Stone Age] Hunter-gatherers, fire, stone tools.";
                case 1: return "[Bronze Age] Agriculture, early cities, bronze.";
                case 2: return "[Iron Age] Empires, iron, mathematics.";
                case 3: return "[Age of Sail] Gunpowder, navigation, printing.";
                case 4: return "[Industrial] Steam power, mechanization, telegraphs.";
                case 5: return "[Mechanized] Internal combustion, early flight, radio.";
                case 6: return "[Nuclear] Fission, early computing, spaceflight.";
                case 7: return "[Micro-Tech] Global networks, genetic engineering.";
                case 8: return "[Early Stellar] Fusion, AI, interplanetary colonization."; // GURPS standard sci-fi start
                case 9: return "[Interstellar] Antimatter, FTL drives, gravity control.";
                case 10: return "[Galactic] Force fields, total terraforming, synthetic bodies.";
                case 11: return "[Trans-Galactic] Dyson spheres, mind uploading.";
                case 12: return "[Singularity] Reality engineering, post-physical existence.";
                default: return "[Unknown Technology]";
            }
        }

        private void EvaluateAdaptationViability()
        {
            if (_adaptationWarningLabel == null) return;

            string warning = "";

            if (_envData.TechLevel < 6 && _envData.Adaptation == SettlementAdaptation.SealedDome)
                warning = "⚠️ TL < 6: Maintaining large-scale sealed biospheres is nearly impossible. Expect massive failure rates.";

            else if (_envData.TechLevel < 8 && _envData.Adaptation == SettlementAdaptation.OrbitalTether)
                warning = "⚠️ TL < 8: Materials science cannot support a space elevator tether. Structure will collapse.";

            else if (_envData.TechLevel < 7 && _envData.Adaptation == SettlementAdaptation.AtmosphericFloat)
                warning = "⚠️ TL < 7: Anti-gravity or massive buoyancy structures are unavailable. Extremely hazardous.";

            else if (_envData.Adaptation == SettlementAdaptation.Native && _envData.TerraformingProgress < 20f && _envData.LocalBiomeName != "Temperate Plains")
                warning = "⚠️ Native adaptation selected, but environment is hostile and terraforming is low. Residents require environmental suits.";

            _adaptationWarningLabel.text = warning;

            // Hide if no warning
            _adaptationWarningLabel.style.display = string.IsNullOrEmpty(warning) ? DisplayStyle.None : DisplayStyle.Flex;
        }

        public Action<VisualElement> GetGuiBuilder() => (root) => root.Add(CreateGui(new GuiContext()));
        public void ToUIDocument(string assetPath) { }
        public void FromUIDocument(string assetPath) { }
    }
}