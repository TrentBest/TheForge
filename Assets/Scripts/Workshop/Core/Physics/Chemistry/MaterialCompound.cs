// File: Assets/Scripts/Workshop/Core/Physics/Chemistry/MaterialCompound.cs
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace TheSingularityWorkshop.Core.Physics.Chemistry
{
    public class MaterialCompound
    {
        public string CompoundName { get; set; } = "Unknown Compound";
        public Dictionary<AtomicElement, int> Composition { get; private set; } = new Dictionary<AtomicElement, int>();

        public void AddElement(AtomicElement element, int count = 1)
        {
            if (Composition.ContainsKey(element)) Composition[element] += count;
            else Composition[element] = count;
        }

        // --- Macroscopic Property Derivation ---

        public float CalculateMolarMass() => Composition.Sum(kvp => kvp.Key.AtomicMass * kvp.Value);

        public float GetAverageSpecificHeat()
        {
            if (Composition.Count == 0) return 0;
            // Weighted average of specific heat capacity based on atomic mass contribution
            float totalMass = CalculateMolarMass();
            return Composition.Sum(kvp => (kvp.Key.SpecificHeat * (kvp.Key.AtomicMass * kvp.Value)) / totalMass);
        }

        public bool IsMetallic()
        {
            // Simplified logic: If the majority composition is Transition/Post-Transition metals
            int metalAtoms = Composition.Where(kvp => kvp.Key.Category == ElementCategory.TransitionMetal || kvp.Key.Category == ElementCategory.PostTransition).Sum(kvp => kvp.Value);
            return metalAtoms > Composition.Values.Sum() / 2;
        }

        // Derive Unity Rendering Properties from Chemistry!
        public Color DeriveAlbedo()
        {
            if (Composition.Count == 0) return Color.white;

            // Blend the display colors of the constituent atoms
            float r = 0, g = 0, b = 0;
            int totalAtoms = Composition.Values.Sum();

            foreach (var kvp in Composition)
            {
                float weight = (float)kvp.Value / totalAtoms;
                r += kvp.Key.DisplayColor.r * weight;
                g += kvp.Key.DisplayColor.g * weight;
                b += kvp.Key.DisplayColor.b * weight;
            }
            return new Color(r, g, b, 1f);
        }

        public float DeriveSmoothness()
        {
            // Metals and Noble gases tend to have higher specular/smoothness properties in a solid state
            return IsMetallic() ? 0.8f : 0.2f;
        }
    }
}