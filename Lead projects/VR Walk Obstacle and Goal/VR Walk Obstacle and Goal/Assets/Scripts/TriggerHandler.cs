////////////////////////////////////////////////////////////////////////////////////
// Experiment: Risk Aversion
// Purpose: Supplimentary object control functions
// Code by: Butler, Michael
// Email: msbcoding@gmail.com
////////////////////////////////////////////////////////////////////////////////////
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TriggerHandler : MonoBehaviour {

    public bool MarkerController;
    private BoxCollider MyTrigger;

	// Use this for initialization
	void Start ()
    {
        MyTrigger = gameObject.GetComponent<BoxCollider>();

        if(MarkerController)
        { MyTrigger.size = new Vector3(10, 10, MasterControl.Instance.MarkerDistance - MasterControl.Instance.MarkerHide); }
        else { MyTrigger.size = new Vector3(10, 10, MasterControl.Instance.MarkerDistance - MasterControl.Instance.PostsDisplay); }
	}

    public void orient(GameObject Feed)
    {
        transform.LookAt(Feed.transform.position);
    }

    void OnTriggerEnter(Collider other)
    {
        if(other.tag == "Player" && (MasterControl.Instance.hasStarted))
        {
            if (MarkerController)
            { MasterControl.Instance.EndMarkers(); }
            else
            { MasterControl.Instance.ActivateGoal(); }
        }
    }
}
