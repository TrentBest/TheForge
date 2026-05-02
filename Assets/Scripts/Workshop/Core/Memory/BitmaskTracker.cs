using System;
using System.IO;
using Unity.Collections;
using UnityEngine;

namespace Workshop.Core.Memory
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
        /// <summary>
        /// Serializes the bitmask state to the disk "Passenger Car".
        /// </summary>
        public void Export(BinaryWriter writer)
        {
            writer.Write(Capacity);
            writer.Write(Bits.Length);
            for (int i = 0; i < Bits.Length; i++)
            {
                writer.Write(Bits[i]);
            }
        }

        /// <summary>
        /// Rehydrates the bitmask state from the domain dump.
        /// </summary>
        public void Import(BinaryReader reader)
        {
            int savedCapacity = reader.ReadInt32();
            int arrayLength = reader.ReadInt32();

            if (savedCapacity != Capacity)
            {
                // Note: Schema changes during reload are dangerous for unmanaged memory.
                Debug.LogWarning($"[Memory] Bitmask Capacity Mismatch: Current {Capacity}, Saved {savedCapacity}. Alignment may fail.");
            }

            for (int i = 0; i < arrayLength; i++)
            {
                ulong bitData = reader.ReadUInt64();

                // Only write if we have the internal buffer space
                if (i < Bits.Length)
                {
                    Bits[i] = bitData;
                }
            }
        }
        public void Dispose()
        {
            if (Bits.IsCreated) Bits.Dispose();
        }
    }
}