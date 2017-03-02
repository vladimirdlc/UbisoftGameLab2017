using UnityEngine.UI;
using UnityEngine;

public class Timer : NetworkingCharacterAttachment
{
    public Text timer;
    private float spawnTime;
    public bool go = false;

    public void StartTimer()
    {
        go = true;
        spawnTime = Time.time;
        timer = GameObject.Find("Timer").GetComponent<Text>();
        if (clientsHost)
            NetMessenger.Instance.CmdStartTimer();
    }

    void Update()
    {
        if (clientsHost && !go)
        {
            StartTimer();
        }
        if (!go)
            return;
        timer.text = Mathf.Floor(Time.time - spawnTime).ToString();
    }
}
