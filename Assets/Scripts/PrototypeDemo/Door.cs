using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Door : MonoBehaviour, OSControllable {

    public GameObject mesh;
    public bool isTimed;
    public GameObject[] pressurePlates;
    public float timer;
    public float coolDown;
    public bool openByDefault;


    private int count;
    private float closeAtTime;
    private float readyAtTime;
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
    }

    public void IncCount() {
        count++;
        if (count == pressurePlates.Length)
            Open();
    }

    public void DecCount() {
        count--;
    }

    public void Open() {
        isOpen = true;
        readyAtTime = Time.time + coolDown;
        closeAtTime = Time.time + timer;
        mesh.GetComponent<Transform>().Translate(new Vector3(0, 3, 0));
    }

    public void Close() {
        isOpen = false;
        readyAtTime = Time.time + coolDown;
        mesh.GetComponent<Transform>().Translate(new Vector3(0, -3, 0));
    }

    public void triggerAction()
    {
        if (!isOpen && Time.time >= readyAtTime)
        {
            Open();
        }
        else if (isOpen && Time.time >= readyAtTime)
        {
            Close();
        }
    }
}
