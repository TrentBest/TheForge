using System;
using System.Collections.Generic;

[Serializable]
public class TournamentLeague
{
    public string LeagueName = "New Pro League";
    public string Description = "The elite tier of FSM competition.";
    public string BannerImagePath; // Path for ImageGuiBuilder
    public List<string> TournamentIds = new(); // Linked tournament definitions
    public float LeagueCurrencyPool = 10000f; // In-world betting liquidity
}

[Serializable]
public class BracketDefinition
{
    public string DisplayName = "Fight Night: Heavies";
    public string PosterPath;
    public BracketSkin ActiveSkin = BracketSkin.Digital;
    public List<BracketSeed> Seeds = new();

    // Simulated Betting Offramp
    public bool BettingEnabled = true;
    public float MinimumBet = 10f;
    public float TotalPool = 0f;
}