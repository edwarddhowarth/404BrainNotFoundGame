using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraMovement : MonoBehaviour
{
    public bool rotateLoop = true;

    Vector3 startingRotation;

    public List<Vector3> rotationList;

    Queue<Vector3> taskQueue;

    bool playerDetected = false;

    Vector3 lookAt;

    //bool coroutineRunning = false;
    IEnumerator co = null;
    // Start is called before the first frame update
    void Start()
    {
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

        if(playerDetected)
        {
            if (co != null)
            {
                StopCoroutine(co);
                co = null;
            }
            //Debug.Log("Angle from camera to detected thing: " + Vector3.Angle(lookAt - transform.position, transform.forward) + " and distance: " + Vector3.Distance(transform.position, lookAt) + "\nis raycast gunna work: " + Physics.Raycast(transform.position, (lookAt - transform.position + new Vector3(0f, .2f, 0f)).normalized, out hit, Mathf.Infinity) + "\nhit: " + hit.collider.name);
            //Debug.DrawRay(transform.position, (lookAt - transform.position).normalized * 1000f);
            if (Vector3.Angle(lookAt - transform.position, transform.forward) < 50 && 
                Physics.Raycast(transform.position, (lookAt - transform.position + new Vector3 (0f, .2f,0f)).normalized, out hit, Mathf.Infinity))
            {
                if(hit.collider.tag == "Player")
                {
                    //Debug.Log("Player is at direction: " + lookAtDirection);
                    EventManager.TriggerEvent(EventManager.EventType.PlayerDetected, new Dictionary<string, object> { { "location", lookAt } });
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
                    playerDetected = false;
                    //Debug.Log("Lost Player, going back to loop");
                }
            }
            else
            {
                playerDetected = false;
                //Debug.Log("Lost Player, going back to loop");
            }
            
        }
        else
        {
            if(rotateLoop)
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

    private void PlayerDetection(Dictionary<string, object> message)
    {
        //Debug.Log("player being checked for detection");
        if (message["playerIntensity"] is float)
        {
            RaycastHit hit;
            //Debug.Log("intensity: " + (float)message["playerIntensity"] + "\nPlayer Detection Script Angle: " + Vector3.Angle((Vector3)message["playerLocation"] - transform.position, transform.forward));
            if ((float)message["playerIntensity"] > .05f &&
                Vector3.Angle((Vector3)message["playerLocation"] - transform.position, transform.forward) < 50 &&
                Physics.Raycast(transform.position, ((Vector3)message["playerLocation"] - transform.position + new Vector3(0f, 0.2f, 0f)).normalized, out hit, Mathf.Infinity))
            {
                if (hit.collider.tag == "Player")
                {
                    playerDetected = true;
                    lookAt = (Vector3)message["playerLocation"];
                    //Debug.Log("player detected");
                }
               
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
