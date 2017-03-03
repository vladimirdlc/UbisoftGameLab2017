using UnityEngine.UI;
using UnityEngine;

public class Timer : NetworkingCharacterAttachment
{
    public Text timer;
    private float spawnTime;
    public bool go = false;

    public void StartTimer()
    {
        timer = GameObject.FindGameObjectWithTag("Timer").GetComponent<Text>();

        go = true;
        spawnTime = Time.time;
        timer.color = Color.white;
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
