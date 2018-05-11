////////////////////////////////////////////////////////////////////////////////////
// Experiment: Reaching 
// **Easily portable to other experiments, see prefabs for setup example
// Code by: Butler, Michael
// Email: msbcoding@gmail.com
////////////////////////////////////////////////////////////////////////////////////

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SettingsSingleton : MonoBehaviour {

    //Refer to this static variable to access the subjects settings/information
    public static SettingsSingleton instance = null;
    public float EyeHeight, Cal_H;
    public string ParticipantID;
    public bool isFemale;
    public string MainLevel;

    private bool isSetG, isSetID, isSetEH, isSetCH;

    void Awake()
    {
        //Check if there is an instance of the settings handler that already exists, if not assign this to be the settings handler. Otherwise destroy this gameObject.
        #region Instance Creation/check
        if(instance == null)
        {
            instance = this;
        }
        else if(instance != this)
        {
            Destroy(gameObject);
        }
        #endregion
        DontDestroyOnLoad(gameObject);
    }
	// Use this for initialization
	void Start ()
    {
        //Default values
        EyeHeight = 150;
        Cal_H = 178; //5"10
        ParticipantID = "null";
        isFemale = true;
        isSetEH = false;
        isSetID = false;
        isSetG = false;
        isSetCH = false;
	}

    public void SetID(string val)
    {
        ParticipantID = val;
        isSetID = true;
    }

    public void SetGender(int val)
    {
        if(val == 2)
        {
            isFemale = false;
            isSetG = true;
        }
        else if (val == 1)
        {
            isFemale = true;
            isSetG = true;
        }
        else { Debug.Log("gender unselected"); isSetG = false; }

    }

    public void SetEH(string val1)
    {
        float val = float.Parse(val1);
        EyeHeight = val;
        isSetEH = true;
    }

    public void SetCH(string val1)
    {
        float val = float.Parse(val1);
        Cal_H = val;
        isSetCH = true;
    }

    public void ClickButton()
    {
        if (isSetID == true && isSetG == true && isSetEH == true && isSetCH == true)
        {
            Debug.Log(string.Format("EH:{0}  Ge:{1}  ID:{2}  CH:{3}", EyeHeight, isFemale, ParticipantID, Cal_H));
            Application.LoadLevel(MainLevel);
        }
        else
        {
            Debug.Log("not all values valid");
            Debug.Log(string.Format("EH:{0}  Ge:{1}  ID:{2}  CH:{3}", isSetEH, isSetG, isSetID, isSetCH));
        }
    }

}
