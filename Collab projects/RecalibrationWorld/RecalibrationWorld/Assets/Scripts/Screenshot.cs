using UnityEngine;
using System.Collections;

public class Screenshot : MonoBehaviour {

    private bool takeShot = false;
    public int superSize = 1;

	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
	
        if(Input.GetKeyDown(KeyCode.F11))
        {
            takeShot = true;
        }

        if(takeShot)
        {
            takeShot = false;
            int copy = 1;
            string filename = "Screenshot" + copy + ".png";
            while(System.IO.File.Exists(filename))
            {
                copy++;
                filename = "Screenshot" + copy + ".png";
            }
            ScreenCapture.CaptureScreenshot(filename,superSize);
        }
	}
}
