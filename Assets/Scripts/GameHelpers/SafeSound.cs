using UnityEngine;

namespace FluurMat.GameHelpers
{
    [DefaultExecutionOrder(-79)]
    public class SafeSound : MonoBehaviour
    {
        [Tooltip("Max virtual sounds. " +
                 "Those are attached to respective game objects and will not play if this number is exceeded.")]
        [SerializeField]
        private int maxSounds = 5;

        [Tooltip("Sound Pools = Sound Sources attached to this gameObject.")] [SerializeField]
        private int soundPools = 1;

        public static SafeSound instance { get; private set; }
        public AudioSource[] audioSources { get; private set; }
        public int scheduledVirtualSounds { get; private set; }

        [Tooltip("Virtual Pools with maxSounds each.")] [SerializeField]
        private int[] virtualSoundPools = new int[5]; // Only numbers; can be used by external sound sources

        [Tooltip("The default sound; useful for buttons.")] [SerializeField]
        private AudioClip defaultSound;

        public bool soundEnabled { get; private set; }

        void Awake()
        {
            if (!instance)
            {
                instance = this;

                maxSounds = Mathf.Clamp(maxSounds, 1, 16);
                soundPools = Mathf.Clamp(soundPools, 1, 16);
                ResetSoundSources();
            }
            else
            {
                Destroy(this);
            }

            if (defaultSound == null)
            {
                if (audioSources.Length > 0)
                {
                    defaultSound = audioSources[0].clip;
                }
            }
        }

        void Start()
        {
            scheduledVirtualSounds = 0;
            for (int i = 0; i < virtualSoundPools.Length; i++)
            {
                virtualSoundPools[i] = 0;
            }
        }

        private void ResetSoundSources()
        {
            AudioSource[] currentSources = this.gameObject.GetComponents<AudioSource>();
            int curAudioSourcesLen = currentSources.Length;

            AudioSource latestPresent = null;
            int latestId = 0;

            if (curAudioSourcesLen > 0)
            {
                latestId = currentSources.Length - 1;
                latestPresent = currentSources[latestId];
            }

            if (curAudioSourcesLen > soundPools)
            {
                for (int i = soundPools; i < curAudioSourcesLen; i++)
                {
                    Destroy(currentSources[i]);
                }
            }
            else if (soundPools > curAudioSourcesLen)
            {
                for (int i = curAudioSourcesLen; i < soundPools; i++)
                {
                    AudioSource newSource = this.gameObject.AddComponent<AudioSource>();
                    if (latestPresent)
                    {
                        newSource.pitch = latestPresent.pitch;
                        newSource.volume = latestPresent.volume;
                        newSource.priority = latestPresent.priority;
                        newSource.spatialBlend = latestPresent.spatialBlend;
                        newSource.panStereo = latestPresent.panStereo;
                    }
                }
            }

            audioSources = this.gameObject.GetComponents<AudioSource>();
        }

        public void SetMaxSounds(int setTo)
        {
            maxSounds = Mathf.Clamp(setTo, 1, 16);
        }

        public void SetPools(int setTo)
        {
            soundPools = Mathf.Clamp(setTo, 1, 16);
            ResetSoundSources();
        }

        public void SetSound(bool setTo)
        {
            if (soundEnabled != setTo)
            {
                for (int i = 0; i < audioSources.Length; i++)
                {
                    audioSources[i].mute = !setTo;
                }
            }

            soundEnabled = setTo;
        }

        public bool SafeToPlay(int pool = -1)
        {
            if (!soundEnabled)
                return false;

            if (pool < 0 || pool >= virtualSoundPools.Length)
            {
                if (scheduledVirtualSounds < maxSounds)
                {
                    scheduledVirtualSounds += 1;
                    return true;
                }
                else
                {
                    return false;
                }
            }
            else
            {
                if (virtualSoundPools[pool] < maxSounds)
                {
                    virtualSoundPools[pool] += 1;
                    return true;
                }
                else
                {
                    return false;
                }
            }
        }

        public bool SafeToPlaySource(AudioClip clipToPlay, int pool = 0, float pitch = 1f)
        {
            if (!soundEnabled)
                return false;

            if (audioSources == null || audioSources.Length == 0)
                return false;

            if (clipToPlay == null)
            {
                if (defaultSound != null)
                {
                    clipToPlay = defaultSound;
                }
                else
                {
                    return false;
                }
            }

            if (pitch < 0f)
            {
                pitch = Random.Range(0.9f, 1.1f);
            }

            if (pool < 0 || pool >= audioSources.Length)
            {
                bool didPlay = false;

                for (int i = 0; i < audioSources.Length; i++)
                {
                    if (!audioSources[i].isPlaying)
                    {
                        if (!Mathf.Approximately(pitch, 1f))
                        {
                            audioSources[i].pitch = pitch;
                        }
                        audioSources[i].PlayOneShot(clipToPlay);
                        didPlay = true;
                        break;
                    }
                }

                return didPlay;
            }
            else
            {
                if (!audioSources[pool].isPlaying)
                {
                    if (!Mathf.Approximately(pitch, 1f))
                    {
                        audioSources[pool].pitch = pitch;
                    }
                    audioSources[pool].PlayOneShot(clipToPlay);
                    return true;
                }
                else
                {
                    return false;
                }
            }
        }

        public void PlayUnsafe(AudioClip clipToPlay, int pool = -1, float pitch = 1f)
        {
            if (!soundEnabled)
                return;

            if (audioSources == null || audioSources.Length == 0)
                return;


            if (clipToPlay == null)
            {
                if (defaultSound != null)
                {
                    clipToPlay = defaultSound;
                }
                else
                {
                    return;
                }
            }

            bool played = false;

            if (pitch < 0f)
            {
                pitch = Random.Range(0.9f, 1.1f);
            }

            if (pool < 0 || pool >= audioSources.Length)
            {
                for (int i = 0; i < audioSources.Length; i++)
                {
                    if (!audioSources[i].isPlaying)
                    {
                        if (!Mathf.Approximately(pitch, 1f))
                        {
                            audioSources[i].pitch = pitch;
                        }

                        audioSources[i].PlayOneShot(clipToPlay);
                        played = true;
                        break;
                    }
                }

                pool = 0;
            }

            if (!played)
            {
                if (!Mathf.Approximately(pitch, 1f))
                {
                    audioSources[pool].pitch = pitch;
                }
                audioSources[pool].PlayOneShot(clipToPlay);
            }
        }

        void LateUpdate()
        {
            scheduledVirtualSounds = Mathf.Max(0, scheduledVirtualSounds - 1);

            for (int i = 0; i < virtualSoundPools.Length; i++)
            {
                virtualSoundPools[i] = Mathf.Max(0, scheduledVirtualSounds - 1);
            }
        }
    }
}