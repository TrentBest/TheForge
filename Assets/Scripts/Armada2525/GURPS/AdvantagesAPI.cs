using TheSingularityWorkshop.MicroPackages;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;

namespace TheSingularityWorkshop.Armada2525.GURPS
{
    public class AdvantagesAPI : IGurpsApiProvider
    {
        // --- IProvider Implementation ---
        public int Id => 1002; // Unique ID for Advantages
        public ProviderType ProviderType => ProviderType.GurpsApi;

        // --- IGurpsApiProvider Implementation ---
        public string ModuleName => "Advantages";

        private List<GURPSAdvantage> _advantages = new List<GURPSAdvantage>();

        [Serializable]
        private class CacheAdvantageWrapper
        {
            public List<GURPSAdvantage> Advantages = new List<GURPSAdvantage>();
        }
        private string CachePath => Application.persistentDataPath + "/DGURPS_Advantages.json";

        public void Initialize() => LoadFromCache();

        public void SaveToCache()
        {
            try
            {
                string json = JsonUtility.ToJson(new CacheAdvantageWrapper { Advantages = _advantages }, true);
                File.WriteAllText(CachePath, json);
            }
            catch (Exception e)
            {
                Debug.LogError($"[DGURPS] Advantages Save Error: {e.Message}");
            }
        }

        public void LoadFromCache()
        {
            if (File.Exists(CachePath))
            {
                try
                {
                    var cache = JsonUtility.FromJson<CacheAdvantageWrapper>(File.ReadAllText(CachePath));
                    if (cache != null)
                    {
                        _advantages = cache.Advantages ?? new List<GURPSAdvantage>();
                        return;
                    }
                }
                catch (Exception e)
                {
                    Debug.LogError($"[DGURPS] Advantages Load Error: {e.Message}");
                }
            }

            // Fallback: Seed basic advantages if cache is empty or missing
            SeedStandardAdvantages();
        }

        // --- PUBLIC API LOGIC ---

        /// <summary>Returns all registered advantages.</summary>
        public List<GURPSAdvantage> GetAll() => _advantages;

        /// <summary>Finds an advantage by name.</summary>
        public GURPSAdvantage GetByName(string name) => _advantages.FirstOrDefault(a => a.Name == name);

        /// <summary>Registers a new advantage and persists it to disk.</summary>
        public void Register(GURPSAdvantage advantage)
        {
            if (!_advantages.Any(a => a.Name == advantage.Name))
            {
                _advantages.Add(advantage);
                SaveToCache();
            }
        }

        /// <summary>Seeds the system with classic GURPS 3e advantages if no data exists.</summary>
        private void SeedStandardAdvantages()
        {
            _advantages.Clear();

            Register(new GURPSAdvantage { Name = "Absolute Direction", BaseCost = 5, Description = "Always knows which way is North." });
            Register(new GURPSAdvantage { Name = "Ambidexterity", BaseCost = 10, Description = "No off-hand penalty." });
            Register(new GURPSAdvantage { Name = "Combat Reflexes", BaseCost = 15, Description = "+1 to active defenses, +2 to Fright Checks." });
            Register(new GURPSAdvantage { Name = "High Pain Threshold", BaseCost = 10, Description = "Ignore shock penalties." });

            SaveToCache();
        }
    }
}
