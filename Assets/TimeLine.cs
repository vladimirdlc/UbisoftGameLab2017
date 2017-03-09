using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TimeLine : MonoBehaviour
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

    void Start()
    {
        startTime = Time.time;
        latestSecond = intervalCount;
        midpoint = Screen.width / 2;
        secondsOnScreen = new List<Text>();
        var timeLine = GameObject.FindGameObjectWithTag("TimeLine");
        transform = GetComponent<RectTransform>();
        widthBetweenSeconds = Screen.width / (float)10;
        for (int i = 0; i < intervalCount + 1; ++i)
        {
            var temp = Instantiate(second);
            temp.transform.parent = timeLine.transform;
            temp.text = i.ToString();
            temp.rectTransform.anchoredPosition = new Vector2(i * widthBetweenSeconds, -5);
            secondsOnScreen.Add(temp);
        }
    }

    private int direction = 1;
    private int temp;
    void Update()
    {
        if (Input.GetKey(KeyCode.L))
        {
            direction = -direction;
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
                        temp = int.Parse(second.text);
                        second.rectTransform.anchoredPosition = Vector2.right * Screen.width;
                        second.text = (latestSecond++ + 1).ToString();
                    }
                }
                else if (direction == -1)
                {
                    if (charOrigin - widthBetweenSeconds > Screen.width)
                    {
                        latestSecond = int.Parse(second.text) - 1;
                        second.rectTransform.anchoredPosition = Vector2.left * charWidth;
                        second.text = (temp--).ToString();
                    }
                }
            }
        }

    }
}
