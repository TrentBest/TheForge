namespace TheSingularityWorkshop.Forge;

/// <summary>
/// Portable boundary for an externally authored Experience entering the Workshop ecosystem.
/// The Forge does not require the author to have used the Forge UI.
/// </summary>
public sealed record ForgeSubmission(
    ForgeExperience Experience,
    string SourceFormat,
    string SourceVersion)
{
    public void Validate()
    {
        ArgumentNullException.ThrowIfNull(Experience);

        if (string.IsNullOrWhiteSpace(SourceFormat))
            throw new ArgumentException("A source format is required.", nameof(SourceFormat));

        if (string.IsNullOrWhiteSpace(SourceVersion))
            throw new ArgumentException("A source version is required.", nameof(SourceVersion));
    }
}
