using TheSingularityWorkshop.Forge.Builders.GuiBuilders;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.UIElements;

namespace TheSingularityWorkshop
{
    public class PeriodicTableOfElementsBuilder
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
            WithAtom(new AtomBuilder("Hydrogen", 1, "H").WithAtomicWeight(1.008f));
            WithAtom(new AtomBuilder("Helium", 2, "He").WithAtomicWeight(4.0026f));
            WithAtom(new AtomBuilder("Lithium", 3, "Li").WithAtomicWeight(6.94f));
            WithAtom(new AtomBuilder("Beryllium", 4, "Be").WithAtomicWeight(9.0122f));
            WithAtom(new AtomBuilder("Boron", 5, "B").WithAtomicWeight(10.81f));
            WithAtom(new AtomBuilder("Carbon", 6, "C").WithAtomicWeight(12.011f));
            WithAtom(new AtomBuilder("Nitrogen", 7, "N").WithAtomicWeight(14.007f));
            WithAtom(new AtomBuilder("Oxygen", 8, "O").WithAtomicWeight(15.999f));
            WithAtom(new AtomBuilder("Fluorine", 9, "F").WithAtomicWeight(18.998f));
            WithAtom(new AtomBuilder("Neon", 10, "Ne").WithAtomicWeight(20.180f));
            WithAtom(new AtomBuilder("Sodium", 11, "Na").WithAtomicWeight(22.990f));
            WithAtom(new AtomBuilder("Magnesium", 12, "Mg").WithAtomicWeight(24.305f));
            WithAtom(new AtomBuilder("Aluminum", 13, "Al").WithAtomicWeight(26.982f));
            WithAtom(new AtomBuilder("Silicon", 14, "Si").WithAtomicWeight(28.085f));
            WithAtom(new AtomBuilder("Phosphorus", 15, "P").WithAtomicWeight(30.974f));
            WithAtom(new AtomBuilder("Sulfur", 16, "S").WithAtomicWeight(32.06f));
            WithAtom(new AtomBuilder("Chlorine", 17, "Cl").WithAtomicWeight(35.45f));
            WithAtom(new AtomBuilder("Argon", 18, "Ar").WithAtomicWeight(39.948f));
            WithAtom(new AtomBuilder("Potassium", 19, "K").WithAtomicWeight(39.098f));
            WithAtom(new AtomBuilder("Calcium", 20, "Ca").WithAtomicWeight(40.078f));
            WithAtom(new AtomBuilder("Scandium", 21, "Sc").WithAtomicWeight(44.956f));
            WithAtom(new AtomBuilder("Titanium", 22, "Ti").WithAtomicWeight(47.867f));
            WithAtom(new AtomBuilder("Vanadium", 23, "V").WithAtomicWeight(50.942f));
            WithAtom(new AtomBuilder("Chromium", 24, "Cr").WithAtomicWeight(51.996f));
            WithAtom(new AtomBuilder("Manganese", 25, "Mn").WithAtomicWeight(54.938f));
            WithAtom(new AtomBuilder("Iron", 26, "Fe").WithAtomicWeight(55.845f));
            WithAtom(new AtomBuilder("Cobalt", 27, "Co").WithAtomicWeight(58.933f));
            WithAtom(new AtomBuilder("Nickel", 28, "Ni").WithAtomicWeight(58.693f));
            WithAtom(new AtomBuilder("Copper", 29, "Cu").WithAtomicWeight(63.546f));
            WithAtom(new AtomBuilder("Zinc", 30, "Zn").WithAtomicWeight(65.38f));
            WithAtom(new AtomBuilder("Gallium", 31, "Ga").WithAtomicWeight(69.723f));
            WithAtom(new AtomBuilder("Germanium", 32, "Ge").WithAtomicWeight(72.63f));
            WithAtom(new AtomBuilder("Arsenic", 33, "As").WithAtomicWeight(74.922f));
            WithAtom(new AtomBuilder("Selenium", 34, "Se").WithAtomicWeight(78.971f));
            WithAtom(new AtomBuilder("Bromine", 35, "Br").WithAtomicWeight(79.904f));
            WithAtom(new AtomBuilder("Krypton", 36, "Kr").WithAtomicWeight(83.798f));
            WithAtom(new AtomBuilder("Rubidium", 37, "Rb").WithAtomicWeight(85.468f));
            WithAtom(new AtomBuilder("Strontium", 38, "Sr").WithAtomicWeight(87.62f));
            WithAtom(new AtomBuilder("Yttrium", 39, "Y").WithAtomicWeight(88.906f));
            WithAtom(new AtomBuilder("Zirconium", 40, "Zr").WithAtomicWeight(91.224f));
            WithAtom(new AtomBuilder("Niobium", 41, "Nb").WithAtomicWeight(92.906f));
            WithAtom(new AtomBuilder("Molybdenum", 42, "Mo").WithAtomicWeight(95.95f));
            WithAtom(new AtomBuilder("Technetium", 43, "Tc").WithAtomicWeight(98f)); // Mass number
            WithAtom(new AtomBuilder("Ruthenium", 44, "Ru").WithAtomicWeight(101.07f));
            WithAtom(new AtomBuilder("Rhodium", 45, "Rh").WithAtomicWeight(102.91f));
            WithAtom(new AtomBuilder("Palladium", 46, "Pd").WithAtomicWeight(106.42f));
            WithAtom(new AtomBuilder("Silver", 47, "Ag").WithAtomicWeight(107.87f));
            WithAtom(new AtomBuilder("Cadmium", 48, "Cd").WithAtomicWeight(112.41f));
            WithAtom(new AtomBuilder("Indium", 49, "In").WithAtomicWeight(114.82f));
            WithAtom(new AtomBuilder("Tin", 50, "Sn").WithAtomicWeight(118.71f));
            WithAtom(new AtomBuilder("Antimony", 51, "Sb").WithAtomicWeight(121.76f));
            WithAtom(new AtomBuilder("Tellurium", 52, "Te").WithAtomicWeight(127.6f));
            WithAtom(new AtomBuilder("Iodine", 53, "I").WithAtomicWeight(126.9f));
            WithAtom(new AtomBuilder("Xenon", 54, "Xe").WithAtomicWeight(131.29f));
            WithAtom(new AtomBuilder("Cesium", 55, "Cs").WithAtomicWeight(132.91f));
            WithAtom(new AtomBuilder("Barium", 56, "Ba").WithAtomicWeight(137.33f));
            WithAtom(new AtomBuilder("Lanthanum", 57, "La").WithAtomicWeight(138.91f));
            WithAtom(new AtomBuilder("Cerium", 58, "Ce").WithAtomicWeight(140.12f));
            WithAtom(new AtomBuilder("Praseodymium", 59, "Pr").WithAtomicWeight(140.91f));
            WithAtom(new AtomBuilder("Neodymium", 60, "Nd").WithAtomicWeight(144.24f));
            WithAtom(new AtomBuilder("Promethium", 61, "Pm").WithAtomicWeight(145f)); // Mass number
            WithAtom(new AtomBuilder("Samarium", 62, "Sm").WithAtomicWeight(150.36f));
            WithAtom(new AtomBuilder("Europium", 63, "Eu").WithAtomicWeight(151.96f));
            WithAtom(new AtomBuilder("Gadolinium", 64, "Gd").WithAtomicWeight(157.25f));
            WithAtom(new AtomBuilder("Terbium", 65, "Tb").WithAtomicWeight(158.93f));
            WithAtom(new AtomBuilder("Dysprosium", 66, "Dy").WithAtomicWeight(162.5f));
            WithAtom(new AtomBuilder("Holmium", 67, "Ho").WithAtomicWeight(164.93f));
            WithAtom(new AtomBuilder("Erbium", 68, "Er").WithAtomicWeight(167.26f));
            WithAtom(new AtomBuilder("Thulium", 69, "Tm").WithAtomicWeight(168.93f));
            WithAtom(new AtomBuilder("Ytterbium", 70, "Yb").WithAtomicWeight(173.05f));
            WithAtom(new AtomBuilder("Lutetium", 71, "Lu").WithAtomicWeight(174.97f));
            WithAtom(new AtomBuilder("Hafnium", 72, "Hf").WithAtomicWeight(178.49f));
            WithAtom(new AtomBuilder("Tantalum", 73, "Ta").WithAtomicWeight(180.95f));
            WithAtom(new AtomBuilder("Tungsten", 74, "W").WithAtomicWeight(183.84f));
            WithAtom(new AtomBuilder("Rhenium", 75, "Re").WithAtomicWeight(186.21f));
            WithAtom(new AtomBuilder("Osmium", 76, "Os").WithAtomicWeight(190.23f));
            WithAtom(new AtomBuilder("Iridium", 77, "Ir").WithAtomicWeight(192.22f));
            WithAtom(new AtomBuilder("Platinum", 78, "Pt").WithAtomicWeight(195.08f));
            WithAtom(new AtomBuilder("Gold", 79, "Au").WithAtomicWeight(196.97f));
            WithAtom(new AtomBuilder("Mercury", 80, "Hg").WithAtomicWeight(200.59f));
            WithAtom(new AtomBuilder("Thallium", 81, "Tl").WithAtomicWeight(204.38f));
            WithAtom(new AtomBuilder("Lead", 82, "Pb").WithAtomicWeight(207.2f));
            WithAtom(new AtomBuilder("Bismuth", 83, "Bi").WithAtomicWeight(208.98f));
            WithAtom(new AtomBuilder("Polonium", 84, "Po").WithAtomicWeight(209f)); // Mass number
            WithAtom(new AtomBuilder("Astatine", 85, "At").WithAtomicWeight(210f)); // Mass number
            WithAtom(new AtomBuilder("Radon", 86, "Rn").WithAtomicWeight(222f)); // Mass number
            WithAtom(new AtomBuilder("Francium", 87, "Fr").WithAtomicWeight(223f)); // Mass number
            WithAtom(new AtomBuilder("Radium", 88, "Ra").WithAtomicWeight(226f)); // Mass number
            WithAtom(new AtomBuilder("Actinium", 89, "Ac").WithAtomicWeight(227f)); // Mass number
            WithAtom(new AtomBuilder("Thorium", 90, "Th").WithAtomicWeight(232.04f));
            WithAtom(new AtomBuilder("Protactinium", 91, "Pa").WithAtomicWeight(231.04f));
            WithAtom(new AtomBuilder("Uranium", 92, "U").WithAtomicWeight(238.03f));
            WithAtom(new AtomBuilder("Neptunium", 93, "Np").WithAtomicWeight(237f)); // Mass number
            WithAtom(new AtomBuilder("Plutonium", 94, "Pu").WithAtomicWeight(244f)); // Mass number
            WithAtom(new AtomBuilder("Americium", 95, "Am").WithAtomicWeight(243f)); // Mass number
            WithAtom(new AtomBuilder("Curium", 96, "Cm").WithAtomicWeight(247f)); // Mass number
            WithAtom(new AtomBuilder("Berkelium", 97, "Bk").WithAtomicWeight(247f)); // Mass number
            WithAtom(new AtomBuilder("Californium", 98, "Cf").WithAtomicWeight(251f)); // Mass number
            WithAtom(new AtomBuilder("Einsteinium", 99, "Es").WithAtomicWeight(252f)); // Mass number
            WithAtom(new AtomBuilder("Fermium", 100, "Fm").WithAtomicWeight(257f)); // Mass number
            WithAtom(new AtomBuilder("Mendelevium", 101, "Md").WithAtomicWeight(258f)); // Mass number
            WithAtom(new AtomBuilder("Nobelium", 102, "No").WithAtomicWeight(259f)); // Mass number
            WithAtom(new AtomBuilder("Lawrencium", 103, "Lr").WithAtomicWeight(262f)); // Mass number
            WithAtom(new AtomBuilder("Rutherfordium", 104, "Rf").WithAtomicWeight(267f)); // Mass number
            WithAtom(new AtomBuilder("Dubnium", 105, "Db").WithAtomicWeight(270f)); // Mass number
            WithAtom(new AtomBuilder("Seaborgium", 106, "Sg").WithAtomicWeight(271f)); // Mass number
            WithAtom(new AtomBuilder("Bohrium", 107, "Bh").WithAtomicWeight(270f)); // Mass number
            WithAtom(new AtomBuilder("Hassium", 108, "Hs").WithAtomicWeight(277f)); // Mass number
            WithAtom(new AtomBuilder("Meitnerium", 109, "Mt").WithAtomicWeight(278f)); // Mass number
            WithAtom(new AtomBuilder("Darmstadtium", 110, "Ds").WithAtomicWeight(281f)); // Mass number
            WithAtom(new AtomBuilder("Roentgenium", 111, "Rg").WithAtomicWeight(282f)); // Mass number
            WithAtom(new AtomBuilder("Copernicium", 112, "Cn").WithAtomicWeight(285f)); // Mass number
            WithAtom(new AtomBuilder("Nihonium", 113, "Nh").WithAtomicWeight(286f)); // Mass number
            WithAtom(new AtomBuilder("Flerovium", 114, "Fl").WithAtomicWeight(289f)); // Mass number
            WithAtom(new AtomBuilder("Moscovium", 115, "Mc").WithAtomicWeight(290f)); // Mass number
            WithAtom(new AtomBuilder("Livermorium", 116, "Lv").WithAtomicWeight(293f)); // Mass number
            WithAtom(new AtomBuilder("Tennessine", 117, "Ts").WithAtomicWeight(294f)); // Mass number
            WithAtom(new AtomBuilder("Oganesson", 118, "Og").WithAtomicWeight(294f)); // Mass number



            return this;
        }

        public GraphicalUserInterfaceBuilder GetGuiBuilder()
        {
            // We create a new top-level GUI for the Table
            var tableGui = new GraphicalUserInterfaceBuilder("Periodic Table")
                .WithTitle("Periodic Table of Elements")
                .WithBackgroundColor(new Color(0.12f, 0.12f, 0.12f, 1f))
                .WithPadding(12)
                .WithEditorMode(false)
                .WithFlexLayout(FlexDirection.Row, Justify.FlexStart, Align.FlexStart)
                .WithScrollable(true, ScrollViewMode.Vertical);

            foreach (var atomBuilder in atoms)
            {
                // Each atom gets a "Mini-Panel" inside the table
                //tableGui.AddChild(ctx => CreateMiniElementView(atomBuilder, ctx));
                tableGui.AddChild(atomBuilder.GetGuiBuilder().WithEditorMode(false));
            }

            return tableGui;
        }

        private VisualElement CreateMiniElementView(AtomBuilder atomBuilder, GuiContext ctx)
        {
            // Build a temporary atom to get current data for the label
            var atom = atomBuilder.Build();

            // Use the GUI builder to create a small square "Cell" for the element
            return new GraphicalUserInterfaceBuilder($"{atom.Symbol}_Cell")
                .WithSize(65, 80)
                .WithMargins(2)
                .WithBorderWidth(1)
                .WithBorderColor(new Color(0.4f, 0.4f, 0.4f))
                .WithBackgroundColor(new Color(0.2f, 0.2f, 0.2f))
                .WithFlexLayout(FlexDirection.Column, Justify.Center, Align.Center)
                .AddChild(new Label(atom.AtomicNumber.ToString()) { style = { fontSize = 10, alignSelf = Align.FlexStart } })
                .AddChild(new Label(atom.Symbol) { style = { fontSize = 20, unityFontStyleAndWeight = FontStyle.Bold } })
                .AddChild(new Label(atom.Name) { style = { fontSize = 8 } })
                .CreateGui(ctx);
        }


    }


}
