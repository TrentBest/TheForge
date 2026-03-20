using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;
using TheSingularityWorkshop.MicroPackages;

namespace TheSingularityWorkshop.Armada2525.GURPS
{
    public class TechnologyLevelAPI : IGurpsApiProvider
    {
        // --- IProvider Implementation ---
        public int Id => 1004; // Unique ID for TechLevels Levels
        public ProviderType ProviderType => ProviderType.GurpsApi;

        // --- IGurpsApiProvider Implementation ---
        public string ModuleName => "TechLevels & Progress";

        private List<TechnologyLevel> _techLevels = new List<TechnologyLevel>();

        [Serializable]
        private class CacheWrapper { public List<TechnologyLevel> Levels = new List<TechnologyLevel>(); }
        private string CachePath => Application.persistentDataPath + "/DGURPS_TechLevels.json";

        public void Initialize() => LoadFromCache();

        public void SaveToCache()
        {
            try
            {
                string json = JsonUtility.ToJson(new CacheWrapper { Levels = _techLevels }, true);
                File.WriteAllText(CachePath, json);
            }
            catch (Exception e)
            {
                Debug.LogError($"[DGURPS] TechLevel Save Error: {e.Message}");
            }
        }

        public void LoadFromCache()
        {
            if (File.Exists(CachePath))
            {
                try
                {
                    var cache = JsonUtility.FromJson<CacheWrapper>(File.ReadAllText(CachePath));
                    if (cache != null)
                    {
                        _techLevels = cache.Levels ?? new List<TechnologyLevel>();
                        return;
                    }
                }
                catch (Exception e)
                {
                    Debug.LogError($"[DGURPS] TechLevel Load Error: {e.Message}");
                }
            }
            SeedStandardTechLevels();
        }

        public List<TechnologyLevel> GetAll() => _techLevels.OrderBy(tl => tl.Level).ThenBy(tl => tl.DivergentOffset).ToList();

        public TechnologyLevel GetByLevel(int level, int divergent = 0) =>
            _techLevels.FirstOrDefault(tl => tl.Level == level && tl.DivergentOffset == divergent);

        public void Register(TechnologyLevel techLevel)
        {
            var existing = GetByLevel(techLevel.Level, techLevel.DivergentOffset);
            if (existing != null)
            {
                _techLevels.Remove(existing);
            }
            _techLevels.Add(techLevel);
            SaveToCache(); // Serialization happens here
        }

        public void UnRegister(TechnologyLevel techLevel)
        {
            var existing = GetByLevel(techLevel.Level, techLevel.DivergentOffset);
            if (existing != null)
            {
                _techLevels.Remove(existing);
                SaveToCache(); // Added this! Serialization now happens on deletion
            }
        }

        private void SeedStandardTechLevels()
        {
            _techLevels.Clear();
            _techLevels.Add(new TechnologyLevel(0, "Stone Age", 0.05f));
            _techLevels.Add(new TechnologyLevel(3, "Medieval", 0.2f));
            _techLevels.Add(new TechnologyLevel(7, "Nuclear Age", 1.0f));
            _techLevels.Add(new TechnologyLevel(8, "Digital Age", 1.5f));
            _techLevels.Add(new TechnologyLevel(9, "Early Starfaring", 2.0f));

            SaveToCache();
        }
    }
}