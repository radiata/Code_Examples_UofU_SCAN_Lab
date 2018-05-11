using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class Timer : MonoBehaviour {

    public Text timerText;

    private float currentTime = 0;

	// Use this for initialization
	void Start () {
        currentTime = 0;

        System.TimeSpan t = System.TimeSpan.FromSeconds(currentTime);

        string answer = string.Format("{0:D2}:{1:D2}:{2:D2}",
                        t.Hours,
                        t.Minutes,
                        t.Seconds);

        //timerText.text = ((int)(currentTime / 60)).ToString() + ":" + ((int)(currentTime % 60)).ToString();
        timerText.text = answer;
    }
	
	// Update is called once per frame
	void Update () {
        currentTime += Time.deltaTime;
        System.TimeSpan t = System.TimeSpan.FromSeconds(currentTime);

        string answer = string.Format("{0:D2}:{1:D2}:{2:D2}",
                        t.Hours,
                        t.Minutes,
                        t.Seconds);

        //timerText.text = ((int)(currentTime / 60)).ToString() + ":" + ((int)(currentTime % 60)).ToString();
        timerText.text = answer;
    }
}
