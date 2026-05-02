using System;
using System.Collections.Generic;

// Explicitly defining your abstraction paths to avoid naming collisions
using Assets.Scripts.MastersOfOrionII;
using Assets.Scripts.MastersOfOrionII.Economy;
using Assets.Scripts.MastersOfOrionII.Government;
using Workshop.GURPS;


// Using your own Workshop MicroPackages for the lowest level Engine State
using Workshop.Systems.MicroPackages;

namespace Workshop.UI_And_Tools.Forge.Builders.Staging
{
    // Removed the generic interface to satisfy your non-generic IBuilder constraints
    public class OpeningSequenceBuilder
    {
        public List<ISequenceNode> Build()
        {
            int activeSeed = CosmicSeedManager.ConsumeAndMutateSeed();
            var sequence = new List<ISequenceNode>();

            if (activeSeed == 42)
            {
                // THE CANONICAL BIG BANG (The Pitch Sequence)
                sequence.Add(new SequenceNode("MACRO_COSMOS", typeof(UniverseData), 3.0f));
                sequence.Add(new SequenceNode("MESO_SYSTEM", typeof(ColonizedSystem), 2.5f));
                sequence.Add(new SequenceNode("MICRO_PLANET", typeof(ColonyData), 2.0f));

                // Diving into the FSM core
                sequence.Add(new SequenceNode("NANO_ENGINE", typeof(State), 3.5f));
            }
            else
            {
                // THE CHAOS DIVE (Procedural Generation)
                System.Random rng = new System.Random(activeSeed);
                int jumpCount = rng.Next(3, 6); // Randomize how many jumps they take

                for (int i = 0; i < jumpCount; i++)
                {
                    Type randomContext = GetProceduralContext(rng);
                    float randomDuration = (float)(rng.NextDouble() * 2.0 + 1.0); // 1 to 3 seconds

                    sequence.Add(new SequenceNode($"PROCEDURAL_JUMP_{i}", randomContext, randomDuration));
                }

                // Always end on the engine logic before kicking to the Lobby
                sequence.Add(new SequenceNode("NANO_ENGINE", typeof(State), 2.0f));
            }

            return sequence;
        }

        private Type GetProceduralContext(System.Random rng)
        {
            // Now the compiler knows exactly where these types live
            int roll = rng.Next(0, 4);
            return roll switch
            {
                0 => typeof(GalaxyMorphology),       // from Assets.Scripts.MastersOfOrionII
                1 => typeof(GalacticStockExchange),  // from Assets.Scripts.MastersOfOrionII.Economy
                2 => typeof(PoliticalFaction),       // from Assets.Scripts.MastersOfOrionII.Government
                _ => typeof(GURPSUniverse)           // from Assets.Scripts.MastersOfOrionII.GURPS
            };
        }
    }
}