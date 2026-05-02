using UnityEngine;
using UnityEngine.UIElements;
using Workshop.BardsTale;
using Workshop.BardsTale.World;
using Workshop.UI_And_Tools.Forge.Builders.GuiBuilders;

namespace Workshop.BardsTale.UI
{
    /// <summary>
    /// Procedural Viewport Generator for the Bard's Tale Experience.
    /// Reconstructs the 1985 first-person perspective by projecting
    /// Sovereign Map data into 3D primitive "Atoms".
    /// </summary>
    public class BardsTaleViewportProvider
    {
        private BardsTaleExperienceContext _ctx;
        private LiveModelPreviewBuilder _preview;
        private GameObject _worldRoot;

        // Constants defining the corridor scale
        private const float CELL_DEPTH = 2.0f;
        private const float WALL_WIDTH = 2.0f;
        private const float WALL_HEIGHT = 2.0f;
        private const float WALL_THICKNESS = 0.1f;

        public BardsTaleViewportProvider(BardsTaleExperienceContext ctx)
        {
            _ctx = ctx;
            // Establish the root for the LiveModelPreview
            _worldRoot = new GameObject("Dungeon_World_Root");
            _worldRoot.hideFlags = HideFlags.HideAndDontSave;

            _preview = new LiveModelPreviewBuilder(_worldRoot)
                .WithCameraPosition(new Vector3(0, 0, -1f)) // Place camera inside the corridor
                .WithBackgroundColor(Color.black)
                .WithGizmos(false);
        }

        public VisualElement CreateViewport()
        {
            var ve = _preview.Build();
            // High-frequency update to keep the viewport synced with player movement
            ve.schedule.Execute(UpdateWorld).Every(16);
            return ve;
        }

        private void UpdateWorld()
        {
            if (_worldRoot == null || _ctx == null || _ctx.CurrentMap == null) return;

            // Purge old geometry before the next procedural synthesis
            foreach (Transform child in _worldRoot.transform)
                UnityEngine.Object.Destroy(child.gameObject);

            // Calculate relative directions based on player orientation
            Vector2Int facing = _ctx.CurrentFacingDirection;
            Vector2Int left = new Vector2Int(-facing.y, facing.x);
            Vector2Int right = new Vector2Int(facing.y, -facing.x);

            // Reconstruct the corridor 4 steps deep
            for (int depth = 0; depth < 4; depth++)
            {
                Vector2Int cellPos = _ctx.CurrentGridPosition + (facing * depth);
                Vector3 worldOffset = new Vector3(0, 0, depth * CELL_DEPTH);

                // --- 1. SENSE SIDE WALLS ---
                // We check the map context for walls relative to the player's current path
                if (_ctx.CurrentMap.GetWall(cellPos, left) == TileFeature.Wall)
                    CreateProceduralSideWall(worldOffset, -1); // Left Wall

                if (_ctx.CurrentMap.GetWall(cellPos, right) == TileFeature.Wall)
                    CreateProceduralSideWall(worldOffset, 1);  // Right Wall

                // --- 2. SENSE FORWARD OBSTRUCTION ---
                // If a wall is directly ahead, we cap the corridor and stop rendering further
                if (_ctx.CurrentMap.GetWall(cellPos, facing) == TileFeature.Wall)
                {
                    CreateProceduralWall(worldOffset, Vector3.one);
                    break;
                }
            }
        }

        /// <summary>
        /// Creates a side wall relative to the corridor center.
        /// side: -1 for Left, 1 for Right.
        /// </summary>
        private void CreateProceduralSideWall(Vector3 offset, int side)
        {
            var wall = GameObject.CreatePrimitive(PrimitiveType.Cube);
            wall.transform.SetParent(_worldRoot.transform);

            // Shift the wall to the side of the 2-unit wide corridor
            float lateralOffset = (WALL_WIDTH / 2f) * side;
            wall.transform.localPosition = offset + new Vector3(lateralOffset, 0, CELL_DEPTH / 2f);

            // Orient the wall to face the center of the corridor
            wall.transform.localScale = new Vector3(WALL_THICKNESS, WALL_HEIGHT, CELL_DEPTH);

            ApplyDungeonMaterial(wall);
        }

        /// <summary>
        /// Creates a front-facing wall to block the corridor.
        /// </summary>
        private void CreateProceduralWall(Vector3 offset, Vector3 scaleMultiplier)
        {
            var wall = GameObject.CreatePrimitive(PrimitiveType.Cube);
            wall.transform.SetParent(_worldRoot.transform);

            // Place at the far end of the current cell depth
            wall.transform.localPosition = offset + new Vector3(0, 0, CELL_DEPTH);

            // Scale to fill the corridor width
            wall.transform.localScale = Vector3.Scale(
                new Vector3(WALL_WIDTH + WALL_THICKNESS, WALL_HEIGHT, WALL_THICKNESS),
                scaleMultiplier);

            ApplyDungeonMaterial(wall);
        }

        private void ApplyDungeonMaterial(GameObject go)
        {
            var renderer = go.GetComponent<Renderer>();
            if (renderer != null)
            {
                // Simple Unlit color for that high-contrast 1985 aesthetic
                renderer.material = new Material(Shader.Find("Unlit/Color"));
                renderer.material.color = new Color(0.2f, 0.2f, 0.25f); // Deep dungeon stone
            }
        }
    }
}