using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TimeLine : MonoBehaviour
{

    new RectTransform transform;
    void Start()
    {
        transform = GetComponent<RectTransform>();

    }
    void Update()
    {
        transform.anchoredPosition += Vector2.right * Time.deltaTime * 10;
    }
}
