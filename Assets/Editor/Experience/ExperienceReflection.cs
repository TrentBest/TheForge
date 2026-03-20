#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using TheSingularityWorkshop.Ontology;

public static class ExperienceReflection
{
    public class OntologyNode
    {
        public Type Type;
        public ExperienceMetaAttribute Meta;
        public List<OntologyNode> Children = new List<OntologyNode>();
    }

    public static OntologyNode BuildOntologyTree()
    {
        // 1. Get all types that implement IExperience
        var allTypes = AppDomain.CurrentDomain.GetAssemblies()
            .SelectMany(a => a.GetTypes())
            .Where(t => typeof(IExperience).IsAssignableFrom(t) && t != typeof(IExperience))
            .ToList();

        // 2. Create Root
        var root = new OntologyNode
        {
            Type = typeof(IExperience),
            Meta = GetMeta(typeof(IExperience))
        };

        // 3. Recursive Build
        AddChildren(root, allTypes);
        return root;
    }

    private static void AddChildren(OntologyNode parent, List<Type> allTypes)
    {
        // Find types that implement the parent directly (simplification for tree view)
        // In C#, finding "Direct" implementation via reflection is tricky, 
        // so we check if it implements/inherits Parent, and isn't implemented by another candidate.

        var children = allTypes.Where(t =>
            IsDirectChild(parent.Type, t, allTypes)
        ).ToList();

        foreach (var childType in children)
        {
            var node = new OntologyNode
            {
                Type = childType,
                Meta = GetMeta(childType)
            };
            parent.Children.Add(node);
            AddChildren(node, allTypes); // Recurse
        }
    }

    private static bool IsDirectChild(Type parent, Type candidate, List<Type> allTypes)
    {
        if (!parent.IsAssignableFrom(candidate)) return false;
        if (parent == candidate) return false;

        // Ensure there isn't a closer intermediate type in the list
        // e.g. If Parent=IExperience, Candidate=IGame, ensure ISimulation isn't in between.
        foreach (var other in allTypes)
        {
            if (other == candidate || other == parent) continue;
            if (parent.IsAssignableFrom(other) && other.IsAssignableFrom(candidate))
                return false; // 'other' is strictly between parent and candidate
        }
        return true;
    }

    private static ExperienceMetaAttribute GetMeta(Type t)
    {
        var attr = t.GetCustomAttribute<ExperienceMetaAttribute>();
        return attr ?? new ExperienceMetaAttribute(t.Name, "No description provided.");
    }
}
#endif