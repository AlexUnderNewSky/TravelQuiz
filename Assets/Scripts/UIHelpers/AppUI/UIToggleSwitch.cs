using UnityEngine;
using UnityEngine.UI;

public class UIToggleSwitch : MonoBehaviour
{
    public bool isOn = false;

    void Awake()
    {
        this.GetComponent<Toggle>().isOn = isOn;
    }

    public void Toggle()
    {
        this.GetComponent<Toggle>().isOn = !this.GetComponent<Toggle>().isOn;
        isOn = this.GetComponent<Toggle>().isOn;
    }
}