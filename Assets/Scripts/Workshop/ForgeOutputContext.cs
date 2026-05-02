using System;

namespace Workshop
{
    public class ForgeOutputContext
    {
        // The event the Agent's Brain subscribes to
        public Action<string> OnMessageReceived { get; set; }

        /// <summary>
        /// Called by the SingularityDataBus or NetworkTransport when reality updates.
        /// </summary>
        public void ReceiveMessage(string message)
        {
            OnMessageReceived?.Invoke(message);
        }
    }
}