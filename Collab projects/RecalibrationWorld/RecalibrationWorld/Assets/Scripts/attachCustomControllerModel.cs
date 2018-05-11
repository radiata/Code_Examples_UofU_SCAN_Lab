using UnityEngine;
using System.Collections;
using Valve.VR;
using System.Collections.Generic;

public class attachCustomControllerModel : MonoBehaviour {

    private bool setControllerPosition;
    private IVRSystem ivrInterface;
    private List<GameObject> controllers = new List<GameObject>();

    public GameObject trackedDevices;
    public GameObject controllerObject;

    [Tooltip("Setting this to true hides the base stations mesh in game.")]
    public bool hideBaseStations = false;

    private bool setBaseStationsHidden = false;


    // Use this for initialization
    void Start () {
        if (!hideBaseStations)
            setBaseStationsHidden = true;
	}
	
	// Update is called once per frame
	void Update () {
	
        if(!setControllerPosition)
        {
            SteamVR_TrackedObject trackedObjectScript;
            //Get all sensors
            foreach (Transform child in trackedDevices.transform)
            {
                //Only add controllers to the controller list
                trackedObjectScript = child.gameObject.GetComponent<SteamVR_TrackedObject>();
                if (trackedObjectScript.isValid && OpenVR.System.GetTrackedDeviceClass((uint)trackedObjectScript.index) == ETrackedDeviceClass.Controller)
                {
                    
                    Transform controllerBody = trackedObjectScript.gameObject.transform.Find("body");
                    if (controllerBody)
                    {
                        GameObject controller = Instantiate(controllerObject);
                        controller.SetActive(true);
                        controller.transform.position = controllerBody.position;
                        controller.transform.rotation = controllerBody.rotation;
                        controller.transform.SetParent(controllerBody);
                        controller.transform.localRotation = Quaternion.Euler(new Vector3(0, 180, 0));

                        controllers.Add(trackedObjectScript.gameObject);
                        //controllers.Add(trackedObjectScript.gameObject);

                        controllerBody.GetComponent<MeshRenderer>().enabled = false;
                    }
                }
                

            }

            if (controllers.Count > 0)
                setControllerPosition = true;
        }
        if(!setBaseStationsHidden)
        {
            SteamVR_TrackedObject trackedObjectScript;
            if (hideBaseStations)
            {
                //Get all sensors
                foreach (Transform child in trackedDevices.transform)
                {

                    //Disable base stations
                    trackedObjectScript = child.gameObject.GetComponent<SteamVR_TrackedObject>();
                    if (trackedObjectScript.isValid && OpenVR.System.GetTrackedDeviceClass((uint)trackedObjectScript.index) == ETrackedDeviceClass.TrackingReference)
                    {
                        MeshRenderer renderer = child.gameObject.GetComponent<MeshRenderer>();
                        if (renderer != null)
                        {
                            //Debug.Log("Disable: " + child.name);
                            renderer.enabled = false;
                            setBaseStationsHidden = true;
                        }
                    }
                }
            }
            
        }
	}
}
