using System;
using System.Collections.Generic;
using System.IO;
using Unity.VisualScripting;
using UnityEngine;
using static UnityEngine.Rendering.STP;

namespace Workshop
{
    /// <summary>
    /// Fluent API for constructing Experience manifests programmatically (non-diegetic authoring tool).
    /// Produces the JSON schema required for booting environments via the SingularityDataBus.
    /// </summary>
    public class ExperienceGuiBuilder
    {
        private ExperienceManifest _manifest = new ExperienceManifest
        {
            Senses = new List<SenseManifestItem>(),
            Description = string.Empty,
            ExperienceId = Guid.NewGuid().ToString(),
            Name = "New Experience"
        };

        public ExperienceGuiBuilder WithId(string id)
        {
            if (!string.IsNullOrWhiteSpace(id))
                _manifest.ExperienceId = id;
            return this;
        }

        public ExperienceGuiBuilder WithName(string name)
        {
            if (!string.IsNullOrWhiteSpace(name))
                _manifest.Name = name;
            return this;
        }

        public ExperienceGuiBuilder WithDescription(string description)
        {
            _manifest.Description = description ?? string.Empty;
            return this;
        }

        public ExperienceGuiBuilder AddSense(Sense sense, string provider = null, Dictionary<string, string> config = null)
        {
            var item = new SenseManifestItem
            {
                // Using the Smart Class 'Id' property we built earlier
                Type = sense.Id,
                Provider = provider ?? string.Empty,
                Config = new List<ConfigKV>()
            };

            if (config != null)
            {
                foreach (var kv in config)
                {
                    item.Config.Add(new ConfigKV { Key = kv.Key, Value = kv.Value });
                }
            }

            _manifest.Senses.Add(item);
            return this;
        }

        // Add a custom sense by string name
        public ExperienceGuiBuilder AddCustomSense(string typeName, string provider = null, Dictionary<string, string> config = null)
        {
            var item = new SenseManifestItem
            {
                Type = typeName ?? "Custom",
                Provider = provider ?? string.Empty,
                Config = new List<ConfigKV>()
            };

            if (config != null)
            {
                foreach (var kv in config)
                {
                    item.Config.Add(new ConfigKV { Key = kv.Key, Value = kv.Value });
                }
            }

            _manifest.Senses.Add(item);
            return this;
        }

        // Produce JSON that can be saved to disk or pushed to the NetworkBus
        public string BuildJson()
        {
            return JsonUtility.ToJson(_manifest, true);
        }

        // Save to a file (StreamingAssets/Experiences recommended for authoring; must exist)
        public void SaveToStreamingAssets(string filenameWithoutExtension)
        {
            var dir = Path.Combine(Application.streamingAssetsPath, "Experiences");
            try
            {
                if (!Directory.Exists(dir))
                    Directory.CreateDirectory(dir);

                var path = Path.Combine(dir, filenameWithoutExtension + ".json");
                File.WriteAllText(path, BuildJson());
                Debug.Log($"[ExperienceGuiBuilder] Saved manifest to {path}");
            }
            catch (Exception ex)
            {
                Debug.LogError($"[ExperienceGuiBuilder] Failed to save manifest: {ex.Message}");
            }
        }
    }
}