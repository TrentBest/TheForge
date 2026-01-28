using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using UnityEngine;

namespace Assets.Scripts
{
    public static class SingularityRegistry<T> where T : class, ISingularityDefinition, new()
    {
        private static string RootPath => Path.Combine(Application.streamingAssetsPath, "Singularity", typeof(T).Name + "s");
        private static Dictionary<string, T> _cache = new();

        public static void Save(T item)
        {
            try
            {
                // Each item gets its own file per your "Micro Package" requirement
                string directory = Path.Combine(RootPath, item.LibraryPath ?? "");
                Directory.CreateDirectory(directory);

                string filePath = Path.Combine(directory, $"{item.Name}.json");
                string json = JsonUtility.ToJson(item, true);
                File.WriteAllText(filePath, json);

                _cache[item.Name] = item;
            }
            catch (Exception e) { Debug.LogError($"[SingularityRegistry] Failed to save {item.Name}: {e.Message}"); }
        }

        public static T Load(string name)
        {
            if (_cache.TryGetValue(name, out var cached)) return cached;

            string[] files = Directory.GetFiles(RootPath, $"{name}.json", SearchOption.AllDirectories);
            if (files.Length > 0)
            {
                T item = JsonUtility.FromJson<T>(File.ReadAllText(files[0]));
                _cache[name] = item;
                return item;
            }
            return null;
        }

        public static List<T> GetAll()
        {
            if (!Directory.Exists(RootPath)) return new List<T>();

            return Directory.GetFiles(RootPath, "*.json", SearchOption.AllDirectories)
                .Select(path => JsonUtility.FromJson<T>(File.ReadAllText(path)))
                .ToList();
        }

        public static void Delete(string name)
        {
            string[] files = Directory.GetFiles(RootPath, $"{name}.json", SearchOption.AllDirectories);
            foreach (var file in files) File.Delete(file);
            _cache.Remove(name);
        }
    }

    // Every Micro-Package needs a Name and a Path to be tracked by the Registry
    public interface ISingularityDefinition
    {
        string Name { get; set; }
        string LibraryPath { get; set; }
    }
}

