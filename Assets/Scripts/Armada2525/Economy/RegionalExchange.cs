using TheSingularityWorkshop.FSM_API;

namespace TheSingularityWorkshop.Armada2525.Economy
{
    // --- 2. THE REGIONAL EXCHANGE (The Marketplace) ---
    public class RegionalExchange : IStateContext
    {
        public bool IsValid { get; set; } = true;
        public string Name { get; set; } = "New Exchange";
        public string CentralHubWorld { get; set; } = "Core Sector";

        public float TradingTaxRate { get; set; } = 0.02f; // 2% tax on transactions
        public double TotalMarketCap { get; set; } = 0;
    }
}