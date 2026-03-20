using TheSingularityWorkshop.MicroPackages;
using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace TheSingularityWorkshop.Armada2525.GURPS
{
    public class BooksAPI : IGurpsApiProvider
    {
        // --- IProvider Implementation ---
        public int Id => 1001; // Give it a unique ID
        public ProviderType ProviderType => ProviderType.GurpsApi; // Your new enum value

        // --- IGurpsApiProvider Implementation ---
        public string ModuleName => "Library & Books";

        private List<GURPSBook> _ruleBooks = new List<GURPSBook>();
        private List<GURPSBook> _activeBooks = new List<GURPSBook>();

        [Serializable]
        private class CacheWrapper { public List<GURPSBook> Books = new List<GURPSBook>(); }
        private string CachePath => Application.persistentDataPath + "/DGURPS_Library.json";

        public void Initialize() => LoadFromCache();

        public void SaveToCache()
        {
            File.WriteAllText(CachePath, JsonUtility.ToJson(new CacheWrapper { Books = _ruleBooks }, true));
        }

        public void LoadFromCache()
        {
            if (File.Exists(CachePath))
            {
                Debug.Log($"Load From Cache:  {CachePath}");
                try
                {
                    var cache = JsonUtility.FromJson<CacheWrapper>(File.ReadAllText(CachePath));
                    if (cache != null && cache.Books.Count > 0)
                    {
                        _ruleBooks = cache.Books;
                        _activeBooks = new List<GURPSBook>(_ruleBooks);
                        // Sort logic..
                        return;
                    }
                }
                catch (Exception e) { Debug.LogError($"[DGURPS] Library Load Error: {e.Message}"); }
            }
            
            SaveToCache();
        }

        // --- PUBLIC API LOGIC ---
        public List<GURPSBook> GetAll() => _ruleBooks;

        public void Register(GURPSBook book)
        {
            // Your logic..
            SaveToCache(); // The provider manages its own save calls!
        }
    }
}
