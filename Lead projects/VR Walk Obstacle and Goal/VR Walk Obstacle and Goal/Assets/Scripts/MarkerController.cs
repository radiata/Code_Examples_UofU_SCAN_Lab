////////////////////////////////////////////////////////////////////////////////////
// Experiment: Risk Aversion
// Purpose: Functions to control marker behaviours
// Code by: Butler, Michael
// Email: msbcoding@gmail.com
////////////////////////////////////////////////////////////////////////////////////
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MarkerController : MonoBehaviour {
	public Material red;
	public Material green;

	private Renderer render;
	private bool isActive = false;

	void Start(){
		gameObject.SetActive(true);
		isActive = false;
		render = GetComponent<Renderer>();
		render.material = red;
	}

	public void MakeGreen(){
		render.material = green;
	}

	public void MakeRed(){
		render.material = red;
	}

	public void Activate(){
		gameObject.SetActive(true);
		MakeGreen();
		isActive = true;
	}

	public void Show(){
		gameObject.SetActive(true);
	}

	public void Hide(){
		isActive = false;
		MakeRed();
		gameObject.SetActive(false);
	}

	void OnTriggerEnter(Collider other){
        if(other.tag == "Player")
        {
            MasterControl.Instance.MarkerSet(gameObject);
        }
	}
}
