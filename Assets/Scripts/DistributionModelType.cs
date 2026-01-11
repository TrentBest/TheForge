namespace Assets.Scripts
{
    public enum DistributionModelType
    {
        Homogeneous,        // uniform throughout the volume
        LayeredSurface,     // coating of thickness t on one or more faces
        DepthGradient,      // fraction varies along depth (e.g., from surface inward)
        RadialGradient,     // fraction varies with radius (e.g., wire/rod)
        ParticulateInclusions // dispersed particles in a matrix at a volume fraction
    }

}
