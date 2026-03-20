using TheSingularityWorkshop.Armada2525.DataModels;
using UnityEngine;

namespace TheSingularityWorkshop.Armada2525.Builders
{
    public static class UniverseCartographer
    {
        // Generates organic (non-lopsided) galaxy positions using basic rejection sampling
        public static void GenerateGalaxies(UniverseData universe, int targetCount)
        {
            universe.Galaxies.Clear();
            System.Random rng = new System.Random(universe.Seed.GetHashCode());

            float minimumSpacing = universe.Radius * 0.15f; // Ensures they don't clump

            for (int i = 0; i < targetCount; i++)
            {
                Vector3 candidatePos = Vector3.zero;
                bool valid = false;
                int attempts = 0;

                while (!valid && attempts < 50)
                {
                    // Generate inside a sphere
                    candidatePos = new Vector3(
                        (float)(rng.NextDouble() * 2 - 1),
                        (float)(rng.NextDouble() * 2 - 1) * 0.2f, // Flatten the universe slightly on the Y axis
                        (float)(rng.NextDouble() * 2 - 1)
                    ).normalized * (float)rng.NextDouble() * universe.Radius;

                    valid = true;
                    foreach (var g in universe.Galaxies)
                    {
                        if (Vector3.Distance(candidatePos, g.UniversePosition) < minimumSpacing)
                        {
                            valid = false;
                            break;
                        }
                    }
                    attempts++;
                }

                if (valid)
                {
                    universe.Galaxies.Add(new GalaxyData
                    {
                        Name = $"Galaxy {i + 1}",
                        UniversePosition = candidatePos,
                        Radius = (float)rng.NextDouble() * 50f + 50f, // 50 to 100 units wide
                        ThemeColor = new Color((float)rng.NextDouble(), (float)rng.NextDouble() * 0.5f + 0.5f, 1f),
                        NumberOfArms = rng.Next(2, 6)
                    });
                }
            }
        }

        // Generates logarithmic spirals for the stars inside a galaxy
        public static void GenerateStarSystems(GalaxyData galaxy, string universeSeed)
        {
            galaxy.StarSystems.Clear();
            System.Random rng = new System.Random((universeSeed + galaxy.Id).GetHashCode());

            int numStars = rng.Next(100, 300); // Amount of visual stars per galaxy

            for (int i = 0; i < numStars; i++)
            {
                // T = distance from center (0 to 1). We square it to cluster more stars in the dense galactic core.
                float t = Mathf.Pow((float)rng.NextDouble(), 2f);

                // Spiral Math
                float angle = t * Mathf.PI * 2f * galaxy.NumberOfArms;
                float r = t * galaxy.Radius;

                float x = r * Mathf.Cos(angle) + (float)(rng.NextDouble() * 2 - 1) * galaxy.ChaosFactor * t;
                float z = r * Mathf.Sin(angle) + (float)(rng.NextDouble() * 2 - 1) * galaxy.ChaosFactor * t;

                // Bulbous core, flat edges
                float y = (float)(rng.NextDouble() * 2 - 1) * (galaxy.ChaosFactor * 0.5f) * (1f - t);

                galaxy.StarSystems.Add(new StarSystemData
                {
                    Name = $"System-{i}",
                    LocalPosition = new Vector3(x, y, z),
                    StarColor = Color.Lerp(Color.white, galaxy.ThemeColor, t) // Core is bright white, edges take on galaxy theme color
                });
            }
        }

        // Creates the physical 3D markers for the previewer
        public static GameObject RenderGalaxyMarker(GalaxyData galaxy, Material unlitMat)
        {
            GameObject gObj = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            gObj.name = $"Macro_{galaxy.Name}";
            gObj.transform.localPosition = galaxy.UniversePosition;
            gObj.transform.localScale = Vector3.one * (galaxy.Radius * 0.2f); // Abstraction size

            Object.DestroyImmediate(gObj.GetComponent<Collider>());

            Material mat = new Material(unlitMat);
            mat.color = new Color(galaxy.ThemeColor.r, galaxy.ThemeColor.g, galaxy.ThemeColor.b, 0.6f);
            gObj.GetComponent<MeshRenderer>().sharedMaterial = mat;
            return gObj;
        }

        public static GameObject RenderStarMarker(StarSystemData star, Material unlitMat)
        {
            GameObject sObj = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            sObj.name = $"Star_{star.Name}";
            sObj.transform.localPosition = star.LocalPosition;
            sObj.transform.localScale = Vector3.one * 1.5f;

            Object.DestroyImmediate(sObj.GetComponent<Collider>());

            Material mat = new Material(unlitMat);
            mat.color = star.StarColor;
            sObj.GetComponent<MeshRenderer>().sharedMaterial = mat;
            return sObj;
        }
    }
}