using UnityEngine;
using FluurMat.GameHelpers;

namespace FluurMat.UI
{
    public class UIHighlightLanguage : MonoBehaviour
    {
        [SerializeField] private Transform checkChildren;
        ToggleButton[] itemsToCheck;

        void OnEnable()
        {
            OnLocalize();
        }

        public void OnLocalize()
        {
            if (!checkChildren)
                return;

            var language = I2.Loc.LocalizationManager.CurrentLanguage;

            if (itemsToCheck == null)
            {
                itemsToCheck = checkChildren.GetComponentsInChildren<ToggleButton>();
            }

            if (itemsToCheck == null)
                return;

            for (int i = 0; i < itemsToCheck.Length; i++)
            {
                if (itemsToCheck[i].gameObject.name == language)
                {
                    itemsToCheck[i].DoUpdate(false);
                }
                else
                {
                    itemsToCheck[i].DoUpdate(true);
                }
            }
        }
    }
}