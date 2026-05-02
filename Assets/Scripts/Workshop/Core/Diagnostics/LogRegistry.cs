using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace Workshop.Core.Diagnostics
{
    [Serializable]
    public class LogChannelConfig
    {
        public string ChannelId;
        public string HexColor;
        public bool IsEnabled = true;
    }

    [Serializable]
    public class LogRegistryData { public List<LogChannelConfig> Channels; }

    public static class LogRegistry
    {
        private static Dictionary<string, LogChannelConfig> _channels = new Dictionary<string, LogChannelConfig>();
        private static string SavePath => Path.Combine(Application.persistentDataPath, "ForgeLogConfig.json");

        // MicroPackages call this in their boot sequence
        public static void RegisterChannel(string channelId, string defaultHexColor)
        {
            if (_channels.ContainsKey(channelId)) return; // Already exists (perhaps loaded from save)

            _channels[channelId] = new LogChannelConfig
            {
                ChannelId = channelId,
                HexColor = defaultHexColor,
                IsEnabled = true
            };
        }

        public static LogChannelConfig GetChannel(string channelId)
        {
            if (_channels.TryGetValue(channelId, out var config)) return config;

            // Auto-register unknown channels with a default white color so they don't break
            RegisterChannel(channelId, "#FFFFFF");
            return _channels[channelId];
        }

        public static void Save()
        {
            var data = new LogRegistryData { Channels = new List<LogChannelConfig>(_channels.Values) };
            File.WriteAllText(SavePath, JsonUtility.ToJson(data, true));
        }

        public static void Load()
        {
            if (!File.Exists(SavePath)) return;

            var data = JsonUtility.FromJson<LogRegistryData>(File.ReadAllText(SavePath));
            foreach (var channel in data.Channels)
            {
                _channels[channel.ChannelId] = channel;
            }
        }
    }
}