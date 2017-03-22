using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DogSounds : MonoBehaviour
{

    public AudioClip m_DogStepOneShot;

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
        AudioSource.PlayClipAtPoint(m_DogStepOneShot, transform.position);
    }
}
