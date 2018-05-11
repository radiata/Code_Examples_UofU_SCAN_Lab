using UnityEngine;

public class QuitApplication : MonoBehaviour {
	
	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		if(Input.GetKey("escape"))
		{
			Debug.Log("Quit to main menu");
			Application.LoadLevel("MainMenu");
		}
	}
}
