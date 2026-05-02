using System.Collections.Generic;
using UnityEngine;

namespace Assets.Scripts.Workshop.Gameplay.Grid
{
    public class GridTilePalette
    {
        public struct TileVisual
        {
            public Mesh Mesh;
            public Material Material;
        }

        private Dictionary<int, TileVisual> _palette = new Dictionary<int, TileVisual>();

        public void RegisterTile(int typeId, Mesh mesh, Material material)
        {
            // Ensure the material has instancing enabled for massive rendering!
            if (material != null && !material.enableInstancing)
                material.enableInstancing = true;

            _palette[typeId] = new TileVisual { Mesh = mesh, Material = material };
        }

        public bool TryGetVisual(int typeId, out TileVisual visual)
        {
            return _palette.TryGetValue(typeId, out visual);
        }
    }
}