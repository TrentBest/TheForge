using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;
using TheSingularityWorkshop.MicroPackages;

namespace TheSingularityWorkshop.Armada2525.GURPS
{
    public class TechnologyCategoryAPI : IGurpsApiProvider
    {
        public int Id => 1005; // Unique ID
        public ProviderType ProviderType => ProviderType.GurpsApi;

        public string ModuleName => "Tech Categories";

        private List<TechnologyCategory> _categories = new List<TechnologyCategory>();

        [Serializable]
        private class CacheWrapper { public List<TechnologyCategory> Categories = new List<TechnologyCategory>(); }
        private string CachePath => Application.persistentDataPath + "/DGURPS_TechCategories.json";

        public void Initialize() => LoadFromCache();

        public void SaveToCache()
        {
            try
            {
                string json = JsonUtility.ToJson(new CacheWrapper { Categories = _categories }, true);
                File.WriteAllText(CachePath, json);
            }
            catch (Exception e)
            {
                Debug.LogError($"[DGURPS] Tech Category Save Error: {e.Message}");
            }
        }

        public void LoadFromCache()
        {
            if (File.Exists(CachePath))
            {
                try
                {
                    var cache = JsonUtility.FromJson<CacheWrapper>(File.ReadAllText(CachePath));
                    if (cache != null && cache.Categories != null && cache.Categories.Count > 0)
                    {
                        _categories = cache.Categories;
                        return;
                    }
                }
                catch (Exception e)
                {
                    Debug.LogError($"[DGURPS] Tech Category Load Error: {e.Message}");
                }
            }
            SeedStandardCategories();
        }

        public List<TechnologyCategory> GetAll() => _categories.OrderBy(c => c.Name).ToList();

        public TechnologyCategory GetExact(string name, string sourceBook) =>
            _categories.FirstOrDefault(c => c.Name == name && c.SourceBookName == sourceBook);

        public void Register(TechnologyCategory category)
        {
            var existing = GetExact(category.Name, category.SourceBookName);
            if (existing != null) _categories.Remove(existing);

            _categories.Add(category);
            SaveToCache();
        }

        public void UnRegister(TechnologyCategory category)
        {
            var existing = GetExact(category.Name, category.SourceBookName);
            if (existing != null)
            {
                _categories.Remove(existing);
                SaveToCache();
            }
        }

        private void SeedStandardCategories()
        {
            _categories.Clear();
            _categories.Add(new TechnologyCategory("Transportation", "Vehicles, starships, and movement tech."));
            _categories.Add(new TechnologyCategory("Weapons", "Personal and ship-to-ship offensive capabilities."));
            _categories.Add(new TechnologyCategory("Armor & Defenses", "Personal armor, force fields, and ship plating."));
            _categories.Add(new TechnologyCategory("Power", "Energy generation and storage."));
            _categories.Add(new TechnologyCategory("Biotechnology", "Genetic engineering, cloning, and cybernetics."));
            _categories.Add(new TechnologyCategory("Medicine", "Healing, drugs, and life extension."));
            _categories.Add(new TechnologyCategory("Communications", "FTL radios, planetary networks, and encryption."));
            _categories.Add(new TechnologyCategory("Sensors", "Radar, grav-scanners, and detection tech."));
            _categories.Add(new TechnologyCategory("Science & Education", "Research speed, teaching tools, and general knowledge."));
            SaveToCache();
        }
    }
}