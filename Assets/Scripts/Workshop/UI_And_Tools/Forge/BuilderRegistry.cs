// File: Assets/Scripts/Workshop/Forge/BuilderRegistry.cs
using System;
using System.Collections.Generic;
using UnityEngine;
using Workshop.UI_And_Tools.Forge.Builders;

namespace Workshop.UI_And_Tools.Forge
{
    public static class BuilderRegistry
    {
        // Map: Product Type -> List of Builders that can build it
        private static Dictionary<Type, List<IBuilder>> _registry = new Dictionary<Type, List<IBuilder>>();

        public static void Register(IBuilder builder)
        {
            Type productType = builder.GetProductType();
            if (!_registry.ContainsKey(productType))
            {
                _registry[productType] = new List<IBuilder>();
            }

            if (!_registry[productType].Contains(builder))
            {
                _registry[productType].Add(builder);
                Debug.Log($"[Registry] {builder.ToolName} volunteered to build {productType.Name}");
            }
        }

        public static List<IBuilder> FindBuildersFor(Type productType)
        {
            if (_registry.TryGetValue(productType, out var list))
            {
                return list;
            }
            return new List<IBuilder>();
        }
    }
}