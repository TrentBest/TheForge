using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;
using Workshop.Core.Memory;
using Workshop.UI_And_Tools.Forge.Builders;
using Workshop.UI_And_Tools.Forge.Builders.GuiBuilders;

public class BracketsGuiBuilder : IForgeBuilder, IGuiProvider
{
    public string Title => "LEAGUE ARCHITECT";
    public string ToolName => "Tournament_Architect";

    private CRUD_Builder<TournamentLeague> _leagueManager;
    private bool _isManagementMode = true;

    public BracketsGuiBuilder()
    {
        InitializeLeagueArchitect();
    }

    private void InitializeLeagueArchitect()
    {
        _leagueManager = new CRUD_Builder<TournamentLeague>(
            "PRO LEAGUES",
            () => GetAllLeagues(),
            l => l.LeagueName,
            BuildLeagueEditor,
            l => SaveLeague(l),
            l => DeleteLeague(l),
            l => $"{l.TournamentIds.Count} Tournaments Registered"
        );
    }

    private VisualElement BuildLeagueEditor(TournamentLeague league)
    {
        var root = new GraphicalUserInterfaceBuilder("LeagueEditor")
            .WithPadding(20);

        // SPICE: Visual Branding
        if (!string.IsNullOrEmpty(league.BannerImagePath))
        {
            root.AddImage(league.BannerImagePath, 400, 150);
        }

        root.AddHeader("LEAGUE IDENTITY", Color.yellow)
            .AddStringData("League Name", league.LeagueName, v => league.LeagueName = v)
            .AddStringData("Banner Path", league.BannerImagePath, v => league.BannerImagePath = v)
            .AddSeparator(Color.gray, 1)

            // BETTING OFFRAMP CONTROLS
            .AddHeader("IN-WORLD ECONOMICS", Color.green)
            .AddFloatData("Initial Liquidity", league.LeagueCurrencyPool, v => league.LeagueCurrencyPool = v)

            .AddHeader("TOURNAMENTS IN LEAGUE")
            .AddButton("MANAGE BRACKETS", () => OpenBracketEditor(league));

        return root.Build();
    }

    private void OpenBracketEditor(TournamentLeague league)
    {
        // This would transition to a CRUD for BracketDefinition 
        // that filters by league.TournamentIds
        Debug.Log($"[Architect] Opening Bracket Editor for {league.LeagueName}");
    }

    // --- Persistence via DataWarehouse ---
    private IEnumerable<TournamentLeague> GetAllLeagues()
    {
        if (DataWarehouse.Default.TryGetAsset<List<TournamentLeague>>("FORGED_LEAGUES", out var list))
            return list;
        return new List<TournamentLeague>();
    }

    private void SaveLeague(TournamentLeague league)
    {
        if (!DataWarehouse.Default.TryGetAsset<List<TournamentLeague>>("FORGED_LEAGUES", out var list))
        {
            list = new List<TournamentLeague>();
            DataWarehouse.Default.RegisterAsset("FORGED_LEAGUES", list);
        }
        if (!list.Contains(league)) list.Add(league);
    }

    /// <summary>
    /// Fixes error: The name 'DeleteLeague' does not exist in the current context
    /// </summary>
    private void DeleteLeague(TournamentLeague league)
    {
        if (DataWarehouse.Default.TryGetAsset<List<TournamentLeague>>("FORGED_LEAGUES", out var list))
        {
            if (list.Contains(league))
            {
                Debug.Log($"[Architect] Purging League: {league.LeagueName}");
                list.Remove(league);
            }
        }
    }

    // --- IForgeBuilder & IGuiProvider boilerplate ---
    public Action<VisualElement> GetGuiBuilder() => root => root.Add(CreateGui(new GuiContext()));
    public VisualElement CreateGui(GuiContext ctx) => _leagueManager.CreateGui(ctx);
    public Type GetProductType() => typeof(TournamentLeague);
    public object Build() => null;
    public IGuiProvider GetGuiProvider() => this;

    /// <summary>
    /// Fixes error: 'BracketsGuiBuilder' does not implement interface member 'IGuiProvider.ToUIDocument(string)'
    /// </summary>
    public void ToUIDocument(string path) { }
    public void FromUIDocument(string path) { }
}