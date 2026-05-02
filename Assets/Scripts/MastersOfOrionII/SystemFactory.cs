using Assets.Scripts.MastersOfOrionII.Math;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace Assets.Scripts.MastersOfOrionII
{
    public class SystemFactory
    {
        // --- STAR GENERATION ---
        public static Star GenerateStar(int seed)
        {
            // 1. Determine Class based on seed
            // We use the first "random" value to pick the spectral class
            float classRoll = GalaxyMath.Range((uint)seed, 1, 0f, 1f);
            StarSpectralClass type = DetermineSpectralClass(classRoll);

            // 2. Derive Physics from Class (Simplified Main Sequence logic)
            float mass = GetBaseMass(type) * GalaxyMath.Range((uint)seed, 2, 0.8f, 1.2f);
            float radius = Mathf.Pow(mass, 0.8f); // Mass-Radius relation
            float luminosity = Mathf.Pow(mass, 3.5f); // Mass-Luminosity relation
            float temp = 5778f * Mathf.Pow(mass, 0.5f); // Solar temp rough scaling

            return new Star
            {
                Id = seed,
                Class = type,
                Mass = mass,
                Radius = radius,
                Luminosity = luminosity,
                TemperatureK = temp,
                ChromaticAberration = GalaxyMath.GetStarColor((uint)seed)
            };
        }

        // --- PLANET GENERATION ---
        public static List<Planet> GeneratePlanets(Star star)
        {
            var planets = new List<Planet>();

            // Number of planets: 0 to 8 based on seed
            int count = (int)GalaxyMath.Range((uint)star.Id, 10, 0, 9);

            // Goldilocks Zone (Habitable)
            float habitableCenter = Mathf.Sqrt(star.Luminosity);

            for (int i = 0; i < count; i++)
            {
                // Unique Seed for this planet
                uint pSeed = GalaxyMath.GetStarSeed(star.Id, i * 37); // Hash combo

                // Orbit: Titius-Bode-ish law (exponential spacing)
                float orbitBase = 0.4f + (i * 0.3f);
                float orbit = orbitBase * GalaxyMath.Range(pSeed, 1, 0.9f, 1.4f);

                // Determine Type based on Distance & Temperature
                PlanetType type = DeterminePlanetType(orbit, habitableCenter, pSeed);

                // Stats
                float radius = DetermineRadius(type, pSeed);
                float gravity = DetermineGravity(type, radius);

                planets.Add(new Planet
                {
                    Id = (int)pSeed,
                    SystemIndex = i,
                    Type = type,
                    SemiMajorAxisAU = orbit,
                    RadiusEarth = radius,
                    GravityG = gravity,
                    Seed = pSeed,

                    // Economy Data
                    MineralRichness = (ResourceRichness)(GalaxyMath.Range(pSeed, 5, 0, 4)),
                    EnergyPotential = GalaxyMath.Range(pSeed, 6, 0f, 2f),
                    BioDiversity = (type == PlanetType.Terrestrial) ? GalaxyMath.Range(pSeed, 7, 0.5f, 1.5f) : 0f
                });
            }

            return planets;
        }

        // --- HELPERS ---

        private static StarSpectralClass DetermineSpectralClass(float roll)
        {
            // Weighted distribution (M-class is common, O-class is rare)
            if (roll < 0.40f) return StarSpectralClass.M; // 40% Red Dwarf
            if (roll < 0.65f) return StarSpectralClass.K; // 25% Orange
            if (roll < 0.85f) return StarSpectralClass.G; // 20% Yellow (Sun)
            if (roll < 0.95f) return StarSpectralClass.F; // 10% White
            if (roll < 0.98f) return StarSpectralClass.A; // 3%
            if (roll < 0.995f) return StarSpectralClass.B; // 1.5%
            return StarSpectralClass.O; // 0.5% Blue Giant
        }

        private static float GetBaseMass(StarSpectralClass type)
        {
            switch (type)
            {
                case StarSpectralClass.M: return 0.3f;
                case StarSpectralClass.K: return 0.7f;
                case StarSpectralClass.G: return 1.0f;
                case StarSpectralClass.F: return 1.4f;
                case StarSpectralClass.A: return 2.1f;
                case StarSpectralClass.B: return 5.0f;
                case StarSpectralClass.O: return 16.0f;
                default: return 1.0f;
            }
        }

        private static PlanetType DeterminePlanetType(float dist, float habZone, uint seed)
        {
            // Hot Zone (Too close)
            if (dist < habZone * 0.7f)
            {
                if (GalaxyMath.Range(seed, 20, 0, 1) > 0.8f) return PlanetType.Molten;
                return PlanetType.Barren;
            }
            // Goldilocks (Just right)
            if (dist >= habZone * 0.7f && dist <= habZone * 1.4f)
            {
                float roll = GalaxyMath.Range(seed, 21, 0, 1);
                if (roll > 0.6f) return PlanetType.Terrestrial;
                if (roll > 0.3f) return PlanetType.Oceanic;
                return PlanetType.Desert;
            }
            // Cold Zone (Too far)
            else
            {
                float roll = GalaxyMath.Range(seed, 22, 0, 1);
                if (roll > 0.5f) return PlanetType.GasGiant;
                if (roll > 0.2f) return PlanetType.IceGiant;
                return PlanetType.Tundra;
            }
        }

        private static float DetermineRadius(PlanetType type, uint seed)
        {
            switch (type)
            {
                case PlanetType.GasGiant: return GalaxyMath.Range(seed, 30, 8f, 14f);
                case PlanetType.IceGiant: return GalaxyMath.Range(seed, 30, 3f, 6f);
                default: return GalaxyMath.Range(seed, 30, 0.5f, 1.5f); // Rocky worlds
            }
        }

        private static float DetermineGravity(PlanetType type, float radius)
        {
            // G ~ Density * Radius. 
            // Rocky worlds are dense (1.0), Gas are light (0.2).
            float density = (type == PlanetType.GasGiant || type == PlanetType.IceGiant) ? 0.3f : 1.0f;
            return radius * density;
        }
    }
}