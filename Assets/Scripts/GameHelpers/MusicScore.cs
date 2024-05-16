using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace FluurMat.GameHelpers
{
    [DefaultExecutionOrder(-77)]
    public class MusicScore : MonoBehaviour
    {
        public static MusicScore instance { get; private set; }
        public AudioSource theSource { get; private set; }

        [Tooltip("Set music groups and behaviours.")] [SerializeField]
        private MusicGroup[] musicGroups;

        private int currentGroup = 0;

        [Tooltip("Special clip for intro (will auto play on start is AutoPlay is on.")] [SerializeField]
        private AudioClip musicIntro;

        [Tooltip("Quick music scores for events.")] [SerializeField]
        private AudioClip[] stingers;

        [Tooltip("Should it start playing if on or wiat external?")] [SerializeField]
        private bool autoStart = true;

        private bool canStart = false;
        private bool started = false;

        [Tooltip("Allow instance in scene (usually for ambient)?")] [SerializeField]
        private bool noSingleton = false;

        public bool NoSingletonPattern
        {
            get { return noSingleton; }
        }

        [Serializable]
        public class MusicGroup
        {
            public AudioClip[] musicTracks;
            public bool mustPlayAll = true;
            public bool playTracksInSequence = false;
            private int currentTrack = 0;
            private List<int> trackList = new List<int>();

            public MusicGroup(AudioClip[] tracks, bool playall, bool inasequence)
            {
                musicTracks = tracks;
                mustPlayAll = playall;
                playTracksInSequence = inasequence;
            }

            public void PrepareTracks()
            {
                if (musicTracks == null || musicTracks.Length < 1)
                    return;

                if (trackList == null)
                {
                    trackList = new List<int>();
                }
                else
                {
                    trackList.Clear();
                }

                currentTrack = 0;

                int numTracks = musicTracks.Length;
                if (!mustPlayAll)
                {
                    numTracks = UnityEngine.Random.Range(1, musicTracks.Length);
                }

                if (playTracksInSequence)
                {
                    for (int i = 0; i < numTracks; i++)
                    {
                        trackList.Add(i);
                    }
                }
                else
                {
                    List<int> tempChoice = new List<int>();
                    for (int i = 0; i < musicTracks.Length; i++)
                    {
                        tempChoice.Add(i);
                    }

                    for (int i = 0; i < numTracks; i++)
                    {
                        int randomChoice = UnityEngine.Random.Range(0, tempChoice.Count);
                        trackList.Add(tempChoice[randomChoice]);
                        tempChoice.RemoveAt(randomChoice);
                    }
                }
            }

            public bool IsLastTrack()
            {
                // also checks if tracks have been altered extrnally
                return (currentTrack + 1 > trackList.Count - 1);
            }

            public void NextTrack()
            {
                currentTrack += 1;

                if (currentTrack > trackList.Count - 1)
                {
                    if (playTracksInSequence)
                    {
                        currentTrack = 0;
                    }
                    else
                    {
                        PrepareTracks();
                    }
                }
            }

            public AudioClip CurrentTrack()
            {
                return musicTracks[trackList[currentTrack]];
            }
        }

        private void Awake()
        {
            if (noSingleton)
                return;

            if (!instance || instance.NoSingletonPattern)
            {
                instance = this;
            }
            else
            {
                Destroy(this);
            }
        }

        void Start()
        {
            if (!theSource)
            {
                theSource = this.gameObject.GetComponent<AudioSource>();
            }

            if (!theSource)
                return;

            theSource.loop = false;

            // aka start from Unity main
            if (!canStart && !started)
            {
                currentGroup = UnityEngine.Random.Range(0, musicGroups.Length);
                musicGroups[currentGroup].PrepareTracks();
            }

            if (!autoStart && !canStart)
                return;

            started = true;

            if (musicIntro != null)
            {
                theSource.clip = musicIntro;
                theSource.Play();
                return;
            }

            theSource.clip = musicGroups[currentGroup].CurrentTrack();
            theSource.Play();
        }

        public void StartMusicScore()
        {
            if (autoStart || started)
                return;

            canStart = true;
            Start();
        }

        public void FadeToStinger(int which, bool endAfterPlay = false, float inSec = 1f)
        {
            if (stingers == null || stingers.Length <= which)
                return;

            StopAllCoroutines();

            StartCoroutine(FadeToClip(stingers[which], endAfterPlay, inSec));
        }

        private IEnumerator FadeToClip(AudioClip track, bool endAfterPlay = false, float inSec = 1f)
        {
            if (!theSource)
                yield break;

            float maxVol = theSource.volume;
            float curVol = maxVol;

            if (maxVol <= 0f)
                yield break;

            inSec = Mathf.Clamp(inSec, 0.2f, 5.0f);
            float step = Time.fixedDeltaTime / inSec;

            while (curVol > 0f)
            {
                curVol -= step;
                curVol = Mathf.Max(0f, curVol);
                theSource.volume = curVol;
                yield return new WaitForSecondsRealtime(step);
            }

            theSource.Stop();
            theSource.clip = track;
            theSource.Play();
            started = !endAfterPlay;

            while (curVol < maxVol)
            {
                curVol += step;
                curVol = Mathf.Min(maxVol, curVol);
                theSource.volume = curVol;
                yield return new WaitForSecondsRealtime(step);
            }
        }

        void Update()
        {
            if (!theSource)
                return;

            if (!theSource.isPlaying && started)
            {
                if (musicGroups[currentGroup].IsLastTrack())
                {
                    currentGroup = UnityEngine.Random.Range(0, musicGroups.Length);
                    musicGroups[currentGroup].PrepareTracks();
                    theSource.clip = musicGroups[currentGroup].CurrentTrack();
                    theSource.Play();
                }
                else
                {
                    musicGroups[currentGroup].NextTrack();
                    theSource.clip = musicGroups[currentGroup].CurrentTrack();
                    theSource.Play();
                }
            }
        }
    }
}