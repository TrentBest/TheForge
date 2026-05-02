using UnityEngine;
using System;
using Assets.Scripts.MastersOfOrionII.Math; // Utilizing your existing Rng

namespace Workshop.UI_And_Tools.Forge.Builders.Staging
{
    public static class CosmicSeedManager
    {
        private const string SEED_KEY = "Singularity_Cosmic_Seed";

        public static int ConsumeAndMutateSeed()
        {
            // Default to 42 on the very first load
            int currentSeed = PlayerPrefs.GetInt(SEED_KEY, 42);

            // Generate the next seed based on the current one to ensure true chaos
            var rng = new System.Random(currentSeed);
            int nextSeed = rng.Next(10000, 999999);

            // Mutate the seed for the next visit
            PlayerPrefs.SetInt(SEED_KEY, nextSeed);
            PlayerPrefs.Save();

            Debug.Log($"<color=purple><b>[SEED MUTATOR]</b></color> Consumed Seed: {currentSeed}. Next trajectory locked to: {nextSeed}");

            return currentSeed;
        }
    }
}