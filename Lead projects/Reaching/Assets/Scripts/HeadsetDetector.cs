////////////////////////////////////////////////////////////////////////////////////
// Purpose: Sets which HMD to use and set up for 
// **Easily portable to other experiments, see prefabs for setup example
// Code by: Butler, Michael
// Email: msbcoding@gmail.com
////////////////////////////////////////////////////////////////////////////////////
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HeadsetDetector : MonoBehaviour {

    public bool UseRift, UseVive;
    [SerializeField] private GameObject ViveRig, RiftRig;

	// Use this for initialization
	void Start ()
    {
		if(UseRift)
        {
            ViveRig.SetActive(false);
            RiftRig.SetActive(true);
        }
        else if(UseVive)
        {
            ViveRig.SetActive(true);
            RiftRig.SetActive(false);
        }

        if(UseVive & UseRift)
        {
            Debug.Log("Both headsets are set to active, May cause conflict");
        }
	}

}
