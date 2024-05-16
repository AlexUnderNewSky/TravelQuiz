using UnityEngine;
using UnityEngine.UI;

// *

namespace FluurMat.GameHelpers
{
    /// <summary>
    /// Plays a sound from an audio source by also avoiding duplicates.
    /// </summary>
    public class PlaySoundSafe : MonoBehaviour
    {
        [Tooltip("The sound to play.")] [SerializeField]
        private AudioClip sound;

        [Tooltip("Play the sound when the object is activated?")] [SerializeField]
        private bool playOnStart = false;

        [Tooltip("Play the sound when the object is spawned?")] [SerializeField]
        private bool playOnSpawn = true;

        [Tooltip("Play the sound when the button is pressed (needs to have a button component).")] [SerializeField]
        private bool playOnButtonPress = false;

        [Tooltip("A random range for the pitch of the audio source, to make the sound more varied.")] [SerializeField]
        private Vector2 pitchRange = new Vector2(0.9f, 1.1f);

        [Tooltip("Defualt Pool, -1 is choose whichever is free.")] [SerializeField]
        private int targetPool = -1;

        private Button thisButton;

        private void Awake()
        {
            if (playOnButtonPress)
            {
                thisButton = this.gameObject.GetComponent<Button>();

                if (thisButton != null)
                {
                    thisButton.onClick.AddListener(PlayDefaultSound);
                }
                else
                {
                    playOnButtonPress = false;
                }
            }
        }

        private void Start()
        {
            if (playOnStart)
            {
                PlaySound(sound);
            }
        }

        public void PlayDefaultSound()
        {
            PlaySound(sound);
        }

        public void PlaySound(AudioClip sound)
        {
            if (SafeSound.instance != null)
            {
                SafeSound.instance.SafeToPlaySource(sound, targetPool,
                    Random.Range(pitchRange.x, pitchRange.y)); // Play the sound
            }
        }

        void OnSpawned()
        {
            if (playOnSpawn)
            {
                PlaySound(sound);
            }
        }
    }
}