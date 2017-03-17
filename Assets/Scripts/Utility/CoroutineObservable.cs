using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ParadoxWarningCoroutine : MonoBehaviour {

    bool executing;
    float warningMessageTime;

    private TimeManager timeManager;

    public ParadoxWarningCoroutine(float warningMessageTime)
    {
        this.warningMessageTime = warningMessageTime;
    }

    public void PlayWarningMessage()
    {
        StartCoroutine(WarningMessage(warningMessageTime));
    }

    public IEnumerator WarningMessage(float warningMessageTime)
    {
        executing = true;
        Debug.Log("In warning message");
        yield return new WaitForSeconds(warningMessageTime);
        executing = false;
    }

    public IEnumerator WaitUntilDone()
    {
        while (executing)
            yield return new WaitForSeconds(0.1f);
    }
}
