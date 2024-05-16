using UnityEngine;
using FluurMat.GameHelpers;

// *

namespace KMTW
{
    [DefaultExecutionOrder(-80)]
    public class PauseGame : MonoBehaviour
    {
        public static PauseGame instance { get; private set; }

        [Tooltip("Should time pausing be on on init?")] [SerializeField]
        private bool initWithTimePauseOn = true;

        private bool timePauseIsOn = true;

        public bool TimePauseIsOn
        {
            get { return timePauseIsOn; }
        }

        [Tooltip("Should time pausing be on on init?")] [SerializeField]
        private bool autoSwitchFootInputModule = true;

        void Awake()
        {
            if (!instance)
            {
                instance = this;
                timePauseIsOn = initWithTimePauseOn;
            }
            else
            {
                Destroy(this);
            }
        }


        void OnEnable()
        {
            if (timePauseIsOn)
            {
                PauseTime(true);
            }
            
            SwitchInputModules(false);
            
            if (OnPauseRaised != null)
            {
                OnPauseRaised();
            }
        }

        void OnDisable()
        {
            if (timePauseIsOn)
            {
                PauseTime(false);
            }
            
            SwitchInputModules(true);
            
            if (OnUnPauseRaised != null)
            {
                OnUnPauseRaised();
            }
        }


        public void PauseTimeIfOn()
        {
            if (timePauseIsOn)
            {
                PauseTime(true);
            }
        }

        public void UnPauseTimeIfOn()
        {
            if (timePauseIsOn)
            {
                PauseTime(false);
            }
        }

        public void SwitchInputModules(bool footOn)
        {
            if (!autoSwitchFootInputModule)
                return;
        }

        public void SetShouldPauseTimeOn()
        {
            timePauseIsOn = true;
        }

        public void SetShouldPauseTimeOff()
        {
            timePauseIsOn = true;
        }

        public void SetShouldPauseTimeTo(bool setTo)
        {
            timePauseIsOn = setTo;
        }


        public void PauseTime(bool pauseIt = true)
        {
            if (pauseIt)
            {
                Time.timeScale = 0f;
            }
            else
            {
                Time.timeScale = 1f;
            }
        }

        private void OnDestroy()
        {
            Time.timeScale = 1f;
        }

        public bool IsPaused()
        {
            return this.gameObject.activeSelf;
        }

        public delegate void PauseRaised();

        public event PauseRaised OnPauseRaised;

        public delegate void UnPauseRaised();

        public event UnPauseRaised OnUnPauseRaised;
    }
}