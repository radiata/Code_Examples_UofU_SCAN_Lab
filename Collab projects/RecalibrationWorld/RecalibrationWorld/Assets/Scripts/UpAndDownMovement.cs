using UnityEngine;
using System.Collections;

public class UpAndDownMovement : MonoBehaviour {

    [SerializeField]
    private bool movingUp = false;
    private float initialHeight;
    private Vector3 bottomPosition;
    private Vector3 topPosition;
    private float lerpPosition = 0.5f;
    [SerializeField]
    private float time = 0.5f;
    public float moveAmount = 1f;
    public float moveSpeed = 0.1f;

	// Use this for initialization
	protected virtual void Start () {

        //Start randoming moving up or down
        
        movingUp = Random.value > 0.5f;
        initialHeight = transform.position.y;
        bottomPosition = new Vector3(transform.position.x, initialHeight - moveAmount, transform.position.z);
        topPosition = new Vector3(transform.position.x, initialHeight + moveAmount, transform.position.z);

        time = Random.Range(-4.5f,4.5f);
        lerpPosition = 1 / (1 + Mathf.Exp(time));
        transform.position = Vector3.Lerp(bottomPosition, topPosition, lerpPosition);

        
    }


    protected virtual void FixedUpdate()
    {
        if (movingUp)
            time -= Time.fixedDeltaTime * moveSpeed;
        else
            time += Time.fixedDeltaTime * moveSpeed;

        lerpPosition = 1 / (1 + Mathf.Exp(time));

        if(lerpPosition <= 0.01 && !movingUp)
        {
            //lerpPosition = 0;
            movingUp = true;
        }
        else if (lerpPosition >= 0.99f && movingUp)
        {
            //lerpPosition = 1;
            movingUp = false;
        }
        transform.position = Vector3.Lerp(bottomPosition, topPosition, lerpPosition);

        /*
            if (movingUp)
        {
            if(lerpPosition < slowRegion)
            {

            }
            else
                lerpPosition += Time.fixedDeltaTime * moveSpeed;
        }
        else
            lerpPosition -= Time.fixedDeltaTime * moveSpeed;

        if(lerpPosition >= 1 && movingUp)
        {
            movingUp = false;
            lerpPosition = 1f;
        }
        else if(lerpPosition <= 0 && !movingUp)
        {
            movingUp = true;
            lerpPosition = 0f;
        }
        else if(lerpPosition > 1-slowRegion && !movingUp)
        {
            lerpPosition = Mathf.Pow(lerpPosition, 2);
        }
        else if(lerpPosition < slowRegion)
        {
            lerpPosition = Mathf.Pow(lerpPosition, 1/2);
        }

        


        /*
        if (movingUp && Mathf.Abs(transform.position.y - (initialHeight + (movingUp ? moveAmount : (-moveAmount)))) < 0.05f)
        {
            movingUp = false;
        }
        else if (!movingUp && Mathf.Abs(transform.position.y - (initialHeight + (movingUp ? moveAmount : (-moveAmount)))) < 0.05f)
        {
            movingUp = true;
        }
        */
    }
}
