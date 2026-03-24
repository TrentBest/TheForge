using System;
using System.Collections.Generic;
using UnityEngine;

namespace TheSingularityWorkshop.Forge.Builders.GuiBuilders
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

        public void DrawLine(Vector2 start, Vector2 end) { /* Implementation */ }
        public void PlaceSymbol(string symbolId, Vector2 position) { /* Implementation */ }

        internal void PlaceSymbol(string v, object centerPoint)
        {
            throw new NotImplementedException();
        }
    }

    public struct MapIcon { public string Id; public Vector2 Position; }
}