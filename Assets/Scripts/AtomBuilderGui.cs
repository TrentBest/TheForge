using TheSingularityWorkshop.Forge.Builders.GuiBuilders;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

namespace TheSingularityWorkshop
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
            var root = new VisualElement { name = "AtomBuilderGui" };
            if (ctx?.StyleSheet != null) root.styleSheets.Add(ctx.StyleSheet);

            // --- Title (optional) ---
            var title = new Label(Title)
            {
                style =
                {
                    unityFontStyleAndWeight = FontStyle.Bold,
                    fontSize = TitleFontSize,
                    marginBottom = MarginBottom
                }
            };
            root.Add(title);

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
            root.Add(orbitView);


            // --- Fields ---
            var nameField = new TextField("Name") { isDelayed = true, value = "Hydrogen" };
            var symbolField = new TextField("Symbol") { isDelayed = true, value = "H"};
            var zField = new IntegerField("Atomic Number (Z)") { isDelayed = true, value = 1 };
            var awField = new FloatField("Atomic Weight (u)") { isDelayed = true, value = 1.0f };
            var nField = new IntegerField("Neutrons (N)") { isDelayed = true, value = 0 };
            var pField = new IntegerField("Protons (p)") { isDelayed = true, value = 1 };
            var eField = new IntegerField("Electrons (e-)") { isDelayed = true, value = 1 };

            var cfgField = new TextField("Electron Config (tokens, e.g., 1s2 2s2 2p6)")
            {
                isDelayed = true,
                multiline = true,
                value = "1s1"
            };

            root.Add(nameField);
            root.Add(symbolField);
            root.Add(zField);
            root.Add(awField);
            root.Add(nField);
            root.Add(pField);
            root.Add(eField);
            root.Add(cfgField);

            // --- Focus & dirty tracking for p/e ---
            pField.RegisterCallback<FocusInEvent>(_ => protonsDirtyByUser = true);
            eField.RegisterCallback<FocusInEvent>(_ => electronsDirtyByUser = true);

            // Optionally clear dirty when focus leaves (comment out if you prefer once-dirty, always-dirty)
            // pField.RegisterCallback<FocusOutEvent>(_ => protonsDirtyByUser = false);
            // eField.RegisterCallback<FocusOutEvent>(_ => electronsDirtyByUser = false);

            // When Z changes, we only update p/e if the user hasn't “claimed” those fields.
            zField.RegisterValueChangedCallback(ev =>
            {
                if (suppressChange) return;

                suppressChange = true;
                try
                {
                    if (!protonsDirtyByUser) pField.value = ev.newValue;
                    if (!electronsDirtyByUser) eField.value = ev.newValue;
                }
                finally { suppressChange = false; }
            });

            // If you also want an “auto N from A and Z” behavior later, you could do it similarly here.

            // --- Buttons row ---
            var buttonsRow = new VisualElement { style = { flexDirection = FlexDirection.Row,  marginTop = 8 } };

            var applyBtn = new Button(() =>
            {
                try
                {
                    // Apply using fluent methods (order = last write wins)
                    // If you add WithName/WithSymbol to AtomBuilder, enable next two lines:
                    // builder.WithName(nameField.value);
                    // builder.WithSymbol(symbolField.value);

                    builder
                        .WithAtomicWeight(awField.value)
                        .WithNeutrons(Mathf.Max(0, nField.value))
                        .WithProtons(Mathf.Max(0, pField.value))
                        .WithElectrons(Mathf.Max(0, eField.value));

                    // Electron configuration tokens
                    var tokens = ParseTokens(cfgField.value);
                    if (tokens.Count > 0) builder.WithElectronConfiguration(tokens);

                    ctx?.Log?.Invoke("AtomBuilder fields applied.");
                }
                catch (Exception ex)
                {
                    ctx?.Log?.Invoke($"Apply failed: {ex.Message}");
                }
            })
            { text = "Apply" };

            var buildBtn = new Button(() =>
            {
                try
                {
                    var atom = builder.Build();
                    var preview = new Label(
                        $"{atom.Name} ({atom.Symbol})  Z={atom.AtomicNumber}  " +
                        $"p={atom.Protons}  n={atom.Neutrons}  e-={atom.Electrons}  q={(atom.Protons - atom.Electrons):+#;-#;0}\n" +
                        $"Cfg: {string.Join(' ', atom.ElectronConfiguration)}")
                    {
                        style = { whiteSpace = WhiteSpace.Normal, marginTop = 6 }
                    };
                    root.Add(preview);
                    ctx?.OnBuilt?.Invoke(root);
                    ctx?.Log?.Invoke("Atom built.");
                }
                catch (Exception ex)
                {
                    ctx?.Log?.Invoke($"Build failed: {ex.Message}");
                }
            })
            { text = "Build" };

            var resetBtn = new Button(() =>
            {
                suppressChange = true;
                try
                {
                    nameField.value = string.Empty;
                    symbolField.value = string.Empty;

                    zField.value = 1;
                    awField.value = 1.0f;
                    nField.value = 0;
                    pField.value = 1;
                    eField.value = 1;
                    cfgField.value = string.Empty;

                    protonsDirtyByUser = false;
                    electronsDirtyByUser = false;
                }
                finally { suppressChange = false; }
            })
            { text = "Reset" };

            buttonsRow.Add(applyBtn);
            buttonsRow.Add(buildBtn);
            buttonsRow.Add(resetBtn);
            root.Add(buttonsRow);

            return root;
        }

        private static List<string> ParseTokens(string raw)
        {
            var tokens = new List<string>();
            if (string.IsNullOrWhiteSpace(raw)) return tokens;
            var parts = raw.Split(new[] { ' ', '\t', '\r', '\n', ',', ';' }, StringSplitOptions.RemoveEmptyEntries);
            foreach (var p in parts) tokens.Add(p.Trim());
            return tokens;
        }

        public Action<VisualElement> GetGuiBuilder()
        {
            throw new NotImplementedException();
        }

        public void ToUIDocument(string assetPath)
        {
            throw new NotImplementedException();
        }

        public void FromUIDocument(string assetPath)
        {
            throw new NotImplementedException();
        }
    }
}
