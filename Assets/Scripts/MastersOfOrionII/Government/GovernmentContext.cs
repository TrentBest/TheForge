using Assets.Scripts.MastersOfOrionII.Economy;
using System.Collections.Generic;
using TheSingularityWorkshop.FSM_API;


namespace Assets.Scripts.MastersOfOrionII.Government
{
    public class GovernmentContext : IStateContext
    {
        public bool IsValid { get; set; } = true;
        public string Name { get; set; } = "The Imperial Throne";

        public double ImperialTreasury { get; set; } = 10000000000; // 10 Billion starting credits

        public List<CabinetMember> ActiveCabinet { get; set; } = new List<CabinetMember>();
        public List<ImperialEdict> ActiveEdicts { get; set; } = new List<ImperialEdict>();

        // Link to the economy we built!
        public GalacticStockExchange Exchange { get; set; }
    }
}