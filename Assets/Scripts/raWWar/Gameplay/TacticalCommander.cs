using UnityEngine;
using Workshop.Core;
using Workshop.Core.Memory;
using Assets.Scripts.raWWar.Editors;

namespace Assets.Scripts.raWWar.Gameplay
{
    public static class TacticalCommander
    {
        // We track the active count so the Render Pipeline knows exactly how many to draw
        public static int ActiveTroopCount { get; private set; } = 0;

        public static void DeployFormation(EchelonScale scale, Color factionColor)
        {
            var warehouse = SingularityBootloader.MainWarehouse;
            if (warehouse == null)
            {
                Debug.LogWarning("[Tactical Command] MainWarehouse is offline. Enter Play Mode.");
                return;
            }

            ActiveTroopCount = (int)scale;

            // Ask the warehouse for enough unmanaged memory for the whole army
            var shelf = warehouse.GetOrCreateShelf<FSMAgentData>(ActiveTroopCount);

            System.Diagnostics.Stopwatch sw = System.Diagnostics.Stopwatch.StartNew();

            // Calculate strict military block dimensions (Perfect Square)
            int columns = Mathf.CeilToInt(Mathf.Sqrt(ActiveTroopCount));
            float spacing = 2.0f; // 2 meters between soldiers

            unsafe
            {
                for (int i = 0; i < ActiveTroopCount; i++)
                {
                    shelf.LifeTracker.Set(i);

                    // Grid Math
                    int col = i % columns;
                    int row = i / columns;

                    // Center the massive army at 0,0,0
                    float x = (col * spacing) - ((columns * spacing) / 2f);
                    float z = (row * spacing) - ((columns * spacing) / 2f);

                    // Write to raw unmanaged memory
                    shelf.RawData[i].Position = new Vector3(x, 0, z);
                    shelf.RawData[i].Scale = 1.0f;
                    shelf.RawData[i].Color = factionColor;
                }
            }

            sw.Stop();
            Debug.Log($"[TACTICAL COMMAND] Deployed {ActiveTroopCount:N0} troops to Unmanaged Memory in {sw.Elapsed.TotalMilliseconds:F2} ms.");
        }
    }
}