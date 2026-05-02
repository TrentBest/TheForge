using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UIElements;
using Workshop.UI_And_Tools.Forge.Builders.GuiBuilders;
using TheSingularityWorkshop.FSM_API;

namespace Workshop.UI_And_Tools.Forge.Builders.GuiBuilders
{
    public enum Waveform { Sine, Square, Sawtooth, Noise }

    // THE GRAND TAXONOMY
    public enum SfxCategory { Foley_Clip, Ambient_Loop, Broadcast_Diegetic, Voice_Creature, Musical_Instrument }

    // The Data Model (Now expanded for the Acoustic Engine)
    public class SFXProfile
    {
        public string Name = "New Sound";
        public SfxCategory Category = SfxCategory.Foley_Clip;
        public Waveform WaveType = Waveform.Sine;

        public float BaseFrequency = 440f;
        public float Attack = 0.05f;
        public float Decay = 0.2f;
        public float Sustain = 0.5f;
        public float Release = 0.5f;

        // Environmental Context
        public bool IsLooping = false;
        public float SpatialBlend = 1.0f; // 1 = 3D (Positional), 0 = 2D (Everywhere)
    }

    public class Workshop_Gui_SFX_Builder : IGuiProvider
    {
        public string Title => "ACOUSTIC REALITY ENGINE";

        private AudioSource _previewSource;
        private List<SFXProfile> _soundRoster = new List<SFXProfile>();
        private SFXProfile _activeProfile;

        // UI State
        private VisualElement _rightPanelContent;
        private string _activeTab = "PRO";
        private SfxCategory _currentFilter = SfxCategory.Foley_Clip; // Default filter

        public Workshop_Gui_SFX_Builder()
        {
            // --- SEEDING THE REUSE/REPURPOSE PRESETS ---

            // FOLEY (Clips)
            _soundRoster.Add(new SFXProfile { Name = "Heavy Footfall", Category = SfxCategory.Foley_Clip, BaseFrequency = 60f, WaveType = Waveform.Noise, Attack = 0.01f, Decay = 0.1f, Sustain = 0f, Release = 0.05f });
            _soundRoster.Add(new SFXProfile { Name = "Gunshot (Pistol)", Category = SfxCategory.Foley_Clip, BaseFrequency = 1000f, WaveType = Waveform.Noise, Attack = 0.005f, Decay = 0.2f, Sustain = 0f, Release = 0.4f });

            // AMBIENT (Loops)
            _soundRoster.Add(new SFXProfile { Name = "Starship Engine Hum", Category = SfxCategory.Ambient_Loop, BaseFrequency = 45f, WaveType = Waveform.Sine, Attack = 2f, Decay = 0f, Sustain = 1f, Release = 2f, IsLooping = true });
            _soundRoster.Add(new SFXProfile { Name = "Eerie Wind", Category = SfxCategory.Ambient_Loop, BaseFrequency = 120f, WaveType = Waveform.Noise, Attack = 3f, Decay = 1f, Sustain = 0.8f, Release = 4f, IsLooping = true });

            // BROADCAST (Diegetic)
            _soundRoster.Add(new SFXProfile { Name = "Sports Bar TV Muffle", Category = SfxCategory.Broadcast_Diegetic, BaseFrequency = 300f, WaveType = Waveform.Sawtooth, Attack = 0.5f, Decay = 0.5f, Sustain = 0.6f, Release = 0.5f, IsLooping = true, SpatialBlend = 1f });
            _soundRoster.Add(new SFXProfile { Name = "Elevator Music (Muzak)", Category = SfxCategory.Broadcast_Diegetic, BaseFrequency = 523.25f, WaveType = Waveform.Sine, Attack = 0.1f, Decay = 0.3f, Sustain = 0.5f, Release = 1.0f, IsLooping = true });

            // INSTRUMENTS (Hermit Orchestra)
            _soundRoster.Add(new SFXProfile { Name = "Hermit Saxophone", Category = SfxCategory.Musical_Instrument, BaseFrequency = 293.66f, WaveType = Waveform.Sawtooth, Attack = 0.15f, Decay = 0.1f, Sustain = 0.8f, Release = 0.2f });

            _activeProfile = _soundRoster[0];
        }

        public VisualElement CreateGui(GuiContext ctx)
        {
            // ==========================================
            // LEFT PANEL: The Categorized CRUD
            // ==========================================
            var leftPanelVisual = new GraphicalUserInterfaceBuilder("SFX_LeftPanel")
                .WithWidth(270)
                .WithBackgroundColor(new Color(0.15f, 0.15f, 0.18f))
                .WithBorderRightWidth(2).WithBorderRightColor(Color.black)
                .WithPadding(10)
                .AddHeader("LIBRARY PRESETS", Color.cyan)
                .AddEnumData("Filter By:", _currentFilter, v => { _currentFilter = v; RefreshRosterUI(null); })
                .AddSeparator()
                .Build();

            // Setup dynamic container for list
            var listContainer = new GraphicalUserInterfaceBuilder("roster-list").WithAutoGrow().WithScrollable(true).Build();
            leftPanelVisual.Add(listContainer);
            RefreshRosterUI(leftPanelVisual);

            var addBtn = new Button(() => {
                var newProfile = new SFXProfile { Name = $"New {_currentFilter}", Category = _currentFilter };
                _soundRoster.Add(newProfile);
                _activeProfile = newProfile;
                RefreshRosterUI(leftPanelVisual);
            })
            { text = "+ CLONE AS NEW", style = { height = 35, marginTop = 10, backgroundColor = new Color(0.2f, 0.4f, 0.2f), unityFontStyleAndWeight = FontStyle.Bold } };
            leftPanelVisual.Add(addBtn);

            // ==========================================
            // RIGHT PANEL: The Dominant Tabbed Canvas
            // ==========================================
            var tabBarVisual = new GraphicalUserInterfaceBuilder("SFX_TabBar")
                .WithFlexLayout(FlexDirection.Row)
                .WithHeight(40)
                .WithBackgroundColor(new Color(0.08f, 0.08f, 0.1f))
                .AddChild(CreateTabButton("NOOB (Fluent)", "NOOB"))
                .AddChild(CreateTabButton("STANDARD", "STD"))
                .AddChild(CreateTabButton("PRO STUDIO", "PRO"))
                .Build();

            _rightPanelContent = new GraphicalUserInterfaceBuilder("SFX_Content")
                .WithAutoGrow().WithPadding(20).Build();

            var rightPanelVisual = new GraphicalUserInterfaceBuilder("SFX_RightPanel")
                .WithFlexLayout(FlexDirection.Column).WithAutoGrow()
                .AddChild(tabBarVisual)
                .AddChild(_rightPanelContent)
                .Build();

            RenderActiveTab();

            // ROOT UNIFICATION
            return new GraphicalUserInterfaceBuilder("SFX_Studio_Root")
                .WithFlexLayout(FlexDirection.Row)
                .WithBackgroundColor(new Color(0.1f, 0.1f, 0.12f))
                .WithAutoGrow()
                .AddChild(leftPanelVisual)
                .AddChild(rightPanelVisual)
                .Build();
        }

        private void RefreshRosterUI(VisualElement rootFallback)
        {
            var listContainer = _rightPanelContent?.parent?.parent?.Q("roster-list");
            if (listContainer == null && rootFallback != null) listContainer = rootFallback.Q("roster-list");
            if (listContainer == null) return;

            listContainer.Clear();

            var filtered = _soundRoster.Where(p => p.Category == _currentFilter).ToList();

            foreach (var profile in filtered)
            {
                var btn = new Button(() => { _activeProfile = profile; RenderActiveTab(); })
                {
                    text = profile.Name,
                    style = { height = 30, marginBottom = 2,
                              backgroundColor = _activeProfile == profile ? new Color(0.8f, 0.4f, 0.1f) : new Color(0.2f, 0.2f, 0.2f),
                              color = _activeProfile == profile ? Color.white : Color.silver }
                };
                listContainer.Add(btn);
            }
        }

        private Button CreateTabButton(string label, string tabId)
        {
            return new Button(() => { _activeTab = tabId; RenderActiveTab(); })
            {
                text = label,
                style = { flexGrow = 1, borderTopLeftRadius = 5, borderTopRightRadius = 5,
                          backgroundColor = _activeTab == tabId ? new Color(0.2f, 0.2f, 0.25f) : new Color(0.1f, 0.1f, 0.1f),
                          color = _activeTab == tabId ? Color.white : Color.gray,
                          unityFontStyleAndWeight = FontStyle.Bold }
            };
        }

        private void RenderActiveTab()
        {
            if (_rightPanelContent == null) return;
            _rightPanelContent.Clear();

            var headerGui = new GraphicalUserInterfaceBuilder("TabHeader")
                .AddHeader($"EDITING: {_activeProfile.Name} [{_activeProfile.Category}]", Color.yellow)
                .Build();
            _rightPanelContent.Add(headerGui);

            if (_activeTab == "PRO") BuildProTab();
            else if (_activeTab == "STD") BuildStandardTab();
            else if (_activeTab == "NOOB") BuildNoobTab();
        }

        #region --- The Three Bears ---

        private void BuildProTab()
        {
            var gui = new GraphicalUserInterfaceBuilder("ProTab")
                .AddHeader("ENVIRONMENTAL CONTEXT", Color.white)
                .AddEnumData("Classification", _activeProfile.Category, v => { _activeProfile.Category = v; RefreshRosterUI(null); })
                .AddToggleData("Continuous Loop (Ambient/Broadcast)", _activeProfile.IsLooping, v => _activeProfile.IsLooping = v)
                .AddSliderData("Spatial Blend (0=2D, 1=3D)", 0f, 1f, _activeProfile.SpatialBlend, v => _activeProfile.SpatialBlend = v)
                .AddSeparator()
                .AddHeader("SYNTHESIZER OSCILLATOR", Color.magenta)
                .AddEnumData("Waveform", _activeProfile.WaveType, v => _activeProfile.WaveType = v)
                .AddSliderData("Base Frequency (Hz)", 20f, 2000f, _activeProfile.BaseFrequency, v => _activeProfile.BaseFrequency = v)
                .AddSeparator()
                .AddHeader("AMPLITUDE ENVELOPE (ADSR)", Color.cyan)
                .AddSliderData("Attack", 0.001f, 2f, _activeProfile.Attack, v => _activeProfile.Attack = v)
                .AddSliderData("Decay", 0.001f, 2f, _activeProfile.Decay, v => _activeProfile.Decay = v)
                .AddSliderData("Sustain Level", 0f, 1f, _activeProfile.Sustain, v => _activeProfile.Sustain = v)
                .AddSliderData("Release", 0.001f, 5f, _activeProfile.Release, v => _activeProfile.Release = v)
                .AddSeparator()
                .AddButton("▶ COMPILE & PLAY", () => SynthesizeAndPlay(_activeProfile))
                .Build();

            _rightPanelContent.Add(gui);
        }

        private void BuildStandardTab()
        {
            var gui = new GraphicalUserInterfaceBuilder("StdTab")
                .AddHeader("EASY CONTROLS", Color.white)
                .AddStringData("Sound Name", _activeProfile.Name, v => { _activeProfile.Name = v; RefreshRosterUI(null); })
                .AddEnumData("Category", _activeProfile.Category, v => { _activeProfile.Category = v; RefreshRosterUI(null); })
                .AddSliderData("Pitch", 50f, 1000f, _activeProfile.BaseFrequency, v => _activeProfile.BaseFrequency = v)
                .AddSliderData("Duration", 0.1f, 5f, _activeProfile.Release, v => _activeProfile.Release = v)
                .AddSeparator()
                .AddButton("▶ PLAY", () => SynthesizeAndPlay(_activeProfile))
                .Build();
            _rightPanelContent.Add(gui);
        }

        private void BuildNoobTab()
        {
            var gui = new GraphicalUserInterfaceBuilder("NoobTab")
                .AddHeader("WHAT DO YOU WANT IT TO DO?", new Color(0.5f, 1f, 0.5f))
                .AddButton("💥 BOOM / EXPLOSION", () => {
                    _activeProfile.Category = SfxCategory.Foley_Clip; _activeProfile.WaveType = Waveform.Noise;
                    _activeProfile.Attack = 0.01f; _activeProfile.Decay = 0.5f; _activeProfile.Sustain = 0f; _activeProfile.Release = 1.5f;
                    SynthesizeAndPlay(_activeProfile);
                })
                .AddButton("📺 BROKEN TV STATIC", () => {
                    _activeProfile.Category = SfxCategory.Broadcast_Diegetic; _activeProfile.WaveType = Waveform.Noise;
                    _activeProfile.Attack = 0.1f; _activeProfile.Sustain = 0.8f; _activeProfile.IsLooping = true;
                    SynthesizeAndPlay(_activeProfile);
                })
                .AddButton("🎷 HERMIT SAX HONK", () => {
                    _activeProfile.Category = SfxCategory.Musical_Instrument; _activeProfile.WaveType = Waveform.Sawtooth;
                    _activeProfile.BaseFrequency = 349.23f; // F4
                    _activeProfile.Attack = 0.1f; _activeProfile.Decay = 0.1f; _activeProfile.Sustain = 0.9f; _activeProfile.Release = 0.3f;
                    SynthesizeAndPlay(_activeProfile);
                })
                .Build();

            foreach (var child in gui.Children()) { if (child is Button b) { b.style.height = 50; b.style.fontSize = 18; } }
            _rightPanelContent.Add(gui);
        }

        #endregion

        #region --- Raw Audio Synthesis Engine ---

        private void SynthesizeAndPlay(SFXProfile profile)
        {
            int sampleRate = 44100;
            // Cap duration for math synthesis so we don't freeze the main thread on massive loops.
            // In a real engine, loops are buffered, but here we synthesize a chunk and tell Unity to loop it.
            float totalDuration = profile.IsLooping ? 2.0f : profile.Attack + profile.Decay + profile.Release;
            int totalSamples = (int)(sampleRate * totalDuration);
            float[] samples = new float[totalSamples];

            for (int i = 0; i < totalSamples; i++)
            {
                float time = (float)i / sampleRate;
                float phase = time * profile.BaseFrequency * 2f * Mathf.PI;
                float rawSample = 0f;

                switch (profile.WaveType)
                {
                    case Waveform.Sine: rawSample = Mathf.Sin(phase); break;
                    case Waveform.Square: rawSample = Mathf.Sign(Mathf.Sin(phase)); break;
                    case Waveform.Sawtooth: rawSample = 2f * (time * profile.BaseFrequency - Mathf.Floor(time * profile.BaseFrequency + 0.5f)); break;
                    case Waveform.Noise: rawSample = UnityEngine.Random.Range(-1f, 1f); break;
                }

                float amplitude = 1f;
                // Only apply ADSR if it's NOT a continuous ambient loop
                if (!profile.IsLooping)
                {
                    if (time < profile.Attack) amplitude = time / profile.Attack;
                    else if (time < profile.Attack + profile.Decay) amplitude = 1f - ((time - profile.Attack) / profile.Decay) * (1f - profile.Sustain);
                    else if (time < profile.Attack + profile.Decay + profile.Release) amplitude = profile.Sustain * (1f - ((time - (profile.Attack + profile.Decay)) / profile.Release));
                    else amplitude = 0f;
                }

                samples[i] = rawSample * amplitude;
            }

            var clip = AudioClip.Create(profile.Name, totalSamples, 1, sampleRate, false);
            clip.SetData(samples, 0);

            if (_previewSource == null) _previewSource = new GameObject("SFX_Synth_Engine").AddComponent<AudioSource>();

            _previewSource.clip = clip;
            _previewSource.loop = profile.IsLooping;
            _previewSource.spatialBlend = profile.SpatialBlend;
            _previewSource.Play();
        }

        #endregion

        public Action<VisualElement> GetGuiBuilder() => root => root.Add(CreateGui(new GuiContext()));
        public void ToUIDocument(string assetPath) { }
        public void FromUIDocument(string assetPath) { }
    }
}