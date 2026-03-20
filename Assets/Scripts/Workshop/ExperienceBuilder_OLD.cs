using System;
using System.IO;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
internal class ExperienceManifest
{
    public string id;
    public string name;
    public string description;
    public List<SenseManifestItem> senses;
}

[Serializable]
internal class SenseManifestItem
{
    public string type;        // e.g. "Vision", "Audio", "Touch", "Custom01"
    public string provider;    // e.g. "DefaultVision", "SpatialAudio", "HapticsV1"
    public List<ConfigKV> config;
}

[Serializable]
internal class ConfigKV
{
    public string key;
    public string value;
}

public class SenseDescriptor
{
    public Sense Sense;
    public string Provider;
    public Dictionary<string, string> Config = new Dictionary<string, string>();
}

public class Experience
{
    public string Id;
    public string Name;
    public string Description;
    public List<SenseDescriptor> Senses = new List<SenseDescriptor>();
}

public class ExperienceBuilder_OLD
{
    private string CacheDir => Path.Combine(Application.persistentDataPath, "ExperienceCache");

    public ExperienceBuilder_OLD()
    {
        try
        {
            if (!Directory.Exists(CacheDir))
                Directory.CreateDirectory(CacheDir);
        }
        catch (Exception ex)
        {
            Debug.LogWarning($"ExperienceBuilder_OLD: failed to ensure cache directory '{CacheDir}': {ex}");
        }
    }

    public Experience LoadFromStreamingAssets(string manifestFileName)
    {
        var path = Path.Combine(Application.streamingAssetsPath, "Experiences", manifestFileName + ".json");
        if (!File.Exists(path))
            throw new FileNotFoundException($"Manifest not found at '{path}'");

        var json = File.ReadAllText(path);
        var manifest = JsonUtility.FromJson<ExperienceManifest>(json);
        if (manifest == null)
            throw new InvalidDataException($"Unable to parse manifest at '{path}'");

        SaveToCache(manifest);
        return ToExperience(manifest);
    }

    public Experience LoadFromCache(string id)
    {
        var path = Path.Combine(CacheDir, id + ".json");
        if (!File.Exists(path))
            return null;

        var json = File.ReadAllText(path);
        var manifest = JsonUtility.FromJson<ExperienceManifest>(json);
        if (manifest == null)
            return null;

        return ToExperience(manifest);
    }

    private void SaveToCache(ExperienceManifest manifest)
    {
        if (manifest == null || string.IsNullOrWhiteSpace(manifest.id))
            return;

        try
        {
            var path = Path.Combine(CacheDir, manifest.id + ".json");
            File.WriteAllText(path, JsonUtility.ToJson(manifest));
        }
        catch (Exception ex)
        {
            Debug.LogWarning($"ExperienceBuilder_OLD: failed to write cache for '{manifest.id}': {ex}");
        }
    }

    private Experience ToExperience(ExperienceManifest m)
    {
        var e = new Experience
        {
            Id = m.id,
            Name = m.name,
            Description = m.description
        };

        if (m.senses != null)
        {
            foreach (var item in m.senses)
            {
                if (item == null || string.IsNullOrWhiteSpace(item.type))
                    continue;

                var desc = new SenseDescriptor
                {
                    Provider = item.provider ?? string.Empty
                };

                if (Enum.TryParse<Sense>(item.type, true, out var parsed))
                    desc.Sense = parsed;
                else
                    desc.Sense = Sense.Custom;

                if (item.config != null)
                {
                    foreach (var kv in item.config)
                    {
                        if (kv == null || string.IsNullOrWhiteSpace(kv.key))
                            continue;
                        // last-one-wins
                        desc.Config[kv.key] = kv.value ?? string.Empty;
                    }
                }

                e.Senses.Add(desc);
            }
        }

        return e;
    }

    public Experience Load(string manifestName = null)
    {
        try
        {
            var fromExec = TryLoadFromExecutableDirectory(manifestName);
            if (fromExec != null)
            {
                Debug.Log($"ExperienceBuilder_OLD: Loaded manifest from executable directory (manifestName='{manifestName ?? "<auto>"}').");
                return fromExec;
            }
        }
        catch (Exception ex)
        {
            Debug.LogWarning($"ExperienceBuilder_OLD: error while trying to load from executable directory: {ex}");
        }

        if (!string.IsNullOrWhiteSpace(manifestName))
        {
            try
            {
                return LoadFromStreamingAssets(manifestName);
            }
            catch (FileNotFoundException)
            {
            }

            var cached = LoadFromCache(manifestName);
            if (cached != null)
                return cached;
        }

        throw new FileNotFoundException($"Manifest '{manifestName ?? "<auto>"}' not found adjacent to executable, in StreamingAssets, nor cache.");
    }

    private Experience TryLoadFromExecutableDirectory(string manifestFileName = null)
    {
        string execDir = Path.GetDirectoryName(Application.dataPath) ?? Application.dataPath;

        if (string.IsNullOrWhiteSpace(execDir) || !Directory.Exists(execDir))
            return null;

        if (!string.IsNullOrWhiteSpace(manifestFileName))
        {
            var candidate = Path.Combine(execDir, manifestFileName + ".json");
            if (File.Exists(candidate))
            {
                var e = TryLoadManifestFile(candidate);
                if (e != null)
                    return e;
            }

            candidate = Path.Combine(execDir, manifestFileName);
            if (File.Exists(candidate))
            {
                var e = TryLoadManifestFile(candidate);
                if (e != null)
                    return e;
            }
        }

        try
        {
            var files = Directory.GetFiles(execDir, "*.json");
            foreach (var f in files)
            {
                var e = TryLoadManifestFile(f);
                if (e != null)
                    return e;
            }
        }
        catch (Exception ex)
        {
            Debug.LogWarning($"ExperienceBuilder_OLD: error scanning executable directory '{execDir}': {ex}");
        }

        return null;
    }

    private Experience TryLoadManifestFile(string path)
    {
        try
        {
            var json = File.ReadAllText(path);
            var manifest = JsonUtility.FromJson<ExperienceManifest>(json);
            if (manifest == null)
                return null;
            if (string.IsNullOrWhiteSpace(manifest.id))
                return null;

            SaveToCache(manifest);
            Debug.Log($"ExperienceBuilder_OLD: Discovered manifest '{manifest.id}' at '{path}'.");
            return ToExperience(manifest);
        }
        catch (Exception ex)
        {
            Debug.LogWarning($"ExperienceBuilder_OLD: failed to parse manifest file '{path}': {ex}");
            return null;
        }
    }
}



public class ExperienceBuilder
{

    public ExperienceBuilder WithSense(SenseDescriptor sense)
    {

        return this;
    }

    public ExperienceBuilder WithSenses(List<SenseDescriptor> senses)
    {

        return this;
    }

    public ExperienceBuilder WithScale(ExperienceScale scale)
    {

        return this;
    }
}

public enum ExperienceScale
{
    Reality = 0,
    Atom = -100,
    Microscopic = - 50,

    Universe = 50,
    Multiverse = 100
}

/*public enum ExperienceScale

{

    Reality = 0,

    Atom = -100,

    Microscopic = - 50,



    Universe = 50,

    Multiverse = 100

}



I want varying scales, but I want to make sure I'm understanding what I'm actually doing.. the purpose behind this enum is to allow a user to inform me how much they need.  Obviously I'll need to use a massive amount of macro magic to support a multiverse, whereas an inner space like situation doesn't require that full amount.



We are trying to establish the draft workflow so we can see how it goes, but our living gui would basically ask the user various questions to help us understand what they do and don't need.  Example question would be to ask there will be multiplayer features.  Player doesn't just say yes, but the yes turns into the gui to ask the user as much as they want to answer, with the ability to come back later and make changes.  We are trying to recognize the Experience as the ideal descriptor of data and situational story, a book is just a certain camera system looking into the experience, a movie is a completely different one, a game.. etc. we aren't forging the game with this questionnaire, rather we are establishing what tools and systems we should provide the user to be able to define whatever they are creating.  <= we fully allow stuff to be brought into a builder*/