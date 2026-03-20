using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;
using TheSingularityWorkshop.MicroPackages;

namespace TheSingularityWorkshop.Armada2525.GURPS
{
    public class WeaponsAPI : IGurpsApiProvider
    {
        public int Id => 1007; // Unique ID for Weapons
        public ProviderType ProviderType => ProviderType.GurpsApi;

        public string ModuleName => "Armory Database";

        private List<GURPSWeapon> _weapons = new List<GURPSWeapon>();

        [Serializable]
        private class CacheWrapper { public List<GURPSWeapon> Weapons = new List<GURPSWeapon>(); }
        private string CachePath => Application.persistentDataPath + "/DGURPS_Weapons.json";

        public WeaponsAPI() { }

        public void Initialize() => LoadFromCache();

        public void LoadFromCache()
        {
            if (File.Exists(CachePath))
            {
                try
                {
                    var cache = JsonUtility.FromJson<CacheWrapper>(File.ReadAllText(CachePath));
                    if (cache != null && cache.Weapons != null)
                    {
                        _weapons = cache.Weapons;
                    }
                }
                catch (Exception e)
                {
                    Debug.LogError($"[DGURPS] Weapons Load Error: {e.Message}");
                }
            }

            // If the file didn't exist, or it was completely empty, seed the defaults
            if (_weapons.Count == 0)
            {
                SeedStandardWeapons();
            }
        }

        public void SaveToCache()
        {
            try
            {
                string json = JsonUtility.ToJson(new CacheWrapper { Weapons = _weapons }, true);
                File.WriteAllText(CachePath, json);
            }
            catch (Exception e)
            {
                Debug.LogError($"[DGURPS] Weapons Save Error: {e.Message}");
            }
        }

        // --- CRUD OPERATIONS ---

        public List<GURPSWeapon> GetAll() => _weapons.OrderBy(w => w.Name).ToList();

        public GURPSWeapon GetExact(string name, string sourceBook) =>
            _weapons.FirstOrDefault(w => w.Name == name && w.SourceBookName == sourceBook);

        public void Register(GURPSWeapon weapon)
        {
            var existing = GetExact(weapon.Name, weapon.SourceBookName);
            if (existing != null)
            {
                _weapons.Remove(existing);
            }

            _weapons.Add(weapon);
            SaveToCache();
        }

        public void UnRegister(GURPSWeapon weapon)
        {
            var existing = GetExact(weapon.Name, weapon.SourceBookName);
            if (existing != null)
            {
                _weapons.Remove(existing);
                SaveToCache();
            }
        }

        // --- SEED DATA ---

        private void SeedStandardWeapons()
        {
            
            _weapons.Clear();

           

            

            // --- GURPS Basic Set (Low & Modern Tech) ---
            _weapons.Add(new GURPSWeapon { Name = "Broadsword", DamageString = "sw+1 cut", Cost = 500, SourceBookName = "GURPS Basic Set (3rd Ed. Revised)" });
            _weapons.Add(new GURPSWeapon { Name = "Crossbow", DamageString = "thr+4 imp", Cost = 150, SourceBookName = "GURPS Basic Set (3rd Ed. Revised)" });
            _weapons.Add(new GURPSWeapon { Name = "9mm Auto Pistol", DamageString = "2d-1 pi", Cost = 200, SourceBookName = "GURPS Basic Set (3rd Ed. Revised)" });
            _weapons.Add(new GURPSWeapon { Name = "Pump Shotgun", DamageString = "1d+1 pi", Cost = 240, SourceBookName = "GURPS Basic Set (3rd Ed. Revised)" });
            _weapons.Add(new GURPSWeapon
            {
                Name = "Assault Rifle, 5.56mm",
                SourceBookName = "GURPS Basic Set (3rd Ed. Revised)",
                Description = "A rugged, dependable kinetic weapon favored by low-tech mercenaries and planetary militias.",
                TechLevel = 7,
                DamageString = "5d pi",
                Cost = 500,
                Weight = 9.0f,
                MinimumStrength = 9,
                LegalityClass = 2,
                Accuracy = "5",
                Range = "500/3500",
                RateOfFire = "12",
                Shots = "30(3)",
                Bulk = "-4",
                Recoil = "2"
            });

            // --- GURPS Ultra-Tech (Future & Energy Weapons) ---
            _weapons.Add(new GURPSWeapon
            {
                Name = "Laser Pistol",
                SourceBookName = "GURPS Ultra-Tech (2nd Ed Revised)",
                Description = "A standard issue sidearm for spacefarers. Fires a coherent beam of light. Quiet and recoilless.",
                TechLevel = 9,
                DamageString = "3d(2) burn",
                Cost = 250,
                Weight = 3.3f,
                MinimumStrength = 4,
                LegalityClass = 3,
                Accuracy = "6",
                Range = "300/900",
                RateOfFire = "10",
                Shots = "100(3)",
                Bulk = "-2",
                Recoil = "1"
            });
            _weapons.Add(new GURPSWeapon { Name = "Heavy Laser Rifle", DamageString = "5d(2) burn", Cost = 2000, SourceBookName = "GURPS Ultra-Tech (2nd Ed Revised)" });
            _weapons.Add(new GURPSWeapon { Name = "Blaster Rifle", DamageString = "6d(5) sur", Cost = 4000, SourceBookName = "GURPS Ultra-Tech (2nd Ed Revised)" });
            _weapons.Add(new GURPSWeapon { Name = "Plasma Gun", DamageString = "8d ex", Cost = 5000, SourceBookName = "GURPS Ultra-Tech (2nd Ed Revised)" });
            _weapons.Add(new GURPSWeapon { Name = "Gauss Rifle (4mm)", DamageString = "6d(3) pi-", Cost = 4100, SourceBookName = "GURPS Ultra-Tech (2nd Ed Revised)" });
            _weapons.Add(new GURPSWeapon { Name = "Monowire Sword", DamageString = "sw+2(10) cut", Cost = 900, SourceBookName = "GURPS Ultra-Tech (2nd Ed Revised)" });
            //_weapons.Add(new GURPSWeapon { Name = "Electrolaser", DamageString = "1d-3 HT-4 aff", Cost = 1200, SourceBookName = "GURPS Ultra-Tech (2nd Ed Revised)" });
            //_weapons.Add(new GURPSWeapon { Name = "Sonic Stunner", DamageString = "HT-3 aff", Cost = 800, SourceBookName = "GURPS Ultra-Tech (2nd Ed Revised)" });

            // --- GURPS Space (Ship-to-Ship Weapons) ---
            _weapons.Add(new GURPSWeapon { Name = "Light Ship Laser", DamageString = "3dx10(2) burn", Cost = 500000, SourceBookName = "GURPS Space (3rd Ed)" });
            _weapons.Add(new GURPSWeapon { Name = "Particle Beam Battery", DamageString = "5dx10 sur", Cost = 1500000, SourceBookName = "GURPS Space (3rd Ed)" });
            _weapons.Add(new GURPSWeapon { Name = "Anti-Matter Missile", DamageString = "6dx100 ex", Cost = 50000, SourceBookName = "GURPS Space (3rd Ed)" });
            _weapons.Add(new GURPSWeapon { Name = "Railgun Mass Driver", DamageString = "8dx10(3) cr", Cost = 800000, SourceBookName = "GURPS Space (3rd Ed)" });

            // --- GURPS High-Tech (Mid-Tech transitional) ---
            _weapons.Add(new GURPSWeapon { Name = "Gatling Gun", DamageString = "7d pi", Cost = 5000, SourceBookName = "GURPS High-Tech (3rd Ed)" });
            _weapons.Add(new GURPSWeapon { Name = "Flamethrower", DamageString = "3d burn", Cost = 1500, SourceBookName = "GURPS High-Tech (3rd Ed)" });

            SaveToCache();
        }
    }
}