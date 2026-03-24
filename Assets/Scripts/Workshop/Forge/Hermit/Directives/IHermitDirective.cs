// File: Assets/Scripts/Workshop/Forge/Hermit/Directives/IHermitDirective.cs
using Assets.Scripts.Workshop.Forge.Hermit.Core;
using System.Collections;
using TheSingularityWorkshop.Forge.Hermit.Core;


namespace TheSingularityWorkshop.Forge.Hermit.Directives
{
    /// <summary>
    /// The fundamental contract for any action Hermit can perform in the Forge.
    /// </summary>
    public interface IHermitDirective
    {
        /// <summary>
        /// The JSON string key that triggers this directive (e.g., "MoveTo", "Spawn", "SetTime").
        /// </summary>
        string ActionType { get; }

        /// <summary>
        /// The execution logic. Returns an IEnumerator so Hermit can perform actions over time.
        /// </summary>
        IEnumerator Execute(HermitAction payload, HermitChassis body);
    }
}