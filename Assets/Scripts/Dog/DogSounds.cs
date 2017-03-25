using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DogSounds : MonoBehaviour
{

    public AudioClip m_DogStepOneShot;
    public bool bypass;

    // Use this for initialization
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {

    }

    public void DogStepOneShot()
    {
        if (bypass)
            return;
        AudioSource.PlayClipAtPoint(m_DogStepOneShot, transform.position);
    }
}
