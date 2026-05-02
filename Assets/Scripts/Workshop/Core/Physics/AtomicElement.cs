// File: Assets/Scripts/Workshop/Core/Physics/Chemistry/AtomicElement.cs
using UnityEngine;

namespace TheSingularityWorkshop.Core.Physics.Chemistry
{
    public enum ElementCategory { Alkali, AlkalineEarth, TransitionMetal, PostTransition, Metalloid, NonMetal, Halogen, NobleGas, Actinide }

    public struct AtomicElement
    {
        public int AtomicNumber;
        public string Symbol;
        public string Name;
        public float AtomicMass;        // g/mol
        public float Electronegativity; // Pauling scale
        public float MeltingPointK;     // Kelvin
        public float BoilingPointK;     // Kelvin
        public float SpecificHeat;      // J/(kg·K)
        public ElementCategory Category;
        public Color DisplayColor;      // Base visualization color
    }
}