using System;
using System.Collections.Generic;
using System.Text;

namespace Assets.Scripts
{
    class PeriodicTableOfElementsBuilder
    {
        List<AtomBuilder> atoms = new List<AtomBuilder>();
        public PeriodicTableOfElementsBuilder()
        {

        }

        public PeriodicTableOfElementsBuilder WithAtom(AtomBuilder atomBuilder)
        {
            atoms.Add(atomBuilder);
            return this;
        }

        public List<Atom> Build()
        {
            List<Atom> builtAtoms = new List<Atom>();
            foreach (var atomBuilder in atoms)
            {
                builtAtoms.Add(atomBuilder.Build());
            }
            return builtAtoms;
        }

        public PeriodicTableOfElementsBuilder WithKnownElements()
        {

            return this;
        }
    }


}
