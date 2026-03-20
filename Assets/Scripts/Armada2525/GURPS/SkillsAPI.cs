using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;
using TheSingularityWorkshop.MicroPackages;

namespace TheSingularityWorkshop.Armada2525.GURPS
{
    public class SkillsAPI : IGurpsApiProvider
    {
        public int Id => 1006; // Unique ID for Skills API
        public ProviderType ProviderType => ProviderType.GurpsApi;

        public string ModuleName => "Skills Library";

        private List<GURPSSkillDefinition> _skills = new List<GURPSSkillDefinition>();

        [Serializable]
        private class CacheWrapper { public List<GURPSSkillDefinition> Skills = new List<GURPSSkillDefinition>(); }
        private string CachePath => Application.persistentDataPath + "/DGURPS_Skills.json";

        public SkillsAPI() { }

        public void Initialize() => LoadFromCache();

        public void LoadFromCache()
        {
            if (File.Exists(CachePath))
            {
                try
                {
                    var cache = JsonUtility.FromJson<CacheWrapper>(File.ReadAllText(CachePath));
                    if (cache != null && cache.Skills != null)
                    {
                        _skills = cache.Skills;
                        return;
                    }
                }
                catch (Exception e)
                {
                    Debug.LogError($"[DGURPS] Skills Load Error: {e.Message}");
                }
            }

            // Fallback if no cache exists
            SeedStandardSkills();
        }

        public void SaveToCache()
        {
            try
            {
                string json = JsonUtility.ToJson(new CacheWrapper { Skills = _skills }, true);
                File.WriteAllText(CachePath, json);
            }
            catch (Exception e)
            {
                Debug.LogError($"[DGURPS] Skills Save Error: {e.Message}");
            }
        }

        // --- CRUD OPERATIONS ---

        public List<GURPSSkillDefinition> GetAll() => _skills.OrderBy(s => s.Name).ToList();

        public GURPSSkillDefinition GetExact(string name, string sourceBook) =>
            _skills.FirstOrDefault(s => s.Name == name && s.SourceBookName == sourceBook);

        public void Register(GURPSSkillDefinition skill)
        {
            var existing = GetExact(skill.Name, skill.SourceBookName);
            if (existing != null)
            {
                _skills.Remove(existing);
            }

            _skills.Add(skill);
            SaveToCache();
        }

        public void UnRegister(GURPSSkillDefinition skill)
        {
            var existing = GetExact(skill.Name, skill.SourceBookName);
            if (existing != null)
            {
                _skills.Remove(existing);
                SaveToCache();
            }
        }

        // --- SEED DATA ---

        private void SeedStandardSkills()
        {
            _skills.Clear();

            // Porting the mock data you built in the UI prototype over to the Global DB
            _skills.Add(new GURPSSkillDefinition { Name = "Astrogation", BaseAttribute = GURPSAttributeType.IQ, Difficulty = GURPSDifficulty.Average, SourceBookName = "GURPS Basic Set", Description = "Navigating through interstellar space." });
            _skills.Add(new GURPSSkillDefinition { Name = "Piloting (Starship)", BaseAttribute = GURPSAttributeType.DX, Difficulty = GURPSDifficulty.Average, SourceBookName = "GURPS Basic Set", Description = "Operating a spacecraft." });
            _skills.Add(new GURPSSkillDefinition { Name = "Beam Weapons (Pistol)", BaseAttribute = GURPSAttributeType.DX, Difficulty = GURPSDifficulty.Easy, SourceBookName = "GURPS Basic Set", Description = "Firing hand-held energy weapons." });
            _skills.Add(new GURPSSkillDefinition { Name = "Computer Programming", BaseAttribute = GURPSAttributeType.IQ, Difficulty = GURPSDifficulty.Hard, SourceBookName = "GURPS Basic Set", Description = "Writing and debugging software." });
            _skills.Add(new GURPSSkillDefinition { Name = "Free Fall", BaseAttribute = GURPSAttributeType.DX, Difficulty = GURPSDifficulty.Average, SourceBookName = "GURPS Space", Description = "Operating in zero gravity without nausea." });

            SaveToCache();
        }
    }
}