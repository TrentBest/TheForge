using System;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;

namespace TheSingularityWorkshop.Memory
{
    // The "Office" bitmask tracking
    public struct BitmaskTracker
    {
        // Using a long as a bitmask for 64 "slots" per page
        public NativeArray<ulong> Bits;

        public bool IsDirty(int index) => (Bits[index / 64] & (1UL << (index % 64))) != 0;
        public void SetDirty(int index) => Bits[index / 64] |= (1UL << (index % 64));
    }

    public unsafe class DataWarehouse : IDisposable
    {
        public BitmaskTracker LifeTracker;
        public BitmaskTracker DirtyTracker;

        // This is the "Shelf" - raw bytes that we cast to types
        public void* RawDataShelf;
        public int PageSize;

        public void Dispose()
        {
            if (RawDataShelf != null) UnsafeUtility.Free(RawDataShelf, Allocator.Persistent);
            // Dispose NativeArrays..
        }
    }
}