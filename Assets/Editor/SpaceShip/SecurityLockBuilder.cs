using System;
using UnityEngine;
using UnityEngine.UIElements;
using Workshop.UI_And_Tools.Forge.Builders.GuiBuilders;
using Workshop.UI_And_Tools.Forge.IO;

namespace Workshop.UI_And_Tools.Forge.Security
{
    /// <summary>
    /// A high-fidelity security keypad for diegetic interaction.
    /// Reforged to follow the Forge Protocol and the Singularity Experience Model.
    /// </summary>
    public class SecurityLockBuilder : IGuiProvider
    {
        public string Title => "LOCK ARCHITECT";

        private readonly string _name;
        private readonly string _correctCode;
        private string _inputBuffer = "";

        // UI References for dynamic updates
        private Label _displayLabel;

        public Action OnUnlocked;

        public SecurityLockBuilder(string name, string code = "1234")
        {
            _name = name;
            _correctCode = code;
        }

        public VisualElement CreateGui(GuiContext ctx)
        {
            var rootBuilder = new ForgeContainerBuilder(_name)
                .WithPadding(20f)
                .WithBackgroundColor(new Color(0.05f, 0.05f, 0.05f)) // Industrial Obsidian
                .WithBorderWidth(2f)
                .WithBorderColor(new Color(0.3f, 0.3f, 0.35f))
                .WithAlignItems(Align.Center);

            // 1. Header Designation
            rootBuilder.AddChild(new ForgeLabelBuilder(_name.ToUpper())
                .WithFontSize(14)
                .WithBold()
                .WithColor(Color.cyan)
                .WithMarginBottom(15f));

            // 2. Display Screen (Wrapped in DynamicGuiProvider for reactivity)
            rootBuilder.AddChild(new ForgeContainerBuilder("LockDisplay")
                .WithWidth(180f)
                .WithHeight(50f)
                .WithBackgroundColor(Color.black)
                .WithPadding(10f)
                .WithMarginBottom(20f)
                .WithBorderWidth(1f)
                .WithBorderColor(new Color(0f, 1f, 0f, 0.2f)) // Faint green glow
                .AddChild(new DynamicGuiProvider(c => {
                    _displayLabel = new Label(GetMaskedBuffer())
                    {
                        style = {
                            color = Color.green,
                            fontSize = 24,
                            unityTextAlign = TextAnchor.MiddleCenter,
                            flexGrow = 1
                        }
                    };
                    return _displayLabel;
                })));

            // 3. 10-Digit Keypad Grid
            var keypadGrid = new ForgeContainerBuilder("KeypadGrid")
                .WithDirection(FlexDirection.Row)
                .WithFlexWrap(Wrap.Wrap)
                .WithWidth(180f)
                .WithJustifyContent(Justify.Center);

            // Generate 1-9
            for (int i = 1; i <= 9; i++)
            {
                keypadGrid.AddChild(CreateNumButton(i.ToString()));
            }

            // Bottom Row
            keypadGrid.AddChild(CreateNumButton("CLR", new Color(0.4f, 0.1f, 0.1f)));
            keypadGrid.AddChild(CreateNumButton("0"));
            keypadGrid.AddChild(CreateNumButton("ENT", new Color(0.1f, 0.4f, 0.2f)));

            rootBuilder.AddChild(keypadGrid);

            return rootBuilder.Build();
        }

        private IGuiProvider CreateNumButton(string text, Color? color = null)
        {
            return new ForgeButtonBuilder(text)
                .WithWidth(50f)
                .WithHeight(50f)
                .WithMargin(2f)
                .WithBackgroundColor(color ?? new Color(0.15f, 0.15f, 0.18f))
                .WithBold()
                .OnClick(() => HandleInput(text));
        }

        private void HandleInput(string input)
        {
            if (input == "CLR")
            {
                _inputBuffer = "";
            }
            else if (input == "ENT")
            {
                if (_inputBuffer == _correctCode)
                {
                    Debug.Log("<color=green>[SECURITY]</color> ACCESS GRANTED.");
                    OnUnlocked?.Invoke();
                }
                _inputBuffer = ""; // Reset regardless of success for security
            }
            else if (_inputBuffer.Length < 8)
            {
                _inputBuffer += input;
            }

            // Reactive Update
            if (_displayLabel != null)
            {
                _displayLabel.text = GetMaskedBuffer();
            }
        }

        private string GetMaskedBuffer() => new string('*', _inputBuffer.Length);

        // --- IGUIProvider Implementation ---

        public Action<VisualElement> GetGuiBuilder() => root => root.Add(CreateGui(new GuiContext()));

        public void ToUIDocument(string assetPath)
        {
            var snapshot = CreateGui(new GuiContext());
            WorkshopUxmlBaker.Bake(snapshot, $"{_name}_Keypad_Export");
        }

        public void FromUIDocument(string assetPath)
        {
            Debug.LogWarning("[SecurityLock] Static hydration is bypassed. Keypad logic is procedurally driven.");
        }
    }
}