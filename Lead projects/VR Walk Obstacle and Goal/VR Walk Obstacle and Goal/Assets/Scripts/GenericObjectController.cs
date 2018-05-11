////////////////////////////////////////////////////////////////////////////////////
// Experiment: Risk Aversion
// Purpose: Generic functions to activate or disable other associated functions
// Code by: Butler, Michael
// Email: msbcoding@gmail.com
////////////////////////////////////////////////////////////////////////////////////
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class GenericObjectController : MonoBehaviour {
	public UnityEvent OnTrigger;
	public bool startActive = false;

	void Start () {
		gameObject.SetActive(startActive);
	}

	public void Activate(){
		gameObject.SetActive(true);
	}

	public void Hide(){
		gameObject.SetActive(false);
	}

	void OnTriggerEnter(Collider other){
		if(gameObject.activeInHierarchy && other.tag == "Player")
        {
			OnTrigger.Invoke();
		}
	}

    void OnTriggerStay(Collider other)
    {
        OnTriggerEnter(other);
    }

    public void Derplog()
    {
        Debug.Log("in trigger");
    }
}
