using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Door : OSControllable
{
#if DEBUG_VERBOSE
    public bool hackDoors = false;
#endif

    public bool isTimed;
    public GameObject[] pressurePlates;
    public float timer;
    public bool openByDefault;

    [Header("------ Animator Specific Variables ------")]
    public float animOpenCloseDoorSpeed;

    private int count;
    private float closeAtTime;
    private bool setToClose;
    private bool isOpen;

    private Animator anim;
    private DoorSounds m_DoorSounds;

    private void Start()
    {
        count = 0;
        isOpen = false;
        closeAtTime = 0;
        setToClose = false;

        // Configure animator variables
        anim = GetComponent<Animator>();
        anim.SetFloat("objectActionSpeed",animOpenCloseDoorSpeed);

        m_DoorSounds = GetComponentInParent<DoorSounds>();

        if (openByDefault) Open();
    }

    private void Update()
    {
        if (setToClose && Time.time >= closeAtTime)
        {
            Close();
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
        if (count == pressurePlates.Length)
        {
            setToClose = true;
            closeAtTime = Time.time + timer;
            if (m_DoorSounds != null)
                m_DoorSounds.timer((int)timer);
        }

        count--;
    }

    public void Open()
    {
        setToClose = false;
        if (m_DoorSounds != null && m_DoorSounds.m_AS.isPlaying)
        {
            m_DoorSounds.m_AS.Stop();
        }
        if (isOpen)
            return;

        isOpen = true;

        if (m_DoorSounds != null)
            m_DoorSounds.close();

        TriggerAnimator(gameObject.layer == LayerMask.NameToLayer("OSControllable Nested"));
    }

    public void Close()
    {
        if (!isOpen)
            return;

        isOpen = false;
        setToClose = false;
        if (m_DoorSounds != null)
            m_DoorSounds.close();

        TriggerAnimator(gameObject.layer == LayerMask.NameToLayer("OSControllable Nested"));
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
