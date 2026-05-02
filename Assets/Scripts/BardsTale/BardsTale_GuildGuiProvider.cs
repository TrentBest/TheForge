using System;
using UnityEngine;
using UnityEngine.UIElements;
using Workshop.UI_And_Tools.Forge.Builders.GuiBuilders;

namespace Workshop.BardsTale
{
    public class BardsTale_GuildGuiProvider : IGuiProvider
    {
        public string Title => "ADVENTURER'S GUILD";

        private readonly BardsTaleExperienceContext _ctx;
        private GuiContext _lastCtx;
        public BardsTale_GuildGuiProvider() { }
        public BardsTale_GuildGuiProvider(BardsTaleExperienceContext ctx)
        {
            _ctx = ctx;
        }

        public VisualElement CreateGui(GuiContext ctx)
        {
            _lastCtx = ctx;

            var crud = new CRUD_Builder<GURPS3eCharacterData>(
                "Tavern Roster",
                () => _ctx.ActiveParty.Characters, // DataSource

                // Safely grab the display name
                (charData) => string.IsNullOrEmpty(charData.CharacterName) ? "Unknown Adventurer" : charData.CharacterName,

                // THE UPGRADE: Replaced the raw Label placeholder with our automated Reflective Editor!
                (charData) => new ReflectiveGuiBuilder<GURPS3eCharacterData>(charData)
                                .WithTitle($"Editing: {charData.CharacterName}")
                                .Build(),

                // OnAdd Action (Fixed from OnSave)
                (charData) => {
                    if (!_ctx.ActiveParty.Characters.Contains(charData))
                    {
                        if (string.IsNullOrEmpty(charData.CharacterName)) charData.CharacterName = "New Recruit";
                        _ctx.ActiveParty.Characters.Add(charData);
                    }
                },

                // OnDelete Action
                (charData) => _ctx.ActiveParty.Characters.Remove(charData)
            );

            // Optional: Give the Bards Tale roster some distinct tavern styling
            crud.Style.ListWidth = 160f;
            crud.Style.AccentColor = new Color(0.8f, 0.5f, 0.2f); // Tavern Gold

            return crud.CreateGui(ctx);
        }

        public Action<VisualElement> GetGuiBuilder() => (root) => root.Add(CreateGui(new GuiContext()));

        // Safely stubbed serialization methods
        public void FromUIDocument(string assetPath) { }
        public void ToUIDocument(string assetPath) { }
    }
}