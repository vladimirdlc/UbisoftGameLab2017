using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SelfDestruct : MonoBehaviour
{

    public float selfDesctructCountDown;
    public bool startTimerByDefault = false;
    public bool onlyDeactivate;

    private float timer;
    private bool started;

    // Use this for initialization
    void Start()
    {
        timer = selfDesctructCountDown;
        started = startTimerByDefault;
    }

    // Update is called once per frame
    void Update()
    {
        if (started)
        {
            timer -= Time.deltaTime;

            if (timer <= 0)
                if (onlyDeactivate)
                    gameObject.SetActive(false);
                else
                    Destroy(gameObject);
        }
    }

    public void StartSelfDestruct()
    {
        if (!started)
            started = true;
    }
}
