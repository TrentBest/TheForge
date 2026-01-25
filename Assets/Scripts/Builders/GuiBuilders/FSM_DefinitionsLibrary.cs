using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;
using TheSingularityWorkshop.FSM_API;

namespace Assets.Scripts
{
    public static class FSM_DefinitionsLibrary
    {
        // --- Runtime Registries (Code) ---
        // These map "String Name" -> "Actual Function"
        private static Dictionary<string, Action<IStateContext>> _enterRegistry = new();
        private static Dictionary<string, Action<IStateContext>> _updateRegistry = new();
        private static Dictionary<string, Action<IStateContext>> _exitRegistry = new();
        private static Dictionary<string, > _stateRegistry = new();


        // --- Serialization Data (Manifest) ---
        // We only save the NAMES to disk.
        [Serializable]
        private class MethodRegistryManifest
        {
            public List<string> EnterMethodNames = new List<string>();
            public List<string> UpdateMethodNames = new List<string>();
            public List<string> ExitMethodNames = new List<string>();
        }

        private static string FilePath => Path.Combine(Application.streamingAssetsPath, "Singularity", "FsmMethodRegistry.json");

        // --- Public API ---

        // LOAD: Call this when the Editor opens
        public static void LoadRegistry()
        {
            if (!File.Exists(FilePath)) return;

            try
            {
                string json = File.ReadAllText(FilePath);
                var manifest = JsonUtility.FromJson<MethodRegistryManifest>(json);

                // Populate dictionaries with null actions (placeholders) if not already registered
                foreach (var name in manifest.EnterMethodNames) if (!_enterRegistry.ContainsKey(name)) _enterRegistry[name] = null;
                foreach (var name in manifest.UpdateMethodNames) if (!_updateRegistry.ContainsKey(name)) _updateRegistry[name] = null;
                foreach (var name in manifest.ExitMethodNames) if (!_exitRegistry.ContainsKey(name)) _exitRegistry[name] = null;
            }
            catch (Exception e) { Debug.LogError($"Failed to load FSM Registry: {e.Message}"); }
        }

        // SAVE: Call this whenever a user adds/removes a name in the UI
        public static void SaveRegistry()
        {
            var manifest = new MethodRegistryManifest
            {
                EnterMethodNames = _enterRegistry.Keys.ToList(),
                UpdateMethodNames = _updateRegistry.Keys.ToList(),
                ExitMethodNames = _exitRegistry.Keys.ToList()
            };

            try
            {
                string path = FilePath;
                Directory.CreateDirectory(Path.GetDirectoryName(path)); // Ensure folder exists
                string json = JsonUtility.ToJson(manifest, true);
                File.WriteAllText(path, json);
            }
            catch (Exception e) { Debug.LogError($"Failed to save FSM Registry: {e.Message}"); }
        }

        // --- CRUD Operations ---

        public static List<string> GetEnterMethods() => _enterRegistry.Keys.OrderBy(x => x).ToList();
        public static List<string> GetUpdateMethods() => _updateRegistry.Keys.OrderBy(x => x).ToList();
        public static List<string> GetExitMethods() => _exitRegistry.Keys.OrderBy(x => x).ToList();

        public static void AddEnterMethod(string name)
        {
            if (!_enterRegistry.ContainsKey(name)) { _enterRegistry[name] = null; SaveRegistry(); }
        }
        public static void RemoveEnterMethod(string name)
        {
            if (_enterRegistry.ContainsKey(name)) { _enterRegistry.Remove(name); SaveRegistry(); }
        }

        public static void AddUpdateMethod(string name)
        {
            if (!_updateRegistry.ContainsKey(name)) { _updateRegistry[name] = null; SaveRegistry(); }
        }
        public static void RemoveUpdateMethod(string name)
        {
            if (_updateRegistry.ContainsKey(name)) { _updateRegistry.Remove(name); SaveRegistry(); }
        }

        public static void AddExitMethod(string name)
        {
            if (!_exitRegistry.ContainsKey(name)) { _exitRegistry[name] = null; SaveRegistry(); }
        }
        public static void RemoveExitMethod(string name)
        {
            if (_exitRegistry.ContainsKey(name)) { _exitRegistry.Remove(name); SaveRegistry(); }
        }

        // Runtime Binding (Call this from your Game Scripts)
        public static void BindEnterAction(string name, Action<IStateContext> action) => _enterRegistry[name] = action;
        public static void BindUpdateAction(string name, Action<IStateContext> action) => _updateRegistry[name] = action;
        public static void BindExitAction(string name, Action<IStateContext> action) => _exitRegistry[name] = action;

        public static StateDefinition GetStates()
        {
            throw new NotImplementedException();
        }

        public static void AddState(StateDefinition selectedState)
        {
            throw new NotImplementedException();
        }
    }
}