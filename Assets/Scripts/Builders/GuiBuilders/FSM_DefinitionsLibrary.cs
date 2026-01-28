using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;
using TheSingularityWorkshop.FSM_API;

namespace Assets.Scripts
{
    [Serializable]
    public class StateDefinition
    {
        public string Name;
        public string EnterMethodName;
        public string UpdateMethodName;
        public string ExitMethodName;
        public string LibraryPath; // e.g., "Combat/Aggressive"
    }

    [Serializable]
    public class FsmDefinition
    {
        public string Name;
        public List<string> StateNames = new List<string>();
        public string LibraryPath; // e.g., "Units/Soldier"
    }

    public static class FSM_DefinitionsLibrary
    {
        // --- Runtime Registries (Code-to-Action mapping) ---
        private static Dictionary<string, Action<IStateContext>> _enterRegistry = new();
        private static Dictionary<string, Action<IStateContext>> _updateRegistry = new();
        private static Dictionary<string, Action<IStateContext>> _exitRegistry = new();
        private static Dictionary<string, Func<IStateContext, bool>> _conditionRegistry = new();

        // --- Serialization Data (Manifest) ---
        [Serializable]
        private class MethodRegistryManifest
        {
            public List<string> EnterMethodNames = new List<string>();
            public List<string> UpdateMethodNames = new List<string>();
            public List<string> ExitMethodNames = new List<string>();
            public List<string> ConditionMethodNames = new List<string>();
            public List<string> RegisteredStateNames = new List<string>();
            public List<string> RegisteredFsmNames = new List<string>();
            public List<string> MethodNames = new List<string>();
            public List<string> DllPaths = new List<string>();
        }

        private static string RootPath => Path.Combine(Application.streamingAssetsPath, "Singularity");
        private static string ManifestPath => Path.Combine(RootPath, "FsmMethodRegistry.json");
        private static string StatesFolder => Path.Combine(RootPath, "States");
        private static string FsmsFolder => Path.Combine(RootPath, "FSMs");
        private static Dictionary<string, string> _methodDllPaths = new();

        // --- Public API ---
        public static string GetDllPath(string methodName) =>
    _methodDllPaths.TryGetValue(methodName, out var path) ? path : string.Empty;

        public static void SetDllPath(string methodName, string path)
        {
            _methodDllPaths[methodName] = path;
            SaveRegistry();
        }
        public static void LoadRegistry()
        {
            if (!File.Exists(ManifestPath)) return;

            try
            {
                string json = File.ReadAllText(ManifestPath);
                var manifest = JsonUtility.FromJson<MethodRegistryManifest>(json);

                // Initialize dictionaries with null placeholders
                foreach (var name in manifest.EnterMethodNames) if (!_enterRegistry.ContainsKey(name)) _enterRegistry[name] = null;
                foreach (var name in manifest.UpdateMethodNames) if (!_updateRegistry.ContainsKey(name)) _updateRegistry[name] = null;
                foreach (var name in manifest.ExitMethodNames) if (!_exitRegistry.ContainsKey(name)) _exitRegistry[name] = null;
                foreach (var name in manifest.ConditionMethodNames) if (!_conditionRegistry.ContainsKey(name)) _conditionRegistry[name] = null;
            }
            catch (Exception e) { Debug.LogError($"Failed to load FSM Registry: {e.Message}"); }
        }

        public static void SaveRegistry()
        {
            var manifest = new MethodRegistryManifest
            {
                EnterMethodNames = _enterRegistry.Keys.ToList(),
                UpdateMethodNames = _updateRegistry.Keys.ToList(),
                ExitMethodNames = _exitRegistry.Keys.ToList(),
                ConditionMethodNames = _conditionRegistry.Keys.ToList(),
                // Cache the registered file names for quick UI lookup
                RegisteredStateNames = GetRegisteredStateNames(),
                RegisteredFsmNames = GetRegisteredFsmNames()
            };

            try
            {
                Directory.CreateDirectory(RootPath);
                string json = JsonUtility.ToJson(manifest, true);
                File.WriteAllText(ManifestPath, json);
            }
            catch (Exception e) { Debug.LogError($"Failed to save FSM Registry: {e.Message}"); }
        }

        // --- State Serialization ---

        public static void SaveState(StateDefinition state)
        {
            try
            {
                string directory = Path.Combine(StatesFolder, state.LibraryPath ?? "");
                Directory.CreateDirectory(directory);

                string filePath = Path.Combine(directory, $"{state.Name}.json");
                File.WriteAllText(filePath, JsonUtility.ToJson(state, true));

                SaveRegistry();
            }
            catch (Exception e) { Debug.LogError($"Failed to save State {state.Name}: {e.Message}"); }
        }

        public static StateDefinition LoadState(string stateName)
        {
            if (!Directory.Exists(StatesFolder)) return null;

            string[] files = Directory.GetFiles(StatesFolder, $"{stateName}.json", SearchOption.AllDirectories);
            if (files.Length > 0)
            {
                return JsonUtility.FromJson<StateDefinition>(File.ReadAllText(files[0]));
            }
            return null;
        }

        public static List<string> GetRegisteredStateNames()
        {
            if (!Directory.Exists(StatesFolder)) return new List<string>();
            return Directory.GetFiles(StatesFolder, "*.json", SearchOption.AllDirectories)
                            .Select(Path.GetFileNameWithoutExtension)
                            .OrderBy(x => x)
                            .ToList();
        }

        // --- FSM Serialization ---

        public static void SaveFsm(FsmDefinition fsm)
        {
            try
            {
                string directory = Path.Combine(FsmsFolder, fsm.LibraryPath ?? "");
                Directory.CreateDirectory(directory);

                string filePath = Path.Combine(directory, $"{fsm.Name}.json");
                File.WriteAllText(filePath, JsonUtility.ToJson(fsm, true));

                SaveRegistry();
            }
            catch (Exception e) { Debug.LogError($"Failed to save FSM {fsm.Name}: {e.Message}"); }
        }

        public static FsmDefinition LoadFsm(string fsmName)
        {
            if (!Directory.Exists(FsmsFolder)) return null;

            string[] files = Directory.GetFiles(FsmsFolder, $"{fsmName}.json", SearchOption.AllDirectories);
            if (files.Length > 0)
            {
                return JsonUtility.FromJson<FsmDefinition>(File.ReadAllText(files[0]));
            }
            return null;
        }

        public static List<string> GetRegisteredFsmNames()
        {
            if (!Directory.Exists(FsmsFolder)) return new List<string>();
            return Directory.GetFiles(FsmsFolder, "*.json", SearchOption.AllDirectories)
                            .Select(Path.GetFileNameWithoutExtension)
                            .OrderBy(x => x)
                            .ToList();
        }

        // --- CRUD for Registries ---

        public static List<string> GetEnterMethods() => _enterRegistry.Keys.OrderBy(x => x).ToList();
        public static List<string> GetUpdateMethods() => _updateRegistry.Keys.OrderBy(x => x).ToList();
        public static List<string> GetExitMethods() => _exitRegistry.Keys.OrderBy(x => x).ToList();
        public static List<string> GetConditionMethods() => _conditionRegistry.Keys.OrderBy(x => x).ToList();

        public static void AddEnterMethod(string name) { if (!_enterRegistry.ContainsKey(name)) { _enterRegistry[name] = null; SaveRegistry(); } }
        public static void AddUpdateMethod(string name) { if (!_updateRegistry.ContainsKey(name)) { _updateRegistry[name] = null; SaveRegistry(); } }
        public static void AddExitMethod(string name) { if (!_exitRegistry.ContainsKey(name)) { _exitRegistry[name] = null; SaveRegistry(); } }
        public static void AddConditionMethod(string name) { if (!_conditionRegistry.ContainsKey(name)) { _conditionRegistry[name] = null; SaveRegistry(); } }

        public static void RemoveEnterMethod(string name) { if (_enterRegistry.Remove(name)) SaveRegistry(); }
        public static void RemoveUpdateMethod(string name) { if (_updateRegistry.Remove(name)) SaveRegistry(); }
        public static void RemoveExitMethod(string name) { if (_exitRegistry.Remove(name)) SaveRegistry(); }
        public static void RemoveConditionMethod(string name) { if (_conditionRegistry.Remove(name)) SaveRegistry(); }

        // --- Runtime Binding ---

        public static void BindEnterAction(string name, Action<IStateContext> action) => _enterRegistry[name] = action;
        public static void BindUpdateAction(string name, Action<IStateContext> action) => _updateRegistry[name] = action;
        public static void BindExitAction(string name, Action<IStateContext> action) => _exitRegistry[name] = action;
        public static void BindCondition(string name, Func<IStateContext, bool> func) => _conditionRegistry[name] = func;
    }
}