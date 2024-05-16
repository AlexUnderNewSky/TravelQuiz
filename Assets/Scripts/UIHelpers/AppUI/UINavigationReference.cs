using UnityEngine;
using UnityEngine.UI;

namespace KMTW
{
    public class UINavigationReference : MonoBehaviour
    {
        public Button naviReference;
        public Button thisButton;

        void Awake()
        {
            thisButton = this.GetComponent<Button>();

            if (thisButton == null)
            {
                Destroy(this);
            }
        }
    }
}