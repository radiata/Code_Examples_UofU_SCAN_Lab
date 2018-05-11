////////////////////////////////////////////////////////////////////////////////////
// Experiment: Risk Aversion
// Purpose: Functions meant to control buttons presses
// TO DO NOTE: Expand functionality to cover general common needs and add to MenuPackage
// Code by: Butler, Michael
// Email: msbcoding@gmail.com
////////////////////////////////////////////////////////////////////////////////////
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GeneralButtons : MonoBehaviour {

    public void Exit()
    {
        #if UNITY_STANDALONE
            Application.Quit();
        #endif

        #if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
            Debug.Log("Exit Button Clicked");
        #endif
    }

    //Send the level name as a string and it will attempt to load that level"
    public void LoadLevel(string input)
    {
        Application.LoadLevel(input);
    }

}

