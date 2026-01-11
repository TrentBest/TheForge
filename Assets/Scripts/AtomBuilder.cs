using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.UIElements;

namespace Assets.Scripts
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
                electrons
            );
        }

        public GraphicalUserInterfaceBuilder GetGuiBuilder()
        {

            var atomGui = new AtomBuilderGui(this);

            return new GraphicalUserInterfaceBuilder("Atom Builder GUI")
                .WithTitle("Atom Builder")
                .WithSize(480, 560)
                .WithPadding(12)                         // set-all padding
                .WithMargins(8)                           // set-all margin
                .WithBorderWidth(1)                      // set-all border width
                .WithBorderColor(new Color(0.25f, 0.25f, 0.25f, 1f))
                .WithBackgroundColor(new Color(0.12f, 0.12f, 0.12f, 1f))
                .WithBorderRadius(6f)                    // uniform corner radius
                .WithFlexLayout(FlexDirection.Column, Justify.FlexStart, Align.Stretch)
                .WithScrollable(true, ScrollViewMode.Vertical)
                .AddChild(atomGui)                       // inject child builder GUI
                .WithFooter("Use Apply to push values, then Build to preview.");

        }
    }

}
