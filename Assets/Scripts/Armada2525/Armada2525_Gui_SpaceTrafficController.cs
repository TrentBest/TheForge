using TheSingularityWorkshop.Armada2525;
using TheSingularityWorkshop.Builders.GuiBuilders;
using TheSingularityWorkshop.Forge.Builders.GuiBuilders;
using UnityEngine;
using UnityEngine.UIElements;

public class Armada2525_Gui_SpaceTrafficController : IGuiProvider
{
    public string Title => "ORBITAL TRAFFIC CONTROL";

    private Armada2525 _game;
    private int _starportLevel = 1;
    private int _safeLandings = 0;
    private int _maxCapacity = 5;

    public VisualElement CreateGui(GuiContext ctx)
    {
        _game = Object.FindAnyObjectByType<Armada2525>();

        var builder = new GraphicalUserInterfaceBuilder("TrafficController_Root")
            .WithBackgroundColor(new Color(0.02f, 0.05f, 0.02f)) // Faint radar green
            .WithFlexLayout(FlexDirection.Row, Justify.FlexStart, Align.Stretch)
            .WithPercentSize(100, 100);

        // --- LEFT SIDEBAR: HUD & STATS (20%) ---
        builder.AddChild(c => {
            var sidebar = new VisualElement { style = { width = Length.Percent(20), backgroundColor = new Color(0.05f, 0.1f, 0.05f), borderRightWidth = 2, borderRightColor = Color.green, paddingLeft = 15, paddingRight = 15, paddingTop = 20 } };

            sidebar.Add(new Button(() => _game?.SwitchGui("ColonyView")) { text = "<< ABORT SIMULATION", style = { backgroundColor = Color.clear, color = Color.gray, borderBottomWidth = 1, borderBottomColor = Color.gray, marginBottom = 20 } });

            sidebar.Add(new Label("STARPORT RADAR") { style = { color = Color.green, fontSize = 20, unityFontStyleAndWeight = FontStyle.Bold, marginBottom = 20 } });

            var statsBox = new VisualElement { style = { backgroundColor = new Color(0, 0, 0, 0.5f), paddingLeft = 10, paddingTop = 10, paddingBottom = 10, marginBottom = 20, borderLeftWidth = 2, borderLeftColor = Color.cyan } };
            statsBox.Add(new Label($"PORT LEVEL: {_starportLevel}") { style = { color = Color.white, unityFontStyleAndWeight = FontStyle.Bold } });
            statsBox.Add(new Label($"CAPACITY: {_maxCapacity} Ships") { style = { color = Color.gray, fontSize = 10 } });
            statsBox.Add(new Label($"SAFE DOCKS: {_safeLandings}") { style = { color = Color.cyan, fontSize = 16, marginTop = 10 } });
            sidebar.Add(statsBox);

            var upgradeBtn = new Button(() => { _starportLevel++; _maxCapacity += 5; ctx.OnBuilt?.Invoke(null); })
            {
                text = "UPGRADE PORT (10,000cr)",
                style = { backgroundColor = new Color(0.1f, 0.4f, 0.1f), color = Color.white, height = 40, unityFontStyleAndWeight = FontStyle.Bold }
            };
            sidebar.Add(upgradeBtn);

            sidebar.Add(new Label("INCOMING VECTOR QUEUE") { style = { color = Color.gray, fontSize = 10, marginTop = 20, marginBottom = 10 } });
            sidebar.Add(new Label("• Heavy Freighter (ETA 12s)") { style = { color = Color.yellow } });
            sidebar.Add(new Label("• Civilian Shuttle (ETA 18s)") { style = { color = Color.white } });
            sidebar.Add(new Label("• Military Cruiser (ETA 25s)") { style = { color = Color.red } });

            return sidebar;
        });

        // --- RIGHT AREA: THE RADAR SCREEN (80%) ---
        builder.AddChild(c => {
            var radarArea = new VisualElement { style = { flexGrow = 1, alignItems = Align.Center, justifyContent = Justify.Center } };

            radarArea.Add(new Label("[ REAL-TIME 3D RADAR VIEW ]") { style = { color = new Color(0, 1, 0, 0.3f), fontSize = 24, unityFontStyleAndWeight = FontStyle.Bold } });
            radarArea.Add(new Label("Draw flight paths by clicking and dragging from ship icons to the docking bays.") { style = { color = Color.gray, marginTop = 10 } });

            return radarArea;
        });

        return builder.Build();
    }

    public System.Action<VisualElement> GetGuiBuilder() => (root) => root.Add(CreateGui(new GuiContext()));
    public void ToUIDocument(string assetPath) => throw new System.NotImplementedException();
    public void FromUIDocument(string assetPath) => throw new System.NotImplementedException();
}