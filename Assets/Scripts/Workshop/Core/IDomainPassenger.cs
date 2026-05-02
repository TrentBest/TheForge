// File: Assets/Scripts/Workshop/Core/IDomainPassenger.cs
namespace Workshop.Core
{
    /// <summary>
    /// A contract for sovereign systems that must survive the Assembly Reload.
    /// </summary>
    public interface IDomainPassenger
    {
        // Hop aboard: Called just before the assemblies are wiped.
        void OnSuspend();

        // Disembark: Called after the new assemblies are live.
        void OnResume();
    }
}