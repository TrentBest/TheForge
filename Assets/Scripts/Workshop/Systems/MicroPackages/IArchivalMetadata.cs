using UnityEngine;

namespace Workshop.Systems.MicroPackages
{
    /// <summary>
    /// Defines a MicroPackage as a preserved historical artifact and functional tool.
    /// Allows the Singularity Engine to query, filter, and route this package 
    /// before the heavy payload is physically downloaded to the Gondola Yard.
    /// </summary>
    public interface IArchivalMetadata
    {
        // --- MODERN ROUTING & STOREFRONT ---
        string PackageId { get; }
        string Version { get; }
        string Author { get; }
        ForgeSpatialZone SpatialZone { get; }
        string ShortDescription { get; }
        string LongDescription { get; }
        string ThumbnailUrl { get; }

        // --- HISTORICAL PRESERVATION ---
        int ReleaseYear { get; }
        string OriginalAuthor { get; }
        string OriginalPublisher { get; }
        string OriginalPlatform { get; }
        string Era { get; }
        string HistoricalSignificance { get; }
        Color AccentColor { get; }
    }
}