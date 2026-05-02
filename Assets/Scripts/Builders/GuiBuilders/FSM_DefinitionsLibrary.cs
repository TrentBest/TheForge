using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;
using TheSingularityWorkshop.FSM_API;

namespace Assets.Scripts.Builders.GuiBuilders
{
    // --- 1. Data Definitions ---

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
        public string Name = "NewFSM";
        public string Group = "Update";
        public int Rate = 1;
        public string InitialState = "";
        public string LibraryPath; // e.g., "Units/Soldier"

        // References to States by Name (Non-Monolithic)
        public List<string> StateNames = new List<string>();

        // Transitions belong to the FSM structure
        public List<FsmTransitionDefinition> Transitions = new List<FsmTransitionDefinition>();
    }

    [Serializable]
    public class FsmTransitionDefinition
    {
        public string FromState;
        public string ToState;
        public string ConditionMethod; // e.g., "IsEnemyVisible"
    }

    // --- 2. The Library (Persistence & Registry) ---

    public static class FSM_DefinitionsLibrary
    {
        // --- Runtime Registries (Code-to-Action mapping) ---
        private static Dictionary<string, Action<IStateContext>> _enterRegistry = new();
        private static Dictionary<string, Action<IStateContext>> _updateRegistry = new();
        private static Dictionary<string, Action<IStateContext>> _exitRegistry = new();
        private static Dictionary<string, Func<IStateContext, bool>> _conditionRegistry = new();
        private static Dictionary<string, string> _methodDllPaths = new();

        // --- Paths ---
        private static string RootPath => Path.Combine(Application.streamingAssetsPath, "Singularity");
        private static string ManifestPath => Path.Combine(RootPath, "FsmMethodRegistry.json");
        private static string StatesFolder => Path.Combine(RootPath, "States");
        private static string FsmsFolder => Path.Combine(RootPath, "FSMs");

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
            public List<string> StateContexts = new List<string>(); 
        }

        // --- Public Registry API ---

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

                // Initialize dictionaries
                foreach (var name in manifest.EnterMethodNames) if (!_enterRegistry.ContainsKey(name)) _enterRegistry[name] = null;
                foreach (var name in manifest.UpdateMethodNames) if (!_updateRegistry.ContainsKey(name)) _updateRegistry[name] = null;
                foreach (var name in manifest.ExitMethodNames) if (!_exitRegistry.ContainsKey(name)) _exitRegistry[name] = null;
                foreach (var name in manifest.ConditionMethodNames) if (!_conditionRegistry.ContainsKey(name)) _conditionRegistry[name] = null;

                // Restore DLL paths (assuming manifest stores them in parallel lists or we rebuild them)
                // For simplicity in this merge, assuming _methodDllPaths are populated via logic or separate file if not fully in manifest above.
                // If manifest.MethodNames and DllPaths are parallel:
                if (manifest.MethodNames != null && manifest.DllPaths != null)
                {
                    for (int i = 0; i < Mathf.Min(manifest.MethodNames.Count, manifest.DllPaths.Count); i++)
                    {
                        _methodDllPaths[manifest.MethodNames[i]] = manifest.DllPaths[i];
                    }
                }
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
                RegisteredStateNames = GetRegisteredStateNames(),
                RegisteredFsmNames = GetRegisteredFsmNames(),
                MethodNames = _methodDllPaths.Keys.ToList(),
                DllPaths = _methodDllPaths.Values.ToList()
            };

            try
            {
                if (!Directory.Exists(RootPath)) Directory.CreateDirectory(RootPath);
                string json = JsonUtility.ToJson(manifest, true);
                File.WriteAllText(ManifestPath, json);
            }
            catch (Exception e) { Debug.LogError($"Failed to save FSM Registry: {e.Message}"); }
        }

        // --- FSM CRUD ---

        public static void SaveFsm(FsmDefinition fsm)
        {
            try
            {
                string directory = Path.Combine(FsmsFolder, fsm.LibraryPath ?? "");
                if (!Directory.Exists(directory)) Directory.CreateDirectory(directory);

                string filePath = Path.Combine(directory, $"{fsm.Name}.json");
                File.WriteAllText(filePath, JsonUtility.ToJson(fsm, true));

                SaveRegistry();
            }
            catch (Exception e) { Debug.LogError($"Failed to save FSM {fsm.Name}: {e.Message}"); }
        }

        public static FsmDefinition LoadFsm(string fsmName)
        {
            if (!Directory.Exists(FsmsFolder)) return null;

            // Recursive search to find the file even if moved to subfolders
            string[] files = Directory.GetFiles(FsmsFolder, $"{fsmName}.json", SearchOption.AllDirectories);
            if (files.Length > 0)
            {
                return JsonUtility.FromJson<FsmDefinition>(File.ReadAllText(files[0]));
            }
            return null;
        }

        public static void DeleteFsm(string fsmName)
        {
            if (!Directory.Exists(FsmsFolder)) return;

            string[] files = Directory.GetFiles(FsmsFolder, $"{fsmName}.json", SearchOption.AllDirectories);
            if (files.Length > 0)
            {
                File.Delete(files[0]);
                if (File.Exists(files[0] + ".meta")) File.Delete(files[0] + ".meta");
                SaveRegistry();
            }
        }

        public static List<string> GetRegisteredFsmNames()
        {
            if (!Directory.Exists(FsmsFolder)) return new List<string>();
            return Directory.GetFiles(FsmsFolder, "*.json", SearchOption.AllDirectories)
                            .Select(Path.GetFileNameWithoutExtension)
                            .OrderBy(x => x)
                            .ToList();
        }

        // --- State CRUD ---

        public static void SaveState(StateDefinition state)
        {
            try
            {
                string directory = Path.Combine(StatesFolder, state.LibraryPath ?? "");
                if (!Directory.Exists(directory)) Directory.CreateDirectory(directory);

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

        public static void DeleteState(string stateName)
        {
            if (!Directory.Exists(StatesFolder)) return;

            string[] files = Directory.GetFiles(StatesFolder, $"{stateName}.json", SearchOption.AllDirectories);
            if (files.Length > 0)
            {
                File.Delete(files[0]);
                if (File.Exists(files[0] + ".meta")) File.Delete(files[0] + ".meta");
                SaveRegistry();
            }
        }

        public static List<string> GetRegisteredStateNames()
        {
            if (!Directory.Exists(StatesFolder)) return new List<string>();
            return Directory.GetFiles(StatesFolder, "*.json", SearchOption.AllDirectories)
                            .Select(Path.GetFileNameWithoutExtension)
                            .OrderBy(x => x)
                            .ToList();
        }

        // --- Helper: Get Available Lists for UI ---

        public static List<string> GetEnterMethods() => _enterRegistry.Keys.OrderBy(x => x).ToList();
        public static List<string> GetUpdateMethods() => _updateRegistry.Keys.OrderBy(x => x).ToList();
        public static List<string> GetExitMethods() => _exitRegistry.Keys.OrderBy(x => x).ToList();
        public static List<string> GetConditionMethods() => _conditionRegistry.Keys.OrderBy(x => x).ToList();

        // --- Runtime Binding ---

        public static void AddEnterMethod(string name) { if (!_enterRegistry.ContainsKey(name)) { _enterRegistry[name] = null; SaveRegistry(); } }
        public static void AddUpdateMethod(string name) { if (!_updateRegistry.ContainsKey(name)) { _updateRegistry[name] = null; SaveRegistry(); } }
        public static void AddExitMethod(string name) { if (!_exitRegistry.ContainsKey(name)) { _exitRegistry[name] = null; SaveRegistry(); } }
        public static void AddConditionMethod(string name) { if (!_conditionRegistry.ContainsKey(name)) { _conditionRegistry[name] = null; SaveRegistry(); } }

        public static void RemoveEnterMethod(string name) { if (_enterRegistry.Remove(name)) SaveRegistry(); }
        public static void RemoveUpdateMethod(string name) { if (_updateRegistry.Remove(name)) SaveRegistry(); }
        public static void RemoveExitMethod(string name) { if (_exitRegistry.Remove(name)) SaveRegistry(); }
        public static void RemoveConditionMethod(string name) { if (_conditionRegistry.Remove(name)) SaveRegistry(); }

        public static void BindEnterAction(string name, Action<IStateContext> action) => _enterRegistry[name] = action;
        public static void BindUpdateAction(string name, Action<IStateContext> action) => _updateRegistry[name] = action;
        public static void BindExitAction(string name, Action<IStateContext> action) => _exitRegistry[name] = action;
        public static void BindCondition(string name, Func<IStateContext, bool> func) => _conditionRegistry[name] = func;
    }
}