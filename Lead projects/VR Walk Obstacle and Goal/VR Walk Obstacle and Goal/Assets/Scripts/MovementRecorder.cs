////////////////////////////////////////////////////////////////////////////////////
// Experiment: Risk Aversion
// Purpose: Records the pathing taken by the subject (OVR + PPT)
// **Easily Portable
// Code by: Butler, Michael
// Email: msbcoding@gmail.com
////////////////////////////////////////////////////////////////////////////////////
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class MovementRecorder : MonoBehaviour
{
    StreamWriter Output;
    string OutPath;
    [HideInInspector] public string SubjectID;
    [HideInInspector] public bool Record;
    public Transform player;
    public Camera playerCam;
    private int TrialInc;
    private string WriteString;

    // Use this for initialization
    public void SetUp ()
    {
        string temp = System.DateTime.Now.Date.ToString();
        temp = temp.Replace('/', '-');
        temp = temp.Split(' ')[0];
        OutPath = "Subject_" + SubjectID + "_" + temp;
        OutPath = Application.dataPath + "/Data/" + OutPath + ".csv";
        FileChecker();
    }
    void SetupComplete()
    {
        Output = new StreamWriter(File.Create(OutPath));
        Record = false;
        TrialInc = 0;
        Output.WriteLine("TimeStamp, X Coord, Y Coord, Z Coord, Yaw, Pitch, Roll, Trial, Obstacle Angle, Obstalce Distance, Goal Angle, Goal Distance, Hazard Pole");
    }
	
	// Update is called once per frame
	public void Recorder ()
    {
		if(Record)
        {
            string feed = System.DateTime.Now.ToString().Split(' ')[1];
            feed = feed + ',' + player.position.ToString().Trim('(').Trim(')')
                        + ',' + playerCam.transform.rotation.eulerAngles.x 
                        + ',' + playerCam.transform.rotation.eulerAngles.y
                        + ',' + playerCam.transform.rotation.eulerAngles.z;
            Output.WriteLine(feed + ',' + WriteString);
        }
	}

    //Requires input for trials paramaters
    public void NewTrial(string TrialParams)
    {
        WriteString = (TrialInc+1).ToString() + ',' + TrialParams;
        TrialInc++;
    }

    public void SaveFile()
    {
        float x = float.Parse(SubjectSettings.instance.ShWidth);
        x = x * 200;
        Output.WriteLine("Subject Shoulder Width: " + x.ToString() + " CM.");
        Output.WriteLine("Closing Properly");
        Output.Close();
    }

    public void ObstacleCollision()
    {
        string feed = System.DateTime.Now.ToString().Split(' ')[1];
        feed = feed + ',' + player.position.ToString().Trim('(').Trim(')')
                    + ',' + playerCam.transform.rotation.eulerAngles.x
                    + ',' + playerCam.transform.rotation.eulerAngles.y
                    + ',' + playerCam.transform.rotation.eulerAngles.z;
        Output.WriteLine(feed + ',' + WriteString + ", Obstacle Collision!");
    }

    void FileChecker()
    {
        int incrimenter = 0;
        if (!Directory.Exists(Application.dataPath + "/Data"))
        {
            Directory.CreateDirectory(Application.dataPath + "/Data");
        }
        if (System.IO.File.Exists(OutPath))
        {
            while (System.IO.File.Exists(OutPath + "(" + incrimenter.ToString() +")" + ".csv"))
            {
                incrimenter++;
            }
            OutPath = OutPath + "(" + incrimenter.ToString() + ")" + ".csv";
        }
        SetupComplete();
    }

    public void NullLine()
    {
        string feed = System.DateTime.Now.ToString().Split(' ')[1];
        feed = feed + ',' + "null"
                    + ',' + "null"
                    + ',' + "null"
                    + ',' + "null";
        Output.WriteLine(feed);
    }
}
