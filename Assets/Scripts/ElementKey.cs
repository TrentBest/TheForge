namespace TheSingularityWorkshop
{
    public readonly struct ElementKey
    {
        public string Symbol { get; }
        public int? MassNumberA { get; } // null => applies to all isotopes of the symbol

        public ElementKey(string symbol, int? massNumberA = null)
        {
            Symbol = symbol ?? throw new System.ArgumentNullException(nameof(symbol));
            MassNumberA = massNumberA;
        }

        public static ElementKey Of(string symbol, int? massNumberA = null) => new ElementKey(symbol, massNumberA);

        public override string ToString() => MassNumberA.HasValue ? $"{Symbol}-{MassNumberA.Value}" : Symbol;

        public override bool Equals(object obj) => obj is ElementKey other && Symbol.Equals(other.Symbol, System.StringComparison.OrdinalIgnoreCase) && MassNumberA == other.MassNumberA;
        public override int GetHashCode() => (Symbol.ToUpperInvariant(), MassNumberA ?? -1).GetHashCode();
    }


}
