using System.Collections.Generic;

public static class ExperienceInference
{
    public struct Requirement
    {
        public Sense Sense;
        public string DefaultProvider;
        public string DefaultConfig;
    }

    public static List<Requirement> InferRequirements(ExperienceScope scope)
    {
        var list = new List<Requirement>();

        // Base Requirements
        list.Add(new Requirement { Sense = Sense.Vision, DefaultProvider = "DefaultCamera", DefaultConfig = "FOV=60" });

        switch (scope)
        {
            case ExperienceScope.Galactic_3D:
                list.Add(new Requirement { Sense = Sense.Custom01, DefaultProvider = "WFC_Universe_Gen", DefaultConfig = "Seed=Random;Expand=True" });
                list.Add(new Requirement { Sense = Sense.Custom02, DefaultProvider = "GURPS_Physics", DefaultConfig = "Strict=True" });
                list.Add(new Requirement { Sense = Sense.Audio, DefaultProvider = "SpaceAmbience", DefaultConfig = "Vol=0.5" });
                break;

            case ExperienceScope.Planetary:
                list.Add(new Requirement { Sense = Sense.Custom01, DefaultProvider = "TerrainEngine", DefaultConfig = "LOD=4" });
                break;

            case ExperienceScope.Quantum:
                list.Add(new Requirement { Sense = Sense.Custom01, DefaultProvider = "AtomBuilder", DefaultConfig = "" });
                break;
        }
        return list;
    }
}