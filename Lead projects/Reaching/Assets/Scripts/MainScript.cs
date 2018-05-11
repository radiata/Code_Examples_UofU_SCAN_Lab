////////////////////////////////////////////////////////////////////////////////////
// Experiment: Reaching 
// Code by: Butler, Michael
// Email: msbcoding@gmail.com
////////////////////////////////////////////////////////////////////////////////////

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System;
using System.IO;


public class MainScript : MonoBehaviour {

    public GameObject FemAva, MaleAva;
    public GameObject CvError;

    public bool ForcePlayerPosition;
    public float ForcedHeightAdjustment;
    public GameObject player;
    public GameObject playerCam;
    public GameObject fixationCross;

    public Text Trial, TxtBlock, isPaused;

    public float TimeBetweenTrials;
    private float TrialTimer;
    private float ResponseTime;

    [SerializeField]
    public Blocks[] blockList;
    public GameObject Table;
    public GameObject Player_Chair;
    public GameObject Empty_Chair;
    public GameObject Avatar_Chair;
    public GameObject Ball;

    private int SIZE, BLOCK_SIZE; 
    private float Chair_Table_Distance; // distance between chair center and table edge
    private int iterator = 0, BlockIterator = 0;
    private string FilePath = "Reaching";
    private StreamWriter Output;
    private string InPath = "Config.txt";
    private System.IO.StreamReader InputFile;
    private string TrialListingFile = "TrialListing";

    private string InputRecorder;
    private bool RunningTrial;
    private bool Pause;

    [Serializable]
    public struct Blocks
    {
        public Vector2[] ChairSetups; //value is chair type (0,1) value 2 is chair angle
        public Vector2[] BallSetups; //value 1 is distance value 2 is angle
    }


    // Use this for initialization
    void Start()
    {
        //set forcedHeight
        ForcedHeightAdjustment = SettingsSingleton.instance.Cal_H / 100;
        fixationCross.transform.position = new Vector3(0, (SettingsSingleton.instance.EyeHeight / 100) +.06f, 0);
        float Yscale = 0;
        //set avatar display
        if (SettingsSingleton.instance.isFemale)
        {
            Destroy(MaleAva);
            Yscale = (FemAva.transform.localScale.y) + (.1f * (SettingsSingleton.instance.EyeHeight / 100 - 1.5f));
            FemAva.transform.localScale = new Vector3(FemAva.transform.localScale.x, Yscale, FemAva.transform.localScale.z);
        }
        else
        {
            Destroy(FemAva);
            Yscale = (FemAva.transform.localScale.y) + (.1f * (SettingsSingleton.instance.EyeHeight / 100 - 1.2f));
            MaleAva.transform.localScale = new Vector3(MaleAva.transform.localScale.x, Yscale, MaleAva.transform.localScale.z);
        }

        ReadINI();
        if (ForcePlayerPosition)
        { player.transform.position = new Vector3(Player_Chair.transform.position.x, ForcedHeightAdjustment, Player_Chair.transform.position.z); }
        if (blockList[BlockIterator].ChairSetups.Length > blockList[BlockIterator].BallSetups.Length)
        { SIZE = blockList[BlockIterator].ChairSetups.Length; }
        else { SIZE = blockList[BlockIterator].BallSetups.Length; }
        BLOCK_SIZE = blockList.Length;

        // Set files paths
        string temp = System.DateTime.Now.ToString();
        temp = temp.Replace("/", "_").Replace(":", "-");
        FilePath = "Subject_" + SettingsSingleton.instance.ParticipantID + "_" + FilePath + "_" + temp + ".csv";
        TrialListingFile = "Subject_" + SettingsSingleton.instance.ParticipantID + "_" + TrialListingFile + "_" + temp + ".csv";

        RecordTrial();
        WriteTrialList();
        
        //initialize variables for later use
        ResponseTime = 0;
        TrialTimer = 0;
        Pause = true;
        isPaused.text = "Paused = True";
        TxtBlock.text = "Block Set: " + BlockIterator.ToString();
        Trial.text = "Trial Number: " + iterator.ToString();

        Chair_Table_Distance = Player_Chair.transform.position.z - (Table.transform.localScale.x / 2);
        RunningTrial = false;

        //set table location
        //set player chair location
        Player_Chair.transform.RotateAround(Table.transform.position, Vector3.up, 0);
        Player_Chair.transform.LookAt(new Vector3(Table.transform.position.x, Player_Chair.transform.position.y, Table.transform.position.z));
    }

    // Update is called once per frame
    void Update()
    {

        if (RunningTrial == false)
        {
            if (Pause == false)
            {
                TrialTimer += Time.deltaTime;
                if (TrialTimer >= TimeBetweenTrials)
                {
                    TrialsStart();
                }
            }
            fixationCross.active = true;
        }
        else
        {
            ResponseTime += Time.deltaTime;
            fixationCross.active = false;
            if (Input.GetButtonDown("Left_Input") || Input.GetAxis("Oculus_GearVR_LIndexTrigger") > 0.8f)
            {
                InputRecorder = "Left";
                EndTrial();
                Debug.Log("Trial Ending - Left");
            }
            if(Input.GetButtonDown("Right_Input") || Input.GetAxis("Oculus_GearVR_RIndexTrigger") > 0.8f)
            {
                InputRecorder = "Right";
                EndTrial();
                Debug.Log("Trial Ending - Right");
            }
            if (Input.GetButtonDown("Null"))
            {
                InputRecorder = "Can't Reach";
                EndTrial();
                Debug.Log("Trial Ending - OOR");
            }
            
        }
        if(Pause && Input.GetButtonDown("Pause"))
        {
            Pause = false;
            isPaused.text = "Paused = False";
        }
        else if(!Pause && Input.GetButtonDown("Pause"))
        {
            Pause = true;
            isPaused.text = "Paused = True";
            TrialTimer = 0;
        }
        
    }


    void TrialsStart()
    {
        //set chair type and location
        //face chair to table center
        //set ball distance and location
        if (blockList[BlockIterator].ChairSetups[iterator][0] == 0)
        {
            Empty_Chair.transform.RotateAround(Table.transform.position, Vector3.up, blockList[BlockIterator].ChairSetups[iterator][1]);
            Empty_Chair.transform.LookAt(new Vector3(Table.transform.position.x, Empty_Chair.transform.position.y, Table.transform.position.z));

            Ball.transform.position = new Vector3(Empty_Chair.transform.position.x, Ball.transform.position.y, Empty_Chair.transform.position.z);
            Ball.transform.LookAt(new Vector3(Table.transform.position.x, Ball.transform.position.y, Table.transform.position.z));
            Ball.transform.position += Ball.transform.forward * (blockList[BlockIterator].BallSetups[iterator][0] + Chair_Table_Distance);
            Ball.transform.RotateAround(Empty_Chair.transform.position + Empty_Chair.transform.forward * Chair_Table_Distance, Vector3.up, blockList[BlockIterator].BallSetups[iterator][1]);

            Empty_Chair.active = true;
            Ball.active = true;
        }
        else if (blockList[BlockIterator].ChairSetups[iterator][0] == 1)
        {
            Avatar_Chair.transform.RotateAround(Table.transform.position, Vector3.up, blockList[BlockIterator].ChairSetups[iterator][1]);
            Avatar_Chair.transform.LookAt(new Vector3(Table.transform.position.x, Avatar_Chair.transform.position.y, Table.transform.position.z));

            Ball.transform.position = new Vector3(Avatar_Chair.transform.position.x, Ball.transform.position.y, Avatar_Chair.transform.position.z);
            Ball.transform.LookAt(new Vector3(Table.transform.position.x, Ball.transform.position.y, Table.transform.position.z));
            Ball.transform.position += Ball.transform.forward * (blockList[BlockIterator].BallSetups[iterator][0] + Chair_Table_Distance);
            Ball.transform.RotateAround(Avatar_Chair.transform.position + Avatar_Chair.transform.forward * Chair_Table_Distance, Vector3.up, blockList[BlockIterator].BallSetups[iterator][1]);

            Avatar_Chair.active = true;
            Ball.active = true;
        }
        else
        { Debug.Log("Error Trialstart in MainScript"); }


        //set bool to running trial
        RunningTrial = true;
    }

    void IteratorUpdate()
    {
        iterator++;
        if(iterator >= SIZE)
        {
            BlockIterator++;
            if (BlockIterator >= BLOCK_SIZE)
            {
                //end experiment screen
                Debug.Log("END EXPERIMENT!");
                Output.Close();
                Application.Quit();
            }
            else
            {
                //end block screen
                Pause = true;
                isPaused.text = "Paused = True";
                if (blockList[BlockIterator].ChairSetups.Length > blockList[BlockIterator].BallSetups.Length)
                { SIZE = blockList[BlockIterator].ChairSetups.Length; }
                else { SIZE = blockList[BlockIterator].BallSetups.Length; }
                iterator = 0;
                TxtBlock.text = "Block Set: " + BlockIterator.ToString();
            }
            
        }
        Trial.text = "Trial Number: " + iterator.ToString();
    }
    void EndTrial()
    {
        Debug.Log("Attempting Trial End");
        if (iterator > 0 && iterator % 10 == 0)
        {
            isPaused.text = "Paused = True";
            Pause = true;
        }
        RecordTrial();
        Avatar_Chair.active = false;
        Avatar_Chair.transform.position = Player_Chair.transform.position;
        Empty_Chair.active = false;
        Empty_Chair.transform.position = Player_Chair.transform.position;
        Ball.active = false;
        Ball.transform.position = new Vector3(0, Ball.transform.position.y, 0);
        RunningTrial = false;
        ResponseTime = 0;
        TrialTimer = 0;
        InputRecorder = null;
        IteratorUpdate();
    }
    void RecordTrial()
    {
        if(Output == null)
        {
            Output = File.CreateText(FilePath);
            Output.WriteLine(FilePath);
            Output.WriteLine("Subject ID: " + SettingsSingleton.instance.ParticipantID);
            Output.WriteLine("\nChair type, 0 = empty chair, 1 = avatar");
            Output.WriteLine("Negative Ball Rotation, is a ball on the left\n");
            Output.WriteLine("Block Number,Trial Number,Chair Type,Chair Rotation,Ball Distance,Ball Rotation,Input,Response Time, Was Correct, Distance From Table");

            Debug.Log("Output was null, attempted file creation");
            return;
        }
        float mathStuff = Mathf.Abs(playerCam.transform.position.z) - Mathf.Abs(Table.transform.localScale.x / 2);
        bool wasCorrect;
        string WCString;
        if ((blockList[BlockIterator].BallSetups[iterator][1] > 0 && InputRecorder == "Right") || (blockList[BlockIterator].BallSetups[iterator][1] < 0 && InputRecorder == "Left"))
        { wasCorrect = true; WCString = "True";}
        else if(InputRecorder != "Left" && InputRecorder != "Right")
        { wasCorrect = false; WCString = ""; }
        else { wasCorrect = false; WCString = "False";}

        string WriteMe = BlockIterator.ToString() + ',' + iterator.ToString() + ',' + blockList[BlockIterator].ChairSetups[iterator][0].ToString() + ','
            + blockList[BlockIterator].ChairSetups[iterator][1].ToString() + ',' + blockList[BlockIterator].BallSetups[iterator][0].ToString() + ','
            + blockList[BlockIterator].BallSetups[iterator][1].ToString() + ',' + InputRecorder.ToString() + ',' + ResponseTime.ToString()
            + ',' + WCString + ',' + mathStuff;
        Output.WriteLine(WriteMe);
    }


    void ReadINI()
    {
        bool CompileError;
        if (!File.Exists(InPath))
        {
            Debug.Log("Did not find INI!!!");
            return;
        }
        bool firstBlock = true;

        List<Block> ReadBlocks = new List<Block>();
        int BlockTracker = 0;
        InputFile = new StreamReader(InPath);
        string CurrentLine;
        string[] ParseLine;
        #region ReadingFile
        while ((CurrentLine = InputFile.ReadLine()) != null)
        {
            if (CurrentLine.ToLower().Contains("block"))
            {
                ReadBlocks.Add(new Block(1));
            
                if (!firstBlock)
                { BlockTracker++; }
                else { firstBlock = false; }
            }
            else
            {
                if (CurrentLine.ToLower().Contains("chairtypes"))
                {
                    ParseLine = MiniParse(CurrentLine);
                    foreach (string element in ParseLine)
                    {
                        ReadBlocks[BlockTracker].ChairType.Add(int.Parse(element));
                    }
                }
                else if (CurrentLine.ToLower().Contains("chairpos"))
                {
                    ParseLine = MiniParse(CurrentLine);
                    foreach (string element in ParseLine)
                    {
                        ReadBlocks[BlockTracker].ChairPos.Add(float.Parse(element));
                    }
                }
                else if (CurrentLine.ToLower().Contains("balldist"))
                {
                    ParseLine = MiniParse(CurrentLine);
                    foreach (string element in ParseLine)
                    {
                        ReadBlocks[BlockTracker].BallDist.Add(float.Parse(element));
                    }
                }
                else if (CurrentLine.ToLower().Contains("ballangle"))
                {
                    ParseLine = MiniParse(CurrentLine);
                    foreach (string element in ParseLine)
                    {
                        ReadBlocks[BlockTracker].BallAngle.Add(float.Parse(element));
                    }
                }
            }
            
        }
        #endregion

        #region Setup Blocks
        blockList = new Blocks[ReadBlocks.Count];
        BlockTracker = 0;
        System.Random rand = new System.Random();
        
        foreach (Block element in ReadBlocks)
        {
            #region CP=1
            if (element.ChairPos.Count == 1 && element.ChairType.Count == 1)
            {
                List<Vector2> BallSets = new List<Vector2>();
                foreach(float value in element.BallAngle)
                {
                    foreach(float value2 in element.BallDist)
                    {
                        BallSets.Add(new Vector2(value2,value));
                    }
                }
                blockList[BlockTracker].BallSetups = new Vector2[BallSets.Count];
                blockList[BlockTracker].ChairSetups = new Vector2[BallSets.Count];
                float v1 = element.ChairType[0];
                for (int i = 0; i < blockList[BlockTracker].BallSetups.Length; i++)
                {
                    int temp = rand.Next(BallSets.Count);
                    blockList[BlockTracker].BallSetups[i] = BallSets[temp];
                    BallSets.RemoveAt(temp);
                   
                    blockList[BlockTracker].ChairSetups[i] = new Vector2(v1, 0);
                }  
            }
            else if(element.ChairPos.Count == 1 && element.ChairType.Count != 1)
            {
                Debug.Log(element.ChairType.Count);
                Debug.Log(string.Format("Not setup to handle to current configuration of Block{0}. Too many ChairTypes.", BlockTracker));
            }
            #endregion
            #region CP > 1
            else
            {
                do
                {
                    CompileError = false;
                    List<Vector2> BallSets = new List<Vector2>();
                    foreach (float value in element.BallAngle)
                    {
                        foreach (float value2 in element.BallDist)
                        {
                            BallSets.Add(new Vector2(value2, value));
                        }
                    }
                    List<Vector2> ChairSets = new List<Vector2>();
                    foreach (float value in element.ChairPos)
                    {
                        foreach (float value2 in element.ChairType)
                        {
                            ChairSets.Add(new Vector2(value2, value));
                        }
                    }

                    List<Vector4> tempList = new List<Vector4>();
                    foreach (Vector2 value in ChairSets)
                    {
                        foreach (Vector2 value2 in BallSets)
                        {
                            tempList.Add(new Vector4(value.x, value.y, value2.x, value2.y));
                        }
                    }
                    blockList[BlockTracker].BallSetups = new Vector2[tempList.Count];
                    blockList[BlockTracker].ChairSetups = new Vector2[tempList.Count];

                    //val.y can never be the same two trials in a row
                    for (int k = 0; k < blockList[BlockTracker].BallSetups.Length; k++)
                    {
                        int temp = 0;
                        temp = rand.Next(tempList.Count);

                        int safetyRunner = 0;
                        while (k > 0 && blockList[BlockTracker].ChairSetups[k - 1][1] == tempList[temp].y)//verify no repeat here
                        {
                            if (safetyRunner > tempList.Count)
                            {
                                //do something
                                CanvasError();
                                Debug.LogError("Error (SafetyRunner) MainScript");
                                CompileError = true;
                                break;
                            }
                            if (temp < tempList.Count - 1)
                            {
                                temp++;
                            }
                            else if (temp == tempList.Count - 1)
                            {
                                temp = 0;
                            }
                            safetyRunner++;
                        }
                        blockList[BlockTracker].BallSetups[k] = new Vector2(tempList[temp].z, tempList[temp].w);
                        blockList[BlockTracker].ChairSetups[k] = new Vector2(tempList[temp].x, tempList[temp].y);
                        tempList.RemoveAt(temp);
                    }
                } while (CompileError);
            }
            #endregion
            BlockTracker++;
        }

        #endregion
        Debug.Log(blockList.Length);
        Debug.Log(blockList[0].ChairSetups.Length);
        Debug.Log(blockList[0].BallSetups.Length);
        Debug.Log(blockList[1].ChairSetups.Length);
        Debug.Log(blockList[1].BallSetups.Length);
        Debug.Log(blockList[2].ChairSetups.Length);
        Debug.Log(blockList[2].BallSetups.Length);
    }


    String[] MiniParse(string CurrentLine)
    {
        string[] ParseLine;
        CurrentLine = CurrentLine.Trim(' ');
        ParseLine = CurrentLine.Split(',');
        ParseLine[0] = ParseLine[0].Split(':')[1];
        return ParseLine;
    }

    private struct Block
    {
        public List<int> ChairType;
        public List<float> ChairPos;
        public List<float> BallDist;
        public List<float> BallAngle;

        public Block(int manditory = 0)
        {
            ChairType = new List<int>();
            ChairPos = new List<float>();
            BallDist = new List<float>();
            BallAngle = new List<float>();
        }
    }

    void WriteTrialList()
    {

            StreamWriter NewFile = new StreamWriter(TrialListingFile);
            int iter2 = 0;
            foreach(Blocks element in blockList)
            {
                NewFile.WriteLine("Block:,{0}", iter2);
                int iter = 0;
                NewFile.WriteLine("Chair Type,Chair Loc,Ball Dist,Ball Angle");
                foreach(Vector2 elem2 in element.ChairSetups)
                {
                    NewFile.WriteLine("{0},{1},{2},{3}", element.ChairSetups[iter][0], element.ChairSetups[iter][1], element.BallSetups[iter][0], element.BallSetups[iter][1]);
                    iter++;
                }
                iter2++;
            }
            NewFile.Close();
    }

    public void CanvasError()
    {
        CvError.active = true;
        StartCoroutine(TimeKeeper());
        CvError.active = false;
    }
    IEnumerator TimeKeeper()
    {
        yield return new WaitForSeconds(5);
    }
}


