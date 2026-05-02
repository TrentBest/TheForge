using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;
using UnityEngine.UIElements;

namespace Workshop.UI_And_Tools.Forge.Builders.GuiBuilders
{
    /// <summary>
    /// Centralized Dependency Injection and Instantiation Factory for all UI Providers.
    /// Acts as the gateway for UI generation, making it the ideal anchor for future 
    /// memory pooling and telemetry tracking (metaDev).
    /// </summary>
    public static class TypeGuiProviderFactory
    {
        // Nomenclature: 'Registry' implies a definitive map of Type -> Construction Logic
        private static readonly Dictionary<Type, Func<object, IGuiProvider>> _providerRegistry = new Dictionary<Type, Func<object, IGuiProvider>>();
        private static bool _isInitialized = false;

        public static void Initialize()
        {
            if (_isInitialized) return;

            // Auto-Discovery Pass: Scan for [ForgeInspector] attributes
            foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies())
            {
                foreach (var type in assembly.GetTypes())
                {
                    var attribute = type.GetCustomAttribute<ForgeInspectorAttribute>();
                    if (attribute != null && typeof(IGuiProvider).IsAssignableFrom(type))
                    {
                        // Map the target type to a function that instantiates the assigned provider
                        _providerRegistry[attribute.TargetType] = (obj) => (IGuiProvider)Activator.CreateInstance(type, obj);
                    }
                }
            }
            _isInitialized = true;
            Debug.Log($"[Forge Factory] Global UI Routing Initialized. {_providerRegistry.Count} Custom Inspectors Mapped.");
        }

        /// <summary>
        /// Manual Registration: Explicitly map a data type to a specific GuiProvider instantiation logic.
        /// Useful for complex types where Reflection is too slow or messy (The "Stacked Deck").
        /// </summary>
        public static void Register<TData>(Func<TData, IGuiProvider> providerFactory)
        {
            _providerRegistry[typeof(TData)] = (obj) => providerFactory((TData)obj);
        }

        /// <summary>
        /// Attempts to resolve a custom provider without falling back to Reflection.
        /// Used by the ReflectiveGuiBuilder to intercept and wrap complex nested objects.
        /// </summary>
        public static bool TryGetCustomProvider(Type targetType, object instance, out IGuiProvider provider)
        {
            if (!_isInitialized) Initialize();
            provider = null;

            if (instance == null) return false;

            // 1. Direct Match
            if (_providerRegistry.TryGetValue(targetType, out var factory))
            {
                provider = factory(instance);
                return true;
            }

            // 2. Inheritance/Interface Match
            foreach (var kvp in _providerRegistry)
            {
                if (kvp.Key.IsAssignableFrom(targetType))
                {
                    provider = kvp.Value(instance);
                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// The definitive gateway to retrieve a GUI for any object.
        /// Will attempt Custom Registration -> Inheritance -> Static Types -> Reflection Fallback.
        /// </summary>
        public static IGuiProvider GetProviderFor(object target, string fallbackTitle = null)
        {
            if (!_isInitialized) Initialize();

            if (target == null)
                return new DynamicGuiProvider(ctx => new Label("[ NULL OBJECT ]") { style = { color = Color.red } });

            Type targetType = target.GetType();

            // Try to pull from our explicitly mapped registry first
            if (TryGetCustomProvider(targetType, target, out IGuiProvider customProvider))
            {
                return customProvider;
            }

            // Fallback 1: Is it a Static Type? (e.g. looking at a static class like Math)
            if (target is Type staticType)
                return new StaticReflectiveGuiBuilder(staticType, fallbackTitle ?? $"{staticType.Name} TELEMETRY", 100);

            // Fallback 2: The sledgehammer. Use Reflection to dissect the unknown object.
            try
            {
                Type reflectiveBuilderType = typeof(ReflectiveGuiBuilder<>).MakeGenericType(targetType);
                return (IGuiProvider)Activator.CreateInstance(reflectiveBuilderType, target, fallbackTitle ?? targetType.Name.ToUpper());
            }
            catch (Exception ex)
            {
                return new DynamicGuiProvider(ctx => new Label($"Reflection Failed: {ex.Message}") { style = { color = Color.red } });
            }
        }

        /// <summary>
        /// Helper to find and instantiate a Provider purely by its string class name.
        /// </summary>
        public static IGuiProvider GetProviderByName(string providerName)
        {
            if (!_isInitialized) Initialize();

            var type = AppDomain.CurrentDomain.GetAssemblies()
                .SelectMany(a => a.GetTypes())
                .FirstOrDefault(t => t.Name == providerName && typeof(IGuiProvider).IsAssignableFrom(t));

            if (type == null) return null;

            try
            {
                return (IGuiProvider)Activator.CreateInstance(type);
            }
            catch
            {
                return null;
            }
        }

        /// <summary>
        /// Checks if a custom provider is explicitly registered for a specific Type, 
        /// without requiring an instantiated object. Used by Domain Scanners to verify cache mappings.
        /// </summary>
        public static bool HasProviderFor(Type targetType)
        {
            if (!_isInitialized) Initialize();

            if (targetType == null) return false;

            // 1. Direct Match
            if (_providerRegistry.ContainsKey(targetType))
            {
                return true;
            }

            // 2. Inheritance/Interface Match
            foreach (var kvp in _providerRegistry)
            {
                if (kvp.Key.IsAssignableFrom(targetType))
                {
                    return true;
                }
            }

            return false;
        }
    }
}