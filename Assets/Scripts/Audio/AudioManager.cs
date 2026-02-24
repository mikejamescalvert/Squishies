using UnityEngine;

namespace Squishies
{
    public class AudioManager : MonoBehaviour
    {
        public static AudioManager Instance { get; private set; }

        private AudioSource musicSource;
        private AudioSource sfxSource;

        // Cached generated audio clips
        private AudioClip[] popClips;
        private AudioClip mergeClip;
        private AudioClip comboClip;
        private AudioClip uiClickClip;

        private const int PopClipCount = 8;

        private void Awake()
        {
            // Singleton with DontDestroyOnLoad — check for duplicates
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
            DontDestroyOnLoad(gameObject);

            // Create audio sources
            musicSource = gameObject.AddComponent<AudioSource>();
            musicSource.loop = true;
            musicSource.playOnAwake = false;
            musicSource.volume = 0.3f;

            sfxSource = gameObject.AddComponent<AudioSource>();
            sfxSource.loop = false;
            sfxSource.playOnAwake = false;
            sfxSource.volume = 0.5f;

            // Generate all placeholder audio clips
            GenerateAllClips();
        }

        private void OnDestroy()
        {
            if (Instance == this)
            {
                Instance = null;
            }
        }

        /// <summary>
        /// Generates all placeholder audio clips programmatically.
        /// </summary>
        private void GenerateAllClips()
        {
            // Generate pop sounds at escalating pitches
            popClips = new AudioClip[PopClipCount];
            for (int i = 0; i < PopClipCount; i++)
            {
                float pitch = 1f + i * 0.12f;
                popClips[i] = GeneratePopSound(pitch);
            }

            // Generate merge sound (deeper, longer)
            mergeClip = GenerateMergeSound();

            // Generate combo sound (chord)
            comboClip = GenerateComboSound();

            // Generate UI click
            uiClickClip = GenerateClickSound();
        }

        private AudioClip GeneratePopSound(float pitch = 1f)
        {
            int sampleRate = 44100;
            int samples = (int)(sampleRate * 0.1f); // 0.1 second
            float[] data = new float[samples];
            for (int i = 0; i < samples; i++)
            {
                float t = (float)i / sampleRate;
                float envelope = 1f - (float)i / samples; // decay
                data[i] = Mathf.Sin(2 * Mathf.PI * 800 * pitch * t) * envelope * 0.3f;
            }
            AudioClip clip = AudioClip.Create($"Pop_{pitch:F2}", samples, 1, sampleRate, false);
            clip.SetData(data, 0);
            return clip;
        }

        private AudioClip GenerateMergeSound()
        {
            int sampleRate = 44100;
            int samples = (int)(sampleRate * 0.25f); // 0.25 second — longer
            float[] data = new float[samples];
            for (int i = 0; i < samples; i++)
            {
                float t = (float)i / sampleRate;
                float envelope = 1f - (float)i / samples;
                // Lower frequency with a slight pitch drop for a satisfying "thump"
                float freq = Mathf.Lerp(400f, 200f, (float)i / samples);
                data[i] = Mathf.Sin(2 * Mathf.PI * freq * t) * envelope * 0.4f;
                // Add a sub-harmonic
                data[i] += Mathf.Sin(2 * Mathf.PI * freq * 0.5f * t) * envelope * 0.2f;
            }
            AudioClip clip = AudioClip.Create("Merge", samples, 1, sampleRate, false);
            clip.SetData(data, 0);
            return clip;
        }

        private AudioClip GenerateComboSound()
        {
            int sampleRate = 44100;
            int samples = (int)(sampleRate * 0.3f); // 0.3 second
            float[] data = new float[samples];
            for (int i = 0; i < samples; i++)
            {
                float t = (float)i / sampleRate;
                float envelope = 1f - (float)i / samples;
                // Major chord: root, major third, perfect fifth
                float root = Mathf.Sin(2 * Mathf.PI * 523.25f * t);       // C5
                float third = Mathf.Sin(2 * Mathf.PI * 659.25f * t);      // E5
                float fifth = Mathf.Sin(2 * Mathf.PI * 783.99f * t);      // G5
                float octave = Mathf.Sin(2 * Mathf.PI * 1046.5f * t);     // C6
                data[i] = (root + third * 0.8f + fifth * 0.6f + octave * 0.3f) * envelope * 0.15f;
            }
            AudioClip clip = AudioClip.Create("Combo", samples, 1, sampleRate, false);
            clip.SetData(data, 0);
            return clip;
        }

        private AudioClip GenerateClickSound()
        {
            int sampleRate = 44100;
            int samples = (int)(sampleRate * 0.05f); // 0.05 second — very short
            float[] data = new float[samples];
            for (int i = 0; i < samples; i++)
            {
                float t = (float)i / sampleRate;
                float envelope = 1f - (float)i / samples;
                envelope *= envelope; // sharper decay
                data[i] = Mathf.Sin(2 * Mathf.PI * 1200f * t) * envelope * 0.25f;
            }
            AudioClip clip = AudioClip.Create("Click", samples, 1, sampleRate, false);
            clip.SetData(data, 0);
            return clip;
        }

        /// <summary>
        /// Plays a pop sound with pitch escalation based on chain position.
        /// Higher chainIndex = higher pitch for satisfying escalation.
        /// </summary>
        public void PlayPop(int chainIndex = 0)
        {
            if (sfxSource == null) return;

            // Select clip based on chain index (clamped to available clips)
            int clipIndex = Mathf.Clamp(chainIndex, 0, popClips.Length - 1);

            sfxSource.pitch = 1f + chainIndex * 0.08f;
            sfxSource.PlayOneShot(popClips[clipIndex]);
        }

        /// <summary>
        /// Plays the merge sound effect (deeper, longer).
        /// </summary>
        public void PlayMerge()
        {
            if (sfxSource == null || mergeClip == null) return;

            sfxSource.pitch = 1f;
            sfxSource.PlayOneShot(mergeClip);
        }

        /// <summary>
        /// Plays the combo sound with increasing intensity for higher levels.
        /// </summary>
        public void PlayCombo(int level)
        {
            if (sfxSource == null || comboClip == null) return;

            // Higher combo level = higher pitch and volume
            sfxSource.pitch = 1f + (level - 1) * 0.15f;
            float volume = Mathf.Clamp01(0.5f + level * 0.15f);
            sfxSource.PlayOneShot(comboClip, volume);
        }

        /// <summary>
        /// Plays a short UI click sound.
        /// </summary>
        public void PlayUIClick()
        {
            if (sfxSource == null || uiClickClip == null) return;

            sfxSource.pitch = 1f;
            sfxSource.PlayOneShot(uiClickClip);
        }

        /// <summary>
        /// Starts playing background music (placeholder — silent ambient loop).
        /// </summary>
        public void PlayMusic()
        {
            if (musicSource == null) return;

            // Generate a very quiet ambient hum as placeholder music
            int sampleRate = 44100;
            int samples = sampleRate * 4; // 4-second loop
            float[] data = new float[samples];
            for (int i = 0; i < samples; i++)
            {
                float t = (float)i / sampleRate;
                // Very quiet ambient pad
                float wave1 = Mathf.Sin(2 * Mathf.PI * 130.81f * t); // C3
                float wave2 = Mathf.Sin(2 * Mathf.PI * 196f * t);    // G3
                float lfo = 0.5f + 0.5f * Mathf.Sin(2 * Mathf.PI * 0.25f * t); // Slow modulation
                data[i] = (wave1 + wave2 * 0.5f) * lfo * 0.02f; // Very quiet
            }
            AudioClip musicClip = AudioClip.Create("AmbientMusic", samples, 1, sampleRate, false);
            musicClip.SetData(data, 0);

            musicSource.clip = musicClip;
            musicSource.loop = true;
            musicSource.Play();
        }

        /// <summary>
        /// Stops the background music.
        /// </summary>
        public void StopMusic()
        {
            if (musicSource != null)
            {
                musicSource.Stop();
            }
        }

        /// <summary>
        /// Sets the music volume (0 to 1).
        /// </summary>
        public void SetMusicVolume(float v)
        {
            if (musicSource != null)
            {
                musicSource.volume = Mathf.Clamp01(v);
            }
        }

        /// <summary>
        /// Sets the SFX volume (0 to 1).
        /// </summary>
        public void SetSFXVolume(float v)
        {
            if (sfxSource != null)
            {
                sfxSource.volume = Mathf.Clamp01(v);
            }
        }
    }
}
