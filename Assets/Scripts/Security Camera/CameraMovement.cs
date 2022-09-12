using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraMovement : MonoBehaviour
{
    public bool loopRotationList = true;

    Vector3 startingRotation;

    public List<Vector3> rotationList;

    Queue<Vector3> taskQueue;

    bool playerDetected = false; // Stores whether anything has detected the player. This can change if another object detects the player and sends a message notifying this camera

    Vector3 lookAt;

    CameraViewDetection camView;

    //bool coroutineRunning = false;
    IEnumerator co = null;
    // Start is called before the first frame update
    void Start()
    {
        camView = GetComponentInChildren<CameraViewDetection>();

        taskQueue = new Queue<Vector3>();
        startingRotation = transform.rotation.eulerAngles;
        foreach(Vector3 v in rotationList)
        {
            Vector3 temp = new Vector3();
            temp = v + startingRotation;
            taskQueue.Enqueue(temp);
        }
        EventManager.StartListening(EventManager.EventType.PlayerLightIntensity, PlayerDetection);
    }

    // Update is called once per frame
    void Update()
    {
        CurrentCameraState();
    }

    private void CurrentCameraState()
    {
        RaycastHit hit;
        

        
        //var leftAngle = Quaternion.LookRotation(left, transform.up);
        
        //var rightAngle = Quaternion.LookRotation(right, transform.up);

        //Debug.DrawRay(transform.position, transform.forward * 1000f, Color.green);
        


        if (playerDetected) //Player has visibily been detected by someone or something
        {
            if (co != null) //Ensure that we stop the scanning routine
            {
                StopCoroutine(co);
                co = null;
            }
            //Debug.Log("Angle from camera to detected thing: " + Vector3.Angle(lookAt - transform.position, transform.forward) + " and distance: " + Vector3.Distance(transform.position, lookAt) + "\nis raycast gunna work: " + Physics.Raycast(transform.position, (lookAt - transform.position + new Vector3(0f, .2f, 0f)).normalized, out hit, Mathf.Infinity) + "\nhit: " + hit.collider.name);
            Debug.DrawRay(transform.position, (lookAt - transform.position).normalized * 1000f);

            Debug.DrawRay(transform.position, transform.forward * 1000f, Color.yellow);

            if (Vector3.Angle(lookAt - transform.position, transform.forward) < camView.cameraFOV/2 && 
                Physics.Raycast(transform.position, (lookAt - transform.position + new Vector3 (0f, .2f,0f)).normalized, out hit, Mathf.Infinity)) // Check that the player would be in the field of view of the camera and that the player isnt obstructed
            {
                if(hit.collider.tag == "Player")
                {
                    //Debug.Log("Player is at direction: " + lookAtDirection);
                    EventManager.TriggerEvent(EventManager.EventType.PlayerDetected, 
                        new Dictionary<string, object> { { "location", lookAt } 
                        });

                    Vector3 cameraRotation = transform.position;
                    Vector3 playerRotation = lookAt;
                    Vector3 lookAtDirection = (playerRotation - cameraRotation).normalized;
                    Quaternion playerDirection = Quaternion.LookRotation(lookAtDirection, Vector3.up);
                    //Debug.Log("Look at: " + lookAtDirection + "\nPlayer Direction: " + playerDirection);
                    transform.rotation = Quaternion.Lerp(transform.rotation, playerDirection, 1f * Time.deltaTime);
                    //Debug.Log("player detected by camera");
                }
                else
                {
                    playerDetected = false; // the player was detected in the frame before but now cannot see them
                    //Debug.Log("Lost Player, going back to loop");
                }
            }
            else
            {
                playerDetected = false; // the player was detected in the frame before but now cannot see them
                //Debug.Log("Lost Player, going back to loop");
            }
            
        }
        else //Do normal scanning
        {
            
            if(loopRotationList)
            {
                if (co == null)
                {
                    co = LerpToRotation(Quaternion.Euler(taskQueue.Peek()), 5);
                    StartCoroutine(co);
                }

                if (transform.rotation == Quaternion.Euler(taskQueue.Peek()))
                {
                    taskQueue.Enqueue(taskQueue.Peek());
                    taskQueue.Dequeue();
                    //Debug.Log("New Front: " + taskQueue.Peek());
                    co = null;
                }
            }
            
        }
    }

    //Listens for all incoming player light values. If it is high enough and the player is in the field of view, it will start tracking the player
    private void PlayerDetection(Dictionary<string, object> message)
    {
        //Debug.Log("player being checked for detection");
        if (message["playerIntensity"] is float)
        {
            //Debug.Log("intensity: " + (float)message["playerIntensity"] + "\nPlayer Detection Script Angle: " + Vector3.Angle((Vector3)message["playerLocation"] - transform.position, transform.forward));
            if ((float)message["playerIntensity"] > .05f && camView.playerInFieldOfView)
            {
                playerDetected = true;
                lookAt = (Vector3)message["playerLocation"];
                //Debug.Log("player detected");

            }
        }
        
    }


    IEnumerator LerpToRotation(Quaternion endValue, float duration)
    {
        float time = 0;
        Quaternion startValue = transform.rotation;
        while (time < duration)
        {
            transform.rotation = Quaternion.Lerp(startValue, endValue, time / duration);
            time += Time.deltaTime;
            yield return null;
        }
        transform.rotation = endValue;
    }
    
}
