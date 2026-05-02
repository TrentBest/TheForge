using System;

namespace Assets.Scripts.Workshop.Gameplay.Grid
{
    [Serializable]
    public class GridMapData
    {
        public int Width;
        public int Height;
        public float CellSize;

        // 0 = Empty/Default, 1+ = Custom Types (Roads, Walls, Water)
        public int[] Cells;

        public GridMapData(int width, int height, float cellSize = 1.0f)
        {
            Width = width;
            Height = height;
            CellSize = cellSize;
            Cells = new int[width * height];
        }

        public void SetCell(int x, int y, int typeId)
        {
            if (x >= 0 && x < Width && y >= 0 && y < Height)
                Cells[y * Width + x] = typeId;
        }

        public int GetCell(int x, int y)
        {
            if (x >= 0 && x < Width && y >= 0 && y < Height)
                return Cells[y * Width + x];
            return -1; // Out of bounds
        }
    }
}