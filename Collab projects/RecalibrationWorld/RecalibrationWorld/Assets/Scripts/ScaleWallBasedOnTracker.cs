﻿using UnityEngine;
using System.Collections;
using System.IO;
using System.Collections.Generic;
using Valve.VR;
using System;


//NOTES:
// global for Delta X & Z of the trackers for the lab space the tables were scaled for
//great hall config:
//  temp switch case for scale settings
//  faster condition using a hard scale modifier. fix to based on play space. (fixed 8/14)
//  faster condition border tables can be scaled up to ~.015 on Z


public class ScaleWallBasedOnTracker : MonoBehaviour {


    #region DEPRICATED
    [HideInInspector]
    public GameObject[] walls;
    [HideInInspector]
    public GameObject[] tables;
    [HideInInspector]
    public int singleHallOrientation = 0;
    //creates a drop down on the editor under the scaling script to change which configuration the world will run
    public enum Configurations
    {Box_Config, Cross_Config, Double_Hall_Config, Single_Hall_Config, Single_Hall_Meth2_Config, Single_Hall_Gordon, Great_Hall_C};
    [HideInInspector]
    public Configurations Hall_Configuration = Configurations.Box_Config;
    private Configurations live_update;
    [HideInInspector]
    public float wallThickness = 0.25f;
    [HideInInspector]
    public float hallwayHalfWidth = 0.6f;
    [HideInInspector]
    public float hallSize = 1f;
    [HideInInspector]
    public float movingAverageTimeInterval = 1.0f;
    #endregion

    #region VARIABLES
    public GameObject camera_origin;
    public GameObject trackedDevices;
    [SerializeField]
    private GameObject Great_Hall;
    [HideInInspector]
    public float scale = 1f;

    private SettingsSingleton settings;
    [Tooltip("Play area gizmo script in order to get bounds.")]
    public VRTK_RoomExtender_PlayAreaGizmo playAreaGizmo;

    [Tooltip("Minimum time (s) to pass before recording another frame's position and rotation data")]
    public float minTimeBeforeFrameCapture = 0.1f;

    [Tooltip("Minimum net movement speed (m/s) in the X and Z direction to be considered moving)")]
    public float minMovementToNotBePaused = 0.25f;

    [Tooltip("Get camera to record it's position/orientation")]
    public Camera headCamera;

    [HideInInspector]
    public bool setPosition = false;
    private IVRSystem ivrInterface;

    //Get the bounds from the "vrtk_room_extender_play_area_gizmo"
    [HideInInspector]
    public Vector3[] vertices;

    //Get the actual sensors
    private List<GameObject> sensors = new List<GameObject>();

    //Save time after staring to record time at normal intervals
    private float totalTime = 0f;
    private float currentTime = 0f;

    //Save the filename to append recorded data
    private string filename = "";

    //Variables to calculate moving average, not yet functional
    private Vector3 previousPosition = Vector3.zero;
    private bool isPaused = true;
    private float currentTimeMovingAverage = 0;
    private int movingAverageFrameMax = 60;
    private LinkedList<float> speedFrames = new LinkedList<float>();
    private float currentAverageSpeed = 0f;
    


    //Delta X & Z of the trackers for the lab space the tables were scaled for,
    //  used to compare to the run sites play space and scale objects accordingly
    private float DeltaX_DevSpace = 3.129735f;
    private float DeltaZ_DevSpace = 2.873589f;

    //Delta X & Z of the trackers for the run site
    // used for positioning and scaling of objects
    private float DeltaX_RunSpace;
    private float DeltaZ_RunSpace;

    #endregion

    // Use this for initialization
    void Start () {
        //set initial value for the editor preview config changes
        live_update = Hall_Configuration;

        movingAverageFrameMax = (int)Mathf.Round(movingAverageTimeInterval / Time.fixedDeltaTime);

        settings = GameObject.Find("Settings").GetComponent<SettingsSingleton>();
        if (settings == null)
            Debug.Log("No settings object found!");
        else
        {
            filename = settings.filePath;
            
            //Check if filename exists (should be created in MainMenu screen)
            if (!System.IO.File.Exists(filename))
            {
                //Create directory if it doesn't exist
                if (!Directory.Exists(Application.dataPath + "/Data"))
                {
                    Directory.CreateDirectory(Application.dataPath + "/Data");
                }
                //Won't exist if launched from Locomotion scene in editor
                filename = Application.dataPath + "/Data/test_output.txt";
            }

            //Write header data
            System.IO.File.AppendAllText(filename, "\r\nTotal Time, X Position, Y Position, Z Position, Euler alpha (Yaw), Euler gamma (Pitch), Euler beta (Roll)\r\n");
        }
    }
        

	// Update is called once per frame
	void Update () {
        
        //should allow for a change in which config is running in the editor preview (not completely working yet, I think it needs a rework of how the rotations are happening to function properly.)
        if(Hall_Configuration != live_update)
        {
            setPosition = false;
            live_update = Hall_Configuration;
        }

        //Check if the position has already been set
        if(!setPosition)
        {
            //Get all sensors
            foreach (Transform child in trackedDevices.transform)
            {
                        //Only add sensors to sensors list
                        SteamVR_TrackedObject trackedObjectScript = child.gameObject.GetComponent<SteamVR_TrackedObject>();
                        if (trackedObjectScript.isValid && OpenVR.System.GetTrackedDeviceClass((uint)trackedObjectScript.index) == ETrackedDeviceClass.TrackingReference)
                        {
                            sensors.Add(trackedObjectScript.gameObject);
                        }
             }

            //Check if sensors have been added
            if (sensors.Count > 1)
             {
                //getting the play space perimeter
                DeltaX_RunSpace = Mathf.Abs(sensors[0].transform.position.x - sensors[1].transform.position.x);
                DeltaZ_RunSpace = Mathf.Abs(sensors[0].transform.position.z - sensors[1].transform.position.z);

                //Check if sensors have updated from default position
                if (sensors[0].transform.position.x != 0)
                #region HallConifg
                {
                    playAreaGizmo.setBounds();
                    var rect = new HmdQuad_t();
                    playAreaGizmo.GetBounds(ref rect);

                    var corners = new HmdVector3_t[] { rect.vCorners0, rect.vCorners1, rect.vCorners2, rect.vCorners3 };

                    vertices = new Vector3[corners.Length * 2];
                    for (int i = 0; i < corners.Length; i++)
                    {
                        var c = corners[i];
                        vertices[i] = new Vector3(c.v0, 0.01f, c.v2);
                    }

                    for (int i = 0; i < corners.Length; i++)
                    {
                        vertices[corners.Length + i] = vertices[i];
                    }
                    setPosition = true;
                    /*
                    foreach(Vector3 v in vertices)
                        Debug.Log(v);
                    */
                    //checks the config enum and selects a config based on that selection

                    /* switch (Hall_Configuration)
                     {
                         case Configurations.Box_Config:
                             Box_Config();
                             break;
                         case Configurations.Cross_Config:
                             Cross_Config();
                             break;
                         case Configurations.Double_Hall_Config:
                             Double_Hall_Config();
                             break;
                         case Configurations.Single_Hall_Config:
                             Single_Hall_Config();
                             break;
                         case Configurations.Single_Hall_Meth2_Config:
                             Single_Hall_Meth2_Config();
                             break;
                         case Configurations.Single_Hall_Gordon:
                             Single_Hall_Gordon(singleHallOrientation);
                             break;
                         case Configurations.Great_Hall_C:
                             Great_Hall_Config();
                             break;
                         default:
                             break;
                     }*/

                }
                #endregion
             }
            
        }
	
	}

    void FixedUpdate()
    {
        if(setPosition)
        {
            totalTime += Time.fixedDeltaTime;
            currentTime += Time.fixedDeltaTime;
            //Update moving average speed
            UpdateMovingAverageMovement();

            //Write out tracking data at normal intervals
            if(currentTime >= minTimeBeforeFrameCapture)
            {
                currentTime = 0f;
                CaptureFrameTrackingData();
            }
        }
    }

    //Calculate a moving average of the players' speed to determine if they are "stopped" or not, not fully implemented
    private void UpdateMovingAverageMovement()
    {
        //Get the total movement amount only in the X and Z directions
        float movementAmount = Mathf.Sqrt(Mathf.Pow(headCamera.transform.position.x - previousPosition.x,2) + Mathf.Pow(headCamera.transform.position.z - previousPosition.z, 2));

        float currentSpeed = movementAmount / Time.deltaTime;

        //Don't record warping (10 m/s is Usain Bolt territory)
        if (currentSpeed < 10f)
        {
            if (speedFrames.Count >= movingAverageFrameMax)
            {
                speedFrames.RemoveLast();
                speedFrames.AddFirst(currentSpeed);
            }
            else
                speedFrames.AddFirst(currentSpeed);

            float averageSpeed = 0;

            foreach (float s in speedFrames)
            {
                averageSpeed += s;
            }
            averageSpeed /= speedFrames.Count;

            if (averageSpeed < minMovementToNotBePaused)
            {
                isPaused = true;
            }
            else
                isPaused = false;

            currentAverageSpeed = averageSpeed;
            previousPosition = new Vector3(headCamera.transform.position.x, headCamera.transform.position.y, headCamera.transform.position.z);
        }
    }

    private void CaptureFrameTrackingData()
    {
        //Write tracking data and time
        System.IO.File.AppendAllText(filename, totalTime + "," + headCamera.transform.position.x + "," + headCamera.transform.position.y + "," + headCamera.transform.position.z + ","
            + headCamera.transform.rotation.eulerAngles.x + "," + headCamera.transform.rotation.eulerAngles.y + "," + headCamera.transform.rotation.eulerAngles.z + "\r\n");// + "," + currentAverageSpeed + "," + isPaused + "\r\n");
    }

    private void setScale()
    {
        if (settings.currentCondition == SettingsSingleton.Condition.Matched)
        { scale = settings.matchedScale; }
        else if (settings.currentCondition == SettingsSingleton.Condition.Faster)
        { scale = settings.fasterScale; }
        else if (settings.currentCondition == SettingsSingleton.Condition.Slower)
        { scale = settings.slowerScale; }
        else if (settings.currentCondition == SettingsSingleton.Condition.Slowest)
        { scale = settings.slowestScale; }
        else if (settings.currentCondition == SettingsSingleton.Condition.Fastest)
        { scale = settings.fastestScale; }
        else
        {
            scale = 1f;
#if UNITY_EDITOR
            Debug.Log("Scale not properly set");
#endif
        }
    }


    //Hallway configurations not currently implemented
    private void Single_Hall_Config()
    {
        var sens2sens = Mathf.Atan2(Mathf.Abs(sensors[1].transform.position.z - sensors[0].transform.position.z), Mathf.Abs(sensors[0].transform.position.x - sensors[1].transform.position.x)) * (180 / Mathf.PI);
        
        //Iterate and update each wall position depending on the sensor locations
        for (int index = 0; index < walls.Length; index++)
        {
            switch (index)
            {
                case 0: //controlwall
                    walls[index].transform.localScale = new Vector3(10, walls[index].transform.localScale.y, wallThickness);
                    walls[index].transform.position = new Vector3(Mathf.Abs((sensors[0].transform.position.x - sensors[1].transform.position.x) / 2) + Mathf.Min(sensors[0].transform.position.x, sensors[1].transform.position.x), walls[index].transform.position.y, Mathf.Abs(sensors[1].transform.position.z - sensors[0].transform.position.z) / 2 + Mathf.Min(sensors[0].transform.position.z, sensors[1].transform.position.z));
                    walls[index].transform.Rotate(0, (Mathf.Atan2((Mathf.Abs((sensors[1].transform.position.z) - (sensors[0].transform.position.z))), (Mathf.Abs((sensors[0].transform.position.x) - (sensors[1].transform.position.x)))) * (-180 / Mathf.PI)), 0);
                    var temp5 = walls[index].GetComponent<Renderer>();
                    temp5.enabled = false;
                    break;

                case 1: //endwall
                    walls[index].transform.localScale = new Vector3((new Vector2(Mathf.Abs(sensors[1].transform.position.x - sensors[0].transform.position.x), 
                        Mathf.Abs(sensors[1].transform.position.z - sensors[0].transform.position.z)).magnitude / 3.5f), 
                        walls[index].transform.localScale.y, wallThickness);
                    walls[index].transform.position = new Vector3(sensors[1].transform.position.x, walls[index].transform.position.y, sensors[0].transform.position.z);
                    walls[index].transform.rotation = walls[0].transform.rotation;
                    walls[index].transform.Rotate(0, 90, 0);
                    break;
                
                case 2: //endwall
                    walls[index].transform.localScale = new Vector3((new Vector2(Mathf.Abs(sensors[1].transform.position.x - sensors[0].transform.position.x), Mathf.Abs(sensors[1].transform.position.z - sensors[0].transform.position.z)).magnitude / 3.5f), walls[index].transform.localScale.y, wallThickness);
                    walls[index].transform.position = new Vector3(sensors[0].transform.position.x, walls[index].transform.position.y, sensors[1].transform.position.z);
                    walls[index].transform.rotation = walls[0].transform.rotation;
                    walls[index].transform.Rotate(0,90,0);
                    break;
                case 3: //diagwall (long)
                    var temp0 = walls[1].GetComponent<Renderer>();


                    walls[index].transform.localScale = new Vector3(Mathf.Sqrt(Mathf.Pow(sensors[0].transform.position.x - sensors[1].transform.position.x, 2) + Mathf.Pow(sensors[1].transform.position.z - sensors[0].transform.position.z , 2)), walls[index].transform.localScale.y, wallThickness);
                    walls[index].transform.position = new Vector3(walls[0].transform.position.x - (Mathf.Cos(walls[1].transform.eulerAngles.y * Mathf.Deg2Rad) * (temp0.bounds.size.x / 2)), walls[0].transform.position.y, walls[0].transform.position.z + (Mathf.Sin(walls[1].transform.eulerAngles.y * Mathf.Deg2Rad) * (temp0.bounds.size.x / 2)));
                    walls[index].transform.rotation = walls[0].transform.rotation;
                    break;
                    
                case 4: //diagwall (long)
                    var temp1 = walls[1].GetComponent<Renderer>();
                    Debug.Log(temp1.bounds.size);
                    Debug.Log(walls[1].transform.localScale.x);
                    Debug.Log(walls[1].transform.eulerAngles.y);
                    Debug.Log(Mathf.Cos(walls[1].transform.eulerAngles.y * Mathf.Deg2Rad));

                    //scale x = distance between the two corners of the play area without a sensor (if the play area were a perfect rectangle).
                    walls[index].transform.localScale = new Vector3(Mathf.Sqrt(Mathf.Pow(sensors[0].transform.position.x - sensors[1].transform.position.x, 2) + Mathf.Pow(sensors[1].transform.position.z - sensors[0].transform.position.z, 2)), walls[index].transform.localScale.y, wallThickness);

                    //starting with the position of the control wall then adding an offset. offset formula = sin or cos (angle * deg2rad) * distance
                    //I'm not sure whether I'm getting the wrong size from bounds, or if I made a mistake with the sin/cos. I think it is the former, but I'm not sure where to get the proper distance from if thats the case.
                    //Sorry if its something super simple, I've been staring at this for too long and just cant think straight x.x
                    walls[index].transform.position = new Vector3(walls[0].transform.position.x + (Mathf.Cos(walls[1].transform.eulerAngles.y * Mathf.Deg2Rad) * (temp1.bounds.size.x / 2)), walls[0].transform.position.y, walls[0].transform.position.z - (Mathf.Sin(walls[1].transform.eulerAngles.y * Mathf.Deg2Rad) * (temp1.bounds.size.x / 2)));
                   
                    //rotation is based off the control wall, which is based off the angle of the same two corners used in scale x
                    walls[index].transform.rotation = walls[0].transform.rotation; 
                    break;

                default:
                    walls[index].transform.localScale = new Vector3(Mathf.Abs(sensors[0].transform.position.x - sensors[1].transform.position.x), walls[0].transform.localScale.y, wallThickness);
                    walls[index].transform.position = new Vector3((Mathf.Abs(sensors[0].transform.position.x - sensors[1].transform.position.x) / 2) + Mathf.Min(sensors[0].transform.position.x, sensors[1].transform.position.x), walls[0].transform.position.y, sensors[1].transform.position.z);
                    var temp6 = walls[index].GetComponent<Renderer>();
                    temp6.enabled = false;
                    break;
            }
        }
        //Debug.Log(sens2sens);
        //Debug.Log(new Vector2(Mathf.Abs(sensors[1].transform.position.x - sensors[0].transform.position.x), Mathf.Abs(sensors[1].transform.position.z - sensors[0].transform.position.z)).magnitude / 3.5f);
        setPosition = true;
    }

    private void Double_Hall_Config()
    {
        var sens2sens = Mathf.Atan2(Mathf.Abs(sensors[1].transform.position.z - sensors[0].transform.position.z), Mathf.Abs(sensors[0].transform.position.x - sensors[1].transform.position.x)) * (180 / Mathf.PI);

        //Iterate and update each wall position depending on the sensor locations
        for (int index = 0; index < walls.Length; index++)
        {
            switch (index)
            {
                case 0: //controlwall
                    walls[index].transform.localScale = new Vector3(Mathf.Sqrt(Mathf.Pow(sensors[0].transform.position.x - sensors[1].transform.position.x, 2) + Mathf.Pow(sensors[1].transform.position.z - sensors[0].transform.position.z, 2)) * .7143f, walls[index].transform.localScale.y, wallThickness);
                    walls[index].transform.position = new Vector3(Mathf.Abs((sensors[0].transform.position.x - sensors[1].transform.position.x) / 2) + Mathf.Min(sensors[0].transform.position.x, sensors[1].transform.position.x), walls[index].transform.position.y, Mathf.Abs(sensors[1].transform.position.z - sensors[0].transform.position.z) / 2 + Mathf.Min(sensors[0].transform.position.z, sensors[1].transform.position.z));
                    walls[index].transform.Rotate(0, (Mathf.Atan2((Mathf.Abs((sensors[1].transform.position.z) - (sensors[0].transform.position.z))), (Mathf.Abs((sensors[0].transform.position.x) - (sensors[1].transform.position.x)))) * (-180 / Mathf.PI)), 0);
                    break;

                case 1: //endwall
                    walls[index].transform.localScale = new Vector3(2*(new Vector2(Mathf.Abs(sensors[1].transform.position.x - sensors[0].transform.position.x), Mathf.Abs(sensors[1].transform.position.z - sensors[0].transform.position.z)).magnitude / 3.5f), walls[index].transform.localScale.y, wallThickness);
                    walls[index].transform.position = new Vector3(sensors[1].transform.position.x, walls[index].transform.position.y, sensors[0].transform.position.z);
                    walls[index].transform.rotation = walls[0].transform.rotation;
                    walls[index].transform.Rotate(0, 90, 0);
                    break;

                case 2: //endwall
                    walls[index].transform.localScale = new Vector3(2*(new Vector2(Mathf.Abs(sensors[1].transform.position.x - sensors[0].transform.position.x), Mathf.Abs(sensors[1].transform.position.z - sensors[0].transform.position.z)).magnitude / 3.5f), walls[index].transform.localScale.y, wallThickness);
                    walls[index].transform.position = new Vector3(sensors[0].transform.position.x, walls[index].transform.position.y, sensors[1].transform.position.z);
                    walls[index].transform.rotation = walls[0].transform.rotation;
                    walls[index].transform.Rotate(0, 90, 0);
                    break;
                case 3: //diagwall (long)
                    var temp0 = walls[1].GetComponent<Renderer>();


                    walls[index].transform.localScale = new Vector3(Mathf.Sqrt(Mathf.Pow(sensors[0].transform.position.x - sensors[1].transform.position.x, 2) + Mathf.Pow(sensors[1].transform.position.z - sensors[0].transform.position.z, 2)), walls[index].transform.localScale.y, wallThickness);
                    walls[index].transform.position = new Vector3(walls[0].transform.position.x - (Mathf.Cos(walls[1].transform.eulerAngles.y * Mathf.Deg2Rad) * (temp0.bounds.size.x / 2)), walls[0].transform.position.y, walls[0].transform.position.z + (Mathf.Sin(walls[1].transform.eulerAngles.y * Mathf.Deg2Rad) * (temp0.bounds.size.x / 2)));
                    walls[index].transform.rotation = walls[0].transform.rotation;
                    break;

                case 4: //diagwall (long)
                    var temp1 = walls[1].GetComponent<Renderer>();
                    Debug.Log(temp1.bounds.size);
                    Debug.Log(walls[1].transform.localScale.x);
                    Debug.Log(walls[1].transform.eulerAngles.y);
                    Debug.Log(Mathf.Cos(walls[1].transform.eulerAngles.y * Mathf.Deg2Rad));


                    walls[index].transform.localScale = new Vector3(Mathf.Sqrt(Mathf.Pow(sensors[0].transform.position.x - sensors[1].transform.position.x, 2) + Mathf.Pow(sensors[1].transform.position.z - sensors[0].transform.position.z, 2)), walls[index].transform.localScale.y, wallThickness);
                    walls[index].transform.position = new Vector3(walls[0].transform.position.x + (Mathf.Cos(walls[1].transform.eulerAngles.y * Mathf.Deg2Rad) * (temp1.bounds.size.x / 2)), walls[0].transform.position.y, walls[0].transform.position.z - (Mathf.Sin(walls[1].transform.eulerAngles.y * Mathf.Deg2Rad) * (temp1.bounds.size.x / 2)));
                    walls[index].transform.rotation = walls[0].transform.rotation;
                    break;

                default:
                    walls[index].transform.localScale = new Vector3(Mathf.Abs(sensors[0].transform.position.x - sensors[1].transform.position.x), walls[0].transform.localScale.y, wallThickness);
                    walls[index].transform.position = new Vector3((Mathf.Abs(sensors[0].transform.position.x - sensors[1].transform.position.x) / 2) + Mathf.Min(sensors[0].transform.position.x, sensors[1].transform.position.x), walls[0].transform.position.y, sensors[1].transform.position.z);
                    var temp6 = walls[index].GetComponent<Renderer>();
                    temp6.enabled = false;
                    break;
            }
        }
        setPosition = true;
        //Debug.Log(sens2sens);
        //Debug.Log(new Vector2(Mathf.Abs(sensors[1].transform.position.x - sensors[0].transform.position.x), Mathf.Abs(sensors[1].transform.position.z - sensors[0].transform.position.z)).magnitude / 3.5f);

    }

    private void Single_Hall_Meth2_Config()
    {
        //Check if sensors have been added
        if (sensors.Count > 0)
        {
            //Check if sensors have updated from default position
            if (sensors[0].transform.position.x != 0)
            {
                //Iterate and update each wall position depending on the sensor locations
                for (int index = 0; index < walls.Length; index++)
                {
                    float opp;
                    float hyp;
                    float angle;
                    switch (index)
                    {
                        case 0:
                            walls[index].transform.localScale = new Vector3(Mathf.Abs(sensors[0].transform.position.x - sensors[1].transform.position.x), walls[index].transform.localScale.y, wallThickness);
                            walls[index].transform.position = new Vector3((Mathf.Abs(sensors[0].transform.position.x - sensors[1].transform.position.x) / 2) + Mathf.Min(sensors[0].transform.position.x, sensors[1].transform.position.x), walls[index].transform.position.y, sensors[1].transform.position.z);
                            break;
                        case 1:
                            walls[index].transform.localScale = new Vector3(Mathf.Abs(sensors[0].transform.position.x - sensors[1].transform.position.x), walls[index].transform.localScale.y, wallThickness);
                            walls[index].transform.position = new Vector3(Mathf.Abs((sensors[0].transform.position.x - sensors[1].transform.position.x) / 2) + Mathf.Min(sensors[0].transform.position.x, sensors[1].transform.position.x), walls[index].transform.position.y, sensors[0].transform.position.z);
                            break;
                        case 2:
                            walls[index].transform.localScale = new Vector3(Mathf.Abs(sensors[0].transform.position.z - sensors[1].transform.position.z), walls[index].transform.localScale.y, wallThickness);
                            walls[index].transform.position = new Vector3(sensors[1].transform.position.x, walls[index].transform.position.y, Mathf.Abs(sensors[1].transform.position.z - sensors[0].transform.position.z) / 2 + Mathf.Min(sensors[0].transform.position.z, sensors[1].transform.position.z));
                            break;
                        case 3:
                            walls[index].transform.localScale = new Vector3(Mathf.Abs(sensors[0].transform.position.z - sensors[1].transform.position.z), walls[index].transform.localScale.y, wallThickness);
                            walls[index].transform.position = new Vector3(sensors[0].transform.position.x, walls[index].transform.position.y, Mathf.Abs(sensors[1].transform.position.z - sensors[0].transform.position.z) / 2 + Mathf.Min(sensors[0].transform.position.z, sensors[1].transform.position.z));
                            break;
                        case 4:
                            //Rotate wall based on base station locations
                            opp = Mathf.Abs(sensors[1].transform.localPosition.z - sensors[0].transform.localPosition.z);
                            hyp = new Vector2(sensors[1].transform.localPosition.x - sensors[0].transform.localPosition.x, sensors[1].transform.localPosition.z - sensors[0].transform.localPosition.z).magnitude;
                            angle =  Mathf.Asin((opp / hyp));

                            walls[4].transform.localRotation = Quaternion.AngleAxis(angle * Mathf.Rad2Deg, Vector3.up);

                            //Scale wall based on base station locations 
                            walls[4].transform.localScale = new Vector3(new Vector2(sensors[1].transform.position.x - sensors[0].transform.position.x, sensors[1].transform.position.z - sensors[0].transform.position.z).magnitude, walls[4].transform.localScale.y, wallThickness);

                            //Center the angled wall
                            walls[4].transform.position = new Vector3(Mathf.Abs((sensors[0].transform.position.x - sensors[1].transform.position.x) / 2) + Mathf.Min(sensors[0].transform.position.x, sensors[1].transform.position.x), walls[4].transform.position.y, Mathf.Abs(sensors[1].transform.position.z - sensors[0].transform.position.z) / 2 + Mathf.Min(sensors[0].transform.position.z, sensors[1].transform.position.z));

                            //Offset wall from center to allow hallway
                            walls[index].transform.position += walls[index].transform.forward * hallwayHalfWidth;
                            break;
                        case 5:
                            //Rotate wall based on base station locations
                            opp = Mathf.Abs(sensors[1].transform.localPosition.z - sensors[0].transform.localPosition.z);
                            hyp = new Vector2(sensors[1].transform.localPosition.x - sensors[0].transform.localPosition.x, sensors[1].transform.localPosition.z - sensors[0].transform.localPosition.z).magnitude;
                            angle = Mathf.Asin((opp / hyp));
                            walls[index].transform.localRotation = Quaternion.AngleAxis(angle * Mathf.Rad2Deg, Vector3.up);

                            //Scale wall based on base station locations 
                            walls[index].transform.localScale = new Vector3(new Vector2(sensors[1].transform.position.x - sensors[0].transform.position.x, sensors[1].transform.position.z - sensors[0].transform.position.z).magnitude, walls[index].transform.localScale.y, wallThickness);

                            //Center the angled wall
                            walls[index].transform.position = new Vector3(Mathf.Abs((sensors[0].transform.position.x - sensors[1].transform.position.x) / 2) + Mathf.Min(sensors[0].transform.position.x, sensors[1].transform.position.x), walls[index].transform.position.y, Mathf.Abs(sensors[1].transform.position.z - sensors[0].transform.position.z) / 2 + Mathf.Min(sensors[0].transform.position.z, sensors[1].transform.position.z));

                            //Offset wall from center to allow hallway
                            walls[index].transform.position -= walls[index].transform.forward * hallwayHalfWidth;


                            break;
                        case 6:
                            //Rotate wall based on base station locations
                            opp = Mathf.Abs(sensors[1].transform.localPosition.z - sensors[0].transform.localPosition.z);
                            hyp = new Vector2(sensors[1].transform.localPosition.x - sensors[0].transform.localPosition.x, sensors[1].transform.localPosition.z - sensors[0].transform.localPosition.z).magnitude;
                            angle = Mathf.Asin((opp / hyp));

                            //walls[index].transform.rotation.SetLookRotation(sensors[1].transform.position - sensors[0].transform.position);


                            walls[index].transform.rotation = Quaternion.AngleAxis((angle)* Mathf.Rad2Deg, Vector3.up);

                            walls[index].transform.rotation = Quaternion.Euler(walls[index].transform.rotation.eulerAngles.x, walls[index].transform.rotation.eulerAngles.y + 90f, walls[index].transform.rotation.eulerAngles.z);


                            //Scale wall based on base station locations 
                            walls[index].transform.localScale = new Vector3(2*hallwayHalfWidth, walls[index].transform.localScale.y, wallThickness);

                            //Center the end wall at the end of the hallway
                            walls[index].transform.position = new Vector3(sensors[0].transform.position.x, walls[index].transform.position.y, sensors[0].transform.position.z);

                            //Offset wall from sensor to end
                            walls[index].transform.position +=  walls[index].transform.forward * Mathf.Max(Mathf.Abs(Mathf.Tan(angle+Mathf.PI/2)), Mathf.Abs(Mathf.Tan(angle))) * hallwayHalfWidth;
                            break;

                        case 7:
                            //Rotate wall based on base station locations
                            opp = Mathf.Abs(sensors[1].transform.localPosition.z - sensors[0].transform.localPosition.z);
                            hyp = new Vector2(sensors[1].transform.localPosition.x - sensors[0].transform.localPosition.x, sensors[1].transform.localPosition.z - sensors[0].transform.localPosition.z).magnitude;
                            angle = Mathf.Asin((opp / hyp));
                            walls[index].transform.rotation = Quaternion.AngleAxis((angle) * Mathf.Rad2Deg, Vector3.up);

                            walls[index].transform.rotation = Quaternion.Euler(walls[index].transform.rotation.eulerAngles.x, walls[index].transform.rotation.eulerAngles.y + 90f, walls[index].transform.rotation.eulerAngles.z);


                            //Scale wall based on base station locations 
                            walls[index].transform.localScale = new Vector3(2 * hallwayHalfWidth, walls[index].transform.localScale.y, wallThickness);

                            //Center the end wall at the end of the hallway
                            walls[index].transform.position = new Vector3(sensors[1].transform.position.x, walls[index].transform.position.y, sensors[1].transform.position.z);

                            //Offset wall from sensor to end
                            walls[index].transform.position += walls[index].transform.forward * Mathf.Max(Mathf.Abs(Mathf.Tan(angle + Mathf.PI / 2)), Mathf.Abs(Mathf.Tan(angle))) * hallwayHalfWidth;
                            break;

                        default:
                            walls[index].transform.localScale = new Vector3(0, 0, 0);
                            walls[index].transform.position = new Vector3(0, 0, 0);
                            //hides unused walls
                            var temp1 = walls[index].GetComponent<Renderer>();
                            temp1.enabled = false;
                            break;
                    }
                }

                setPosition = true;
            }
        }
        setPosition = true;
    }

    private void Cross_Config()
    {
        
        //Iterate and update each wall position depending on the sensor locations
        for (int index = 0; index < walls.Length; index++)
        {
            switch (index) 
            {
                case 0:
                    //sets the position of outer wall based on sensor position
                    walls[index].transform.localScale = new Vector3(Mathf.Abs(sensors[0].transform.position.x - sensors[1].transform.position.x), walls[index].transform.localScale.y, wallThickness);
                    walls[index].transform.position = new Vector3((Mathf.Abs(sensors[0].transform.position.x - sensors[1].transform.position.x) / 2) + Mathf.Min(sensors[0].transform.position.x, sensors[1].transform.position.x), walls[index].transform.position.y, sensors[1].transform.position.z);
                    break;
                case 1:
                    //see case 0
                    walls[index].transform.localScale = new Vector3(Mathf.Abs(sensors[0].transform.position.x - sensors[1].transform.position.x), walls[index].transform.localScale.y, wallThickness);
                    walls[index].transform.position = new Vector3(Mathf.Abs((sensors[0].transform.position.x - sensors[1].transform.position.x) / 2) + Mathf.Min(sensors[0].transform.position.x, sensors[1].transform.position.x), walls[index].transform.position.y, sensors[0].transform.position.z);
                    break;
                case 2:
                    //see case 0
                    walls[index].transform.localScale = new Vector3(Mathf.Abs(sensors[0].transform.position.z - sensors[1].transform.position.z), walls[index].transform.localScale.y, wallThickness);
                    walls[index].transform.position = new Vector3(sensors[1].transform.position.x, walls[index].transform.position.y, Mathf.Abs(sensors[1].transform.position.z - sensors[0].transform.position.z) / 2 + Mathf.Min(sensors[0].transform.position.z, sensors[1].transform.position.z));
                    break;
                case 3:
                    //see case 0
                    walls[index].transform.localScale = new Vector3(Mathf.Abs(sensors[0].transform.position.z - sensors[1].transform.position.z), walls[index].transform.localScale.y, wallThickness);
                    walls[index].transform.position = new Vector3(sensors[0].transform.position.x, walls[index].transform.position.y, Mathf.Abs(sensors[1].transform.position.z - sensors[0].transform.position.z) / 2 + Mathf.Min(sensors[0].transform.position.z, sensors[1].transform.position.z));
                    break;
                case 4:
                    //sets the position of inner wall based on sensor position
                    walls[index].transform.localScale = new Vector3(Mathf.Abs(sensors[0].transform.position.x - sensors[1].transform.position.x) / 2, walls[index].transform.localScale.y, wallThickness);
                    walls[index].transform.position = new Vector3((Mathf.Abs(sensors[0].transform.position.x - sensors[1].transform.position.x) / 2) + Mathf.Min(sensors[0].transform.position.x, sensors[1].transform.position.x), walls[index].transform.position.y,(Mathf.Abs(sensors[0].transform.position.z - sensors[1].transform.position.z) / 3.5f) + sensors[0].transform.position.z);
                    break;
                case 5:
                    //see case 0
                    walls[index].transform.localScale = new Vector3(Mathf.Abs(sensors[0].transform.position.x - sensors[1].transform.position.x) / 2, walls[index].transform.localScale.y, wallThickness);
                    walls[index].transform.position = new Vector3(Mathf.Abs((sensors[0].transform.position.x - sensors[1].transform.position.x) / 2) + Mathf.Min(sensors[0].transform.position.x, sensors[1].transform.position.x), walls[index].transform.position.y, sensors[1].transform.position.z - (Mathf.Abs(sensors[0].transform.position.z - sensors[1].transform.position.z) / 3.5f));
                    break;
                case 6:
                    //see case 0
                    walls[index].transform.localScale = new Vector3(Mathf.Abs(walls[4].transform.position.z - walls[5].transform.position.z), walls[index].transform.localScale.y, wallThickness);
                    walls[index].transform.position = new Vector3(walls[4].transform.position.x - (walls[4].transform.localScale.x) / 2, walls[index].transform.position.y, Mathf.Abs(sensors[1].transform.position.z - sensors[0].transform.position.z) / 2 + Mathf.Min(sensors[0].transform.position.z, sensors[1].transform.position.z));
                    break;
                case 7:
                    //see case 0
                    walls[index].transform.localScale = new Vector3(Mathf.Abs(walls[4].transform.position.z - walls[5].transform.position.z), walls[index].transform.localScale.y, wallThickness);
                    walls[index].transform.position = new Vector3(walls[4].transform.position.x + (walls[4].transform.localScale.x) / 2, walls[index].transform.position.y, Mathf.Abs(sensors[1].transform.position.z - sensors[0].transform.position.z) / 2 + Mathf.Min(sensors[0].transform.position.z, sensors[1].transform.position.z));
                    break;
                
                    // unfinished past here
                    //~~~~~~~~~~~~~~~~~~~~~
                case 8: //diagwall Set #1 (long)
                    walls[index].transform.localScale = new Vector3(Mathf.Sqrt(Mathf.Pow(walls[2].transform.position.x - sensors[0].transform.position.x,2) + Mathf.Pow(walls[2].transform.position.z - sensors[0].transform.position.z, 2)) + .1f , walls[index].transform.localScale.y, wallThickness);
                    walls[index].transform.position = new Vector3(Mathf.Abs((sensors[0].transform.position.x - sensors[1].transform.position.x) / 2) + Mathf.Min(sensors[0].transform.position.x, sensors[1].transform.position.x), walls[index].transform.position.y, sensors[1].transform.position.z - (Mathf.Abs(sensors[0].transform.position.z - sensors[1].transform.position.z) / 3.5f));
                    walls[index].transform.Rotate((new Vector3(0, Mathf.Atan2(Mathf.Abs(walls[8].transform.position.z - sensors[1].transform.position.z), Mathf.Abs(walls[8].transform.position.x - sensors[0].transform.position.x)) * (-180 / Mathf.PI), 0)));
                    break;
                case 9: //diagwall Set #1 (short)
                    walls[index].transform.position = new Vector3(walls[8].transform.position.x, walls[8].transform.position.y, walls[8].transform.position.z - (Mathf.Abs(sensors[0].transform.position.z - sensors[1].transform.position.z) / 3.5f));
                    walls[index].transform.localScale = new Vector3(Vector3.Distance(walls[index].transform.position, new Vector3((walls[4].transform.position.x - (walls[4].transform.localScale.x /2)), walls[4].transform.position.y, walls[4].transform.position.z)) * 2, walls[index].transform.localScale.y, wallThickness);
                    walls[index].transform.rotation = walls[8].transform.rotation;
                    break;
                case 10: //diagwall Set #1 (supplemental wall)
                    walls[index].transform.position = new Vector3(walls[7].transform.position.x, walls[7].transform.position.y, walls[9].transform.position.z);
                    walls[index].transform.localScale = new Vector3(Mathf.Abs(walls[4].transform.position.z - walls[9].transform.position.z) * 2, walls[index].transform.localScale.y, wallThickness);
                    walls[index].transform.rotation = walls[2].transform.rotation;
                    break;
                case 11:
                    walls[index].transform.localScale = new Vector3(Mathf.Abs(sensors[0].transform.position.z - sensors[1].transform.position.z), walls[index].transform.localScale.y, wallThickness);
                    break;

                default:
                    walls[index].transform.localScale = new Vector3(0, 0, 0);
                    walls[index].transform.position = new Vector3(0, 0, 0);
                    //hides unused walls
                    var temp1 = walls[index].GetComponent<Renderer>();
                    temp1.enabled = false;
                    break;
            }
        }
        setPosition = true;
    }

    private void Box_Config()
    {
        setScale();


            //Iterate and update each wall position depending on the sensor locations
            for (int index = 0; index < walls.Length; index++)
        {
            switch (index)
            { // lines 3 and 4 of each case to be combined with  1 & 2 soon tm. hall width will also need adjusted
                case 0:
                    //sets the position of outer wall based on sensor position
                    walls[index].transform.localScale = new Vector3(Mathf.Abs(sensors[0].transform.position.x - sensors[1].transform.position.x), walls[index].transform.localScale.y, wallThickness);
                    walls[index].transform.position = new Vector3((Mathf.Abs(sensors[0].transform.position.x - sensors[1].transform.position.x) / 2) + Mathf.Min(sensors[0].transform.position.x, sensors[1].transform.position.x), walls[index].transform.position.y, sensors[1].transform.position.z);
                    walls[index].transform.position = new Vector3(walls[index].transform.position.x * scale, walls[index].transform.position.y, walls[index].transform.position.z * scale);
                    walls[index].transform.localScale = new Vector3(walls[index].transform.localScale.x * scale, walls[index].transform.localScale.y, walls[index].transform.localScale.z * scale);
                    break;
                case 1:
                    //see case 0
                    walls[index].transform.localScale = new Vector3(Mathf.Abs(sensors[0].transform.position.x - sensors[1].transform.position.x), walls[index].transform.localScale.y, wallThickness);
                    walls[index].transform.position = new Vector3(Mathf.Abs((sensors[0].transform.position.x - sensors[1].transform.position.x) / 2) + Mathf.Min(sensors[0].transform.position.x, sensors[1].transform.position.x), walls[index].transform.position.y, sensors[0].transform.position.z);
                    walls[index].transform.position = new Vector3(walls[index].transform.position.x * scale, walls[index].transform.position.y, walls[index].transform.position.z * scale);
                    walls[index].transform.localScale = new Vector3(walls[index].transform.localScale.x * scale, walls[index].transform.localScale.y, walls[index].transform.localScale.z * scale);
                    break;
                case 2:
                    //see case 0
                    walls[index].transform.localScale = new Vector3(Mathf.Abs(sensors[0].transform.position.z - sensors[1].transform.position.z), walls[index].transform.localScale.y, wallThickness);
                    walls[index].transform.position = new Vector3(sensors[1].transform.position.x, walls[index].transform.position.y, Mathf.Abs(sensors[1].transform.position.z - sensors[0].transform.position.z) / 2 + Mathf.Min(sensors[0].transform.position.z, sensors[1].transform.position.z));
                    walls[index].transform.position = new Vector3(walls[index].transform.position.x * scale, walls[index].transform.position.y, walls[index].transform.position.z * scale);
                    walls[index].transform.localScale = new Vector3(walls[index].transform.localScale.x * scale, walls[index].transform.localScale.y, walls[index].transform.localScale.z * scale);
                    break;
                case 3:
                    //see case 0
                    walls[index].transform.localScale = new Vector3(Mathf.Abs(sensors[0].transform.position.z - sensors[1].transform.position.z), walls[index].transform.localScale.y, wallThickness);
                    walls[index].transform.position = new Vector3(sensors[0].transform.position.x, walls[index].transform.position.y, Mathf.Abs(sensors[1].transform.position.z - sensors[0].transform.position.z) / 2 + Mathf.Min(sensors[0].transform.position.z, sensors[1].transform.position.z));
                    walls[index].transform.position = new Vector3(walls[index].transform.position.x * scale, walls[index].transform.position.y, walls[index].transform.position.z * scale);
                    walls[index].transform.localScale = new Vector3(walls[index].transform.localScale.x * scale, walls[index].transform.localScale.y, walls[index].transform.localScale.z * scale);
                    break;
                case 4:  
                    //sets the position of inner wall based on sensor position
                    walls[index].transform.localScale = new Vector3(Mathf.Abs(sensors[0].transform.position.x - sensors[1].transform.position.x) / 2, walls[index].transform.localScale.y, wallThickness);
                    walls[index].transform.position = new Vector3((Mathf.Abs(sensors[0].transform.position.x - sensors[1].transform.position.x) / 2) + Mathf.Min(sensors[0].transform.position.x, sensors[1].transform.position.x), walls[index].transform.position.y, Mathf.Max(sensors[0].transform.position.z, sensors[1].transform.position.z) - (Mathf.Abs(sensors[0].transform.position.z - sensors[1].transform.position.z) / 3.5f) );
                    walls[index].transform.position = new Vector3(walls[index].transform.position.x * scale, walls[index].transform.position.y, walls[index].transform.position.z * scale);
                    walls[index].transform.localScale = new Vector3(walls[index].transform.localScale.x * scale, walls[index].transform.localScale.y, walls[index].transform.localScale.z * scale);
                    break;
                case 5:  
                    //see case 4
                    walls[index].transform.localScale = new Vector3(Mathf.Abs(sensors[0].transform.position.x - sensors[1].transform.position.x) / 2, walls[index].transform.localScale.y, wallThickness);
                    walls[index].transform.position = new Vector3(Mathf.Abs((sensors[0].transform.position.x - sensors[1].transform.position.x) / 2) + Mathf.Min(sensors[0].transform.position.x, sensors[1].transform.position.x), walls[index].transform.position.y, Mathf.Min(sensors[0].transform.position.z, sensors[1].transform.position.z) + (Mathf.Abs(sensors[0].transform.position.z - sensors[1].transform.position.z) / 3.5f));
                    walls[index].transform.position = new Vector3(walls[index].transform.position.x * scale, walls[index].transform.position.y, walls[index].transform.position.z * scale);
                    walls[index].transform.localScale = new Vector3(walls[index].transform.localScale.x * scale, walls[index].transform.localScale.y, walls[index].transform.localScale.z * scale);
                    break;
                case 6:
                    //see case 4
                    walls[index].transform.localScale = new Vector3(Mathf.Abs(walls[4].transform.position.z - walls[5].transform.position.z), walls[index].transform.localScale.y, wallThickness);
                    walls[index].transform.position = new Vector3(walls[4].transform.position.x - (walls[4].transform.localScale.x) / 2, walls[index].transform.position.y, Mathf.Abs(sensors[1].transform.position.z - sensors[0].transform.position.z) / 2 + Mathf.Min(sensors[0].transform.position.z, sensors[1].transform.position.z));
                    walls[index].transform.position = new Vector3(walls[index].transform.position.x, walls[index].transform.position.y, walls[index].transform.position.z * scale);
                    walls[index].transform.localScale = new Vector3(walls[index].transform.localScale.x, walls[index].transform.localScale.y, walls[index].transform.localScale.z * scale);
                    break;
                case 7:
                    //see case 4
                    walls[index].transform.localScale = new Vector3(Mathf.Abs(walls[4].transform.position.z - walls[5].transform.position.z), walls[index].transform.localScale.y, wallThickness);
                    walls[index].transform.position = new Vector3(walls[4].transform.position.x + (walls[4].transform.localScale.x) / 2, walls[index].transform.position.y, Mathf.Abs(sensors[1].transform.position.z - sensors[0].transform.position.z) / 2 + Mathf.Min(sensors[0].transform.position.z, sensors[1].transform.position.z));
                    walls[index].transform.position = new Vector3(walls[index].transform.position.x, walls[index].transform.position.y, walls[index].transform.position.z * scale);
                    walls[index].transform.localScale = new Vector3(walls[index].transform.localScale.x, walls[index].transform.localScale.y, walls[index].transform.localScale.z * scale);
                    break;
                default:
                    walls[index].transform.localScale = new Vector3(0,0,0);
                    walls[index].transform.position = new Vector3(0,0,0);
                    //hides unused walls
                    var temp1 = walls[index].GetComponent<Renderer>();
                    temp1.enabled = false;
                    break;
            }
        }
        setPosition = true;
    }

    private void Great_Hall_Config()
    {
        //the point at which to anchor the great hall
        Vector3 anchor = new Vector3(Mathf.Min(sensors[0].transform.position.x, sensors[1].transform.position.x) + Mathf.Abs(sensors[0].transform.position.x - sensors[1].transform.position.x) / 2, Great_Hall.transform.position.y, Mathf.Min(sensors[0].transform.position.z, sensors[1].transform.position.z) + Mathf.Abs(sensors[0].transform.position.z - sensors[1].transform.position.z) / 2);
        Great_Hall.transform.position = anchor;

        this.GetComponent<Renderer>().enabled = false;

        int ScaleMode;

        //check condition and set scale
        /*
        float scale = 0;
        if (settings.condition == "Matched")
        { scale = 1f; }
        else if (settings.condition == "Faster")
        { scale = settings.fasterScale; }
        else if (settings.condition == "Slower")
        { scale = settings.slowerScale; }
        else
        { Debug.Log("Scale not properly set"); }*/

        //temporary scale settings
        switch (settings.currentCondition)
        {
            case SettingsSingleton.Condition.Matched:
                Debug.Log("Scale = Matched.");
                ScaleMode = 0;
                break;
            case SettingsSingleton.Condition.Faster:
                Debug.Log("Scale = Faster.");
                ScaleMode = 1;
                break;
            case SettingsSingleton.Condition.Slower:
                Debug.Log("Scale = Slower.");
                ScaleMode = 2;
                break;
            default:
                Debug.Log("Scale not properly set. Defaulting to matched.");
                ScaleMode = 0;
                break;
        }


        //Iterate and update each wall position depending on the sensor locations
        for (int index = 0; index < walls.Length; index++)
        {
            switch (index)
            {
                default:
                    walls[index].transform.localScale = new Vector3(0, 0, 0);
                    walls[index].transform.position = new Vector3(0, 0, 0);
                    //hides unused walls
                    var temp1 = walls[index].GetComponent<Renderer>();
                    temp1.enabled = false;
                    break;
            }

        }

        //matched condition
        if (ScaleMode == 0)
        {
            var MatchScaleMod = new Vector2(DeltaX_RunSpace / DeltaX_DevSpace, DeltaZ_RunSpace / DeltaZ_DevSpace);
            for (int index = 0; index < tables.Length; index++)
            {
                switch (index)
                {
                    case 0:
                        tables[index].transform.position = new Vector3(Mathf.Min(sensors[0].transform.position.x, sensors[1].transform.position.x) + (DeltaX_RunSpace / 2), tables[index].transform.position.y, Mathf.Min(sensors[0].transform.position.z, sensors[1].transform.position.z) + (DeltaZ_RunSpace / 2));
                        tables[index].transform.localScale = new Vector3(tables[index].transform.localScale.x * MatchScaleMod.x, tables[index].transform.localScale.y, tables[index].transform.localScale.z * .3f * MatchScaleMod.y);
                        break;
                    case 1:
                        tables[index].transform.Rotate(0, 90, 0);
                        tables[index].transform.position = new Vector3(Mathf.Min(sensors[0].transform.position.x, sensors[1].transform.position.x) + (Mathf.Abs(sensors[0].transform.position.x - sensors[1].transform.position.x) / 2), tables[index].transform.position.y, Mathf.Max(sensors[0].transform.position.z, sensors[1].transform.position.z) + (tables[index].GetComponent<Renderer>().bounds.size.x / 2));
                        break;
                    case 2:
                        tables[index].transform.Rotate(0, 90, 0);
                        tables[index].transform.position = new Vector3(Mathf.Min(sensors[0].transform.position.x, sensors[1].transform.position.x) + (Mathf.Abs(sensors[0].transform.position.x - sensors[1].transform.position.x) / 2), tables[index].transform.position.y, Mathf.Min(sensors[0].transform.position.z, sensors[1].transform.position.z) - (tables[index].GetComponent<Renderer>().bounds.size.x / 2));
                        break;
                    default:
                        tables[index].transform.localScale = new Vector3(0, 0, 0);
                        tables[index].transform.position = new Vector3(0, 0, 0);
                        //hides unused tables
                        var temp1 = tables[index].GetComponent<Renderer>();
                        temp1.enabled = false;
                        break;
                }

            }
        }

        //faster condition
        else if (ScaleMode == 1)
        {
            var FastScaleMod = new Vector2(DeltaX_RunSpace / DeltaX_DevSpace, DeltaZ_RunSpace / DeltaZ_DevSpace);

            for (int index = 0; index < tables.Length; index++)
            {
                switch (index)
                {
                    case 0: // current offset positon is wrong by  -.06775376f 
                        tables[index].transform.position = new Vector3(sensors[0].transform.position.x, tables[index].transform.position.y, Mathf.Min(sensors[0].transform.position.z, sensors[1].transform.position.z) + (Mathf.Abs(sensors[0].transform.position.z - sensors[1].transform.position.z) / 2));
                        tables[index].transform.localScale = new Vector3(tables[index].transform.localScale.x * .9f * FastScaleMod.x, tables[index].transform.localScale.y, tables[index].transform.localScale.z * .9f * FastScaleMod.y);
                        break;
                    case 1:
                        tables[index].transform.position = new Vector3(sensors[1].transform.position.x, tables[index].transform.position.y, Mathf.Min(sensors[0].transform.position.z, sensors[1].transform.position.z) + (Mathf.Abs(sensors[0].transform.position.z - sensors[1].transform.position.z) / 2));
                        tables[index].transform.localScale = new Vector3(tables[index].transform.localScale.x * .9f * FastScaleMod.x, tables[index].transform.localScale.y, tables[index].transform.localScale.z * .9f * FastScaleMod.y);
                        break;
                    case 2:
                        tables[index].transform.Rotate(0, 90, 0);
                        tables[index].transform.position = new Vector3(Mathf.Min(sensors[0].transform.position.x, sensors[1].transform.position.x) + (Mathf.Abs(sensors[0].transform.position.x - sensors[1].transform.position.x) / 2), tables[index].transform.position.y, Mathf.Max(sensors[0].transform.position.z, sensors[1].transform.position.z) + (tables[index].GetComponent<Renderer>().bounds.size.x / 2));
                        break;
                    case 3:
                        tables[index].transform.Rotate(0, 90, 0);
                        tables[index].transform.position = new Vector3(Mathf.Min(sensors[0].transform.position.x, sensors[1].transform.position.x) + (Mathf.Abs(sensors[0].transform.position.x - sensors[1].transform.position.x) / 2), tables[index].transform.position.y, Mathf.Min(sensors[0].transform.position.z, sensors[1].transform.position.z) - (tables[index].GetComponent<Renderer>().bounds.size.x / 2));
                        break;
                    default:
                        tables[index].transform.localScale = new Vector3(0, 0, 0);
                        tables[index].transform.position = new Vector3(0, 0, 0);
                        //hides unused tables
                        var temp1 = tables[index].GetComponent<Renderer>();
                        temp1.enabled = false;
                        break;
                }

            }
        }

        //slower condition
        else if (ScaleMode == 2)
        {
            var SlowScaleMod = new Vector2(DeltaX_RunSpace / DeltaX_DevSpace, DeltaZ_RunSpace / DeltaZ_DevSpace);
            for (int index = 0; index < tables.Length; index++)
            {
                switch (index)
                {
                    case 0:
                        tables[index].transform.position = new Vector3(sensors[0].transform.position.x, tables[index].transform.position.y, Mathf.Min(sensors[0].transform.position.z, sensors[1].transform.position.z) + (Mathf.Abs(sensors[0].transform.position.z - sensors[1].transform.position.z) / 2));
                        tables[index].transform.localScale = new Vector3(tables[index].transform.localScale.x * SlowScaleMod.x, tables[index].transform.localScale.y, tables[index].transform.localScale.z * .4f * SlowScaleMod.y);
                        break;
                    case 1:
                        tables[index].transform.position = new Vector3(sensors[1].transform.position.x, tables[index].transform.position.y, Mathf.Min(sensors[0].transform.position.z, sensors[1].transform.position.z) + (Mathf.Abs(sensors[0].transform.position.z - sensors[1].transform.position.z) / 2));
                        tables[index].transform.localScale = new Vector3(tables[index].transform.localScale.x * SlowScaleMod.x, tables[index].transform.localScale.y, tables[index].transform.localScale.z * .4f * SlowScaleMod.y);
                        break;
                    case 2:
                        tables[index].transform.Rotate(0, 90, 0);
                        tables[index].transform.position = new Vector3(Mathf.Min(sensors[0].transform.position.x, sensors[1].transform.position.x) + (Mathf.Abs(sensors[0].transform.position.x - sensors[1].transform.position.x) / 2), tables[index].transform.position.y, Mathf.Max(sensors[0].transform.position.z, sensors[1].transform.position.z));
                        break;
                    case 3:
                        tables[index].transform.Rotate(0, 90, 0);
                        tables[index].transform.position = new Vector3(Mathf.Min(sensors[0].transform.position.x, sensors[1].transform.position.x) + (Mathf.Abs(sensors[0].transform.position.x - sensors[1].transform.position.x) / 2), tables[index].transform.position.y, Mathf.Min(sensors[0].transform.position.z, sensors[1].transform.position.z));
                        break;
                    default:
                        tables[index].transform.localScale = new Vector3(0, 0, 0);
                        tables[index].transform.position = new Vector3(0, 0, 0);
                        //hides unused tables
                        var temp1 = tables[index].GetComponent<Renderer>();
                        temp1.enabled = false;
                        break;
                }

            }
        }

        setPosition = true;
    }

    private void Single_Hall_Gordon(int orientation)
    {
        Vector3 sensor0 = Vector3.zero;
        Vector3 sensor1 = Vector3.zero;

        //If it's the matched condition just get the position of the base stations
        if (settings.currentCondition == SettingsSingleton.Condition.Matched)
        {
            if(orientation == 0)
            {
                float temp = sensors[0].transform.position.z;
                sensor0 = new Vector3(sensors[0].transform.position.x, sensors[0].transform.position.y, sensors[1].transform.position.z);
                sensor1 = new Vector3(sensors[1].transform.position.x, sensors[1].transform.position.y, temp);
            }
            else if (orientation == 1)
            {
                sensor0 = sensors[0].transform.position;
                sensor1 = sensors[1].transform.position;
            }
        }
        //If it's not the matched condition get boundary from extended playroom gizmo
        else
        {
            if(orientation == 0)
            {
                sensor0 = vertices[0];
                sensor1 = vertices[2];
            }
            else if (orientation == 1)
            {
                sensor0 = vertices[1];
                sensor1 = vertices[3];
            }

        }

        //Calculate center points and wall widths
        float centerX = (Mathf.Abs(sensor0.x - sensor1.x) / 2) + Mathf.Min(sensor0.x, sensor1.x);
        float centerZ = (Mathf.Abs(sensor0.z - sensor1.z) / 2) + Mathf.Min(sensor0.z, sensor1.z);

        float widthX = Mathf.Abs(sensor0.x - sensor1.x);
        float widthZ = Mathf.Abs(sensor0.z - sensor1.z);

        //Calculate angles for rotated and end walls
        float opp = Mathf.Abs(sensor1.z - sensor0.z);
        float hyp = new Vector2(sensor1.x - sensor0.x, sensor1.z - sensor0.z).magnitude;
        float angle = Mathf.Asin((opp / hyp));

        float longDiagonalLength = new Vector2(widthX, widthZ).magnitude;

        //Iterate and update each wall position depending on the sensor locations
        for (int index = 0; index < walls.Length; index++)
        {


            //First enable all walls then turn off unwanted ones
            walls[index].GetComponent<Renderer>().enabled = true;


            setScale();


            switch (index)
            {
                case 0:
                    //sets the position of outer wall based on sensor position
                    walls[index].transform.localScale = new Vector3(widthX, walls[index].transform.localScale.y, wallThickness);
                    walls[index].transform.position = new Vector3(centerX, walls[index].transform.position.y, sensor1.z);
                    walls[index].transform.rotation = Quaternion.AngleAxis(0f, Vector3.right);
                    //Disable for now
                    walls[index].GetComponent<Renderer>().enabled = false;
                    break;
                case 1:
                    //see case 0
                    walls[index].transform.localScale = new Vector3(widthX, walls[index].transform.localScale.y, wallThickness);
                    walls[index].transform.position = new Vector3(centerX, walls[index].transform.position.y, sensor0.z);
                    walls[index].transform.rotation = Quaternion.AngleAxis(0f, Vector3.right);
                    walls[index].GetComponent<Renderer>().enabled = false;
                    break;
                case 2:
                    //see case 0
                    walls[index].transform.localScale = new Vector3(widthZ, walls[index].transform.localScale.y, wallThickness);
                    walls[index].transform.position = new Vector3(sensor1.x, walls[index].transform.position.y, centerZ);
                    walls[index].transform.rotation = Quaternion.AngleAxis(90f, Vector3.up);
                    walls[index].GetComponent<Renderer>().enabled = false;
                    break;
                case 3:
                    //see case 0
                    walls[index].transform.localScale = new Vector3(widthZ, walls[index].transform.localScale.y, wallThickness);
                    walls[index].transform.position = new Vector3(sensor0.x, walls[index].transform.position.y, centerZ);
                    walls[index].transform.rotation = Quaternion.AngleAxis(90f, Vector3.up);
                    walls[index].GetComponent<Renderer>().enabled = false;
                    break;
                case 4:
                    //Rotate wall based on base station locations

                    if(orientation == 0)
                    {
                        walls[index].transform.localRotation = Quaternion.AngleAxis(-angle * Mathf.Rad2Deg, Vector3.up);
                    }
                    else if (orientation == 1)
                        walls[index].transform.localRotation = Quaternion.AngleAxis(angle * Mathf.Rad2Deg, Vector3.up);

                    //Scale wall based on base station locations 
                    walls[index].transform.localScale = new Vector3(longDiagonalLength, walls[index].transform.localScale.y, wallThickness);

                    //Center the angled wall
                    walls[index].transform.position = new Vector3(centerX, walls[index].transform.position.y, centerZ);

                    //Offset wall from center to allow hallway
                    walls[index].transform.position += walls[index].transform.forward * hallwayHalfWidth;
                    break;
                case 5:
                    //Rotate wall based on base station locations
                    if (orientation == 0)
                    {
                        walls[index].transform.localRotation = Quaternion.AngleAxis(-angle * Mathf.Rad2Deg, Vector3.up);
                    }
                    else if (orientation == 1)
                        walls[index].transform.localRotation = Quaternion.AngleAxis(angle * Mathf.Rad2Deg, Vector3.up);

                    //Scale wall based on base station locations 
                    walls[index].transform.localScale = new Vector3(longDiagonalLength, walls[index].transform.localScale.y, wallThickness);

                    //Center the angled wall
                    walls[index].transform.position = new Vector3(centerX, walls[index].transform.position.y, centerZ);

                    //Offset wall from center to allow hallway
                    walls[index].transform.position -= walls[index].transform.forward * hallwayHalfWidth;


                    break;
                case 6:
                    //Scale wall based on base station locations 
                    walls[index].transform.localScale = new Vector3(2*hallwayHalfWidth, walls[index].transform.localScale.y, wallThickness);

                    //Center the end wall at the end of the hallway
                    walls[index].transform.position = new Vector3(sensor0.x, walls[index].transform.position.y, sensor0.z);

                    //Rotate to look at center of play space
                    walls[index].transform.LookAt(new Vector3(centerX, walls[index].transform.position.y, centerZ));


                    //Offset wall from sensor to end
                    if (orientation == 0)
                        walls[index].transform.position += walls[index].transform.forward * Mathf.Max(Mathf.Abs(Mathf.Tan(angle + Mathf.PI / 2)), Mathf.Abs(Mathf.Tan(angle))) * hallwayHalfWidth / 2;
                    else if (orientation == 1)
                        walls[index].transform.position += walls[index].transform.forward * Mathf.Max(Mathf.Abs(Mathf.Tan(angle + Mathf.PI / 2)), Mathf.Abs(Mathf.Tan(angle))) * hallwayHalfWidth;
                    break;

                case 7:
                    //Scale wall based on base station locations 
                    walls[index].transform.localScale = new Vector3(2 * hallwayHalfWidth, walls[index].transform.localScale.y, wallThickness);

                    //Center the end wall at the end of the hallway
                    walls[index].transform.position = new Vector3(sensor1.x, walls[index].transform.position.y, sensor1.z);

                    //Rotate to look at center of play space
                    walls[index].transform.LookAt(new Vector3(centerX, walls[index].transform.position.y, centerZ));


                    //Offset wall from sensor to end
                    if(orientation == 0)
                        walls[index].transform.position += walls[index].transform.forward * Mathf.Max(Mathf.Abs(Mathf.Tan(angle + Mathf.PI / 2)), Mathf.Abs(Mathf.Tan(angle))) * hallwayHalfWidth / 2;
                    else if (orientation == 1)
                        walls[index].transform.position += walls[index].transform.forward * Mathf.Max(Mathf.Abs(Mathf.Tan(angle + Mathf.PI / 2)), Mathf.Abs(Mathf.Tan(angle))) * hallwayHalfWidth;
                    break;
                default:
                    //hides unused walls
                    walls[index].GetComponent<Renderer>().enabled = false;
                    break;

            }
        }
        setPosition = true;
    }


}
