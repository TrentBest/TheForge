using TheSingularityWorkshop.Memory;

namespace TheSingularityWorkshop.Forge.IO
{
    public static class DataWarehouseExtensions
    {
        // Fixes 'StoreTemporary' and 'TryRetrieveTemporary' errors
        public static void StoreTemporary(this DataWarehouse dw, string key, string value)
        {
            if (dw.TemporaryCache.ContainsKey(key)) dw.TemporaryCache[key] = value;
            else dw.TemporaryCache.Add(key, value);
        }

        public static bool TryRetrieveTemporary(this DataWarehouse dw, string key, out string value)
        {
            return dw.TemporaryCache.TryGetValue(key, out value);
        }
    }
}