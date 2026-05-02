using UnityEngine;
using Workshop;
using Workshop.Core.Memory;
using Workshop.GURPS;

namespace Assets.Scripts.Unity
{
    /// <summary>
    /// A concrete bridge between Unity Cloud Services and the Workshop's DataWarehouse.
    /// Maps keys to cloud handshake parameters using established Warehouse accessors.
    /// </summary>
    public static class DataShelfMappingUtility
    {
        // =========================================================================
        // OPTION A: THE DATA-ORIENTED APPROACH (Using Unmanaged Memory)
        // Use this if your game entities are tracked by integer IDs (e.g., Player 0, Player 1)
        // =========================================================================
        public static int GetSkillRating(DataWarehouse warehouse, int playerEntityIndex = 0)
        {
            if (warehouse == null) return 1000;

            // 1. Get the entire unmanaged memory shelf for integers
            var ratingShelf = warehouse.GetOrCreateShelf<int>();

            // 2. Check the bitmask to see if this specific index actually has data
            if (ratingShelf.LifeTracker.IsSet(playerEntityIndex))
            {
                // 3. Grab the value directly from the raw memory pointer
                return ratingShelf.GetRef(playerEntityIndex);
            }

            Debug.LogWarning($"[Liaison] DataShelf<int> at index '{playerEntityIndex}' is uninitialized. Falling back to default (1000).");
            return 1000;
        }

        // =========================================================================
        // OPTION B: THE STRING-KEY APPROACH (Using the Temporary Cache)
        // Use this if Unity Cloud Save dictates that you must use string keys.
        // =========================================================================
        public static int GetSkillRatingFromCloudKey(DataWarehouse warehouse, string cloudKey)
        {
            if (warehouse == null) return 1000;

            // Look up the string dictionary designed specifically for ephemeral/cloud data
            if (warehouse.TryRetrieveTemporary(cloudKey, out string rawValue))
            {
                if (int.TryParse(rawValue, out int rating))
                {
                    return rating;
                }
            }

            Debug.LogWarning($"[Liaison] Cloud Key '{cloudKey}' not discovered in TemporaryCache. Falling back to default (1000).");
            return 1000;
        }
    }
}