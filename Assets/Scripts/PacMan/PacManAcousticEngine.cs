using UnityEngine;
using Workshop.UI_And_Tools.Forge.Builders.GuiBuilders;

namespace Assets.Scripts.PacMan
{
    /// <summary>
    /// Static wrapper for the Workshop's raw audio synthesis math.
    /// Decoupled from UI for use in high-frequency simulation loops.
    /// </summary>
    public static class PacManAcousticEngine
    {
        private static AudioSource _sfxSource;

        // PRESETS (Based on the Acoustic Reality Engine Taxonomy)
        public static readonly SFXProfile WakaChime = new SFXProfile { Name = "Waka", WaveType = Waveform.Square, BaseFrequency = 440f, Attack = 0.01f, Decay = 0.05f, Sustain = 0.2f, Release = 0.1f };
        public static readonly SFXProfile PowerUp = new SFXProfile { Name = "Power", WaveType = Waveform.Sawtooth, BaseFrequency = 220f, Attack = 0.1f, Decay = 0.3f, Sustain = 0.8f, Release = 0.5f, IsLooping = true };
        public static readonly SFXProfile Death = new SFXProfile { Name = "Death", WaveType = Waveform.Sine, BaseFrequency = 880f, Attack = 0.05f, Decay = 0.5f, Sustain = 0f, Release = 1.5f };

        public static void Play(SFXProfile profile)
        {
            // Reuse the synthesis math from Workshop_Gui_SFX_Builder
            int sampleRate = 44100;
            float duration = profile.IsLooping ? 1.0f : profile.Attack + profile.Decay + profile.Release;
            int totalSamples = (int)(sampleRate * duration);
            float[] samples = new float[totalSamples];

            for (int i = 0; i < totalSamples; i++)
            {
                float time = (float)i / sampleRate;
                float phase = time * profile.BaseFrequency * 2f * Mathf.PI;
                float rawSample = profile.WaveType switch
                {
                    Waveform.Sine => Mathf.Sin(phase),
                    Waveform.Square => Mathf.Sign(Mathf.Sin(phase)),
                    Waveform.Sawtooth => 2f * (time * profile.BaseFrequency - Mathf.Floor(time * profile.BaseFrequency + 0.5f)),
                    Waveform.Noise => Random.Range(-1f, 1f),
                    _ => 0f
                };

                float amplitude = 1f;
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

            if (_sfxSource == null) _sfxSource = new GameObject("PacMan_SFX_Emitter").AddComponent<AudioSource>();
            _sfxSource.PlayOneShot(clip);
        }
    }
}