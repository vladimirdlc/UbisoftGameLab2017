using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Door : OSControllable
{
#if DEBUG_VERBOSE
    public bool hackDoors = false;
#endif

    public GameObject meshes;
    public bool isTimed;
    public GameObject[] pressurePlates;
    public float timer;
    public bool openByDefault;

    [Header("------ Animator Specific Variables ------")]
    public float animOpenCloseDoorSpeed;

    private int count;
    private float closeAtTime;
    private bool isOpen;

    private Animator anim;
    private AudioSource m_AudioSource;

    private void Start()
    {
        count = 0;
        isOpen = false;
        closeAtTime = 0;

        // Configure animator variables
        anim = GetComponent<Animator>();
        anim.SetFloat("objectActionSpeed",animOpenCloseDoorSpeed);

        m_AudioSource = GetComponent<AudioSource>();

        if (openByDefault) Open();
    }

    private void Update()
    {
        if (isOpen && isTimed)
        {
            if (Time.time >= closeAtTime) Close();
        }

#if DEBUG_VERBOSE 
        if (Input.GetButtonDown("Overseer B") && hackDoors)
        {
            TriggerAction();
        }
#endif
    }

    public void IncCount()
    {
        count++;
        if (count == pressurePlates.Length)
            Open();
    }

    public void DecCount()
    {
        count--;
    }

    public void Open()
    {
        isOpen = true;
        closeAtTime = Time.time + timer;

        if (m_AudioSource)
            m_AudioSource.Play();

        TriggerAnimator();
    }

    public void Close()
    {
        isOpen = false;

        if (m_AudioSource)
            m_AudioSource.Play();

        TriggerAnimator();
    }

    public override void TriggerAction()
    {
        if (!isOpen)
        {
            Open();
        }
        else if (isOpen)
        {
            Close();
        }
    }
}
