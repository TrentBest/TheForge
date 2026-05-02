using System;
using System.Collections.Generic;
using UnityEngine;
using Workshop.Core.Diagnostics; // Injected for ForgeLogger

namespace Workshop.Core.Memory
{
    public class DataWarehouse : IDisposable
    {
        // Forge Standard: Singleton access for the global state.
        public static DataWarehouse Default { get; } = new DataWarehouse();
        public bool IsDisposed { get; private set; } = false;
        private const int DEFAULT_PAGE_SIZE = 1024;

        private Dictionary<Type, IDataShelf> _unmanagedShelves = new Dictionary<Type, IDataShelf>();
        public Dictionary<string, string> TemporaryCache { get; private set; } = new Dictionary<string, string>();
        private Dictionary<string, object> _managedAssets = new Dictionary<string, object>();
        private List<string> _stringRegistry = new List<string> { "UNDEFINED" };
        private Dictionary<string, int> _stringToIdLookup = new Dictionary<string, int>();
        public Dictionary<Type, IDataShelf> GetAllShelves() => _unmanagedShelves;
        public int RegisterString(string text)
        {
            if (string.IsNullOrEmpty(text)) return 0;
            if (_stringToIdLookup.TryGetValue(text, out int existingId)) return existingId;

            int newId = _stringRegistry.Count;
            _stringRegistry.Add(text);
            _stringToIdLookup[text] = newId;

            ForgeLogger.Log($"Registered String: '{text}' at ID: {newId}")
                .WithHeader("Warehouse")
                .WithColor("#3399FF") // Sky Blue
                .SendToUnity();

            return newId;
        }

        public string ResolveString(int id)
        {
            if (id < 0 || id >= _stringRegistry.Count)
            {
                ForgeLogger.LogWarning($"String Resolution Failed: ID {id} is out of bounds.")
                    .WithHeader("Warehouse")
                    .SendToUnity();
                return "OUT_OF_BOUNDS";
            }

            string resolved = _stringRegistry[id];

            // Note: This is high-traffic, but vital for tracking ID-to-Text shunts
            ForgeLogger.Log($"Resolved ID {id} -> '{resolved}'")
                .WithHeader("Warehouse")
                .WithColor("#3399FF") // Sky Blue
                .SendToUnity();

            return resolved;
        }

        public DataShelf<T> GetOrCreateShelf<T>(int pageSize = DEFAULT_PAGE_SIZE) where T : unmanaged
        {
            Type type = typeof(T);
            if (!_unmanagedShelves.TryGetValue(type, out var shelf))
            {
                ForgeLogger.Log($"Allocating NEW Unmanaged Shelf: {type.Name} (PageSize: {pageSize})")
                    .WithHeader("Memory")
                    .WithColor("#00FFCC") // Neon Cyan
                    .SendToUnity();

                shelf = new DataShelf<T>(pageSize);
                _unmanagedShelves[type] = shelf;
            }
            return (DataShelf<T>)shelf;
        }
        public IDataShelf GetOrCreateShelf(Type type, int pageSize = DEFAULT_PAGE_SIZE)
        {
            if (_unmanagedShelves.TryGetValue(type, out var shelf)) return shelf;

            // Construct the generic DataShelf<T> using the provided Type
            var shelfType = typeof(DataShelf<>).MakeGenericType(type);
            var newShelf = (IDataShelf)Activator.CreateInstance(shelfType, pageSize);
            _unmanagedShelves.Add(type, newShelf);
            return newShelf;
        }
        public DataShelf<T> GetShelf<T>() where T : unmanaged
        {
            if (_unmanagedShelves.TryGetValue(typeof(T), out var shelf))
            {
                //ForgeLogger.Log($"Retrieved Shelf Reference: {typeof(T).Name}")
                //    .WithHeader("Memory")
                //    .WithColor("#00FFCC") // Neon Cyan
                //    .SendToUnity();
                return (DataShelf<T>)shelf;
            }

            ForgeLogger.LogWarning($"Shelf Request Failed: No shelf allocated for type {typeof(T).Name}")
                .WithHeader("Memory")
                .SendToUnity();
            return null;
        }

        // Critical for your GUI Builder: Get a pointer to a specific element in a shelf
        public unsafe T* GetElementPointer<T>(int index) where T : unmanaged
        {
            var shelf = GetOrCreateShelf<T>();
            T* ptr = &((T*)shelf.RawData)[index];

            ForgeLogger.Log($"Exposing Raw Pointer: {typeof(T).Name}* at Index {index}")
                .WithHeader("Unsafe")
                .WithColor("#00FFCC") // Neon Cyan
                .SendToUnity();

            return ptr;
        }

        public void StoreTemporary(string key, string data)
        {
            TemporaryCache[key] = data;
            ForgeLogger.Log($"Cache Update: Key '{key}' stored.")
                .WithHeader("Warehouse")
                .WithColor(Color.gray)
                .SendToUnity();
        }

        public bool TryRetrieveTemporary(string key, out string data)
        {
            bool found = TemporaryCache.TryGetValue(key, out data);

            if (found)
            {
                ForgeLogger.Log($"Cache Hit: '{key}'")
                    .WithHeader("Warehouse")
                    .WithColor(Color.gray)
                    .SendToUnity();
            }

            return found;
        }

        // Aliased to RegisterAsset to support Rule 10: Inter-Experience Handoffs.
        public void RegisterAsset<T>(string id, T asset) where T : class
        {
            _managedAssets[id] = asset;
            ForgeLogger.Log($"Registered Managed Asset: [{id}] of type {typeof(T).Name}")
                .WithHeader("Arbitrator")
                .WithColor(Color.magenta)
                .SendToUnity();
        }

        public void StoreAsset<T>(string id, T asset) where T : class => RegisterAsset(id, asset);

        public bool TryGetAsset<T>(string id, out T asset) where T : class
        {
            if (_managedAssets.TryGetValue(id, out var raw) && raw is T typedAsset)
            {
                asset = typedAsset;
                ForgeLogger.Log($"Asset Retrieval Success: [{id}]")
                    .WithHeader("Arbitrator")
                    .WithColor(Color.magenta)
                    .SendToUnity();
                return true;
            }

            ForgeLogger.LogWarning($"Asset Retrieval Failed: [{id}] not found or type mismatch.")
                .WithHeader("Arbitrator")
                .SendToUnity();
            asset = null;
            return false;
        }

        public void Dispose()
        {
            ForgeLogger.Log("Sovereign Warehouse Shutdown: Purging all unmanaged shelves and managed assets.")
                .WithHeader("Memory")
                .WithColor(Color.red)
                .SendToUnity();
            if (IsDisposed) return;

            foreach (var shelf in _unmanagedShelves.Values) shelf.Dispose();
            _unmanagedShelves.Clear();
            _stringRegistry.Clear();
            _stringToIdLookup.Clear();
            TemporaryCache.Clear();
            _managedAssets.Clear();
            IsDisposed = true;
        }

        public void InjectData(string packageId, object payload)
        {
            ForgeLogger.Log($"Injecting Arbitration Payload: {packageId}_ConfigDelta")
                .WithHeader("Arbitrator")
                .WithColor(Color.magenta)
                .SendToUnity();

            // Records the arbitration payload into the managed assets registry.
            // This allows the package to query its finalized config at O(1) speeds.
            StoreAsset($"{packageId}_ConfigDelta", payload);
        }
    }
}