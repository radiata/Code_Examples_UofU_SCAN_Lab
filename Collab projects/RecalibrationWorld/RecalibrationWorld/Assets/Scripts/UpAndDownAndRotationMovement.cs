using UnityEngine;
using System.Collections;

public class UpAndDownAndRotationMovement : UpAndDownMovement {

    public float rotationAngleMax = 20f;
    public float rotationSpeed = 10f;
    public bool xRot = false;
    public bool yRot = false;
    public bool zRot = true;
    public bool localRotation = false;
    public bool faceOrigin = false;

    private float initialAngle;
    private float currentAngle;
    private bool positiveRotation = true;
    private float rotationTime = 0.5f;
    private Quaternion minRotation;
    private Quaternion maxRotation;
    


	// Use this for initialization
	protected override void Start () {
        base.Start();

        initialAngle = Random.Range(-rotationAngleMax, rotationAngleMax);
        //transform .rotation = Quaternion.AngleAxis(initialAngle, Vector3.forward);
        //transform.rotation = Quaternion.LookRotation(transform.forward, new Vector3(transform.rotation.eulerAngles.x, transform.rotation.eulerAngles.y, initialAngle));
        positiveRotation = Random.value > 0.5f;
        currentAngle = initialAngle;
        rotationTime = Random.value;

        Vector3 facingDirection = transform.forward;
        
        if (faceOrigin)
        {
            facingDirection = new Vector3(-transform.position.x, 0, -transform.position.z);
            transform.GetChild(0).rotation = Quaternion.LookRotation(new Vector3(-transform.position.x, 0, -transform.position.z));
        }

        //Debug.Log(facingDirection);

        transform.rotation = Quaternion.LookRotation(facingDirection, Vector3.up);

        minRotation = Quaternion.AngleAxis(-rotationAngleMax, transform.forward);
        maxRotation = Quaternion.AngleAxis(rotationAngleMax, transform.forward);

        //transform.rotation = Quaternion.Lerp(new Quaternion(transform.rotation.x, transform.rotation.y, minRotation.z, transform.rotation.w), maxRotation, rotationTime);
        transform.rotation = Quaternion.Lerp(minRotation, maxRotation, rotationTime);

        //Debug.Log(minRotation.eulerAngles + " , " + maxRotation.eulerAngles);

        /*
        if(!localRotation)
            transform.rotation = Quaternion.Lerp(minRotation, maxRotation, rotationTime);
        else
            transform.localRotation = Quaternion.Lerp(minRotation, maxRotation, rotationTime);
            */

    }

    // Update is called once per frame
    protected override void FixedUpdate() {
        base.FixedUpdate();

        if(positiveRotation)
        {
            rotationTime += Time.fixedDeltaTime * rotationSpeed;
            if(rotationTime >= 1f)
            {
                rotationTime = 1;
                positiveRotation = false;
            }
        }
        else
        {
            rotationTime -= Time.fixedDeltaTime * rotationSpeed;
            if (rotationTime <= 0f)
            {
                rotationTime = 0;
                positiveRotation = true;
                
            }
        }


        //Smoother step
        //https://chicounity3d.wordpress.com/2014/05/23/how-to-lerp-like-a-pro/
        float t = rotationTime;
        t = t * t * t * (t * (6f * t - 15f) + 10f);

        //Debug.Log(rotationTime);
        transform.rotation = Quaternion.Lerp(minRotation, maxRotation, t);
        //transform.rotation = Quaternion.Lerp(new Quaternion(transform.rotation.x, transform.rotation.y, minRotation.z, transform.rotation.w), new Quaternion(transform.rotation.x, transform.rotation.y, maxRotation.z, transform.rotation.w), t);
        /*
        if (positiveRotation)
        {
            currentAngle += Time.fixedDeltaTime * rotationSpeed;

            if (currentAngle >= rotationAngleMax)
            {
                currentAngle = rotationAngleMax;
                positiveRotation = false;
            }
        }
        else
        {
            currentAngle -= Time.fixedDeltaTime * rotationSpeed;

            if (currentAngle <= -rotationAngleMax)
            {
                currentAngle = -rotationAngleMax;
                positiveRotation = true;
            }
        }
        transform.rotation = Quaternion.AngleAxis(currentAngle, Vector3.forward);
        //transform.rotation = Quaternion.LookRotation(transform.forward, new Vector3(transform.rotation.eulerAngles.x, transform.rotation.eulerAngles.y, currentAngle));
        */
    }
}
