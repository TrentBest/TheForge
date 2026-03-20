using System.Threading.Tasks;
using UnityEngine;
using TheSingularityWorkshop.Armada2525;
using TheSingularityWorkshop.Armada2525.Math;

public static class UniverseGenerator
{
    public static bool IsComplete { get; private set; } = false;
    public static float Progress { get; private set; } = 0f;
    public static string CurrentStage { get; private set; } = "Initializing..";

    public static async void StartGeneration(Armada2525 game, GalaxyConfigData galaxyConfig, RaceConfigData raceConfig)
    {
        IsComplete = false;
        Progress = 0f;

        await Task.Run(() =>
        {
            // STAGE 1: Finding Home
            CurrentStage = "Scanning Sectors..";
            int bestSeed = 0;

            // Fake "Work" - looping to find a seed
            for (int i = 0; i < 500; i++)
            {
                // In real implementation, this calls GalaxyMath to check resources
                System.Threading.Thread.Sleep(2);
                Progress = (float)i / 1000f;
            }
            bestSeed = UnityEngine.Random.Range(0, 999999);

            // STAGE 2: Rivals
            CurrentStage = "Simulating Politics..";
            System.Threading.Thread.Sleep(500);
            Progress = 0.8f;

            // STAGE 3: Finalize
            CurrentStage = "Materializing..";
            Progress = 1.0f;
            IsComplete = true;

            // Prepare Data
            var human = new Civilization { Name = "Terran Federation", IsHuman = true };

            // Commit to GameData (Need main thread context usually, but for data objects it's often safe)
            // We'll queue the assignment for the main thread callback ideally, 
            // but for this prototype, we store it.
            _pendingSeed = bestSeed;
            _pendingCiv = human;
        });

        // Apply
        if (game.Data != null)
        {
            game.Data.InitializeGalaxy(_pendingSeed, _pendingCiv);
        }
    }

    private static int _pendingSeed;
    private static Civilization _pendingCiv;
}