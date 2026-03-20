using TheSingularityWorkshop.MicroPackages;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;

namespace TheSingularityWorkshop.Armada2525.GURPS
{
    public class DisadvantagesAPI : IGurpsApiProvider
    {
        public int Id => 1003; // Unique ID for Disadvantages
        public ProviderType ProviderType => ProviderType.GurpsApi;

        // --- IGurpsApiProvider Implementation ---
        public string ModuleName => "Disadvantages & Powers";

        private List<GURPSDisadvantage> _disadvantages = new List<GURPSDisadvantage>();

        [Serializable]
        private class CacheDisadvantageWrapper 
        { 
            public List<GURPSDisadvantage> Disadvantages = new List<GURPSDisadvantage>();
            
        }
       

        private string CachePath => Application.persistentDataPath + "/DGURPS_Disadvantages.json";

        public void Initialize() => LoadFromCache();

        public void SaveToCache()
        {
            try
            {
                string json = JsonUtility.ToJson(new CacheDisadvantageWrapper { Disadvantages = _disadvantages }, true);
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
                    var cache = JsonUtility.FromJson<CacheDisadvantageWrapper>(File.ReadAllText(CachePath));
                    if (cache != null)
                    {
                        _disadvantages = cache.Disadvantages ?? new List<GURPSDisadvantage>();
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
        public List<GURPSDisadvantage> GetAll() => _disadvantages;

        /// <summary>Finds an advantage by name.</summary>
        public GURPSDisadvantage GetByName(string name) => _disadvantages.FirstOrDefault(a => a.Name == name);

        /// <summary>Registers a new advantage and persists it to disk.</summary>
        public void Register(GURPSDisadvantage disadvantage)
        {
            if (!_disadvantages.Any(a => a.Name == disadvantage.Name))
            {
                _disadvantages.Add(disadvantage);
                SaveToCache();
            }
        }

        /// <summary>Seeds the system with classic GURPS 3e advantages if no data exists.</summary>
        private void SeedStandardAdvantages()
        {
            _disadvantages.Clear();

            Register(new GURPSDisadvantage { Name = "Absolute Direction", BaseCost = 5, Description = "Always knows which way is North." });
            Register(new GURPSDisadvantage { Name = "Ambidexterity", BaseCost = 10, Description = "No off-hand penalty." });
            Register(new GURPSDisadvantage { Name = "Combat Reflexes", BaseCost = 15, Description = "+1 to active defenses, +2 to Fright Checks." });
            Register(new GURPSDisadvantage { Name = "High Pain Threshold", BaseCost = 10, Description = "Ignore shock penalties." });

            SaveToCache();
        }
    }
}
