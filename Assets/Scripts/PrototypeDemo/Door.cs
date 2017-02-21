using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Door : OSControllable
{

    public GameObject mesh;
    public bool isTimed;
    public GameObject[] pressurePlates;
    public float timer;
    public bool openByDefault;


    private int count;
    private float closeAtTime;
    private bool isOpen;

    private void Start()
    {
        count = 0;
        isOpen = false;
        closeAtTime = 0;
        if (openByDefault) Open();
    }

    private void Update()
    {
        if (isOpen && isTimed)
        {
            if (Time.time >= closeAtTime) Close();
        }

        
        if (Input.GetButtonDown("Test Button"))
        {
            TriggerAction();
        }
        

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

        TriggerAnimator();
    }

    public void Close()
    {
        isOpen = false;

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
