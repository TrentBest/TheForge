using System;
using UnityEngine;
using UnityEngine.UIElements;
using Workshop.UI_And_Tools.Forge.Builders.GuiBuilders;
using Workshop.UI_And_Tools.Forge.Security;

public class WeaponsControlTab : IShipTabBuilder, IGuiProvider
{
    public string TabName => "Weapons";
    public string TabIcon => "⚔️";
    public string Title => TabName;

    private bool _isLocked = true;

    /// <summary>
    /// Orchestrates the Weapons Control UI, alternating between a security lock 
    /// and the active armament dashboard.
    /// </summary>
    public VisualElement CreateGui(GuiContext ctx)
    {
        var root = new GraphicalUserInterfaceBuilder(TabName)
            .WithAutoGrow(true, false)
            .WithPadding(20);

        if (_isLocked)
        {
            // --- SECURITY LAYER ---
            // Assuming SecurityLockBuilder follows the IGuiProvider or IForgeBuilder pattern
            var lockWidget = new SecurityLockBuilder("Weapons Systems Lock", "0451");

            lockWidget.OnUnlocked = () => {
                _isLocked = false;

                // Triggering a refresh: In the Experience Model, we often 
                // notify the context so the parent container knows to rebuild.
                ctx.OnBuilt?.Invoke(null);
                Debug.Log("[SYSTEM] Weapons Main Battery Unlocked. Clearance Level Alpha.");
            };

            root.AddChild(lockWidget);
        }
        else
        {
            // --- COMBAT LAYER ---
            root.WithPanel("WeaponStatus")
                .WithTitle("ORDNANCE CONTROL")
                .WithBackgroundColor(new Color(0.15f, 0, 0, 1f)) // Deep crimson
                .WithBorderColor(Color.red)
                .WithBorderWidth(2)
                .WithPadding(15)
                .AddChild(new Label("SYSTEMS ARMED")
                {
                    style = {
                        color = Color.red,
                        unityFontStyleAndWeight = FontStyle.Bold,
                        fontSize = 20,
                        marginBottom = 10,
                        unityTextAlign = TextAnchor.MiddleCenter
                    }
                })
                .AddSeparator(Color.red, 1) // Using the fixed int height overload
                .AddButton("RELEASE MAIN BATTERY", () => {
                    Debug.LogWarning("[FIRE] Main battery discharged. Heat levels rising.");
                })
                .EndPanel();

            root.AddChild(new Label("Targeting: AUTOMATIC") { style = { marginTop = 10, opacity = 0.6f } });
        }

        // We use CreateGui(ctx) here to preserve the existing context/styles
        return root.CreateGui(ctx);
    }

    /// <summary>
    /// Standardized delegate for injecting this tab into a Ship Dashboard or Terminal.
    /// </summary>
    public Action<VisualElement> GetGuiBuilder()
    {
        return (root) => root.Add(CreateGui(new GuiContext { Name = "Ship_Weapon_Tab" }));
    }

    #region --- Serialization & Baking ---
#if UNITY_EDITOR
    public void ToUIDocument(string assetPath)
    {
        var root = CreateGui(new GuiContext());
        GraphicalUserInterfaceBuilder.ConvertToUIDocument(root, assetPath);
    }

    public void FromUIDocument(string assetPath)
    {
        // For dynamic control tabs, we usually rely on the FSM/Logic state 
        // rather than hydrating pure visual trees.
        Debug.Log($"[WeaponsTab] UI Layout sync from {assetPath}");
    }
#else
    public void ToUIDocument(string assetPath) { }
    public void FromUIDocument(string assetPath) { }
#endif
    #endregion
}