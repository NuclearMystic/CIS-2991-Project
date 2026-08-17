using UnityEngine;
using UnityEngine.Audio;

namespace CIS2991Project.Managers
{
    // Central volume knobs for Master/Music/SFX/Ambience, backed by NewAudioMixer's exposed
    // parameters. Plain static state (no scene object needed) so any script can read/write it,
    // e.g. AudioManager.SfxVolume = slider.value from an options menu. Persisted via PlayerPrefs -
    // these are a system/user preference, not character progress, so they're global to the install
    // rather than part of any particular save slot.
    public static class AudioManager
    {
        private const string MixerResourcePath = "Audio/NewAudioMixer";
        private const string MasterParam = "MasterVolume";
        private const string MusicParam = "MusicVolume";
        private const string SfxParam = "SFXVolume";
        private const string AmbienceParam = "AmbienceVolume";
        private const string MusicGroupName = "Music";
        private const string SfxGroupName = "SFX";
        private const string AmbienceGroupName = "Ambience";
        private const float MinDecibels = -80f;

        private const string MasterPrefKey = "Audio.MasterVolume";
        private const string MusicPrefKey = "Audio.MusicVolume";
        private const string SfxPrefKey = "Audio.SfxVolume";
        private const string AmbiencePrefKey = "Audio.AmbienceVolume";

        private static AudioMixer _mixer;

        private static float _masterVolume;
        private static float _musicVolume;
        private static float _sfxVolume;
        private static float _ambienceVolume;

        // Runs once, the first time anything touches this class - loads whatever was saved last
        // session (defaulting to full volume the very first run) and pushes it into the mixer
        // immediately, so audio is correct from the first sound played, not just after a slider moves.
        static AudioManager()
        {
            _masterVolume = PlayerPrefs.GetFloat(MasterPrefKey, 1f);
            _musicVolume = PlayerPrefs.GetFloat(MusicPrefKey, 1f);
            _sfxVolume = PlayerPrefs.GetFloat(SfxPrefKey, 1f);
            _ambienceVolume = PlayerPrefs.GetFloat(AmbiencePrefKey, 1f);

            ApplyToMixer(MasterParam, _masterVolume);
            ApplyToMixer(MusicParam, _musicVolume);
            ApplyToMixer(SfxParam, _sfxVolume);
            ApplyToMixer(AmbienceParam, _ambienceVolume);
        }

        private static AudioMixer Mixer => _mixer != null ? _mixer : _mixer = Resources.Load<AudioMixer>(MixerResourcePath);

        // Lets code that creates AudioSources at runtime (no scene/prefab Inspector slot to wire a
        // group into, e.g. ConsumablePickup) still route through the right mixer group by name.
        public static AudioMixerGroup MusicGroup => GetGroup(MusicGroupName);
        public static AudioMixerGroup SfxGroup => GetGroup(SfxGroupName);
        public static AudioMixerGroup AmbienceGroup => GetGroup(AmbienceGroupName);

        private static AudioMixerGroup GetGroup(string groupName)
        {
            if (Mixer == null)
            {
                return null;
            }

            var groups = Mixer.FindMatchingGroups(groupName);
            return groups.Length > 0 ? groups[0] : null;
        }

        public static float MasterVolume
        {
            get => _masterVolume;
            set => SetVolume(ref _masterVolume, value, MasterParam, MasterPrefKey);
        }

        public static float MusicVolume
        {
            get => _musicVolume;
            set => SetVolume(ref _musicVolume, value, MusicParam, MusicPrefKey);
        }

        public static float SfxVolume
        {
            get => _sfxVolume;
            set => SetVolume(ref _sfxVolume, value, SfxParam, SfxPrefKey);
        }

        public static float AmbienceVolume
        {
            get => _ambienceVolume;
            set => SetVolume(ref _ambienceVolume, value, AmbienceParam, AmbiencePrefKey);
        }

        private static void SetVolume(ref float field, float value, string exposedParameterName, string prefKey)
        {
            field = Mathf.Clamp01(value);
            ApplyToMixer(exposedParameterName, field);
            PlayerPrefs.SetFloat(prefKey, field);
            PlayerPrefs.Save();
        }

        private static void ApplyToMixer(string exposedParameterName, float linearVolume)
        {
            var decibels = linearVolume > 0.0001f ? Mathf.Log10(linearVolume) * 20f : MinDecibels;

            if (Mixer == null || !Mixer.SetFloat(exposedParameterName, decibels))
            {
                Debug.LogWarning($"AudioManager: couldn't set mixer parameter \"{exposedParameterName}\". " +
                                  "Confirm the group's Volume is exposed to script under that exact name.");
            }
        }
    }
}
