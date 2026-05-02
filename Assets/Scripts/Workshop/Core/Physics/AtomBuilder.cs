using Assets.Scripts.Workshop.UI_And_Tools.Forge.Builders.GuiBuilders.Physics;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;
using Workshop.UI_And_Tools.Forge.Builders.GuiBuilders;

namespace Assets.Scripts.Workshop.Core.Physics
{
    public class AtomBuilder
    {
        private string name = "Hydrogen";
        private int atomicNumber = 1;
        private float atomicWeight = 1.008f;
        private List<string> electronConfiguration = new List<string>() { "1s1" };
        private int neutrons = 0;
        private int protons = 1;
        private int electrons = 1;
        private string symbol = "H";
        private string oxidationStates = "+2";
        private string stateAtSTP = "Solid";
        private string category = "Alkaline Earth Metal";

        public AtomBuilder()
        {
            name = "Hydrogen";
            atomicNumber = 1;
            symbol = "H";
        }

        public AtomBuilder(string name, int atomicNumber, string symbol)
        {

            this.name = string.IsNullOrWhiteSpace(name) ? "Hydrogen" : name;
            this.atomicNumber = atomicNumber <= 0 ? 1 : atomicNumber;
            this.symbol = string.IsNullOrWhiteSpace(symbol) ? "H" : symbol;
        }

        public AtomBuilder WithAtomicWeight(float atomicWeight)
        {
            this.atomicWeight = atomicWeight;
            return this;
        }

        public AtomBuilder WithName(string name)
        {
            this.name = name;
            return this;
        }

        public AtomBuilder WithSymbol(string symbol)
        {
            this.symbol = symbol;
            return this;
        }

        public AtomBuilder WithElectronConfiguration(List<string> electronConfiguration)
        {
            this.electronConfiguration = electronConfiguration;
            return this;
        }

        public AtomBuilder WithNeutrons(int neutrons)
        {
            this.neutrons = neutrons;
            return this;
        }

        public AtomBuilder WithProtons(int protons)
        {
            this.protons = protons;
            return this;
        }

        public AtomBuilder WithElectrons(int electrons)
        {
            this.electrons = electrons;
            return this;
        }

        public AtomBuilder WithOxidationStates(string states) { this.oxidationStates = states; return this; }
        public AtomBuilder WithStateAtSTP(string state) { this.stateAtSTP = state; return this; }
        public AtomBuilder WithCategory(string category) { this.category = category; return this; }

        public Atom Build()
        {
            return new Atom(
                name,
                symbol,
                atomicNumber,
                atomicWeight,
                electronConfiguration,
                neutrons,
                protons,
                electrons,
                oxidationStates,
                                stateAtSTP,
                                category
            );
        }

        public GraphicalUserInterfaceBuilder GetGuiBuilder()
        {

            var atomGui = new AtomBuilderGui(this);

            return new GraphicalUserInterfaceBuilder($"{symbol}_Panel")
                //.WithTitle("Atom Builder")
                .WithSize(480, 560)
                .WithPadding(12)                         // set-all padding
                .WithMargins(8)                           // set-all margin
                .WithBorderWidth(1)                      // set-all border width
                .WithBorderColor(new Color(0.25f, 0.25f, 0.25f, 1f))
                .WithBackgroundColor(new Color(0.12f, 0.12f, 0.12f, 1f))
                .WithBorderRadius(6f)                    // uniform corner radius
                .WithFlexWrap(Wrap.Wrap)               // enable flex wrapping
                .WithScrollable(true, ScrollViewMode.Vertical)
                .AddChild(atomGui)                       // inject child builder GUI
                .WithFooter("Use Apply to push values, then Build to preview.");

        }
    }

}
