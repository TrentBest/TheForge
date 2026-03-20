
// Assets/Scripts/Chem/ElectronShells.cs
using System;
using System.Collections.Generic;

namespace TheSingularityWorkshop.Chem
{
    public static class ElectronShells
    {
        /// Build shell occupancy as per 2n^2 capacity rule.
        public static List<int> BuildShellOccupancy(int electronCount, int maxShells = 10)
        {
            var shells = new List<int>();
            int remaining = Math.Max(0, electronCount);
            for (int n = 1; n <= maxShells && remaining > 0; n++)
            {
                int capacity = 2 * n * n;
                int fill = Math.Min(capacity, remaining);
                shells.Add(fill);
                remaining -= fill;
            }
            return shells;
        }

        public static int Sum(List<int> shells)
        {
            int total = 0;
            if (shells == null) return 0;
            foreach (var s in shells) total += Math.Max(0, s);
            return total;
        }

        public static bool ValidateShells(List<int> shells)
        {
            if (shells == null) return false;
            for (int i = 0; i < shells.Count; i++)
            {
                int cap = 2 * (i + 1) * (i + 1);
                if (shells[i] > cap || shells[i] < 0) return false;
            }
            return true;
        }
    }
}
