////////////////////////////////////////////////////////////////////////////////////
// Experiment: Risk Aversion
// Purpose: Create a game object that persists through scenes and can be referenced for general data
// Code by: Butler, Michael
// Email: msbcoding@gmail.com
////////////////////////////////////////////////////////////////////////////////////
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SubjectSettings : MonoBehaviour {

    public static SubjectSettings instance;

    public bool debug;

    [HideInInspector] public string SubjectID = "", Age = "", Gender = "", ShWidth = "", FilePath = "";

    [HideInInspector] public OVRScreenFade Fader = new OVRScreenFade();

    void Awake()
    {

        if (instance != null && instance != this)
        {
            Destroy(this.gameObject);
            return;
        }
        else
        {
            instance = this;
            if(debug == true)
            {
                CreateDebugVersion();
            }
        }
        DontDestroyOnLoad(this.gameObject);
    }


    public void US_SubjectID(string Updater)
    {
        SubjectID = Updater;
    }

    public void US_Age(string Updater)
    {
        Age = Updater;
    }

    public void US_Gender(string Updater)
    {
        Gender = Updater;
    }

    public void US_ShWidth(string Updater)
    {
       float x = float.Parse(Updater);
        x = (x / 2) / 100;
        ShWidth = x.ToString();
    }

    public void DebugOutputter()
    {
        Debug.Log("Debug Log Call > SubjectSettings > DebugOutputter");
        Debug.Log("ID: " + SubjectID);
        Debug.Log("Age: " + Age);
        Debug.Log("Gender: " + Gender);
        Debug.Log("ShoulderWidthRadius: " + ShWidth);
    }

    public void CreateDebugVersion()
    {
        SubjectID = "1";
        Age = "0";
        Gender = "null";
        ShWidth = "30";
    }

}
