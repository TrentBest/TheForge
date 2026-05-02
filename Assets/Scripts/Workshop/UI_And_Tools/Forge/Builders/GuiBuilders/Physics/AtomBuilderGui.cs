using Assets.Scripts.Workshop.Core.Physics;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;
using Workshop;
using Workshop.UI_And_Tools.Forge.Builders.GuiBuilders;
using Workshop.UI_And_Tools.Forge.IO;

namespace Assets.Scripts.Workshop.UI_And_Tools.Forge.Builders.GuiBuilders.Physics
{
    /// <summary>
    /// Provides a UI Toolkit panel for editing an AtomBuilder.
    /// </summary>
    public class AtomBuilderGui : IGuiProvider
    {
        private readonly AtomBuilder builder;
        private AtomOrbitView orbitView;

        // Guards for programmatic changes to avoid re-entrant callbacks.
        private bool suppressChange;

        // Track whether the user has manually edited protons/electrons so we don't auto-overwrite them
        private bool protonsDirtyByUser;
        private bool electronsDirtyByUser;

        public int TitleFontSize { get; private set; } = 13;
        public int MarginBottom { get; private set; } = 6;

        public string Title { get; set; } = "Atom Properties";

        public AtomBuilderGui() : this(new AtomBuilder())
        {
        }

        public AtomBuilderGui(AtomBuilder builder)
        {
            this.builder = builder ?? throw new ArgumentNullException(nameof(builder));
        }

        public VisualElement CreateGui(GuiContext ctx)
        {
            // Local references to native fields so we can programmatically update them (e.g., during Reset)
            TextField nameField = null;
            TextField symbolField = null;
            TextField zField = null;
            TextField awField = null;
            TextField nField = null;
            TextField pField = null;
            TextField eField = null;
            TextField cfgField = null;

            // --- Root Container ---
            var rootBuilder = new ForgeContainerBuilder("AtomBuilderGui")
                .WithFlexGrow(1)
                .WithPadding(10)
                .OnBuild(ve => { if (ctx?.StyleSheet != null) ve.styleSheets.Add(ctx.StyleSheet); });

            // --- Title ---
            rootBuilder.AddChild(new ForgeLabelBuilder(Title)
                .WithFontSize(TitleFontSize)
                .WithFontStyle(FontStyle.Bold)
                .WithMarginBottom(MarginBottom));

            // --- Orbit View Preview ---
            orbitView = new AtomOrbitView
            {
                ProtonColor = new Color(0.9f, 0.35f, 0.35f, 1f),
                NeutronColor = new Color(0.7f, 0.7f, 0.7f, 1f),
                RingColor = new Color(0.7f, 0.7f, 0.7f, 1f),
                ElectronColor = new Color(0.25f, 0.8f, 1f, 1f),
                NucleusRadius = 20f,
                NucleusDotRadius = 4f
            };
            orbitView.SetViewSize(300, 300);

            rootBuilder.AddChild(orbitView);

            // --- Form Fields ---
            rootBuilder.AddChild(new ForgeTextFieldBuilder("Name", "Hydrogen")
                .OnBuild(ve => { nameField = ve as TextField; nameField.isDelayed = true; }));

            rootBuilder.AddChild(new ForgeTextFieldBuilder("Symbol", "H")
                .OnBuild(ve => { symbolField = ve as TextField; symbolField.isDelayed = true; }));

            rootBuilder.AddChild(new ForgeTextFieldBuilder("Atomic Number (Z)", "1")
                .OnBuild(ve => {
                    zField = ve as TextField;
                    zField.isDelayed = true;
                    // Auto-sync Z to Protons and Electrons
                    zField.RegisterValueChangedCallback(ev =>
                    {
                        if (suppressChange) return;
                        suppressChange = true;
                        try
                        {
                            if (!protonsDirtyByUser && pField != null) pField.value = ev.newValue;
                            if (!electronsDirtyByUser && eField != null) eField.value = ev.newValue;
                        }
                        finally { suppressChange = false; }
                    });
                }));

            rootBuilder.AddChild(new ForgeTextFieldBuilder("Atomic Weight (u)", "1.0")
                .OnBuild(ve => { awField = ve as TextField; awField.isDelayed = true; }));

            rootBuilder.AddChild(new ForgeTextFieldBuilder("Neutrons (N)", "0")
                .OnBuild(ve => { nField = ve as TextField; nField.isDelayed = true; }));

            rootBuilder.AddChild(new ForgeTextFieldBuilder("Protons (p)", "1")
                .OnBuild(ve => {
                    pField = ve as TextField;
                    pField.isDelayed = true;
                    pField.RegisterCallback<FocusInEvent>(_ => protonsDirtyByUser = true);
                }));

            rootBuilder.AddChild(new ForgeTextFieldBuilder("Electrons (e-)", "1")
                .OnBuild(ve => {
                    eField = ve as TextField;
                    eField.isDelayed = true;
                    eField.RegisterCallback<FocusInEvent>(_ => electronsDirtyByUser = true);
                }));

            rootBuilder.AddChild(new ForgeTextFieldBuilder("Electron Config (tokens, e.g., 1s2 2s2 2p6)", "1s1")
                .AsMultiline(true)
                .OnBuild(ve => { cfgField = ve as TextField; cfgField.isDelayed = true; }));

            // --- Action Buttons Row ---
            rootBuilder.AddChild(new ForgeContainerBuilder("ActionButtonsRow")
                .WithFlexLayout(FlexDirection.Row, Justify.FlexStart, Align.Center)
                .WithMarginTop(8)

                .AddChild(new ForgeButtonBuilder("Apply")
                    .WithBackgroundColor(new Color(0.1f, 0.4f, 0.1f))
                    .WithMarginRight(5)
                    .OnClick(() =>
                    {
                        try
                        {
                            float.TryParse(awField.value, out float aw);
                            int.TryParse(nField.value, out int n);
                            int.TryParse(pField.value, out int p);
                            int.TryParse(eField.value, out int e);

                            builder
                                .WithAtomicWeight(aw)
                                .WithNeutrons(Mathf.Max(0, n))
                                .WithProtons(Mathf.Max(0, p))
                                .WithElectrons(Mathf.Max(0, e));

                            var tokens = ParseTokens(cfgField.value);
                            if (tokens.Count > 0) builder.WithElectronConfiguration(tokens);

                            ctx?.Log?.Invoke("AtomBuilder fields applied.");
                        }
                        catch (Exception ex)
                        {
                            ctx?.Log?.Invoke($"Apply failed: {ex.Message}");
                        }
                    }))

                .AddChild(new ForgeButtonBuilder("Build")
                    .WithBackgroundColor(new Color(0.1f, 0.4f, 0.6f))
                    .WithMarginRight(5)
                    .OnClick(() =>
                    {
                        try
                        {
                            var atom = builder.Build();
                            string chargeStr = (atom.Protons - atom.Electrons).ToString("+#;-#;0");
                            string cfgStr = string.Join(' ', atom.ElectronConfiguration);

                            string previewText = $"{atom.Name} ({atom.Symbol})  Z={atom.AtomicNumber}  " +
                                                 $"p={atom.Protons}  n={atom.Neutrons}  e-={atom.Electrons}  q={chargeStr}\n" +
                                                 $"Cfg: {cfgStr}";

                            // Output to log context, assuming the parent view renders it (or we could append a label to the root dynamically)
                            ctx?.Log?.Invoke($"Atom Built:\n{previewText}");
                            ctx?.OnBuilt?.Invoke(rootBuilder.Build());
                        }
                        catch (Exception ex)
                        {
                            ctx?.Log?.Invoke($"Build failed: {ex.Message}");
                        }
                    }))

                .AddChild(new ForgeButtonBuilder("Reset")
                    .WithBackgroundColor(new Color(0.5f, 0.1f, 0.1f))
                    .OnClick(() =>
                    {
                        suppressChange = true;
                        try
                        {
                            if (nameField != null) nameField.value = "Hydrogen";
                            if (symbolField != null) symbolField.value = "H";
                            if (zField != null) zField.value = "1";
                            if (awField != null) awField.value = "1.0";
                            if (nField != null) nField.value = "0";
                            if (pField != null) pField.value = "1";
                            if (eField != null) eField.value = "1";
                            if (cfgField != null) cfgField.value = "1s1";

                            protonsDirtyByUser = false;
                            electronsDirtyByUser = false;
                        }
                        finally { suppressChange = false; }
                    }))
            );

            return rootBuilder.Build();
        }

        private static List<string> ParseTokens(string raw)
        {
            var tokens = new List<string>();
            if (string.IsNullOrWhiteSpace(raw)) return tokens;
            var parts = raw.Split(new[] { ' ', '\t', '\r', '\n', ',', ';' }, StringSplitOptions.RemoveEmptyEntries);
            foreach (var p in parts) tokens.Add(p.Trim());
            return tokens;
        }

        public Action<VisualElement> GetGuiBuilder() => root => root.Add(CreateGui(new GuiContext()));

        public void ToUIDocument(string assetPath)
        {
            var snapshotRoot = CreateGui(new GuiContext());
            string fileName = string.IsNullOrEmpty(assetPath) ? "AtomBuilderConfig_Snapshot" : System.IO.Path.GetFileNameWithoutExtension(assetPath);
            WorkshopUxmlBaker.Bake(snapshotRoot, fileName);
        }

        public void FromUIDocument(string assetPath)
        {
            Debug.LogWarning("[AtomBuilderGui] FromUIDocument is not supported. This UI relies heavily on internal physics engine state.");
        }
    }
}