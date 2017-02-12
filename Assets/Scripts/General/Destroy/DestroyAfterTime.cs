﻿using UnityEngine;
using System.Collections;

public class DestroyAfterTime : MonoBehaviour
{
    private float currentTimeAlive;
    public float maxTimeAlive = 2.0f;

    void Update()
    {
        currentTimeAlive += Time.deltaTime;

        if (currentTimeAlive > maxTimeAlive)
        {
            Destroy(gameObject);
        }
    }
}