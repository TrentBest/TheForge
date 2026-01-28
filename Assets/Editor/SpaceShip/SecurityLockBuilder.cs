using Assets.Scripts;
using Assets.Scripts.Builders.GuiBuilders;
using System;
using UnityEngine;
using UnityEngine.UIElements;

public class SecurityLockBuilder : IGuiProvider
{
    private string name;
    private string _inputBuffer = "";
    private string _correctCode = "1234"; // Default code
    public Action OnUnlocked;

    public SecurityLockBuilder(string name, string code = "1234")
    {
        this.name = name;
        this._correctCode = code;
    }

    public VisualElement CreateGui(GuiContext ctx)
    {
        var root = new GraphicalUserInterfaceBuilder(name)
            .WithTitle(name.ToUpper())
            .WithPadding(20)
            .WithBackgroundColor(new Color(0.1f, 0.1f, 0.1f))
            .WithBorderWidth(2)
            .WithBorderColor(Color.gray)
            .WithFlexLayout(FlexDirection.Column, Justify.Center, Align.Center);

        // Display Screen
        root.WithPanel("LockDisplay")
            .WithBackgroundColor(Color.black)
            .WithPadding(10)
            .WithMarginBottom(15)
            .AddChild(ctx2 => new Label(new string('*', _inputBuffer.Length))
            {
                style = { color = Color.green, fontSize = 24, unityTextAlign = TextAnchor.MiddleCenter }
            })
            .EndPanel();

        // 10-Digit Keypad Grid
        root.WithPanel("KeypadGrid")
            .WithFlexLayout(FlexDirection.Row, Justify.Center, Align.Center)
            .WithFlexWrap(Wrap.Wrap)
            .WithSize(180, 0)
            .AddChild(ctx2 => {
                var container = new VisualElement { style = { flexDirection = FlexDirection.Row, flexWrap = Wrap.Wrap, justifyContent = Justify.Center } };

                for (int i = 1; i <= 9; i++) container.Add(CreateNumButton(i.ToString()));

                container.Add(CreateNumButton("CLR", Color.red));
                container.Add(CreateNumButton("0"));
                container.Add(CreateNumButton("ENT", Color.green));

                return container;
            })
            .EndPanel();

        return root.CreateGui(ctx);
    }

    private Button CreateNumButton(string text, Color? color = null)
    {
        var btn = new Button(() => HandleInput(text))
        {
            text = text,
            style = {
                width = 50, height = 50, 
                backgroundColor = color ?? new Color(0.2f, 0.2f, 0.2f),
                color = Color.white, unityFontStyleAndWeight = FontStyle.Bold
            }
        };
        return btn;
    }

    private void HandleInput(string input)
    {
        if (input == "CLR") _inputBuffer = "";
        else if (input == "ENT")
        {
            if (_inputBuffer == _correctCode) OnUnlocked?.Invoke();
            else _inputBuffer = ""; // Reset on fail
        }
        else if (_inputBuffer.Length < 8)
        {
            _inputBuffer += input;
        }
        // In a real editor window, you'd trigger a Refresh() here.
    }
}