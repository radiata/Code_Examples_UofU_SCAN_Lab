using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;

[RequireComponent (typeof (CapsuleCollider))]
public class TargetScript : MonoBehaviour {


    [Tooltip("The 8 waypoints (0-3 the corners, 4-7 halfway across edges.")]
    public GameObject[] waypoints;
    [Tooltip("The order in which the target will move through the waypoints, -1 sets the target to wait for touch before moving.")]
    public int[] waypointOrder;
    [Tooltip("The game object containing the ScaleWallBasedOnTracker script.")]
    public GameObject scaleWallObject;
    [Tooltip("How much further the waypoints are moved into the play area (changes with scale).")]
    public float waypointOffset = 0.5f;
    [Tooltip("Target's movement speeds in m/s.")]
    public float movementSpeed = 1.4f;
    [Tooltip("The target's trigger radius when not waiting for touch (is scaled by target's scale)")]
    public float movingTriggerRadius = 3f;
    [Tooltip("Enable to set the waypoints' heights to the initial HMD height on loading.")]
    public bool startAtEyeHeight = false;
    [Tooltip("Enable to have the target always at HMD eye height, updating every frame.")]
    public bool keepTargetAtCurrentEyeHeight = false;

    [Tooltip("How far above the camera the target may possible move if the player gets too close.")]
    public float maxAdditionalHeightOverHead = 0.5f;
    [Tooltip("How close the player must be before the target will start moving over them.")]
    public float minDistanceBeforeMovingUp = 1.0f;

    [Tooltip("The target's trigger radius when waiting for touch (is scaled by target's scale)")]
    public float stoppedTriggerRadius = 0.125f;
    [Tooltip("The target's trigger height when waiting for touch.")]
    public float capsuleHeight = 5f;

    [Tooltip("Particle system game object associated with the moving particle.")]
    public GameObject movingParticleSystem;
    [Tooltip("Particle system game object associated with the stopped particle.")]
    public GameObject stoppedParticleSystem;

    //Check if the target is currently moving
    private bool isMoving = false;
    //Current waypoint the target is moving to
    private GameObject currentWaypoint;

    private ScaleWallBasedOnTracker scaleScript;
    private CapsuleCollider triggerCapsule;
    private SphereCollider triggerSphere;
    private SettingsSingleton settings;
    //The scale of the gain for the current condition
    private float scale = 1.0f;
    //Setup waypoints after play area is setup
    private bool setupWaypoints = false;
    private bool scaleWasSet = false;
    //Start trial after waypoints setup
    private bool startTrial = false;
    //Current waypoint index
    private int waypointIndex = 0;
    //Waiting for wand to touch target
    private bool waitingForWandTouch = false;

    //Log if the height has been set
    private bool setHeight = false;

    public GameObject[] appearingObjects;
    public float timeInBetweenSpawns = 20f;
    public float objectStayTime = 20f;
    public float objectsHeight = 1.65f;
    public float appearingObjectsOffset = 1.5f;
    private float spawnTimer = 0f;
    private List<int> activeIndices;
    private List<int> waypointIndices;
    private Vector3[] placeObjects;

    

    void Start () {
        placeObjects = new Vector3[4]; 
        activeIndices = new List<int>();
        waypointIndices = new List<int>();
        settings = GameObject.Find("Settings").GetComponent<SettingsSingleton>();
        if (settings == null)
            Debug.Log("No settings object found!");
        else
        {
            setScale();
            //movementSpeed *= scale;
        }

        scaleScript = scaleWallObject.GetComponent<ScaleWallBasedOnTracker>();
        if (!scaleScript)
            Debug.Log("No scale wall script found!");

        //Set trigger parameters
        triggerCapsule = GetComponent<CapsuleCollider>();
        triggerCapsule.radius = movingTriggerRadius * scale;
        triggerCapsule.height = capsuleHeight;
        triggerSphere = GetComponentInChildren<SphereCollider>();

        //Disable stopped particle
        stoppedParticleSystem.GetComponent<ParticleSystem>().Stop(true);
        stoppedParticleSystem.GetComponent<Light>().enabled = false;

        
        

    }

    private void setScale()
    {
        if (settings.currentCondition == SettingsSingleton.Condition.Matched)
        { scale = settings.matchedScale; }
        else if (settings.currentCondition == SettingsSingleton.Condition.Faster)
        { scale = settings.fasterScale; }
        //Scale the target down on slower conditions
        else if (settings.currentCondition == SettingsSingleton.Condition.Slower)
        {
            scale = settings.slowerScale;
            //movingParticleSystem.transform.GetChild(0).transform.localScale *= scale;
            ParticleSystem.ShapeModule movingShapeModule = movingParticleSystem.transform.GetChild(0).GetComponent<ParticleSystem>().shape;
            movingShapeModule.radius *= scale;
            movingParticleSystem.GetComponent<ParticleSystem>().startSize *= scale;
            //movingParticleSystem.transform.localScale *= scale;
            ParticleSystem.ShapeModule stoppedShapeModule = stoppedParticleSystem.transform.GetChild(0).GetComponent<ParticleSystem>().shape;
            stoppedShapeModule.radius *= scale;
            //stoppedParticleSystem.transform.GetChild(0).transform.localScale *= scale;
            //stoppedParticleSystem.transform.localScale *= scale;
            stoppedParticleSystem.GetComponent<ParticleSystem>().startSize *= scale;
        }
        else if (settings.currentCondition == SettingsSingleton.Condition.Slowest)
        {
            scale = settings.slowestScale;
            ParticleSystem.ShapeModule movingShapeModule = movingParticleSystem.transform.GetChild(0).GetComponent<ParticleSystem>().shape;
            movingShapeModule.radius *= scale;
            movingParticleSystem.GetComponent<ParticleSystem>().startSize *= scale;
            ParticleSystem.ShapeModule stoppedShapeModule = stoppedParticleSystem.transform.GetChild(0).GetComponent<ParticleSystem>().shape;
            stoppedShapeModule.radius *= scale;
            stoppedParticleSystem.GetComponent<ParticleSystem>().startSize *= scale;
        }
        else if (settings.currentCondition == SettingsSingleton.Condition.Fastest)
        { scale = settings.fastestScale; }
        else
        {
            scale = 1f;
#if UNITY_EDITOR
            Debug.Log("Scale not properly set");
#endif
        }
        scaleWasSet = true;
    }

    void Update()
    {
        //Check if the position has already been set
        if (scaleScript && scaleScript.setPosition && !setupWaypoints && !startTrial && scaleWasSet)
        {

            if (waypoints.Length != 8)
                Debug.Log("Incorrect number of waypoints!");
            else
            {

                //Set the waypoints to the corners then move them in
                for (int i = 0; i < waypoints.Length; i++)
                {
                    //Set the corners
                    if (i < 4)
                    {
                        waypoints[i].transform.position = new Vector3(scaleScript.vertices[i].x, waypoints[i].transform.position.y, scaleScript.vertices[i].z);
                        //Move in x and z based on position
                        waypoints[i].transform.position = new Vector3(waypoints[i].transform.position.x - Mathf.Sign(waypoints[i].transform.position.x) * waypointOffset * Mathf.Pow(scale, 2), waypoints[i].transform.position.y,
                            waypoints[i].transform.position.z - Mathf.Sign(waypoints[i].transform.position.z) * waypointOffset * Mathf.Pow(scale, 2));// (scale < 1 ? Mathf.Pow(scale,2) : (scale)));

                        //Get object set points
                        if (i < 3)
                        {
                            
                            placeObjects[i] = new Vector3((scaleScript.vertices[i + 1].x - scaleScript.vertices[i].x) / 2, objectsHeight, (scaleScript.vertices[i + 1].z - scaleScript.vertices[i].z) / 2);
                            Vector3 outwards = new Vector3(placeObjects[i].x, 0, placeObjects[i].z).normalized * appearingObjectsOffset;
                            placeObjects[i] += outwards;
                        }
                        else if (i == 3)
                        {
                            placeObjects[i] = new Vector3((scaleScript.vertices[0].x - scaleScript.vertices[i].x) / 2, objectsHeight, (scaleScript.vertices[0].z - scaleScript.vertices[i].z) / 2);
                            Vector3 outwards = new Vector3(placeObjects[i].x, 0, placeObjects[i].z).normalized * appearingObjectsOffset;
                            placeObjects[i] += outwards;
                        }

                    }
                    //Set the halfway points
                    else if(i < 7)
                    {
                        waypoints[i].transform.position = ((waypoints[i - 3].transform.position - waypoints[i - 4].transform.position) / 2) + waypoints[i - 4].transform.position;
                    }
                    else
                        waypoints[i].transform.position = ((waypoints[0].transform.position - waypoints[3].transform.position) / 2) + waypoints[3].transform.position;

                }

                //Set starting position of target
                transform.position = waypoints[0].transform.position;
                currentWaypoint = waypoints[0];
                waypointIndex = 0;

                setupWaypoints = true;
            }
        }

        if (setupWaypoints)
        {

            if (startAtEyeHeight)
            {
                setToEyeHeight();
            }

            transform.position = waypoints[0].transform.position;
            setupWaypoints = false;
            startTrial = true;
        }

        //Update waypoint heights based on current eye height
        /*
        if(startTrial && !setHeight)
        {
            foreach (GameObject w in waypoints)
            {
                if (Mathf.Abs((scaleScript.headCamera.transform.position.y - w.transform.position.y)) > 0.05f)
                {
                    w.transform.position = new Vector3(w.transform.position.x, scaleScript.headCamera.transform.position.y, w.transform.position.z);
                }
            }
        }
        */

        //Hit Fire1 (A button on controller or H on keyboard) to reset waypoints height to current eye height
        if(Input.GetButtonDown("Fire1"))
        {
            setToEyeHeight();
        }

        if(startTrial)
        {
            if(spawnTimer >= timeInBetweenSpawns)
            {
                TrySpawnRandomObject();
            }
            else
                spawnTimer += Time.deltaTime;
        }

    }

    bool TrySpawnRandomObject()
    {
        //Try to spawn
        List<int> allIndices = new List<int>();
        for (int i = 0; i < appearingObjects.Length; i++)
        {
            allIndices.Add(i);
        }
        foreach (int j in activeIndices)
        {
            allIndices.Remove(j);
        }

        if (allIndices.Count > 0)
        {


            List<int> allWaypoints = new List<int>();
            for (int i = 0; i < placeObjects.Length; i++)
            {
                allWaypoints.Add(i);
            }
            foreach (int j in waypointIndices)
            {
                allWaypoints.Remove(j);
            }

            if (allWaypoints.Count > 0)
            {
                int randIndex = allIndices.RandomElement<int>();
                //Debug.Log(randIndex);
                activeIndices.Add(randIndex);
                

                int randWaypoint = allWaypoints.RandomElement<int>();
                //Debug.Log(randWaypoint);
                waypointIndices.Add(randWaypoint);
                GameObject obj = (GameObject)Instantiate(appearingObjects[randIndex], placeObjects[randWaypoint], Quaternion.Euler(new Vector3(-placeObjects[randWaypoint].x, 0, -placeObjects[randWaypoint].z)));
                //Look at origin
                //obj.transform.rotation = Quaternion.Euler(new Vector3(-obj.transform.position.x, 0, -obj.transform.position.z));
                //obj.transform.rotation = Quaternion.LookRotation(new Vector3(-obj.transform.position.x, 0, -obj.transform.position.z));
                


                StartCoroutine(destroyObjectAfterTime(objectStayTime, obj, randWaypoint,randIndex));
                spawnTimer = 0f;
                return true;
            }


        }
        return false;
    }

    IEnumerator destroyObjectAfterTime(float time, GameObject obj, int waypoint, int objIndex)
    {
        yield return new WaitForSeconds(time);

        activeIndices.Remove(objIndex);
        waypointIndices.Remove(waypoint);
        Destroy(obj);

        yield return null;
    }

    void FixedUpdate () {

        if (currentWaypoint)
        {
            if (isMoving)
            {

                //Move towards next waypoint
                transform.position += (currentWaypoint.transform.position - transform.position).normalized * movementSpeed * Time.deltaTime;


                if(keepTargetAtCurrentEyeHeight)
                {
                    //Set height to be current HMD height
                    transform.position = new Vector3(transform.position.x, currentWaypoint.transform.position.y, transform.position.z);
                }
                else
                {
                    //Set height to be next waypoint
                    transform.position = new Vector3(transform.position.x, scaleScript.headCamera.transform.position.y, transform.position.z);
                }


                //If target is moving and close to user move it over them
                float distance = (new Vector2(scaleScript.headCamera.transform.position.x, scaleScript.headCamera.transform.position.z) - new Vector2(transform.position.x,transform.position.z)).magnitude;

                if (distance < (minDistanceBeforeMovingUp*scale))
                {
                    float moveUpAmount = (1-(distance/(minDistanceBeforeMovingUp*scale)))*maxAdditionalHeightOverHead;
                    transform.position = new Vector3(transform.position.x, currentWaypoint.transform.position.y + moveUpAmount, transform.position.z);
                }
            }

            if ((new Vector2(transform.position.x,transform.position.z) - new Vector2(currentWaypoint.transform.position.x, currentWaypoint.transform.position.z)).magnitude < 0.05f)
            {
                isMoving = false;
            }
        }



	}

    public void moveToNextWaypoint()
    {
        waitingForWandTouch = false;
        //GetComponent<Renderer>().material = movingMaterial;
        movingParticleSystem.GetComponent<ParticleSystem>().Play();
        movingParticleSystem.GetComponent<Light>().enabled = true;
        stoppedParticleSystem.GetComponent<ParticleSystem>().Stop();
        stoppedParticleSystem.GetComponent<Light>().enabled = false;
        triggerCapsule.radius = movingTriggerRadius * scale;
        triggerCapsule.height = capsuleHeight;

        //If no waypoint order is set then just circle
        if (waypointOrder == null)
        {
            waypointIndex++;
            if (waypointIndex >= waypoints.Length)
                waypointIndex = 0;

            currentWaypoint = waypoints[waypointIndex];
        }
        //Go in predetermined order
        else
        {
            waypointIndex++;
            if (waypointIndex >= waypointOrder.Length)
                waypointIndex = 0;

            //If not waiting for touch
            if (waypointOrder[waypointIndex] >=0)
                currentWaypoint = waypoints[waypointOrder[waypointIndex]];
            //If waiting for touch
            else
            {
                waitingForWandTouch = true;
                //GetComponent<Renderer>().material = waitingMaterial;
                movingParticleSystem.GetComponent<ParticleSystem>().Stop();
                stoppedParticleSystem.GetComponent<ParticleSystem>().Play();
                movingParticleSystem.GetComponent<Light>().enabled = false;
                stoppedParticleSystem.GetComponent<Light>().enabled = true;
                triggerCapsule.radius = stoppedTriggerRadius;
                triggerCapsule.height = stoppedTriggerRadius;
            }
        }
    }

    //When something is colliding check if target needs to move to next waypoint
    void OnTriggerStay(Collider other)
    {

        //Make sure the target isn't already moving
        if (!isMoving)
        {
            if (!waitingForWandTouch)
            {
                //Make sure triggered by HMD
                if (other.gameObject.tag == "MainCamera")
                {
                    //Debug.Log("Move to next waypoint");
                    moveToNextWaypoint();
                    isMoving = true;
                }
            }
            else
            {
                //Make sure triggered by HMD
                if (other.gameObject.tag == "Wand")
                {
                    //Debug.Log("Move to next waypoint");
                    moveToNextWaypoint();
                    isMoving = true;
                }
            }
        }

    }

    void setToEyeHeight()
    {
        foreach (GameObject w in waypoints)
        {
            if (Mathf.Abs((scaleScript.headCamera.transform.position.y - w.transform.position.y)) > 0.05f)
            {
                w.transform.position = new Vector3(w.transform.position.x, scaleScript.headCamera.transform.position.y, w.transform.position.z);
            }
        }

        setHeight = true;
    }


}

public static class ColectionExtension
{
    private static System.Random rng = new System.Random();

    public static T RandomElement<T>(this IList<T> list)
    {
        return list[rng.Next(list.Count)];
    }

    public static T RandomElement<T>(this T[] array)
    {
        return array[rng.Next(array.Length)];
    }
}
