////////////////////////////////////////////////////////////////////////////////////
// Experiment: Risk Aversion
// Purpose: Main features of the experiment
// Code by: Butler, Michael
// Email: msbcoding@gmail.com
////////////////////////////////////////////////////////////////////////////////////
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MasterControl : MonoBehaviour {
    //File variables
	public string InputPath = "";
    private System.IO.StreamReader InputFile;
    private string BlockIdentifier = "Block";
    private string DataIdentifier = "Data";
    private string PracticeIdentifier = "Practice";
    private MovementRecorder MFW;

    //temporary&testing variables
    public List<TempStruct> TrialSettings;
    private bool TrialSet;
    public bool modeTest = false;
    public string SubjectT;
    public BoxCollider PlayerHitbox;


    //Marker Variables
	public GameObject marker1;
	public GameObject marker2;
    [Tooltip("The distance between the floor markers")]
    public float MarkerDistance;
    private Vector3[] MarkerPositions;
    private GameObject CurrentMarker;
    private bool ActiveMarker;
    public float DiagOffset, RoomOffset;

    //Trigger Related Variables
    [Tooltip("The distance from the active marker where the markers will vanish")]
    public float MarkerHide;
    [Tooltip("The distance from the active marker where the goal and obstacle will appear")]
    public float PostsDisplay;
    public TriggerHandler t1, t2;

    //Goal Variables
    public GameObject goal;
	public Vector3 goalOffset;

    //Obstacle Variables
    public GameObject obstacle;
	public Vector3 obstacleOffset;

    //Obstacle/Goal Shared variables
    private Transform AnchorPoint;

    [HideInInspector]
    public bool hasStarted = false;
	private int randomRow = -1;

	private static MasterControl _instance;
	public static MasterControl Instance{
		get{
			return _instance;
		}
		set{
			if(_instance == null){
				_instance = value;
			}else{
				GameObject.Destroy(value);
			}
		}
	}

    void Awake() {
        MasterControl.Instance = this;
        if (!modeTest)
        {
            InputPath = Application.dataPath + "/Input/" + "Subject" + SubjectSettings.instance.SubjectID + ".csv";
            InputFile = new System.IO.StreamReader(InputPath);
        }
        else
        {
            InputPath = Application.dataPath + "/Input/" + "Subject" + SubjectT + ".csv";
            InputFile = new System.IO.StreamReader(InputPath);
        }
        Time_Trials.TrialCounter = 0;
        TrialSettings = new List<TempStruct>();
        MFW = gameObject.GetComponent<MovementRecorder>();
        PlayerHitbox.transform.localScale = new Vector3(1, 1, 2 * float.Parse(SubjectSettings.instance.ShWidth));
        //temporary function
        MakeArray();
        
    }

    void Start()
    {
        Debug.Log("Starting experiment controller.");
        Reset();
    }
    void Reset()
    {
        MarkerPositions = new Vector3[] { new Vector3(DiagOffset, 0, (MarkerDistance / 2) + (marker1.transform.localScale.z / 2) - RoomOffset), new Vector3(-DiagOffset, 0, (-MarkerDistance / 2) - (marker1.transform.localScale.z / 2) + RoomOffset) };
        ActiveMarker = false;
        TrialSet = false;

        hasStarted = false;
        randomRow = -1;
        SetupMarkers();
        ResetPosts();
    }



    void ResetPosts()
    {
        obstacle.transform.position = new Vector3(0,1,0);
        goal.transform.position = new Vector3(0,1,0);

        //obstacle.transform.localRotation = new Quaternion(0, 0, 0, 0);
        //goal.transform.localRotation = new Quaternion(0, 0, 0, 0);
    }


    void PullTrial()
    {
        string FeederString;
            if (AnchorPoint.transform.position.z > 0)
            {
                obstacle.transform.position = new Vector3(0, obstacle.transform.position.y, AnchorPoint.transform.position.z - TrialSettings[Time_Trials.TrialCounter].OD);
                goal.transform.position = new Vector3(0, goal.transform.position.y, AnchorPoint.transform.position.z - TrialSettings[Time_Trials.TrialCounter].GD);
            }
            else
            {
                obstacle.transform.position = new Vector3(0, obstacle.transform.position.y, AnchorPoint.transform.position.z + TrialSettings[Time_Trials.TrialCounter].OD);
                goal.transform.position = new Vector3(0, goal.transform.position.y, AnchorPoint.transform.position.z + TrialSettings[Time_Trials.TrialCounter].GD);
            }


            obstacle.transform.RotateAround(AnchorPoint.transform.position, Vector3.up, TrialSettings[Time_Trials.TrialCounter].OA + AnchorPoint.transform.rotation.y);
            goal.transform.RotateAround(AnchorPoint.transform.position, Vector3.up, TrialSettings[Time_Trials.TrialCounter].GA + AnchorPoint.transform.rotation.y);

            FeederString = TrialSettings[Time_Trials.TrialCounter].OA.ToString() + ',' +  TrialSettings[Time_Trials.TrialCounter].OD.ToString() + ',' + TrialSettings[Time_Trials.TrialCounter].GA.ToString() + ',' + TrialSettings[Time_Trials.TrialCounter].GD.ToString() + ',' + TrialSettings[Time_Trials.TrialCounter].OR.ToString();
            MFW.NewTrial(FeederString);
    }

    void MakeArray()
    {
        string line;
        string[] parsing;

        while((line = InputFile.ReadLine()) != null)
        {
            if(line.ToLower().Contains(DataIdentifier.ToLower()))
            {
                line = line.Trim(' ');
                line = line.Trim('\n');
                parsing = line.Split(',');
                TrialSettings.Add(new TempStruct(float.Parse(parsing[1]), float.Parse(parsing[2]), float.Parse(parsing[3]), float.Parse(parsing[4]), int.Parse(parsing[5])));
            }
            else if (line.ToLower().Contains(PracticeIdentifier.ToLower()))
            {
                //do stuff for practice seperations TBD
            }
            else if (line.ToLower().Contains("subject"))
            {
                line = line.Trim('\n');
                line = line.Split(' ')[1];
                MFW.SubjectID = line;
            }
        }
        MFW.SetUp();
        InputFile.Close();
    }

	void Update(){
		if(!hasStarted && ActiveMarker && Input.GetButtonDown("Start"))
        {
			hasStarted = true;
			ActivateMarkers();
            MFW.Record = true;
            Time_Trials.ResetTrialTime();
        }
        if(hasStarted && Input.GetButtonDown("Reset")) // only changes made for the reset update are here
        {
            MFW.Record = false;
            MFW.NullLine();
            SetupMarkers();
            goal.GetComponent<GenericObjectController>().Hide();
            obstacle.GetComponent<GenericObjectController>().Hide();
            hasStarted = false;
            ActiveMarker = false;
            PullTrial();
        }
	}

    void FixedUpdate()
    {
        MFW.Recorder();
    }

	void SetupMarkers(){
        marker1.transform.position = MarkerPositions[0];
		marker1.GetComponent<MarkerController>().Show();
		marker1.GetComponent<MarkerController>().MakeRed();
		marker2.transform.position = MarkerPositions[1];
		marker2.GetComponent<MarkerController>().Show();
        marker2.GetComponent<MarkerController>().MakeRed();
        marker1.transform.LookAt(marker2.transform.position);
        marker2.transform.LookAt(marker1.transform.position);
        OrientTriggers(t1);
        OrientTriggers(t2);

    }

	void ActivateMarkers(){
		marker1.GetComponent<MarkerController>().MakeGreen();
		marker2.GetComponent<MarkerController>().MakeGreen();
	}

	// Called from marker2
	public void EndMarkers(){
		marker1.GetComponent<MarkerController>().Hide();
		marker2.GetComponent<MarkerController>().Hide();
	}

	void SetupGoal(){
        goal.transform.position = AnchorPoint.position + goalOffset;
		obstacle.transform.position = AnchorPoint.position + obstacleOffset;
	}

	public void ActivateGoal(){
		goal.GetComponent<GenericObjectController>().Activate();
        obstacle.GetComponent<GenericObjectController>().Activate();
        obstacle.GetComponent<ModelSelector>().Selector(TrialSettings[Time_Trials.TrialCounter].OR);
    }

	// Called from goal
	public void EndGoal(){
        MFW.Record = false;
		goal.GetComponent<GenericObjectController>().Hide();
		obstacle.GetComponent<GenericObjectController>().Hide();
        Time_Trials.TrialCounter++;
        if (Time_Trials.TrialCounter == TrialSettings.Count)
        {
            //end experiment
            MFW.SaveFile();
            Application.Quit();
        }
        Reset();
	}

    public void MarkerSet(GameObject Marker)
    {
        if (!hasStarted)
        {
            CurrentMarker = Marker;
            ActiveMarker = true;
            AnchorPoint = CurrentMarker.transform;
        }
        if(!TrialSet)
        {
            PullTrial();
            TrialSet = true;
        }
    }

    void OrientTriggers(TriggerHandler OrientMe)
    {
        OrientMe.orient(marker1);
    }

    //quick shortcut function to serve as debug ping for tracking and testing
    void Debugger1()
    {
        Debug.Log("debug msg");
        
    }
}
