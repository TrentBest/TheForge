using System;
using System.Collections.Generic;
using UnityEngine;

namespace Workshop
{
    [Serializable]
    public class ExperienceManifest
    {
        // --- BASIC IDENTITY ---
        public string ExperienceId; // Renamed to match Arbitrator
        public string Name;
        public string Version;
        public string Description;

        // --- THE SENSE ENGINE ---
        public List<SenseManifestItem> Senses = new List<SenseManifestItem>();
        public string TargetDomainSkin;
        public string EntryPointContext;

        // --- THE METADEV ENGINE ---
        public List<string> PackageBootOrder = new List<string>();
        public List<string> ConfigKeys = new List<string>();
        public List<string> ConfigValues = new List<string>();
        public Dictionary<string, List<string>> PackageConfigurationDeltas = new();

        // FIX: Parameterless constructor for object initializers and JSON
        public ExperienceManifest()
        {
            Version = Guid.NewGuid().ToString().Substring(0, 8);
        }

        public ExperienceManifest(string id) : this()
        {
            ExperienceId = id;
        }
    }

    [Serializable]
    public class SenseManifestItem
    {
        public string Type;
        public string Provider;
        public List<ConfigKV> Config;
    }

    [Serializable]
    public class ConfigKV { public string Key; public string Value; }
}