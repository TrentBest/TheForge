using System;
using System.Collections.Generic;
using UnityEngine;
using TheSingularityWorkshop.FSM_API;

namespace TheSingularityWorkshop.Armada2525.Economy
{
    // --- THE MACRO ECONOMY FSM ---
    public class GalacticStockExchange : MonoBehaviour, IStateContext
    {
        public bool IsValid { get; set; } = true;
        public string Name { get; set; } = "Core Worlds Exchange";

        public List<ContractFirm> PersistentFirms = new List<ContractFirm>();

        // Macro stats aggregated during FSM updates
        public double TotalMarketCap = 0;
        public float GalacticGDP_Trend = 0f;

        public void Initialize()
        {
            if ( !FSM_API.FSM_API.Interaction.Exists("GalacticEconomyModel", "MacroEconomyTick"))
            {
                FSM_API.FSM_API.Create.CreateFiniteStateMachine("GalacticEconomyModel", -1, "MacroEconomyTick")
                    // The Economy breathes in phases. 
                    .State("BullMarket", OnEnterPhase, OnUpdateBullMarket, null)
                    .State("BearMarket", OnEnterPhase, OnUpdateBearMarket, null)
                    .State("Recession", OnEnterPhase, OnUpdateRecession, null)

                    // Transitions based on macroeconomic thresholds
                    .Transition("BullMarket", "BearMarket", ctx => ((GalacticStockExchange)ctx).GalacticGDP_Trend < 0)
                    .Transition("BearMarket", "Recession", ctx => ((GalacticStockExchange)ctx).GalacticGDP_Trend < -5000000)
                    .Transition("Recession", "BullMarket", ctx => ((GalacticStockExchange)ctx).GalacticGDP_Trend > 100000)
                    .Transition("BearMarket", "BullMarket", ctx => ((GalacticStockExchange)ctx).GalacticGDP_Trend > 500000)
                    .BuildDefinition();
            }
            FSM_API.FSM_API.Create.CreateInstance("GalacticEconomyModel", this, "MacroEconomyTick");
        }

        private static void OnEnterPhase(IStateContext context)
        {
            var exchange = (GalacticStockExchange)context;
            Debug.Log($"[Stock Exchange] Market phase shifted. Adjusting global investor confidence.");
        }

        private static void OnUpdateBullMarket(IStateContext context)
        {
            var exchange = (GalacticStockExchange)context;
            exchange.TotalMarketCap = 0; // Recalculate continually

            // In a bull market, capital flows freely. We apply a positive distribution to firm valuations.
            foreach (var firm in exchange.PersistentFirms)
            {
                if (firm.IsValid)
                {
                    // Economics 101: High demand, price increases.
                    firm.CurrentStockPrice *= 1.0001f; // Micro-tick growth
                    exchange.TotalMarketCap += firm.MarketCap;
                }
            }
            exchange.GalacticGDP_Trend += 100f; // Naturally climbs unless an external event hits it
        }

        private static void OnUpdateBearMarket(IStateContext context)
        {
            var exchange = (GalacticStockExchange)context;
            exchange.TotalMarketCap = 0;

            foreach (var firm in exchange.PersistentFirms)
            {
                if (firm.IsValid)
                {
                    // Investors are pulling out
                    firm.CurrentStockPrice *= 0.9998f;
                    exchange.TotalMarketCap += firm.MarketCap;
                }
            }
        }

        private static void OnUpdateRecession(IStateContext context)
        {
            // Severe contraction. Firms not fulfilling contracts lose liquid capital rapidly.
        }
    }


 
   
}