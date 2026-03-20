using System;
using System.Collections.Generic;
using UnityEngine;

namespace TheSingularityWorkshop.Armada2525.GURPS
{
    public static class DigitalGenericUniversalRolePlayingSystem
    {
        // THE COLLECTION: Holds every GURPS API dynamically via your IProvider pattern!
        private static Dictionary<Type, IGurpsApiProvider> _providers = new Dictionary<Type, IGurpsApiProvider>();
        private static bool _isInitialized = false;

        public static void InitializeCoreSystem()
        {
            if (_isInitialized) return;

            // 1. Register the core APIs into the collection
            RegisterProvider(new BooksAPI());
            RegisterProvider(new AdvantagesAPI());
            RegisterProvider(new DisadvantagesAPI());
            RegisterProvider(new WeaponsAPI());
            RegisterProvider(new TechnologyLevelAPI());
            RegisterProvider(new TechnologyCategoryAPI());           
            RegisterProvider(new SkillsAPI());
            RegisterProvider(new UniverseAPI());//Should be last as the container for everything
            // 2. Iterate and initialize EVERYTHING dynamically!
            foreach (var provider in _providers.Values)
            {
                provider.Initialize();
                Debug.Log($"[DGURPS] Booted Provider: {provider.ModuleName} (ID: {provider.Id})");
            }

            _isInitialized = true;
        }

        // --- REGISTRY LOGIC ---
        public static void RegisterProvider<T>(T provider) where T : IGurpsApiProvider
        {
            _providers[typeof(T)] = provider;
        }

        public static T GetProvider<T>() where T : IGurpsApiProvider
        {
            if (_providers.TryGetValue(typeof(T), out var provider))
                return (T)provider;

            Debug.LogError($"[DGURPS] Requested Provider {typeof(T).Name} is not registered!");
            return default;
        }

        public static void SaveAll()
        {
            foreach (var provider in _providers.Values) provider.SaveToCache();
        }

        // --- CONVENIENCE ACCESSORS (The tip of the iceberg) ---
        // This gives you the beautiful DGURPS.Books syntax while hitting the dynamic collection under the hood!
        public static BooksAPI Books => GetProvider<BooksAPI>();
        public static AdvantagesAPI Advantages => GetProvider<AdvantagesAPI>();
        public static DisadvantagesAPI Disadvantages => GetProvider<DisadvantagesAPI>();
        public static WeaponsAPI Weapons => GetProvider<WeaponsAPI>();
        public static TechnologyLevelAPI TechLevels => GetProvider<TechnologyLevelAPI>();
        public static TechnologyCategoryAPI TechCategories => GetProvider<TechnologyCategoryAPI>();
        public static SkillsAPI Skills => GetProvider<SkillsAPI>();
        public static UniverseAPI Universes => GetProvider<UniverseAPI>();
    }
}
