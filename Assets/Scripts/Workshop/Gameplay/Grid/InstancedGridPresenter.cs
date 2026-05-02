using System.Collections.Generic;
using UnityEngine;

namespace Assets.Scripts.Workshop.Gameplay.Grid
{
    public class InstancedGridPresenter
    {
        private GridMapData _mapData;
        private GridTilePalette _palette;

        // Caches the world-space matrices for every tile type
        private Dictionary<int, Matrix4x4[]> _instancedMatrices = new Dictionary<int, Matrix4x4[]>();
        private Dictionary<int, int> _instanceCounts = new Dictionary<int, int>();

        public InstancedGridPresenter(GridMapData mapData, GridTilePalette palette)
        {
            _mapData = mapData;
            _palette = palette;
            RebuildMatrixCache();
        }

        // Call this whenever the map data changes (e.g., player builds a wall)
        public void RebuildMatrixCache()
        {
            _instancedMatrices.Clear();
            _instanceCounts.Clear();

            // 1. Count how many of each tile type we have
            for (int i = 0; i < _mapData.Cells.Length; i++)
            {
                int type = _mapData.Cells[i];
                if (!_instanceCounts.ContainsKey(type)) _instanceCounts[type] = 0;
                _instanceCounts[type]++;
            }

            // 2. Allocate arrays for the matrices
            foreach (var kvp in _instanceCounts)
            {
                // Note: RenderMeshInstanced has a max of 1023 per call, 
                // but Unity's modern RenderParams API handles larger batches automatically.
                _instancedMatrices[kvp.Key] = new Matrix4x4[kvp.Value];
            }

            // 3. Calculate the Matrix for every cell
            Dictionary<int, int> currentIndices = new Dictionary<int, int>();
            float offset = _mapData.CellSize;
            float startX = -(_mapData.Width * offset) / 2f;
            float startZ = -(_mapData.Height * offset) / 2f;

            for (int y = 0; y < _mapData.Height; y++)
            {
                for (int x = 0; x < _mapData.Width; x++)
                {
                    int type = _mapData.GetCell(x, y);

                    if (!currentIndices.ContainsKey(type)) currentIndices[type] = 0;
                    int index = currentIndices[type]++;

                    // Position the tile in the world
                    Vector3 pos = new Vector3(startX + (x * offset), 0, startZ + (y * offset));
                    _instancedMatrices[type][index] = Matrix4x4.TRS(pos, Quaternion.identity, Vector3.one);
                }
            }
        }

        // Bound to the LiveFormationPreviewBuilder render loop!
        public void RenderMap(Camera renderCamera)
        {
            Bounds mapBounds = new Bounds(Vector3.zero, new Vector3(10000, 10000, 10000));

            foreach (var kvp in _instancedMatrices)
            {
                int typeId = kvp.Key;
                Matrix4x4[] matrices = kvp.Value;

                if (_palette.TryGetVisual(typeId, out var visual) && matrices.Length > 0)
                {
                    RenderParams rparams = new RenderParams(visual.Material);
                    rparams.camera = renderCamera;
                    rparams.worldBounds = mapBounds;
                    rparams.receiveShadows = true;

                    // Instantly draws every tile of this type in a single GPU call!
                    Graphics.RenderMeshInstanced(rparams, visual.Mesh, 0, matrices);
                }
            }
        }
    }
}