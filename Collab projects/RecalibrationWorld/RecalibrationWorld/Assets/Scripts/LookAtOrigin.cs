using UnityEngine;
using System.Collections;

public class LookAtOrigin : MonoBehaviour {

	// Use this for initialization
	void Start () {
        transform.rotation = Quaternion.LookRotation(new Vector3(-transform.position.x, 0, -transform.position.z));
	}
	
	// Update is called once per frame
	void Update () {
	
	}
}
