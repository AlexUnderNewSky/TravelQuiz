using UnityEngine;

namespace KMTW
{
    public class EasyQuit : MonoBehaviour
    {
        public bool useKeyboard = false;
        public bool useTimer = false;
        public float quitTimer = 3.0f;

        [SerializeField] private KeyCode quitKey = KeyCode.Escape;

        private float currentTime = 0.0f;
        private bool timerTick = false;

        [SerializeField] private GameObject enableThis;

        void Start()
        {
            currentTime = 0.0f;
            timerTick = false;
        }

        void Update()
        {
            if (useKeyboard && useTimer)
            {
                timerTick = Input.GetKey(quitKey);
            }

            if (useTimer)
            {
                if (timerTick)
                {
                    currentTime += Time.deltaTime;
                }

                if (currentTime >= quitTimer && timerTick)
                {
                    timerTick = false;
                    if (enableThis)
                    {
                        enableThis.SetActive(true);
                    }
                    else
                    {
                        QuitIt();
                    }
                }
            }

            if (useKeyboard && !useTimer && Input.GetKey(quitKey))
            {
                QuitIt();
            }
        }

        public void OnPress()
        {
            currentTime = 0.0f;
            timerTick = true;
        }

        public void OnRelease()
        {
            currentTime = 0.0f;
            timerTick = false;
        }

        public void QuitIt()
        {
#if UNITY_EDITOR
            Debug.Log("<ed_only>" + "Quitting App");
#endif

            Application.Quit();
        }
    }
}