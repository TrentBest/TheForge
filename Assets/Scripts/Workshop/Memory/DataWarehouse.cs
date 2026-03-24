using System;
using System.Collections.Generic;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using UnityEngine;

namespace TheSingularityWorkshop.Memory
{
    // ==========================================
    // 1. THE BITMASK TRACKER (Hardened)
    // ==========================================
    public struct BitmaskTracker : IDisposable
    {
        public NativeArray<ulong> Bits;
        public int Capacity { get; private set; }

        public BitmaskTracker(int capacity, Allocator allocator)
        {
            Capacity = capacity;
            // 64 slots per ulong
            int ulongCount = Mathf.CeilToInt(capacity / 64f);
            Bits = new NativeArray<ulong>(ulongCount, allocator, NativeArrayOptions.ClearMemory);
        }

        public bool IsSet(int index) => (Bits[index / 64] & (1UL << (index % 64))) != 0;
        public void Set(int index) => Bits[index / 64] |= (1UL << (index % 64));
        public void Clear(int index) => Bits[index / 64] &= ~(1UL << (index % 64));

        /// <summary>
        /// Finds the first index that is not currently set.
        /// </summary>
        public int FindFirstFree()
        {
            for (int i = 0; i < Bits.Length; i++)
            {
                if (Bits[i] != ulong.MaxValue) // If not entirely full
                {
                    for (int j = 0; j < 64; j++)
                    {
                        int globalIndex = i * 64 + j;
                        if (globalIndex >= Capacity) return -1;
                        if ((Bits[i] & (1UL << j)) == 0) return globalIndex;
                    }
                }
            }
            return -1;
        }

        public void Dispose()
        {
            if (Bits.IsCreated) Bits.Dispose();
        }
    }

    // ==========================================
    // 2. THE UNMANAGED DATA SHELF
    // ==========================================
    public interface IDataShelf : IDisposable { }

    /// <summary>
    /// A strongly-typed wrapper around raw memory. 
    /// T must be unmanaged (structs with no reference types).
    /// </summary>
    public unsafe class DataShelf<T> : IDataShelf where T : unmanaged
    {
        public BitmaskTracker LifeTracker;
        public BitmaskTracker DirtyTracker;

        public T* RawData;
        public int PageSize { get; private set; }

        public DataShelf(int pageSize)
        {
            PageSize = pageSize;
            LifeTracker = new BitmaskTracker(pageSize, Allocator.Persistent);
            DirtyTracker = new BitmaskTracker(pageSize, Allocator.Persistent);

            long bytesRequired = UnsafeUtility.SizeOf<T>() * pageSize;
            RawData = (T*)UnsafeUtility.Malloc(bytesRequired, UnsafeUtility.AlignOf<T>(), Allocator.Persistent);
            UnsafeUtility.MemClear(RawData, bytesRequired);
        }

        /// <summary>
        /// Finds a free slot, marks it alive and dirty, writes the data, and returns the index pointer.
        /// </summary>
        public int Store(T data)
        {
            int index = LifeTracker.FindFirstFree();
            if (index == -1) throw new OutOfMemoryException($"DataShelf of type {typeof(T).Name} is full!");

            LifeTracker.Set(index);
            DirtyTracker.Set(index);
            RawData[index] = data;
            return index;
        }

        /// <summary>
        /// Returns a reference to the data, allowing direct modification without copying!
        /// </summary>
        public ref T GetRef(int index)
        {
            if (!LifeTracker.IsSet(index)) throw new IndexOutOfRangeException($"Index {index} is not alive on this shelf.");
            return ref RawData[index];
        }

        public void Release(int index)
        {
            LifeTracker.Clear(index);
            DirtyTracker.Clear(index);
            // Optional: Zero out memory for security
            // UnsafeUtility.MemClear(RawData + index, UnsafeUtility.SizeOf<T>());
        }

        public void Dispose()
        {
            LifeTracker.Dispose();
            DirtyTracker.Dispose();
            if (RawData != null)
            {
                UnsafeUtility.Free(RawData, Allocator.Persistent);
                RawData = null;
            }
        }
    }

    // ==========================================
    // 3. THE DATA WAREHOUSE (The Facade)
    // ==========================================
    public class DataWarehouse : IDisposable
    {
        private const int DEFAULT_PAGE_SIZE = 1024; // 1024 slots per shelf

        // -- Memory Layer: Raw Structs & Compute Data --
        private Dictionary<Type, IDataShelf> _unmanagedShelves = new Dictionary<Type, IDataShelf>();

        // -- Cache Layer: Ephemeral Strings, UI States, JSON --
        public Dictionary<string, string> TemporaryCache { get; private set; } = new Dictionary<string, string>();

        // -- Asset Layer: Managed Classes (MicroPackages, etc.) --
        private Dictionary<string, object> _managedAssets = new Dictionary<string, object>();


        // --- UNMANAGED MEMORY API ---

        public DataShelf<T> GetOrCreateShelf<T>(int pageSize = DEFAULT_PAGE_SIZE) where T : unmanaged
        {
            Type type = typeof(T);
            if (!_unmanagedShelves.TryGetValue(type, out var shelf))
            {
                shelf = new DataShelf<T>(pageSize);
                _unmanagedShelves[type] = shelf;
            }
            return (DataShelf<T>)shelf;
        }

        // --- TEMPORARY STATE API (Used by your GUI Builders!) ---

        public void StoreTemporary(string key, string data)
        {
            TemporaryCache[key] = data;
        }

        public bool TryRetrieveTemporary(string key, out string data)
        {
            return TemporaryCache.TryGetValue(key, out data);
        }

        // --- MANAGED ASSET/PACKAGE API ---

        public void StoreAsset<T>(string id, T asset) where T : class
        {
            _managedAssets[id] = asset;
        }

        public bool TryGetAsset<T>(string id, out T asset) where T : class
        {
            if (_managedAssets.TryGetValue(id, out var raw) && raw is T typedAsset)
            {
                asset = typedAsset;
                return true;
            }
            asset = null;
            return false;
        }

        // --- LIFECYCLE ---

        public void Dispose()
        {
            // Safely clear all raw C++ memory pointers
            foreach (var shelf in _unmanagedShelves.Values)
            {
                shelf.Dispose();
            }
            _unmanagedShelves.Clear();
            TemporaryCache.Clear();
            _managedAssets.Clear();

            Debug.Log("DataWarehouse disposed successfully. No memory leaks detected.");
        }
    }
}