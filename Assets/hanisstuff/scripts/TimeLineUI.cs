﻿using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TimeLineUI : MonoBehaviour
{
    //hide the usual transform since it doesn't make since for UI
    new RectTransform transform;

    public float widthBetweenSeconds;
    //10 intervals should mean 11 positions, starting at 0s
    public int intervalCount = 10;
    public Text second;
    private float midpoint;
    private List<Text> secondsOnScreen;
    private int latestSecond;
    private float startTime;
    private int yOffset = -5;
    private Image playerTimeLine;
    private GameObject timeLine;

    void Start()
    {
        playerTimeLine = GameObject.FindGameObjectWithTag("PlayerTimeLine").GetComponent<Image>();
        //once drawn the first time should never be touched again.
        playerTimeLine.rectTransform.localScale = new Vector3(Screen.width, 55, 1);
        //thisTimeLine = GetComponent<LineRenderer>();
        startTime = Time.time;
        latestSecond = intervalCount;
        midpoint = Screen.width / 2;
        //thisTimeLine.SetPosition(0, new Vector2(0, -10));
        //thisTimeLine.SetPosition(1, new Vector2(midpoint, -10));
        secondsOnScreen = new List<Text>();
        timeLine = GameObject.FindGameObjectWithTag("TimeLine");
        transform = GetComponent<RectTransform>();
        widthBetweenSeconds = Screen.width / (float)10;
        for (int i = 0; i < intervalCount + 1; ++i)
        {
            var temp = Instantiate(second);
            temp.transform.SetParent(timeLine.transform);
            temp.text = i.ToString();
            temp.rectTransform.anchoredPosition = new Vector2(i * widthBetweenSeconds, yOffset);
            secondsOnScreen.Add(temp);
        }
    }

    private int direction = 1;
    private int earliestSecond;
    private List<CloneUI> cloneWarpoutTimes = new List<CloneUI>();

    public class CloneUI
    {
        public Text timeS;
        public float timeF;
        public CloneUI(Text t, float f)
        {
            timeS = t;
            timeF = f;
        }
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.L))
        {
            direction = -direction;
            var cloneWarpoutTime = Time.time - startTime;
            startTime = Time.time;

            var temp = Instantiate(second);
            temp.transform.SetParent(timeLine.transform);
            temp.text = cloneWarpoutTime.ToString();
            temp.rectTransform.anchoredPosition = new Vector2(0, -100);
            cloneWarpoutTimes.Add(new CloneUI(temp, cloneWarpoutTime));
        }
        for (int i = 0; i < cloneWarpoutTimes.Count; ++i)
        {
            cloneWarpoutTimes[i].timeS.text = (cloneWarpoutTimes[i].timeF -= Time.deltaTime).ToString();
            //if (cloneWarpoutTimes[i].timeF < 0)


        }



        foreach (var second in secondsOnScreen)
        {
            animate(second);
        }

        if (transform.anchoredPosition.x < midpoint)
        {

            //transform.anchoredPosition = Vector2.right * direction * widthBetweenSeconds * (Time.time - startTime);
            transform.anchoredPosition += Vector2.right * direction * widthBetweenSeconds * Time.deltaTime;
            if (transform.anchoredPosition.x < 0)
                transform.anchoredPosition = Vector2.zero;
        }
        else
        {
            foreach (var second in secondsOnScreen)
            {
                second.rectTransform.anchoredPosition += Vector2.left * direction * widthBetweenSeconds * Time.deltaTime;
                var charOrigin = second.rectTransform.anchoredPosition.x;
                //In pixels
                //fix this for more the one character!!
                var charWidth = second.font.characterInfo[0].advance;
                //var charWidth = second.font.cha


                if (direction == 1)
                {
                    if (charOrigin + charWidth + widthBetweenSeconds < 0)
                    {
                        earliestSecond = int.Parse(second.text);
                        second.rectTransform.anchoredPosition = new Vector2(Screen.width, yOffset);
                        second.text = (latestSecond++ + 1).ToString();
                    }
                }
                else if (direction == -1)
                {
                    if (second.text == "0" && second.rectTransform.anchoredPosition.x > 0)
                    {
                        transform.anchoredPosition += Vector2.right * -1;
                        return;
                    }
                    else if (charOrigin - widthBetweenSeconds > Screen.width)
                    {
                        latestSecond = int.Parse(second.text) - 1;
                        second.rectTransform.anchoredPosition = new Vector2(-charWidth, yOffset);
                        second.text = (earliestSecond--).ToString();
                    }
                }
            }
        }

    }

    void animate(Text second)
    {
        var currPos = second.rectTransform.position.x;
        //10 percent of the screen
        var offsetFromMidpoint = Screen.width / 10;
        var tickPosition = transform.position.x;
        if (currPos < tickPosition + offsetFromMidpoint && currPos > tickPosition - offsetFromMidpoint)
        {
            var distanceFromMidpoint = Mathf.Abs(currPos - tickPosition);
            var ratio = (offsetFromMidpoint - distanceFromMidpoint) / offsetFromMidpoint;
            second.fontSize = 15 + (int)(ratio * 15);
        }
    }
}