using TheSingularityWorkshop.MicroPackages;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;

namespace TheSingularityWorkshop.Armada2525.GURPS
{
    public class UniverseAPI : IGurpsApiProvider
    {
        public int Id => 1010;
        public ProviderType ProviderType => ProviderType.GurpsApi;
        public string ModuleName => "Cosmology & Universes";

        private List<GURPSUniverse> _universes = new List<GURPSUniverse>();
        private string CachePath => Application.persistentDataPath + "/DGURPS_Multiverse.json";

        public void Initialize() => LoadFromCache();

        public List<GURPSUniverse> GetAll() => _universes.OrderBy(u => u.UniverseSeed).ToList();

        public void Register(GURPSUniverse uni)
        {
            var existing = _universes.FirstOrDefault(u => u.UniverseSeed == uni.UniverseSeed);
            if (existing != null) _universes.Remove(existing);
            _universes.Add(uni);
            SaveToCache();
        }

        public void LoadFromCache()
        {
            if (File.Exists(CachePath))
            {
                try
                {
                    var cache = JsonUtility.FromJson<CacheWrapper>(File.ReadAllText(CachePath));
                    if (cache != null && cache.Universes != null)
                        _universes = cache.Universes;
                }
                catch (Exception e)
                {
                    Debug.LogError($"[DGURPS] Universe Load Error: {e.Message}");
                }
            }

            if (_universes.Count == 0) SeedStandardUniverses();
        }

        private void SeedStandardUniverses()
        {
            _universes.Clear();

            // 1. ANCHOR: Universe 42 (The Real World / AEC Seed)
            _universes.Add(new GURPSUniverse
            {
                Name = "Earth (Prime)",
                UniverseSeed = 42,
                Description = "The Real-World anchor for all AEC and extracted data.",
                DefaultTechLevel = 8 // Modern era
            });

            // 2. CROSS-REFERENCE: Query Books for specific Universe definitions
            // This pulls from the existing Library managed by BooksAPI
            var books = DigitalGenericUniversalRolePlayingSystem.Books.GetAll();
            foreach (var book in books)
            {
                // Logic to extract Universe definitions from Book metadata
                if (book.Category == "Setting")
                {
                    _universes.Add(new GURPSUniverse
                    {
                        Name = book.Name,
                        Description = book.Description,
                        IncludedRuleBooks = new List<string> { book.Name }
                    });
                }
            }

            SaveToCache();
        }

        public void SaveToCache()
        {
            try
            {
                string json = JsonUtility.ToJson(new CacheWrapper { Universes = _universes }, true);
                File.WriteAllText(CachePath, json);
            }
            catch (Exception e) { Debug.LogError($"[DGURPS] Universe Save Error: {e.Message}"); }
        }

        [Serializable]
        private class CacheWrapper { public List<GURPSUniverse> Universes = new List<GURPSUniverse>(); }
    }
}