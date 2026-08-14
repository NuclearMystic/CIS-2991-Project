using UnityEngine;
using UnityEngine.Audio;

namespace CIS2991Project.Managers
{
    // Central volume knobs for Master/Music/SFX/Ambience, backed by NewAudioMixer's exposed
    // parameters. Plain static state (no scene object needed) so any script can read/write it,
    // e.g. AudioManager.SfxVolume = slider.value from an options menu.
    public static class AudioManager
    {
        private const string MixerResourcePath = "Audio/NewAudioMixer";
        private const string MasterParam = "MasterVolume";
        private const string MusicParam = "MusicVolume";
        private const string SfxParam = "SFXVolume";
        private const string AmbienceParam = "AmbienceVolume";
        private const float MinDecibels = -80f;

        private static AudioMixer _mixer;

        private static float _masterVolume = 1f;
        private static float _musicVolume = 1f;
        private static float _sfxVolume = 1f;
        private static float _ambienceVolume = 1f;

        private static AudioMixer Mixer => _mixer != null ? _mixer : _mixer = Resources.Load<AudioMixer>(MixerResourcePath);

        public static float MasterVolume
        {
            get => _masterVolume;
            set => SetVolume(ref _masterVolume, value, MasterParam);
        }

        public static float MusicVolume
        {
            get => _musicVolume;
            set => SetVolume(ref _musicVolume, value, MusicParam);
        }

        public static float SfxVolume
        {
            get => _sfxVolume;
            set => SetVolume(ref _sfxVolume, value, SfxParam);
        }

        public static float AmbienceVolume
        {
            get => _ambienceVolume;
            set => SetVolume(ref _ambienceVolume, value, AmbienceParam);
        }

        private static void SetVolume(ref float field, float value, string exposedParameterName)
        {
            field = Mathf.Clamp01(value);
            var decibels = field > 0.0001f ? Mathf.Log10(field) * 20f : MinDecibels;

            if (Mixer == null || !Mixer.SetFloat(exposedParameterName, decibels))
            {
                Debug.LogWarning($"AudioManager: couldn't set mixer parameter \"{exposedParameterName}\". " +
                                  "Confirm the group's Volume is exposed to script under that exact name.");
            }
        }
    }
}
