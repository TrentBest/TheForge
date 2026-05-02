using System;
using System.Collections.Generic;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Workshop.UI_And_Tools.Forge.Hermit.UI
{
    [Serializable]
    public class HermitUIState
    {
        public List<string> ChatSenders = new List<string>();
        public List<string> ChatContents = new List<string>();
        public List<string> EnvIds = new List<string>();
        public List<string> EnvDescs = new List<string>();
        public string CurrentInput = "";
        public bool IsGhostLinked = false;
    }

    public static class HermitSessionManager
    {
        private const string SESSION_KEY = "Hermit_UI_State_Data";

        public static void SaveState(HermitUIState state)
        {
#if UNITY_EDITOR
            string json = JsonUtility.ToJson(state);
            SessionState.SetString(SESSION_KEY, json);
#endif
        }

        public static HermitUIState LoadState()
        {
#if UNITY_EDITOR
            string json = SessionState.GetString(SESSION_KEY, "");
            if (!string.IsNullOrEmpty(json)) return JsonUtility.FromJson<HermitUIState>(json);
#endif
            return new HermitUIState(); // Return empty if fresh boot
        }

        public static void ClearState()
        {
#if UNITY_EDITOR
            SessionState.EraseString(SESSION_KEY);
#endif
        }
    }
}