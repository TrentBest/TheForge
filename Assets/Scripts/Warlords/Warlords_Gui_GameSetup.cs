using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;
using Workshop.UI_And_Tools.Forge.Builders.GuiBuilders;

namespace Assets.Scripts.Warlords
{
    public class Warlords_Gui_GameSetup : IGuiProvider
    {
        public string Title => "WARLORDS: CAMPAIGN SETUP";
        private GuiContext _lastCtx;

        private VisualElement _loreCardContainer;
        private WarlordFactionData _activeFaction;
        private IGuiRouter _router;

        // Tracks whether each faction is set to Human, AI, or Closed
        private Dictionary<string, string> _factionControl = new Dictionary<string, string>();
        // Tracks the AI difficulty for each faction
        private Dictionary<string, string> _factionDifficulty = new Dictionary<string, string>();

        [Serializable]
        public class WarlordFactionData
        {
            public string Name;
            public Color ThemeColor;
            public string Subtitle;
            public string Lore;
            public string Strengths;
            public Texture2D FactionRender;
        }

        private readonly List<WarlordFactionData> _factions = new List<WarlordFactionData>
        {
            new WarlordFactionData {
                Name = "Sirians", ThemeColor = Color.white, Subtitle = "Defenders of the White Citadel",
                Strengths = "Centralized Strongholds, Versatile Infantry, Access to Aerial Mounts",
                Lore = "The noble Sirians stand as the enduring shield of humanity against the encroaching darkness. Centered around the grand city of Siria, their knights are renowned for their unwavering valor.",
                FactionRender = UnityEngine.Resources.Load<Texture2D>("Images/Warlords/SiriansFactionArt")
            },
            new WarlordFactionData {
                Name = "Storm Giants", ThemeColor = Color.yellow, Subtitle = "Lords of the Cloud Peaks",
                Strengths = "Rugged Terrain Defense, Heavy Shock Troops, High-Altitude Beasts",
                Lore = "Towering above the petty squabbles of men and elves, the Storm Giants dwell in the craggy, thunder-lashed heights of the north. They are an ancient, proud race, slow to anger but devastating when roused.",
                FactionRender = UnityEngine.Resources.Load<Texture2D>("Images/Warlords/StormGiants")
            },
            new WarlordFactionData {
                Name = "Grey Dwarves", ThemeColor = new Color(1f, 0.5f, 0f), Subtitle = "Masters of the Deep Forge",
                Strengths = "Mountainous Chokepoints, Unyielding Shield Walls, Heavy Infantry",
                Lore = "Hewn from the living rock of Illuria's mountain ranges, the Grey Dwarves are as unyielding as the stone they mine. Driven by ancient grudges, they have forged a bitter, martial society.",
                FactionRender = UnityEngine.Resources.Load<Texture2D>("Images/Warlords/GreyDwarves")
            },
            new WarlordFactionData {
                Name = "Orcs of Kor", ThemeColor = new Color(0.8f, 0.1f, 0.1f), Subtitle = "The Crimson Horde",
                Strengths = "Rapid Mustering, Swarm Tactics, Desolate Terrain Control",
                Lore = "From the desolate wastelands and festering swamps, the Orcs of Kor pour forth in a never-ending tide of violence. Driven only by an innate bloodlust and the dark will of their shamans.",
                FactionRender = UnityEngine.Resources.Load<Texture2D>("Images/Warlords/OrcsofKor")
            },
            new WarlordFactionData {
                Name = "Elvallie", ThemeColor = new Color(0.1f, 0.6f, 0.2f), Subtitle = "Wardens of the Deep Wood",
                Strengths = "Dense Forest Protection, Elite Archery, Mythical Woodland Allies",
                Lore = "Deep within the ancient, whispering forests of Illuria reside the Elvallie. Aloof and immortal, they view the wars of shorter-lived races with sorrow and disdain.",
                FactionRender = UnityEngine.Resources.Load<Texture2D>("Images/Warlords/Elvelie")
            },
            new WarlordFactionData {
                Name = "Selentines", ThemeColor = new Color(0.1f, 0.2f, 0.6f), Subtitle = "The Azure Empire",
                Strengths = "Wealthy Coastal Cities, Naval Dominance, Disciplined Legions",
                Lore = "The Selentine Empire once stretched across the known world, bringing law and science to the untamed lands. Now a shadow of its former glory, the empire still commands strategic coastal regions.",
                FactionRender = UnityEngine.Resources.Load<Texture2D>("Images/Warlords/Selentines")
            },
            new WarlordFactionData {
                Name = "Horse Lords", ThemeColor = new Color(0.4f, 0.7f, 1.0f), Subtitle = "Nomads of the Endless Plains",
                Strengths = "Unrivaled Map Mobility, Swift Cavalry, Expansive Open Borders",
                Lore = "Born in the saddle and raised on the sweeping winds of the eastern steppes, the Horse Lords are a nomadic people of unparalleled mobility. They strike like lightning and vanish before a counterattack.",
                FactionRender = UnityEngine.Resources.Load<Texture2D>("Images/Warlords/HorseLords")
            },
            new WarlordFactionData {
                Name = "Lord Bane", ThemeColor = new Color(0.5f, 0f, 0.8f), Subtitle = "Sovereign of the Black Wastes",
                Strengths = "Monstrous City Roster, Proximity to Dark Allies, Psychological Terror",
                Lore = "An ancient and malevolent entity, Lord Bane rules from his dark citadel, seeking to plunge all of Illuria into eternal night. His armies are a nightmare made flesh—undead thralls, summoned demons, and twisted beasts.",
                FactionRender = UnityEngine.Resources.Load<Texture2D>("Images/Warlords/LordBane")
            }
        };

        public Warlords_Gui_GameSetup() { }

        public Warlords_Gui_GameSetup(IGuiRouter r)
        {
            _router = r;
        }

        public VisualElement CreateGui(GuiContext ctx)
        {
            _lastCtx = ctx;
            _activeFaction = _factions[0];
            _factionControl.Clear();
            _factionDifficulty.Clear();

            var rootBuilder = new GraphicalUserInterfaceBuilder("Warlords_GameSetup_Root")
                .WithFlexLayout(FlexDirection.Row, Justify.FlexStart, Align.Stretch)
                .WithPercentSize(100, 100)
                .WithBackgroundColor(new Color(0.05f, 0.05f, 0.08f));

            // --- 1. LEFT PANEL (ROSTER) ---
            var leftPanelBuilder = new GraphicalUserInterfaceBuilder("LeftPanel")
                .WithBackgroundColor(new Color(0.08f, 0.08f, 0.1f))
                .WithFlexLayout(FlexDirection.Column, Justify.FlexStart, Align.Stretch)
                .OnBuild(ve =>
                {
                    ve.style.width = 500;
                    ve.style.paddingTop = 20; ve.style.paddingBottom = 20; ve.style.paddingLeft = 20; ve.style.paddingRight = 20;
                    ve.style.borderRightWidth = 2; ve.style.borderRightColor = new Color(0.2f, 0.2f, 0.2f);
                })
                .AddChild(new Label("BATTLE OF ILLURIA") { style = { fontSize = 28, color = new Color(0.8f, 0.6f, 0.2f), unityFontStyleAndWeight = FontStyle.Bold, marginBottom = 5 } })
                .AddChild(new Label("CONFIGURE FACTIONS") { style = { fontSize = 14, color = Color.gray, marginBottom = 20 } })

                // Table Header
                .AddChild(new GraphicalUserInterfaceBuilder("HeaderRow")
                    .WithFlexLayout(FlexDirection.Row, Justify.FlexStart, Align.Stretch)
                    .OnBuild(ve => { ve.style.borderBottomWidth = 1; ve.style.borderBottomColor = Color.gray; ve.style.paddingBottom = 5; ve.style.marginBottom = 10; })
                    .AddChild(new Label("FACTION") { style = { width = 200, color = Color.gray, unityFontStyleAndWeight = FontStyle.Bold } })
                    .AddChild(new Label("CONTROL") { style = { width = 120, color = Color.gray, unityFontStyleAndWeight = FontStyle.Bold } })
                    .AddChild(new Label("DIFFICULTY") { style = { width = 120, color = Color.gray, unityFontStyleAndWeight = FontStyle.Bold } })
                    .Build());

            // Build slots
            foreach (var faction in _factions)
            {
                leftPanelBuilder.AddChild(CreateFactionSlot(faction));
            }

            // Command Buttons using ForgeButtonBuilder
            leftPanelBuilder.AddChild(new GraphicalUserInterfaceBuilder("ButtonsRow")
                .WithFlexLayout(FlexDirection.Row, Justify.SpaceBetween, Align.Center)
                .OnBuild(ve => { ve.style.marginTop = Length.Auto(); })

                // RETREAT BUTTON
                .AddChild(bCtx => new ForgeButtonBuilder("RETREAT", () => { _router?.NavigateTo("MainMenu"); })
                    .WithWidth(150)
                    .WithHeight(40)
                    .WithBackgroundColor(new Color(0.15f, 0.15f, 0.15f))
                    .WithTextColor(Color.white)
                    .WithFontStyle(FontStyle.Bold)
                    .CreateGui(bCtx))

                // MARCH TO WAR BUTTON
                .AddChild(bCtx =>
                {
                    var btn = new ForgeButtonBuilder("MARCH TO WAR", HandleMarchToWar)
                        .WithHeight(40)
                        .WithBackgroundColor(new Color(0.2f, 0.05f, 0.05f))
                        .WithTextColor(Color.white)
                        .WithFontStyle(FontStyle.Bold)
                        .WithMargin(0, 0, 0, 10)
                        .CreateGui(bCtx);
                    btn.style.flexGrow = 1;
                    return btn;
                })
                .Build());

            rootBuilder.AddChild(leftPanelBuilder.Build());

            // --- 2. RIGHT PANEL (LORE CARD MOUNT) ---
            rootBuilder.AddChild(guiCtx =>
            {
                _loreCardContainer = new GraphicalUserInterfaceBuilder("LoreCardContainer")
                    .WithFlexLayout(FlexDirection.Column, Justify.Center, Align.Center)
                    .OnBuild(ve => {
                        ve.style.flexGrow = 1;
                        ve.style.paddingTop = 40; ve.style.paddingBottom = 40; ve.style.paddingLeft = 40; ve.style.paddingRight = 40;
                    }).Build();

                RefreshLoreCard();
                return _loreCardContainer;
            });

            return rootBuilder.Build();
        }

        private VisualElement CreateFactionSlot(WarlordFactionData faction)
        {
            string defaultControl = faction.Name == "Sirians" ? "Human" : "AI";
            _factionControl[faction.Name] = defaultControl;
            _factionDifficulty[faction.Name] = "Baron";

            var nameLabel = new Label(faction.Name) { style = { color = Color.white, fontSize = 14, unityFontStyleAndWeight = FontStyle.Bold } };

            var colorSwatch = new VisualElement { style = { width = 16, height = 16, backgroundColor = faction.ThemeColor, marginRight = 10 } };
            colorSwatch.style.borderTopLeftRadius = 2; colorSwatch.style.borderTopRightRadius = 2; colorSwatch.style.borderBottomLeftRadius = 2; colorSwatch.style.borderBottomRightRadius = 2;
            colorSwatch.style.borderTopWidth = 1; colorSwatch.style.borderBottomWidth = 1; colorSwatch.style.borderLeftWidth = 1; colorSwatch.style.borderRightWidth = 1;
            colorSwatch.style.borderTopColor = Color.white; colorSwatch.style.borderBottomColor = Color.white; colorSwatch.style.borderLeftColor = Color.white; colorSwatch.style.borderRightColor = Color.white;

            var controlDropdown = new DropdownField(new List<string> { "Human", "AI", "Closed" }, defaultControl) { style = { width = 110, marginRight = 10, backgroundColor = new Color(0.1f, 0.1f, 0.1f), color = Color.white } };
            var diffDropdown = new DropdownField(new List<string> { "Knight", "Baron", "Lord", "Warlord" }, "Baron") { style = { width = 110, backgroundColor = new Color(0.1f, 0.1f, 0.1f), color = Color.white } };

            diffDropdown.SetEnabled(defaultControl == "AI");

            controlDropdown.RegisterValueChangedCallback(evt => {
                _factionControl[faction.Name] = evt.newValue;
                diffDropdown.SetEnabled(evt.newValue == "AI");
                if (evt.newValue == "Closed")
                {
                    nameLabel.style.color = Color.gray;
                    colorSwatch.style.opacity = 0.3f;
                }
                else
                {
                    nameLabel.style.color = Color.white;
                    colorSwatch.style.opacity = 1f;
                }
            });

            diffDropdown.RegisterValueChangedCallback(evt => {
                _factionDifficulty[faction.Name] = evt.newValue;
            });

            return new GraphicalUserInterfaceBuilder($"Slot_{faction.Name}")
                .WithFlexLayout(FlexDirection.Row, Justify.FlexStart, Align.Center)
                .WithBackgroundColor(new Color(0.12f, 0.12f, 0.15f))
                .OnBuild(ve =>
                {
                    ve.style.marginBottom = 8;
                    ve.style.paddingTop = 5; ve.style.paddingBottom = 5; ve.style.paddingLeft = 5; ve.style.paddingRight = 5;
                    ve.style.borderTopLeftRadius = 4; ve.style.borderTopRightRadius = 4; ve.style.borderBottomLeftRadius = 4; ve.style.borderBottomRightRadius = 4;

                    ve.RegisterCallback<MouseEnterEvent>(e => {
                        ve.style.backgroundColor = new Color(0.18f, 0.18f, 0.22f);
                        _activeFaction = faction;
                        RefreshLoreCard();
                    });
                    ve.RegisterCallback<MouseLeaveEvent>(e => {
                        ve.style.backgroundColor = new Color(0.12f, 0.12f, 0.15f);
                    });
                })
                .AddChild(new GraphicalUserInterfaceBuilder("NameContainer")
                    .WithFlexLayout(FlexDirection.Row, Justify.FlexStart, Align.Center)
                    .OnBuild(ve => { ve.style.width = 200; })
                    .AddChild(colorSwatch)
                    .AddChild(nameLabel)
                    .Build())
                .AddChild(controlDropdown)
                .AddChild(diffDropdown)
                .Build();
        }

        private void RefreshLoreCard()
        {
            _loreCardContainer.Clear();

            var cardBuilder = new GraphicalUserInterfaceBuilder("LoreCard")
                .WithFlexLayout(FlexDirection.Column, Justify.FlexStart, Align.Stretch)
                .WithBackgroundColor(new Color(0.1f, 0.1f, 0.12f))

                .OnBuild(ve =>
                {
                    ve.style.width = 450;
                    ve.style.height = 750;
                    ve.style.minHeight = 750;

                    ve.style.paddingTop = 25; ve.style.paddingBottom = 25; ve.style.paddingLeft = 25; ve.style.paddingRight = 25;
                    ve.style.borderTopWidth = 8; ve.style.borderBottomWidth = 8; ve.style.borderLeftWidth = 8; ve.style.borderRightWidth = 8;
                    ve.style.borderTopColor = _activeFaction.ThemeColor; ve.style.borderBottomColor = _activeFaction.ThemeColor; ve.style.borderLeftColor = _activeFaction.ThemeColor; ve.style.borderRightColor = _activeFaction.ThemeColor;
                    ve.style.borderTopLeftRadius = 8; ve.style.borderTopRightRadius = 8; ve.style.borderBottomLeftRadius = 8; ve.style.borderBottomRightRadius = 8;
                });

            if (_activeFaction.FactionRender != null)
            {
                cardBuilder.AddChild(new GraphicalUserInterfaceBuilder("RenderFrameWrapper")
                    .OnBuild(ve => {
                        ve.style.height = 300;
                        ve.style.minHeight = 300;
                        ve.style.marginBottom = 20;
                    })
                    .AddChild(ctx => new ImageGuiBuilder(_activeFaction.FactionRender)
                        .WithScaleMode(ScaleMode.ScaleToFit)
                        .WithBackgroundColor(new Color(0.02f, 0.02f, 0.02f))
                        .WithBorder(_activeFaction.ThemeColor, 2f)
                        .CreateGui(ctx))
                    .Build());
            }
            else
            {
                cardBuilder.AddChild(new GraphicalUserInterfaceBuilder("LogoCircle")
                    .WithFlexLayout(FlexDirection.Row, Justify.Center, Align.Center)
                    .OnBuild(ve => {
                        ve.style.width = 80; ve.style.height = 80;
                        ve.style.minHeight = 80;
                        ve.style.borderTopLeftRadius = 40; ve.style.borderTopRightRadius = 40; ve.style.borderBottomLeftRadius = 40; ve.style.borderBottomRightRadius = 40;
                        ve.style.alignSelf = Align.Center; ve.style.marginBottom = 20;
                        ve.style.borderTopWidth = 3; ve.style.borderBottomWidth = 3; ve.style.borderLeftWidth = 3; ve.style.borderRightWidth = 3;
                        ve.style.borderTopColor = _activeFaction.ThemeColor; ve.style.borderBottomColor = _activeFaction.ThemeColor; ve.style.borderLeftColor = _activeFaction.ThemeColor; ve.style.borderRightColor = _activeFaction.ThemeColor;
                        ve.style.backgroundColor = new Color(0.05f, 0.05f, 0.05f);
                    })
                    .AddChild(new Label(_activeFaction.Name.Substring(0, 1)) { style = { color = _activeFaction.ThemeColor, fontSize = 42, unityFontStyleAndWeight = FontStyle.Bold } })
                    .Build());
            }

            cardBuilder
                .AddChild(new Label(_activeFaction.Name.ToUpper()) { style = { color = Color.white, fontSize = 24, unityFontStyleAndWeight = FontStyle.Bold, unityTextAlign = TextAnchor.MiddleCenter } })
                .AddChild(new Label(_activeFaction.Subtitle) { style = { color = _activeFaction.ThemeColor, fontSize = 14, unityFontStyleAndWeight = FontStyle.Italic, unityTextAlign = TextAnchor.MiddleCenter, marginBottom = 15 } })
                .AddChild(new VisualElement { style = { height = 1, backgroundColor = new Color(0.3f, 0.3f, 0.3f), marginBottom = 15 } });

            cardBuilder.AddChild(root => {
                var scroll = new ScrollView(ScrollViewMode.Vertical);
                scroll.style.flexGrow = 1;

                scroll.Add(new Label(_activeFaction.Lore) { style = { color = new Color(0.8f, 0.8f, 0.8f), whiteSpace = WhiteSpace.Normal, marginBottom = 20, fontSize = 14 } });
                scroll.Add(new Label("KNOWN STRENGTHS") { style = { color = Color.gray, fontSize = 12, unityFontStyleAndWeight = FontStyle.Bold, marginBottom = 5 } });
                scroll.Add(new Label(_activeFaction.Strengths) { style = { color = new Color(0.9f, 0.8f, 0.2f), fontSize = 13 } });

                return scroll;
            });

            _loreCardContainer.Add(cardBuilder.Build());
        }

        private void HandleMarchToWar()
        {
            // --- GENERALIZED DATA EXTRACTION ---

            // 1. Locate or create the generalized WarlordsContext
            var context = UnityEngine.Object.FindObjectOfType<WarlordsContext>();
            if (context == null)
            {
                var go = new GameObject("[WARLORDS_CONTEXT]");
                context = go.AddComponent<WarlordsContext>();
            }

            context.ActiveFactions.Clear();

            // 2. Map UI selections directly into the Generalized State Container
            foreach (var factionData in _factions)
            {
                if (_factionControl.TryGetValue(factionData.Name, out string controlState) && controlState != "Closed")
                {
                    context.ActiveFactions.Add(new WarlordFaction
                    {
                        FactionName = factionData.Name,
                        FactionColor = factionData.ThemeColor,
                        IsAI = (controlState == "AI"),
                        Difficulty = _factionDifficulty.TryGetValue(factionData.Name, out string diff) ? diff : "Baron"
                    });
                }
            }

            if (context.ActiveFactions.Count < 2)
            {
                Debug.LogWarning("[WARLORDS SETUP] You must have at least 2 active factions to march to war.");
                return;
            }

            // 3. Initialize the FSM Engine using the Forge ecosystem
            context.InitializeFSM();

            // Kickoff the state machine
            context.Status.TransitionTo("Deployment");

            // 4. Route to Tactical View
            if (_router != null)
            {
                _router.NavigateTo("InGame");
            }
            else
            {
                Debug.LogWarning("Cannot March to War: IGuiRouter is null.");
            }
        }

        public Action<VisualElement> GetGuiBuilder() => (root) => root.Add(CreateGui(new GuiContext()));
#if UNITY_EDITOR
        public void ToUIDocument(string assetPath) => GraphicalUserInterfaceBuilder.ConvertToUIDocument(CreateGui(new GuiContext()), assetPath);
#endif
        public void FromUIDocument(string assetPath) => _lastCtx?.OnBuilt?.Invoke(GraphicalUserInterfaceBuilder.ConvertFromUIDocument(assetPath));
    }
}