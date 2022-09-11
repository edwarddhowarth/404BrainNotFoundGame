using System.Collections;
using System.Collections.Generic;
using UnityEngine.AI;
using UnityEngine;

//Need to change this and AIWaypointController as to decide when to choose a new waypoint is determined by the distance of the object's center to the center of the waypoint
//This means large/tall objects require more slack (distance can be higher) compared to smaller objects which would be closer to the waypoint

[RequireComponent(typeof(AIWaypointController))]
[RequireComponent(typeof(AIAnimationController))]
public class AIMovementController : MonoBehaviour
{
    public bool lookAt; //Look at object rather than look forward

    public GameObject enemy; //Need to change for EventManager

    private AIWaypointController aiwc;

    NavMeshAgent agent;

    public float maxSpeed;
    public float maxTurnSpeed;
    // Start is called before the first frame update
    void Start()
    {
        aiwc = GetComponent<AIWaypointController>();

        agent = GetComponent<NavMeshAgent>();

        maxSpeed = agent.speed;
        maxTurnSpeed = agent.angularSpeed;
    }

    // Update is called once per frame
    void Update()
    {
        /*
        if(!agent.hasPath)
        {
            agent.destination = aiwc.currentWaypoint.position;
        }

        if(Vector3.Distance(transform.position, aiwc.currentWaypoint.position) < 10f)
        {
            agent.destination = aiwc.currentWaypoint.position;
        }

        */
        agent.destination = aiwc.currentWaypoint.position;

        /*
        if (lookAt)
        {
            agent.updateRotation = false;
            Vector3 targetDirection = (enemy.transform.position - transform.position).normalized;
            Quaternion dir = Quaternion.LookRotation(targetDirection, Vector3.up);
            agent.transform.rotation = Quaternion.Lerp(transform.rotation, dir, 5f * Time.deltaTime);
        }
        else
        {
            agent.updateRotation = true;
        }
        */
        //Vector3 targetDirection = (target.transform.position - transform.position).normalized;
        //Quaternion dir = Quaternion.LookRotation(targetDirection, Vector3.up);
        //agent.transform.rotation = Quaternion.Lerp(transform.rotation, dir, 5f * Time.deltaTime);
    }
}
