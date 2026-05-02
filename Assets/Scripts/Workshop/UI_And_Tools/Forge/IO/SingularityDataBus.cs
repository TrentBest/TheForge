using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using TheSingularityWorkshop.FSM_API;
using Workshop.Systems.MicroPackages; // IStateContext namespace

namespace Workshop.UI_And_Tools.Forge.IO
{
    /// <summary>
    /// The Central Nervous System. 
    /// Lobotomized from MonoBehaviour to run deterministically within the FSM API.
    /// </summary>
    public class SingularityDataBus : IStateContext
    {
        // Pure C# Singleton - Zero GameObject overhead
        private static SingularityDataBus _instance;
        public static SingularityDataBus Instance => _instance ??= new SingularityDataBus();

        // --- IStateContext Implementation ---
        public bool IsValid { get; set; } = true;
        public string Name { get; set; } = "Singularity_Data_Bus";

        // O(1) Type-Safe Local Pub/Sub (Zero Boxing)
        private readonly Dictionary<string, Delegate> _subscribers = new Dictionary<string, Delegate>();

        // External Routing & Binary Networking
        private readonly Dictionary<string, IDataTransport> _routes = new Dictionary<string, IDataTransport>();
        private readonly Dictionary<string, Queue<byte[]>> _outboundQueues = new Dictionary<string, Queue<byte[]>>();
        private readonly ConcurrentDictionary<string, byte[]> _latestInboundData = new ConcurrentDictionary<string, byte[]>();

        // Pre-allocated stream for binary packing to prevent GC spikes
        private readonly MemoryStream _sharedMemoryStream;
        private readonly BinaryWriter _sharedBinaryWriter;

        private SingularityDataBus()
        {
            _sharedMemoryStream = new MemoryStream(8192); // 8KB initial buffer
            _sharedBinaryWriter = new BinaryWriter(_sharedMemoryStream);
        }

        public void RegisterRoute(string targetId, IDataTransport transport)
        {
            _routes[targetId] = transport;
            if (!_outboundQueues.ContainsKey(targetId))
            {
                _outboundQueues[targetId] = new Queue<byte[]>();
            }

            transport.OnDataReceived += (data) =>
            {
                _latestInboundData[targetId] = data;
            };
        }

        // --- LOCAL PUB/SUB (Zero Allocation) ---
        public void Subscribe<T>(string channel, Action<T> handler)
        {
            if (_subscribers.TryGetValue(channel, out var existing))
                _subscribers[channel] = Delegate.Combine(existing, handler);
            else
                _subscribers[channel] = handler;
        }

        public void Unsubscribe<T>(string channel, Action<T> handler)
        {
            if (_subscribers.TryGetValue(channel, out var existing))
            {
                var current = Delegate.Remove(existing, handler);
                if (current == null) _subscribers.Remove(channel);
                else _subscribers[channel] = current;
            }
        }

        public void SendLocal<T>(string channel, T payload)
        {
            if (_subscribers.TryGetValue(channel, out var del) && del is Action<T> action)
            {
                action.Invoke(payload);
            }
        }

        // --- EXTERNAL BINARY TRANSMISSION ---
        public void TransmitRaw(string channel, byte[] payload)
        {
            if (_outboundQueues.TryGetValue(channel, out Queue<byte[]> queue))
            {
                queue.Enqueue(payload);
            }
        }

        public void TransmitPackable<T>(string channel, T payload) where T : IBinaryPackable
        {
            if (_outboundQueues.TryGetValue(channel, out Queue<byte[]> queue))
            {
                _sharedMemoryStream.SetLength(0); // Reset without reallocating
                payload.Pack(_sharedBinaryWriter);
                queue.Enqueue(_sharedMemoryStream.ToArray());
            }
        }

        public bool ReceiveRaw(string targetId, out byte[] latestData)
        {
            return _latestInboundData.TryRemove(targetId, out latestData);
        }

        /// <summary>
        /// Replaces LateUpdate(). Called deterministically by the FSM pipeline.
        /// </summary>
        public void FlushOutboundRoutes()
        {
            foreach (var kvp in _outboundQueues)
            {
                string targetId = kvp.Key;
                Queue<byte[]> queue = kvp.Value;

                if (queue.Count > 0 && _routes.TryGetValue(targetId, out IDataTransport transport))
                {
                    if (queue.Count == 1)
                    {
                        transport.Transmit(queue.Dequeue());
                        continue;
                    }

                    // Contiguous Binary Batching
                    using (MemoryStream batchStream = new MemoryStream())
                    using (BinaryWriter batchWriter = new BinaryWriter(batchStream))
                    {
                        while (queue.Count > 0)
                        {
                            byte[] chunk = queue.Dequeue();
                            batchWriter.Write(chunk.Length);
                            batchWriter.Write(chunk);
                        }
                        transport.Transmit(batchStream.ToArray());
                    }
                }
            }
        }
    }
}