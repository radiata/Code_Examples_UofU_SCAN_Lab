using UnityEngine;
using System.Collections;
using System;

//notes:
//1. Not sure if pause is suspending the data recording. Theoretically it should, but it hasnt been tested yet.

public class PauseMenu : MonoBehaviour {

    private bool isPaused;
    private Rect pauseWindow;

	// Use this for initialization
	void Start ()
    {
        isPaused = false;

        //sets the size and position of the pause window that the test administrator will see.
        pauseWindow = new Rect(Screen.width / 10, Screen.height / 10, Screen.width - Screen.width / 5, Screen.height - Screen.height / 5);

    }
	
	// Update is called once per frame
	void Update ()
    {
        //the key used to pause the program
	    if(Input.GetKeyDown(KeyCode.Pause))
        {
            isPaused = !isPaused;
            PauseGame();
        }
	}

    //Function that suspends data recording.
    private void PauseGame()
    {
        if (isPaused)
        {
            Time.timeScale = 0;
            //DisplayMenu();
        }
        else
        {
            Time.timeScale = 1;
        }
    }

    private void DisplayMenu(int windowID)
    {
        if (GUI.Button(new Rect(pauseWindow.width / 2 - 50, pauseWindow.height / 2 - 50, 100, 45), "Resume"))
        {
            Debug.Log("clicked resume test.");
            isPaused = false;
            PauseGame();
        }

        if (GUI.Button(new Rect(pauseWindow.width / 2 - 50, pauseWindow.height / 2 + 25, 100, 45), "End Test"))
        {
            Debug.Log("clicked end test.");
            Application.LoadLevel("MainMenu");
            isPaused = false;
            PauseGame();
        }
        
    }

    public void OnGUI()
    {
        if (isPaused)
        {
            GUI.Window(0, pauseWindow, DisplayMenu, "Pause Menu");

            //GUI.backgroundColor = Color.black;
            //GUI.Button(new Rect(Screen.width / 2, Screen.height / 2 -30, 100, 45), "Resume");
            //GUI.Button(new Rect(Screen.width / 2, Screen.height / 2 +30, 100, 45), "End Test");
        }
    }
}
