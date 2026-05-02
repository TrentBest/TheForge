using UnityEngine;
using Workshop.UI_And_Tools.Forge.Builders.GuiBuilders;

namespace Assets.Scripts.PillVirus
{
    public static class PillVirusAcousticEngine
    {
        private static AudioSource _sfxSource;

        public static readonly SFXProfile Move = new SFXProfile { Name = "Move", WaveType = Waveform.Square, BaseFrequency = 220f, Attack = 0.01f, Decay = 0.05f, Sustain = 0f, Release = 0.05f };
        public static readonly SFXProfile Rotate = new SFXProfile { Name = "Rot", WaveType = Waveform.Sine, BaseFrequency = 440f, Attack = 0.01f, Decay = 0.05f, Sustain = 0f, Release = 0.05f };
        public static readonly SFXProfile Land = new SFXProfile { Name = "Land", WaveType = Waveform.Noise, BaseFrequency = 110f, Attack = 0.01f, Decay = 0.1f, Sustain = 0f, Release = 0.05f };
        public static readonly SFXProfile Clear = new SFXProfile { Name = "Clear", WaveType = Waveform.Sawtooth, BaseFrequency = 330f, Attack = 0.05f, Decay = 0.2f, Sustain = 0.1f, Release = 0.3f };
        public static readonly SFXProfile Death = new SFXProfile { Name = "Lost", WaveType = Waveform.Sine, BaseFrequency = 165f, Attack = 0.1f, Decay = 0.5f, Sustain = 0f, Release = 1.2f };
        public static readonly SFXProfile Victory = new SFXProfile { Name = "Win", WaveType = Waveform.Sine, BaseFrequency = 660f, Attack = 0.05f, Decay = 0.3f, Sustain = 0.5f, Release = 1.0f };

        public static void Play(SFXProfile profile)
        {
            int sampleRate = 44100;
            float duration = profile.Attack + profile.Decay + profile.Release;
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
                if (time < profile.Attack) amplitude = time / profile.Attack;
                else if (time < profile.Attack + profile.Decay) amplitude = 1f - ((time - profile.Attack) / profile.Decay) * (1f - profile.Sustain);
                else if (time < profile.Attack + profile.Decay + profile.Release) amplitude = profile.Sustain * (1f - ((time - (profile.Attack + profile.Decay)) / profile.Release));
                else amplitude = 0f;
                samples[i] = rawSample * amplitude;
            }

            var clip = AudioClip.Create(profile.Name, totalSamples, 1, sampleRate, false);
            clip.SetData(samples, 0);

            if (_sfxSource == null) { _sfxSource = new GameObject("PillVirus_SFX").AddComponent<AudioSource>(); Object.DontDestroyOnLoad(_sfxSource.gameObject); }
            _sfxSource.PlayOneShot(clip);
        }
    }
}