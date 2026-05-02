// File: Assets/Scripts/Workshop/Core/Memory/DataShelf.cs
using System;
using System.IO;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using Workshop.Core.Diagnostics;

namespace Workshop.Core.Memory
{
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

            ForgeLogger.Log($"Allocated Unmanaged Shelf<{typeof(T).Name}>: {bytesRequired} bytes across {pageSize} slots.")
                .WithHeader("Memory")
                .WithColor("#00FFCC") // Neon Cyan
                .SendToUnity();
        }

        /// <summary>
        /// Finds a free slot, marks it alive and dirty, writes the data, and returns the index pointer.
        /// </summary>
        public int Store(T data)
        {
            int index = LifeTracker.FindFirstFree();
            if (index == -1)
            {
                ForgeLogger.LogError($"CRITICAL: DataShelf<{typeof(T).Name}> is out of memory! PageSize {PageSize} exceeded.")
                    .WithHeader("Warehouse")
                    .SendToUnity();
                throw new OutOfMemoryException($"DataShelf of type {typeof(T).Name} is full!");
            }

            LifeTracker.Set(index);
            DirtyTracker.Set(index);
            RawData[index] = data;
            return index;
        }

        /// <summary>
        /// Overwrites data at a specific index and marks the slot as dirty for synchronization.
        /// Use this to avoid OutOfMemoryExceptions when updating singleton entities like the WorldObserver.
        /// </summary>
        public void Update(int index, T data)
        {
            if (index < 0 || index >= PageSize)
            {
                ForgeLogger.LogError($"Update Failed: Index {index} is out of bounds for Shelf<{typeof(T).Name}> (Size: {PageSize}).")
                    .WithHeader("Memory")
                    .SendToUnity();
                throw new IndexOutOfRangeException();
            }

            // If the slot wasn't active, we activate it now.
            if (!LifeTracker.IsSet(index))
            {
                LifeTracker.Set(index);
            }

            RawData[index] = data;
            DirtyTracker.Set(index);
        }

        /// <summary>
        /// Returns a reference to the data, allowing direct modification without copying!
        /// </summary>
        public ref T GetRef(int index)
        {
            if (!LifeTracker.IsSet(index))
            {
                ForgeLogger.LogError($"Illegal Memory Access: Index {index} on Shelf<{typeof(T).Name}> is not alive.")
                    .WithHeader("Safety")
                    .SendToUnity();
                throw new IndexOutOfRangeException($"Index {index} is not alive on this shelf.");
            }
            return ref RawData[index];
        }

        public void Release(int index)
        {
            ForgeLogger.Log($"Releasing Slot {index} on Shelf<{typeof(T).Name}>. Clearing memory.")
                .WithHeader("Memory")
                .WithColor(UnityEngine.Color.yellow)
                .SendToUnity();

            LifeTracker.Clear(index);
            DirtyTracker.Clear(index);
            UnsafeUtility.MemClear(RawData + index, UnsafeUtility.SizeOf<T>());
        }

        public bool IsActive(int index) => LifeTracker.IsSet(index);

        public void Dispose()
        {
            ForgeLogger.Log($"Purging Unmanaged Shelf<{typeof(T).Name}> and returning memory to OS.")
                .WithHeader("Memory")
                .WithColor(UnityEngine.Color.red)
                .SendToUnity();

            LifeTracker.Dispose();
            DirtyTracker.Dispose();
            if (RawData != null)
            {
                UnsafeUtility.Free(RawData, Allocator.Persistent);
                RawData = null;
            }
        }

        /// <summary>
        /// Serializes the raw unmanaged memory and the life-cycle bitmask.
        /// </summary>
        public void Export(BinaryWriter writer)
        {
            // 1. Store Metadata
            writer.Write(PageSize);

            // 2. Export Bitmasks (Requires BitmaskTracker to have Export/Import)
            LifeTracker.Export(writer);
            DirtyTracker.Export(writer);

            // 3. Export Raw Memory Block
            int structSize = UnsafeUtility.SizeOf<T>();
            byte* bytePtr = (byte*)RawData;
            int totalBytes = structSize * PageSize;

            // Efficiently write the entire block as bytes
            for (int i = 0; i < totalBytes; i++)
            {
                writer.Write(bytePtr[i]);
            }
        }

        /// <summary>
        /// Rehydrates the shelf from a binary stream.
        /// </summary>
        public void Import(BinaryReader reader)
        {
            int savedPageSize = reader.ReadInt32();

            // Safety: Ensure we aren't loading a different schema
            if (savedPageSize != PageSize)
            {
                ForgeLogger.LogError($"Import Failed: PageSize Mismatch on Shelf<{typeof(T).Name}>. Expected {PageSize}, got {savedPageSize}.")
                    .WithHeader("Memory")
                    .SendToUnity();
                return;
            }

            // 2. Import Bitmasks
            LifeTracker.Import(reader);
            DirtyTracker.Import(reader);

            // 3. Import Raw Memory Block
            int structSize = UnsafeUtility.SizeOf<T>();
            byte* bytePtr = (byte*)RawData;
            int totalBytes = structSize * PageSize;

            for (int i = 0; i < totalBytes; i++)
            {
                bytePtr[i] = reader.ReadByte();
            }
        }
    }
}