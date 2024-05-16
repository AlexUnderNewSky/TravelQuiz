using UnityEngine;
using UnityEngine.UI;
using CodeStage.AntiCheat.ObscuredTypes;

namespace FluurMat.GameHelpers
{
    public class UISingleSettingController : MonoBehaviour
    {
        [SerializeField] private string gamePrefName = "FM";
        [SerializeField] private string settingName = "";

        [SerializeField] private bool autoSave = true;
        [SerializeField] private bool autoLoad = true;

        [SerializeField] private Slider thisSlider;
        [SerializeField] private Toggle thisToggle;

        private void Awake()
        {
            if (thisSlider == null)
            {
                thisSlider = gameObject.GetComponent<Slider>();
            }

            if (thisToggle == null)
            {
                thisToggle = gameObject.GetComponent<Toggle>();
            }
        }

        void Start()
        {
            if (autoLoad)
            {
                LoadSetting();
            }
        }

        private void OnDisable()
        {
            if (autoSave)
            {
                SaveSetting();
            }
        }

        public void SaveSetting()
        {
            if (thisSlider != null)
            {
                ObscuredPrefs.SetFloat(ConstructPrefName(), thisSlider.value);
            }
            else if (thisToggle != null)
            {
                ObscuredPrefs.SetBool(ConstructPrefName(), thisToggle.isOn);
            }
        }

        public void LoadSetting()
        {
            if (thisSlider != null)
            {
                float defValue = thisSlider.value;
                float curValue = ObscuredPrefs.GetFloat(ConstructPrefName(), defValue);
                if (!Mathf.Approximately(thisSlider.value, curValue))
                {
                    thisSlider.value = curValue;
                }
            }
            else if (thisToggle != null)
            {
                bool defValue = thisToggle.isOn;
                bool curValue = ObscuredPrefs.GetBool(ConstructPrefName(), defValue);
                if (thisToggle.isOn != curValue)
                {
                    thisToggle.isOn = curValue;
                }
            }
        }

        public string ConstructPrefName()
        {
            return "GS_" + gamePrefName + "_" + settingName;
        }

        public bool GetSettingBoolValue()
        {
            if (!thisToggle)
            {
                return false;
            }

            bool defValue = thisToggle.isOn;
            return ObscuredPrefs.GetBool(ConstructPrefName(), defValue);
        }

        public float GetSettingFlatValue()
        {
            if (!thisSlider)
            {
                return -1.0f;
            }

            float defValue = thisSlider.value;
            return ObscuredPrefs.GetFloat(ConstructPrefName(), defValue);
        }
    }
}