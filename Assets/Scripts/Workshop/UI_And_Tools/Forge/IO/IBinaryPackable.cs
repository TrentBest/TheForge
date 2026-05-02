using System.IO;

namespace Workshop.UI_And_Tools.Forge.IO
{
    /// <summary>
    /// Contract for objects that can serialize their state directly into a binary stream,
    /// bypassing JSON and string allocation entirely.
    /// </summary>
    public interface IBinaryPackable
    {
        void Pack(BinaryWriter writer);
    }
}