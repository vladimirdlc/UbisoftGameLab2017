using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OSCinematicTarget : MonoBehaviour {
    public float timeToChange = 2;
    public float currentTime;
    public OverseerTarget target;
    public bool isCounting;

    private void Start()
    {
        currentTime = 0;
    }

    private void Update()
    {
        if (!isCounting) return;

        currentTime += Time.deltaTime;
        if (currentTime >= timeToChange)
        {
            OverseerCamera.Instance.setNewTargetRaw(target);
            isCounting = false;
        }
    }

    public void startCount()
    {
        isCounting = true;
    }
}
