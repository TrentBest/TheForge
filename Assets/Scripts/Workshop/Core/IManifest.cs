
namespace Workshop.Core.IO
{
    public interface IManifest
    {
        bool IsPublished { get; }
        bool AutoStart { get; }
        string EntryTrajectory { get; }
    }
}
