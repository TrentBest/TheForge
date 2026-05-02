using System;
using System.Collections.Generic;
using UnityEngine;

namespace Workshop.UI_And_Tools.Forge.Builders.GuiBuilders.WorldBuilding
{
    /// <summary>
    /// A buffer representing the "Artifact" being created.
    /// Handles the transformation of world coordinates to map coordinates.
    /// </summary>
    public class CartoCanvas
    {
        public RenderTexture MapTexture;
        public List<Vector2> PathPoints = new List<Vector2>();
        public List<MapIcon> Icons = new List<MapIcon>();

        // Canvas dimensions to properly handle coordinate transformations
        public float CanvasWidth { get; private set; } = 1024f;
        public float CanvasHeight { get; private set; } = 1024f;

        public CartoCanvas() { }

        public CartoCanvas(int width, int height)
        {
            CanvasWidth = width;
            CanvasHeight = height;
            MapTexture = new RenderTexture(width, height, 0, RenderTextureFormat.ARGB32);
            MapTexture.Create();
        }

        public void DrawLine(Vector2 start, Vector2 end)
        {
            // Store the raw path data for the renderer/painter to interpret
            PathPoints.Add(start);
            PathPoints.Add(end);
        }

        public void PlaceSymbol(string symbolId, Vector2 position)
        {
            Icons.Add(new MapIcon { Id = symbolId, Position = position });
        }

        // Fixed the IDE auto-generated stub
        public void PlaceSymbol(string symbolId, object centerPoint)
        {
            if (centerPoint is Vector2 v2)
            {
                PlaceSymbol(symbolId, v2);
            }
            else if (centerPoint is Vector3 v3)
            {
                // Map 3D world coordinates to 2D canvas coordinates (Assuming X/Z top-down mapping)
                PlaceSymbol(symbolId, new Vector2(v3.x, v3.z));
            }
            else
            {
                Debug.LogWarning($"[CartoCanvas] Unsupported point type provided for symbol '{symbolId}': {centerPoint?.GetType()}");
            }
        }

        public void Clear()
        {
            PathPoints.Clear();
            Icons.Clear();

            // Wipe the render texture buffer
            if (MapTexture != null)
            {
                var oldRt = RenderTexture.active;
                RenderTexture.active = MapTexture;
                GL.Clear(true, true, Color.clear);
                RenderTexture.active = oldRt;
            }
        }

        public void Dispose()
        {
            // Prevent memory leaks when the UI is closed
            if (MapTexture != null)
            {
                MapTexture.Release();
                UnityEngine.Object.DestroyImmediate(MapTexture);
                MapTexture = null;
            }
        }
    }

    public struct MapIcon
    {
        public string Id;
        public Vector2 Position;
    }
}