using System;
using Workshop.UI_And_Tools.Forge.Core;

namespace Workshop.UI_And_Tools.Forge.IO
{
    /// <summary>
    /// The fundamental contract for bridging the Forge to external networks.
    /// Operates strictly on raw binary to eliminate allocation overhead.
    /// </summary>
    public interface IDataTransport
    {
        string TransportId { get; }

        /// <summary>
        /// Fires a contiguous block of memory into the void (HTTP, UDP, etc).
        /// Zero string encoding or JSON overhead allowed.
        /// </summary>
        void Transmit(byte[] payload);

        /// <summary>
        /// Fired when the void speaks back, yielding raw bytes to be routed by the DataBus.
        /// </summary>
        event Action<byte[]> OnDataReceived;
    }
}