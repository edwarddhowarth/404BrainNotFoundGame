using System.Collections;
using System.Collections.Generic;
using UnityEngine.AI;
using UnityEngine;

//Need to change this and AIWaypointController as to decide when to choose a new waypoint is determined by the distance of the object's center to the center of the waypoint
//This means large/tall objects require more slack (distance can be higher) compared to smaller objects which would be closer to the waypoint

public class AIMovementController : MonoBehaviour
{
    IEnumerator co;

    public bool lookAt; //Look at object rather than look forward

    public GameObject enemy; //Need to change for EventManager

    private AIWaypointController aiwc;
    private AIStateController aisc;

    public NavMeshAgent agent;

    public float patrolMaxSpeed;
    public float patrolAcceleration;

    public float searchMaxSpeed;
    public float searchAcceleration;

    public float combatMaxSpeed;
    public float combatAcceleration;

    public float maxTurnSpeed;

    bool waiting = false;

    private Vector3 currentWaypoint;
    private Transform guardLocation;


    float nextWaitTime;
    // Start is called before the first frame update
    void Start()
    {
        co = WaitAtWaypointCoroutine();
        aiwc = GetComponent<AIWaypointController>();
        aisc = GetComponent<AIStateController>();

        agent = GetComponent<NavMeshAgent>();

        //maxSpeed = agent.speed;
        //maxTurnSpeed = agent.angularSpeed;

        
    }

    // Update is called once per frame
    void Update()
    {
        //Debug.Log(WaypointManager.GetWaypointGroupByName("WaypointGroup1").name + " " + WaypointManager.GetWaypointGroupByName("WaypointGroup2").name);
        //Really need to refactor and encapsulate things so they can be reused. currently things have dependencies which wont work.
        //Need to uncouple LookAt from enemy and add a function that will cancel a coroutine if the AI detects something so it can instantly change state.

        /*
        switch(aisc.currentAlertState)
        {
            case AIStateController.AIAlertState.Unaware:
                if(aisc.currentActionState == AIStateController.AIActionState.Guard)
                {
                    UnawareGuardMovement();
                }
                else
                {
                    UnawarePatrolMovement();
                }
                break;
            case AIStateController.AIAlertState.Suspicious:
                if(aisc.currentActionState == AIStateController.AIActionState.Patrol)
                {
                    StopPatrolling();
                }
                SuspiciousMovement();
                break;
            case AIStateController.AIAlertState.Aware:
                if(aisc.currentActionState == AIStateController.AIActionState.ActivateButton)
                {
                    AwareActivateButtonMovement();
                }
                else if (aisc.currentActionState == AIStateController.AIActionState.Search)
                {
                    AwareSearchMovement();
                }
                break;
            case AIStateController.AIAlertState.InCombat:
                IncombatMovement();
                break;
            case AIStateController.AIAlertState.PlayerEvaded:
                PlayerEvadedMovement();
                break;

        }
        */

        

        Debug.DrawRay(transform.position, (aiwc.currentWaypoint.position - transform.position).normalized * 100);

    }

    // These methods should take in things from the AIStateController and SHOULD NOT be doing any calculating.
    // It should only be doing movement/actions and any animations required should be passed onto the AIAnimationController

    public void IdleMovement()
    {

    }

    public void UnawareGuardMovement()
    {
        GuardPosition();
    }

    public void UnawarePatrolMovement()
    {
        FollowWaypoint();
    }


    public void SuspiciousMovement(Vector3 location)
    {
        InvestigateSuspicion(location);
    }

    public void AwareIdleEvade()
    {

    }

    public void AwareSearchMovement()
    {

    }

    public void AwareActivateButtonMovement()
    {

    }

    public void IncombatMovement()
    {

    }

    public void PlayerEvadedGuardMovement()
    {

    }

    public void PlayerEvadedSearchMovement()
    {

    }

    public void PlayerEvadedIdleMovement()
    {

    }

    public void StopPatrolling()
    {

    }

    public void GuardPosition()
    {
        if(co != null)
        {
            waiting = false;
            StopCoroutine(co);
            co = null;
        }
        
        guardLocation = aiwc.GetPosition(0); //Get the first node in the waypoint
        
        if (Vector3.Distance(transform.position, guardLocation.position) < 1f)
        {
            transform.rotation = guardLocation.rotation;
        }
        else
        {
            agent.destination = guardLocation.position;
        }
        

    }

    public void FollowWaypoint()
    {
        if(co == null)
        {
            co = WaitAtWaypointCoroutine();
            aiwc.GetNextWaypoint();
        }
        if (waiting == false && aiwc.currentWaypoint.position != currentWaypoint)
        {
            StartCoroutine(co);
        }

    }

    public void InvestigateSuspicion(Vector3 location)
    {
        if(co != null)
        {
            waiting = false;
            StopCoroutine(co);
            co = null;
        }
        agent.destination = location;
    }


    IEnumerator WaitAtWaypointCoroutine()
    {
        waiting = true;
        Debug.Log("next waypoint angle: " + calculateWaypointAngles(aiwc.currentWaypoint.position) + "\nForward direction: " + transform.forward + "Waypoint Location:" + (aiwc.currentWaypoint.position - transform.forward).normalized);
        float angleToNextWaypoint = calculateWaypointAngles(aiwc.currentWaypoint.position);
        if (angleToNextWaypoint > 15)
        {
            angleToNextWaypoint = (angleToNextWaypoint / 120f) + nextWaitTime;
            Debug.Log(angleToNextWaypoint+ " second wait");
            
            StartCoroutine(RotateTowardsWaypoint(angleToNextWaypoint));
            yield return new WaitForSeconds(angleToNextWaypoint);
           
        }
        else
        {
            Debug.Log(".5f second wait");
            yield return new WaitForSeconds(.25f+ nextWaitTime);
        }
        
        agent.destination = aiwc.currentWaypoint.position;
        if(aiwc.currentWaypoint.gameObject.GetComponent<WaypointNode>() != null)
        {
            nextWaitTime = aiwc.currentWaypoint.gameObject.GetComponent<WaypointNode>().additionalWaitTime;
        }
        else
        {
            nextWaitTime = 0;
        }
        currentWaypoint = aiwc.currentWaypoint.position;
        waiting = false;
    }

    IEnumerator RotateTowardsWaypoint(float waitTime)
    {
        float time = 0;
        while (time < waitTime)
        {
            agent.updateRotation = false;
            if (waitTime > 1f && time > waitTime-.5f)
            {
                
                Debug.Log("Turning on spot: " + time);
                float speed = 2f * Time.deltaTime;
                transform.rotation = Quaternion.LookRotation(Vector3.RotateTowards(transform.forward, aiwc.currentWaypoint.position - transform.position, speed, 0.0f));

                
            }

            time += Time.deltaTime;
            yield return null;
        }
        agent.updateRotation = true;
        yield return null;
        
    }

    private float calculateWaypointAngles(Vector3 next)
    {
        float currentToNextAngle = Vector3.Angle(transform.forward,(next - transform.position).normalized);
        
        return currentToNextAngle;
    }

    public void CancelWaypointMovement()
    {
        agent.destination = transform.position;
    }

}
