using UnityEngine;
using CodeStage.AntiCheat.ObscuredTypes;
using System.Collections.Generic;

namespace FluurMat.GameHelpers
{
    [DefaultExecutionOrder(-78)]
    public class ToggleSound : MonoBehaviour
    {
        [SerializeField] private string gamePrefName = "";

        private string soundPrefGlobal = "PlayAndLearn_Sound";
        private string soundPref = "Sound";

        [Tooltip("Sound sources on this game object will be added to the list automatically on Start.")]
        [SerializeField]
        private GameObject soundSourceTarget;

        [Tooltip("Sound sources for game sounds. These can be on different game objects from the target one above.")]
        [SerializeField]
        private AudioSource[] soundSources;

        private string musicPrefGlobal = "PlayAndLearn_Music";
        private string musicPref = "Music";

        [Tooltip("Target Music sources have to be assigned manaully here.")] [SerializeField]
        private AudioSource[] musicSources;

        private bool soundState = true;
        private bool musicState = true;

        private bool globalSoundState = true;
        private bool globalMusicState = true;

        [SerializeField] private ToggleButton soundButton;
        [SerializeField] private ToggleButton musicButton;

        private static ToggleSound _instance;

        public static ToggleSound instance
        {
            get { return _instance; }
        }

        void Awake()
        {
            if (_instance == null)
            {
                _instance = this;
            }
            else if (_instance != this)
            {
                Destroy(this);
            }
        }

        void Start()
        {
            PrepareSoundSources();

            CheckSoundSources();
            CheckMusicSources();

            GetSoundState();
            UpdateButtonAndSound();

            GetMusicState();
            UpdateButtonAndMusic();
        }

        public bool GetSoundState()
        {
            globalSoundState = ObscuredPrefs.GetBool(soundPrefGlobal, true);
            soundState = ObscuredPrefs.GetBool(GetGamePrefName(soundPref), true);

            return soundState && globalSoundState;
        }

        public bool GetMusicState()
        {
            globalMusicState = ObscuredPrefs.GetBool(musicPrefGlobal, true);
            musicState = ObscuredPrefs.GetBool(GetGamePrefName(musicPref), true);

            return musicState && globalMusicState;
        }

        public void SwitchGlobalSound()
        {
            globalSoundState = !globalSoundState;
        }

        public void SwitchGlobalMusic()
        {
            globalMusicState = !globalMusicState;
        }

        public void SwitchSound()
        {
            if (globalSoundState)
            {
                soundState = !soundState;
            }

            UpdateButtonAndSound();
        }

        public void SwitchMusic()
        {
            if (globalMusicState)
            {
                musicState = !musicState;
            }

            UpdateButtonAndMusic();
        }

        private void UpdateButtonAndSound()
        {
            if (!globalSoundState)
            {
                if (soundButton)
                {
                    soundButton.DoFullUpdate(false);
                }
            }
            else
            {
                if (soundButton)
                {
                    soundButton.DoUpdate(IsSoundOn());
                }
            }

            ToggleSoundSources();
        }

        private void UpdateButtonAndMusic()
        {
            if (!globalMusicState)
            {
                if (musicButton)
                {
                    musicButton.DoFullUpdate(false);
                }
            }
            else
            {
                if (musicButton)
                {
                    musicButton.DoUpdate(IsMusicOn());
                }
            }

            ToggleMusicSources();
        }

        public bool IsSoundOn()
        {
            return globalSoundState && soundState;
        }

        public bool IsMusicOn()
        {
            return globalMusicState && musicState;
        }

        private void PrepareSoundSources()
        {
            List<AudioSource> tempSoundSourcesList = new List<AudioSource>(soundSources);
            if (!soundSourceTarget)
            {
                soundSourceTarget = gameObject;
            }

            AudioSource[] soundSourcesOnTarget = soundSourceTarget.gameObject.GetComponents<AudioSource>();

            for (int i = 0; i < soundSourcesOnTarget.Length; i++)
            {
                bool shouldAddSource = !tempSoundSourcesList.Contains(soundSourcesOnTarget[i]);

                for (int j = 0; j < musicSources.Length; j++)
                {
                    if (musicSources[j] == soundSourcesOnTarget[i])
                    {
                        shouldAddSource = false;
                    }
                }

                for (int j = 0; j < soundSources.Length; j++)
                {
                    if (soundSources[j] == soundSourcesOnTarget[i])
                    {
                        shouldAddSource = false;
                    }
                }

                if (shouldAddSource)
                {
                    tempSoundSourcesList.Insert(0, soundSourcesOnTarget[i]);
                }
            }

            soundSources = tempSoundSourcesList.ToArray();
        }

        private void CheckSoundSources()
        {
            AudioSource lastAudio = null;
            int audioId = 0;

            for (int i = 0; i < soundSources.Length; i++)
            {
                if (soundSources[i] != null)
                {
                    lastAudio = soundSources[i];
                    audioId = i;
                }
                else
                {
                    soundSources[i] = lastAudio.gameObject.GetComponents<AudioSource>()[i - audioId];
                }
            }
        }

        private void CheckMusicSources()
        {
            AudioSource lastAudio = null;
            int audioId = 0;

            for (int i = 0; i < musicSources.Length; i++)
            {
                if (musicSources[i] != null)
                {
                    lastAudio = musicSources[i];
                    audioId = i;
                }
                else
                {
                    musicSources[i] = lastAudio.gameObject.GetComponents<AudioSource>()[i - audioId];
                }
            }
        }

        private void SetSafeSound(bool setTo)
        {
            if (SafeSound.instance != null)
            {
                SafeSound.instance.SetSound(setTo);
            }
        }

        public void ToggleSoundSources()
        {
            SetSafeSound(IsSoundOn());
            for (int i = 0; i < soundSources.Length; i++)
            {
                soundSources[i].mute = !IsSoundOn();
            }
        }

        public void ToggleMusicSources()
        {
            for (int i = 0; i < musicSources.Length; i++)
            {
                musicSources[i].mute = !IsMusicOn();
            }
        }

        public void SaveLocalSettings()
        {
            ObscuredPrefs.SetBool(GetGamePrefName(soundPref), soundState);
            ObscuredPrefs.SetBool(GetGamePrefName(musicPref), musicState);
        }

        public void SaveGlobalSettings()
        {
            ObscuredPrefs.SetBool(soundPrefGlobal, globalSoundState);
            ObscuredPrefs.SetBool(musicPrefGlobal, globalMusicState);
        }

        private string GetGamePrefName(string pref)
        {
            if (string.IsNullOrEmpty(gamePrefName) || string.IsNullOrEmpty(pref))
                return "";

            return "GS_" + gamePrefName + "_" + pref;
        }
    }
}