using System.Collections;
using UnityEngine;

namespace KMTW
{
    public class UINotifyTween : MonoBehaviour
    {
        [SerializeField] private UITween tween;

        private enum NotifyBehaviour
        {
            popUp,
            popBack,
            switchBackForth,
            switchForthBack
        }

        [SerializeField] private NotifyBehaviour notifyBehaviour = NotifyBehaviour.popUp;

        public bool notifyOn = false;
        private bool notifySwitcher = false;

        void Start()
        {
            StartCoroutine(SwitchOnBegin());
        }

        private IEnumerator SwitchOnBegin()
        {
            yield return new WaitForEndOfFrame();

            SwitchNotifyOn();
        }

        public void SwitchNotify()
        {
            notifyOn = !notifyOn;
        }

        public void SwitchNotifyOn()
        {
            notifyOn = true;
        }

        public void SwitchNotifyOff()
        {
            notifyOn = false;
        }

        public void Notify()
        {
            if (!notifyOn || !tween)
                return;

            switch (notifyBehaviour)
            {
                case NotifyBehaviour.popBack:
                {
                    tween.PopBack();
                    break;
                }
                case NotifyBehaviour.popUp:
                {
                    tween.PopUp();
                    break;
                }
                case NotifyBehaviour.switchBackForth:
                {
                    if (!notifySwitcher)
                    {
                        tween.PopBack();
                    }
                    else
                    {
                        tween.PopUp();
                    }

                    notifySwitcher = !notifySwitcher;
                    break;
                }
                case NotifyBehaviour.switchForthBack:
                {
                    if (!notifySwitcher)
                    {
                        tween.PopUp();
                    }
                    else
                    {
                        tween.PopBack();
                    }

                    notifySwitcher = !notifySwitcher;
                    break;
                }
            }
        }
    }
}