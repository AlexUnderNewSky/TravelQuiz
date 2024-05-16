using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using CodeStage.AntiCheat.ObscuredTypes;

namespace FluurMat.GameHelpers
{
    public enum SeasonalClipBehaviour
    {
        sequential,
        random
    }

    [Serializable]
    public class SeasonalClip
    {
        public AudioClip clip;
        public SeasonalClipBehaviour behaviour = SeasonalClipBehaviour.sequential;
        public float randomChance = 0.1f;
    }

    public class MusicScoreSeasonal : MonoBehaviour
    {
#if UNITY_EDITOR
        [SerializeField] private string overrideDate; // format 01/29 
#endif

        public AudioSource theSource { get; private set; }
        public bool audioOn = true;
        public bool seasonalOn = true;
        [Range(0.0f, 1.0f)] public float seasonalChance = 0.15f;

        [SerializeField] private SeasonalClip[] outsideSeasonClips;
        [SerializeField] private SeasonalClip[] inSeasonClips;
        private AudioClip lastClip = null;
        [SerializeField] private string seasonStart;
        [SerializeField] private string seasonEnd;
        private bool inSeason = false;

        private bool volumeControllerBusy = false;
        private Coroutine volumeCoro;

        [SerializeField] private Toggle seasonalToggle;

        [SerializeField] private UISingleSettingController seasonalSetting;

        void Start()
        {
            if (seasonalSetting)
            {
                seasonalOn = ObscuredPrefs.GetBool(seasonalSetting.ConstructPrefName(), true);
            }

            inSeason = InSeason();

            if (!theSource)
            {
                theSource = this.gameObject.GetComponent<AudioSource>();
            }

            if (!theSource)
                return;

            theSource.loop = false;

            if (outsideSeasonClips == null || outsideSeasonClips.Length == 0)
                return;

            if (audioOn)
            {
                StartCoroutine(ClipController());
            }
        }

        private DateTime ParseDate(string inDate)
        {
            if (string.IsNullOrEmpty(inDate))
            {
                return DateTime.MinValue;
            }
            
            try
            {
                return DateTime.Parse(inDate);
            }
            catch
            {
                return DateTime.MinValue;
            }
        }

        public void CheckToggle()
        {
            if (!seasonalToggle)
                return;

            seasonalOn = seasonalToggle.isOn;
        }

        private bool InSeason()
        {
            if (!seasonalOn)
                return false;

            bool isInSeason = false;

            DateTime startD = ParseDate(seasonStart);
            DateTime endD = ParseDate(seasonEnd);
            DateTime today = DateTime.Today;

#if UNITY_EDITOR
            if (!string.IsNullOrEmpty(overrideDate))
            {
                today = ParseDate(overrideDate);
            }
#endif

            if (DateTime.Compare(endD, startD) == 0)
            {
                if (DateTime.Compare(endD, today) == 0)
                    isInSeason = true;
            }
            else if (DateTime.Compare(endD, startD) > 0)
            {
                if (DateTime.Compare(today, endD) < 0 && DateTime.Compare(today, startD) > 0)
                    isInSeason = true;
            }
            else
            {
                if ((DateTime.Compare(today, endD) < 0 || DateTime.Compare(today, startD) > 0))
                    isInSeason = true;
            }

            return isInSeason;
        }

        private float clipWaitTime = 0f;

        private IEnumerator ClipController()
        {
            while (audioOn)
            {
                theSource.clip = ChooseClip();
                theSource.Play();
                clipWaitTime = theSource.clip.length;
                yield return new WaitForSecondsRealtime(clipWaitTime);
            }
        }

        private AudioClip ChooseClip()
        {
            if (theSource == null)
                return null;

            if (outsideSeasonClips == null || outsideSeasonClips.Length == 0)
                return null;

            AudioClip outClip = null;
            bool justStarted = lastClip == null;

            SeasonalClip[] clipsToChooseFrom = outsideSeasonClips;

            if (inSeason && (!justStarted || seasonalChance > 0.99f) && inSeasonClips != null &&
                inSeasonClips.Length > 1)
            {
                if (seasonalChance > 0.99f || UnityEngine.Random.value < seasonalChance)
                {
                    clipsToChooseFrom = inSeasonClips;
                }
            }

            if (justStarted)
            {
                for (int i = 0; i < clipsToChooseFrom.Length; i++)
                {
                    if (clipsToChooseFrom[i].behaviour == SeasonalClipBehaviour.sequential)
                    {
                        outClip = clipsToChooseFrom[i].clip;
                    }
                }
            }
            else
            {
                for (int i = 0; i < clipsToChooseFrom.Length; i++)
                {
                    const bool larger = false;
                    //if (UnityEngine.Random.Range(0, 2) == 0)
                    //    larger = true;

                    if (clipsToChooseFrom[i].behaviour == SeasonalClipBehaviour.random &&
                        larger
                            ? UnityEngine.Random.value > 1.0f - clipsToChooseFrom[i].randomChance
                            : UnityEngine.Random.value < clipsToChooseFrom[i].randomChance && 
                              clipsToChooseFrom[i].clip != lastClip)
                    {
                        outClip = clipsToChooseFrom[i].clip;
                    }
                }

                if (outClip == null)
                {
                    for (int i = 0; i < clipsToChooseFrom.Length; i++)
                    {
                        if (clipsToChooseFrom[i].behaviour == SeasonalClipBehaviour.sequential &&
                            clipsToChooseFrom[i].clip != lastClip)
                        {
                            outClip = clipsToChooseFrom[i].clip;
                        }
                    }
                }
            }

            if (outClip == null)
            {
                outClip = lastClip;
            }
            else
            {
                lastClip = outClip;
            }

            return outClip;
        }

        public void VolumeController(bool turnUp, float inTime = 2f)
        {
            if (volumeCoro != null)
            {
                StopCoroutine(volumeCoro);
                volumeControllerBusy = false;
                volumeCoro = null;
            }

            volumeCoro = StartCoroutine(VolumeInOut(turnUp, inTime));
        }

        private IEnumerator VolumeInOut(bool turnUp, float inTime = 2f)
        {
            if (theSource == null)
                yield break;

            volumeControllerBusy = true;

            float mult = 1f;
            float target = 1f;
            float vol = theSource.volume;

            if (!turnUp)
            {
                mult = -1f;
                target = 0f;
            }

            while (!Mathf.Approximately(vol, target))
            {
                vol = Mathf.Clamp01(vol + Time.deltaTime * mult);
                theSource.volume = vol;
                yield return null;
            }

            volumeControllerBusy = true;
        }
    }
}