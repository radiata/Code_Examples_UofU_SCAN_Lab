////////////////////////////////////////////////////////////////////////////////////
// Experiment: Risk Aversion
// Purpose: Simple trial and time trackers for the canvas
// **Easily portable, see prefabs for setup example
// Code by: Butler, Michael
// Email: msbcoding@gmail.com
////////////////////////////////////////////////////////////////////////////////////
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Time_Trials : MonoBehaviour {

    public Text DiagText;
    [HideInInspector] public static int TrialCounter;

    private string TotalTime, TrialTime;

    private static float currentTime = 0, trialTime;
    System.TimeSpan t;

    // Use this for initialization
    void Start()
    {
        currentTime = 0;
        trialTime = 0;
        t = System.TimeSpan.FromSeconds(currentTime);
    }

    // Update is called once per frame
    void Update()
    {
        currentTime += Time.deltaTime;
        t = System.TimeSpan.FromSeconds(currentTime);

        TotalTime = string.Format("{0:D2}:{1:D2}:{2:D2}",
                        t.Hours,
                        t.Minutes,
                        t.Seconds);

        /*
        trialTime += Time.deltaTime;
        t = System.TimeSpan.FromSeconds(trialTime);

        TrialTime = string.Format("{0:D2}:{1:D2}:{2:D2}",
                        t.Hours,
                        t.Minutes,
                        t.Seconds);
        */

        DiagText.text = "Trial Number: " + TrialCounter.ToString() + '\n' +
                        "Time (total): " + TotalTime + '\n'; //+
                        //"Time (trial): " + TrialTime;
    }

    public static void ResetTrialTime()
    {
        trialTime = 0;
    }
}
