using System;

namespace Workshop.UI_And_Tools.Forge.IO
{
    [Serializable]
    public class SingularityMessage
    {
        public string SenderId;
        public string OpCode;
        public string Payload;

        public SingularityMessage(string opCode, string payloadJson, string senderId = "ForgeUI")
        {
            OpCode = opCode;
            Payload = payloadJson;
            SenderId = senderId;
        }
    }
}