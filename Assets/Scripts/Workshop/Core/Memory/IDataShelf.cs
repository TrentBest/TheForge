using System;
using System.IO;

namespace Workshop.Core.Memory
{
    public interface IDataShelf : System.IDisposable
    {
        int PageSize { get; }

        // Sovereign Serialization methods for Domain Reload
        void Export(BinaryWriter writer);
        void Import(BinaryReader reader);
    }
}