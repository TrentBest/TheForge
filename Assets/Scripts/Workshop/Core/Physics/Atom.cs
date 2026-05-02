using System;
using System.Collections.Generic;

namespace Assets.Scripts.Workshop.Core.Physics
{

    public sealed class Atom
    {
        public string Name { get; }
        public string Symbol { get; }
        public int AtomicNumber { get; }
        public float AtomicWeight { get; }   // average atomic mass (u)
        public IReadOnlyList<string> ElectronConfiguration { get; } = new List<string>();
        public int Neutrons { get; }
        public int Protons { get; }
        public int Electrons { get; }
        private string OxidationStates { get; set; } = "+2";
        private string StateAtSTP { get; set; } = "Solid";
        private string Category { get; set; } = "Alkaline Earth Metal";
        public Atom(string name, string symbol, int atomicNumber, float standardAtomicWeight,
                    List<string> electronConfiguration, int neutrons, int protons, int electrons, string oxidationStates, string stateAtSTP, string category)
        {
            if (string.IsNullOrWhiteSpace(name)) throw new ArgumentException("Atom name is required.");
            if (string.IsNullOrWhiteSpace(symbol)) throw new ArgumentException("Atomic symbol is required.");
            if (atomicNumber <= 0) throw new ArgumentOutOfRangeException(nameof(atomicNumber));
            if (protons != atomicNumber) throw new ArgumentException("Protons must equal atomic number (Z).");

            Name = name.Trim();
            Symbol = symbol.Trim();
            AtomicNumber = atomicNumber;
            AtomicWeight = standardAtomicWeight;
            ElectronConfiguration = (electronConfiguration ?? new List<string>()).AsReadOnly();
            Neutrons = neutrons;
            Protons = protons;
            Electrons = electrons;
            if(oxidationStates != null && oxidationStates != string.Empty)
                OxidationStates = oxidationStates;
            if(stateAtSTP != null && stateAtSTP != string.Empty)
                StateAtSTP = stateAtSTP;
            if(category != null && category != string.Empty)
                Category = category;
        }

        public override string ToString() => $"{Name} ({Symbol}), Z={AtomicNumber}, Ar={AtomicWeight}";

    }
}