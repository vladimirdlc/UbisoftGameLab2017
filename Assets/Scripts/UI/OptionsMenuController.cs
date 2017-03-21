using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class OptionsMenuController : MonoBehaviour {

    public Slider sensitivityX;
    public Slider sensitivityY;
    public Toggle invertYAxis;

    //private void Update()
    //{
    //    Debug.Log(PlayerPrefs.GetFloat("SensitivityX"));
    //}

    public void UpdateSensitivityX()
    {
        PlayerPrefs.SetFloat("SensitivityX", sensitivityX.value);
    }

    public void UpdateSensitivityY()
    {
        PlayerPrefs.SetFloat("SensitivityY", sensitivityX.value);
    }

    public void UpdateInveryYAxis()
    {
        PlayerPrefs.SetInt("InvertY", invertYAxis.isOn ? 1 : -1);
    }
}
