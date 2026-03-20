using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

/// <summary>
/// Fluent API for constructing Experience manifests programmatically (useful for editor GUI).
/// Produces the same JSON schema that ExperienceBuilder_OLD expects.
/// </summary>
public class ExperienceGuiBuilder
{
    private ExperienceManifest _manifest = new ExperienceManifest
    {
        senses = new List<SenseManifestItem>(),
        description = string.Empty,
        id = Guid.NewGuid().ToString(),
        name = "New Experience"
    };

    public ExperienceGuiBuilder WithId(string id)
    {
        if (!string.IsNullOrWhiteSpace(id))
            _manifest.id = id;
        return this;
    }

    public ExperienceGuiBuilder WithName(string name)
    {
        if (!string.IsNullOrWhiteSpace(name))
            _manifest.name = name;
        return this;
    }

    public ExperienceGuiBuilder WithDescription(string description)
    {
        _manifest.description = description ?? string.Empty;
        return this;
    }

    public ExperienceGuiBuilder AddSense(Sense sense, string provider = null, Dictionary<string, string> config = null)
    {
        var item = new SenseManifestItem
        {
            type = sense.ToString(),
            provider = provider ?? string.Empty,
            config = new List<ConfigKV>()
        };

        if (config != null)
        {
            foreach (var kv in config)
            {
                item.config.Add(new ConfigKV { key = kv.Key, value = kv.Value });
            }
        }

        _manifest.senses.Add(item);
        return this;
    }

    // Add a custom sense by string name
    public ExperienceGuiBuilder AddCustomSense(string typeName, string provider = null, Dictionary<string, string> config = null)
    {
        var item = new SenseManifestItem
        {
            type = typeName ?? "Custom",
            provider = provider ?? string.Empty,
            config = new List<ConfigKV>()
        };

        if (config != null)
        {
            foreach (var kv in config)
            {
                item.config.Add(new ConfigKV { key = kv.Key, value = kv.Value });
            }
        }

        _manifest.senses.Add(item);
        return this;
    }

    // Produce JSON that can be saved to disk or used by ExperienceBuilder_OLD
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
            Debug.Log($"ExperienceGuiBuilder: Saved manifest to {path}");
        }
        catch (Exception ex)
        {
            Debug.LogError($"ExperienceGuiBuilder: failed to save manifest: {ex}");
        }
    }
}
