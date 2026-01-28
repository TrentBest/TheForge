using Assets.Scripts;
using Assets.Scripts.Builders.GuiBuilders;
using UnityEngine;
using UnityEngine.UIElements;

public class WeaponsControlTab : IShipTabBuilder
{
    public string TabName => "Weapons";
    public string TabIcon => "⚔️";
    private bool _isLocked = true;

    public VisualElement CreateGui(GuiContext ctx)
    {
        var root = new GraphicalUserInterfaceBuilder(TabName)
            .WithAutoGrow(true, false)
            .WithPadding(20);

        if (_isLocked)
        {
            var lockWidget = new SecurityLockBuilder("Weapons Systems Lock", "0451");
            lockWidget.OnUnlocked = () => {
                _isLocked = false;
                // Assuming the Tab/Window has a way to trigger a refresh
                ctx.OnBuilt?.Invoke(null);
            };
            root.AddChild(lockWidget);
        }
        else
        {
            // ARMING PANEL - Only shown when unlocked
            root.WithPanel("WeaponStatus")
                .WithTitle("ORDNANCE CONTROL")
                .WithBackgroundColor(new Color(0.2f, 0, 0)) // Danger red
                .AddChild(new Label("SYSTEMS ARMED") { style = { color = Color.red, unityFontStyleAndWeight = FontStyle.Bold } })
                .AddChild(new Button(() => { /* Fire Logic */ }) { text = "RELEASE MAIN BATTERY" })
                .EndPanel();
        }

        return root.Build();
    }
}