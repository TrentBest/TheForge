using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Workshop.UI_And_Tools.Forge.Builders.GuiBuilders.WorldBuilding
{
    public class CityBuilder
    {
        private SettlementEnvironmentData _envContext;
        private Transform _cityRoot;
        private Terrain _localTerrain;

        private List<BuildingBlueprint> _blueprintRegistry;

        // --- NEW: The Palette Bridge ---
        private List<UrbanZoneType> _zonePalette;

        public CityBuilder(
            SettlementEnvironmentData envContext,
            Transform root,
            Terrain terrain,
            List<BuildingBlueprint> registry,
            List<UrbanZoneType> zonePalette) // Inject the palette
        {
            _envContext = envContext;
            _cityRoot = root;
            _localTerrain = terrain;
            _blueprintRegistry = registry;
            _zonePalette = zonePalette;
        }

        public void GenerateSprawl(Texture2D mapData, Vector2 mapPhysicalSize)
        {
            Color[] pixels = mapData.GetPixels();
            int width = mapData.width;
            int height = mapData.height;

            float lotWidth = mapPhysicalSize.x / width;
            float lotDepth = mapPhysicalSize.y / height;

            GameObject buildingsContainer = new GameObject($"UrbanSprawl_TL{_envContext.TechLevel}");
            buildingsContainer.transform.SetParent(_cityRoot, false);

            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    Color pixel = pixels[y * width + x];

                    byte zoneId = ZoningMath.DecodeZone(pixel.b);
                    float density = pixel.a;

                    if (zoneId != 0 && density > 0.05f)
                    {
                        var validBlueprints = _blueprintRegistry.Where(b =>
                            b.RequiredZoneId == zoneId &&
                            _envContext.TechLevel >= b.MinTechLevel &&
                            _envContext.TechLevel <= b.MaxTechLevel &&
                            density >= b.MinUrbanDensity &&
                            density <= b.MaxUrbanDensity
                        ).ToList();

                        if (validBlueprints.Count > 0)
                        {
                            BuildingBlueprint selectedBlueprint = validBlueprints[UnityEngine.Random.Range(0, validBlueprints.Count)];

                            float worldX = (x * lotWidth) + (lotWidth / 2f);
                            float worldZ = (y * lotDepth) + (lotDepth / 2f);

                            float worldY = _localTerrain.SampleHeight(new Vector3(worldX, 0, worldZ));
                            Vector3 spawnPos = new Vector3(worldX, worldY, worldZ);

                            ConstructBuilding(selectedBlueprint, spawnPos, density, buildingsContainer.transform);
                        }
                    }
                }
            }

            Debug.Log($"[CityBuilder] Sprawl generation complete for Tech Level {_envContext.TechLevel}.");
        }

        private void ConstructBuilding(BuildingBlueprint blueprint, Vector3 position, float localDensity, Transform parent)
        {
            GameObject building = GameObject.CreatePrimitive(PrimitiveType.Cube);
            building.name = $"Bldg_{blueprint.Name}";
            building.transform.position = position;
            building.transform.SetParent(parent, true);

            int[] angles = { 0, 90, 180, 270 };
            building.transform.rotation = Quaternion.Euler(0, angles[UnityEngine.Random.Range(0, angles.Length)], 0);

            float height = blueprint.BaseHeight;
            if (blueprint.ScaleHeightWithDensity)
            {
                height *= (localDensity * 10f);
            }

            building.transform.localScale = new Vector3(blueprint.FootprintDimensions.x, height, blueprint.FootprintDimensions.y);
            building.transform.position += new Vector3(0, height / 2f, 0);

            // --- FIXED: Read direct from the palette, and use Unlit Shader to preserve vibrancy! ---
            Material mat = new Material(Shader.Find("Unlit/Color") ?? Shader.Find("Sprites/Default"));
            mat.color = GetExactPaletteColor(blueprint.RequiredZoneId);
            building.GetComponent<MeshRenderer>().sharedMaterial = mat;
        }

        private Color GetExactPaletteColor(byte zoneId)
        {
            if (_zonePalette != null)
            {
                var definition = _zonePalette.FirstOrDefault(z => z.Id == zoneId);
                if (definition != null)
                {
                    return definition.DisplayColor; // Return the exact UI color
                }
            }
            return Color.magenta; // A harsh magenta means "Missing Data!"
        }
    }
}