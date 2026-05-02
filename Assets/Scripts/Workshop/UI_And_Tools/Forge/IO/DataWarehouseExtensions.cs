// File: Assets/Scripts/Workshop/UI_And_Tools/Forge/IO/DataWarehouseExtensions.cs
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using Workshop.Core.Memory;

namespace Workshop.UI_And_Tools.Forge.IO
{
    public static class DataWarehouseExtensions
    {
        private static readonly List<Texture> _textureRegistry = new List<Texture>();

        // --- EXISTING TEMPORARY CACHE LOGIC ---
        public static void StoreTemporary(this DataWarehouse dw, string key, string value)
        {
            if (dw.TemporaryCache.ContainsKey(key)) dw.TemporaryCache[key] = value;
            else dw.TemporaryCache.Add(key, value);
        }

        public static bool TryRetrieveTemporary(this DataWarehouse dw, string key, out string value)
        {
            return dw.TemporaryCache.TryGetValue(key, out value);
        }

        // --- NEW ECOSYSTEM SERIALIZATION ---

        /// <summary>
        /// Serializes the entire unmanaged memory state and temporary caches.
        /// </summary>
        public static void ExportEcosystem(this DataWarehouse dw, BinaryWriter writer)
        {
            // 1. Export Temporary Cache
            writer.Write(dw.TemporaryCache.Count);
            foreach (var kvp in dw.TemporaryCache)
            {
                writer.Write(kvp.Key);
                writer.Write(kvp.Value);
            }
            
            // 2. Export Unmanaged Shelves
            var shelves = dw.GetAllShelves();
            writer.Write(shelves.Count);

            foreach (var kvp in shelves)
            {
                // Write the type name so we can resolve the shelf on the other side
                writer.Write(kvp.Key.AssemblyQualifiedName);

                // [FIX]: Explicitly write the PageSize to the stream before exporting the data
                writer.Write(kvp.Value.PageSize);

                kvp.Value.Export(writer);
            }
        }

        /// <summary>
        /// Rehydrates the warehouse state from a binary stream.
        /// </summary>
        public static void ImportEcosystem(this DataWarehouse dw, BinaryReader reader)
        {
            // 1. Import Temporary Cache
            int cacheCount = reader.ReadInt32();
            for (int i = 0; i < cacheCount; i++)
            {
                dw.StoreTemporary(reader.ReadString(), reader.ReadString());
            }

            // 2. Import Unmanaged Shelves
            int shelfCount = reader.ReadInt32();
            for (int i = 0; i < shelfCount; i++)
            {
                string typeName = reader.ReadString();

                // [FIX]: Read the PageSize back out from the stream
                int savedPageSize = reader.ReadInt32();

                var shelfType = System.Type.GetType(typeName);

                if (shelfType != null)
                {
                    // [FIX]: Pass the savedPageSize to bypass the 1024 default!
                    var shelf = dw.GetOrCreateShelf(shelfType, savedPageSize);
                    shelf.Import(reader);
                }
            }
        }

        /// <summary>
        /// Bridges a Unity Texture into the sovereign ecosystem, returning a stable ID.
        /// </summary>
        public static int RegisterTexture(this DataWarehouse dw, Texture texture)
        {
            if (texture == null) return -1;

            int existingIndex = _textureRegistry.IndexOf(texture);
            if (existingIndex != -1) return existingIndex;

            _textureRegistry.Add(texture);
            return _textureRegistry.Count - 1;
        }

        public static Texture GetTexture(this DataWarehouse dw, int id)
        {
            if (id < 0 || id >= _textureRegistry.Count) return null;
            return _textureRegistry[id];
        }
    }
}