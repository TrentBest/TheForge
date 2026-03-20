using TheSingularityWorkshop.Armada2525;
using TheSingularityWorkshop.Builders.GuiBuilders;
using TheSingularityWorkshop.Forge.Builders.GuiBuilders;
using UnityEngine;
using UnityEngine.UIElements;

public class Armada2525_Gui_GameBuilder : IGuiProvider
{
    public string Title => "GAME BUILDER";

    // --- SESSION DATA (The "Bones") ---
    // This data persists as we jump back and forth between phases
    public GalaxyConfigData GalaxyConfig { get; private set; } = new GalaxyConfigData();
    public RaceConfigData RaceConfig { get; private set; } = new RaceConfigData();

    // --- FSM STATE ---
    private enum BuilderPhase { Galaxy, Race, Finalizing }
    private BuilderPhase _currentPhase = BuilderPhase.Galaxy;

    // --- SUB-PROVIDERS ---
    private Armada2525_Gui_GalaxyConfig _galaxyGui;
    private Armada2525_Gui_RaceDesigner _raceGui;

    // --- CONTEXT SWITCHING ---
    private VisualElement _rootContainer;
    private Armada2525 _gameInstance;

    public Armada2525_Gui_GameBuilder()
    {
        // Initialize Sub-GUIs with reference to this Orchestrator
        _galaxyGui = new Armada2525_Gui_GalaxyConfig(this);
        _raceGui = new Armada2525_Gui_RaceDesigner(this);
    }

    public VisualElement CreateGui(GuiContext ctx)
    {
        _gameInstance = Object.FindAnyObjectByType<Armada2525>();

        _rootContainer = new VisualElement { style = { flexGrow = 1 } };
        RefreshView();

        return _rootContainer;
    }

    private void RefreshView()
    {
        _rootContainer.Clear();
        switch (_currentPhase)
        {
            case BuilderPhase.Galaxy:
                _rootContainer.Add(_galaxyGui.CreateGui(null));
                break;
            case BuilderPhase.Race:
                _rootContainer.Add(_raceGui.CreateGui(null));
                break;
        }
    }

    // --- NAVIGATION API ---

    public void AdvanceToRace()
    {
        _currentPhase = BuilderPhase.Race;
        // Recalculate Points based on Galaxy Difficulty settings
        RaceConfig.TotalPoints = GalaxyConfig.GetCreationPoints();
        RefreshView();
    }

    public void JumpBackToGalaxy(string highlightParameterId)
    {
        _currentPhase = BuilderPhase.Galaxy;
        RefreshView();

        // Trigger the visual cue on the specific slider/enum
        //_galaxyGui.HighlightParameter(highlightParameterId);
    }

    public void FinishAndLaunch()
    {
        // Pass the constructed data to the Game Engine
        UniverseGenerator.StartGeneration(_gameInstance, GalaxyConfig, RaceConfig);
    }

    public System.Action<VisualElement> GetGuiBuilder() => (root) => root.Add(CreateGui(new GuiContext()));

    public void ToUIDocument(string assetPath)
    {
        throw new System.NotImplementedException();
    }

    public void FromUIDocument(string assetPath)
    {
        throw new System.NotImplementedException();
    }
}

// Simple Data Containers
public class GalaxyConfigData
{
    public float PhysicsStability = 1.0f;
    public float UniverseAge = 5.0f;
    public int OpponentCount = 4;

    public float Abundance { get; internal set; }
    public float LifeSupportingPlanetaryAbundance { get; internal set; }
    public uint MasterSeed { get; internal set; }
    public GalaxyMorphology Morphology { get; internal set; }
    public int ScaleIndex { get; internal set; }
    public string Shape { get; internal set; }
    public float MagicAbundance { get; internal set; }//0f (No Magic) to 1f (Magic is as powerful as science)

    public int GetCreationPoints() => 200; // Simplified logic
}

public class RaceConfigData
{
    public int TotalPoints;
    public string RaceName = "Terran";
    // Traits..
}