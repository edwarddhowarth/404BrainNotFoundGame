using System.Collections;
using System.Collections.Generic;
using UnityEngine.AI;
using UnityEngine;

//Need to change this and AIWaypointController as to decide when to choose a new waypoint is determined by the distance of the object's center to the center of the waypoint
//This means large/tall objects require more slack (distance can be higher) compared to smaller objects which would be closer to the waypoint

public class AIMovementController : MonoBehaviour
{
    IEnumerator co;
    IEnumerator coIn;

    public bool lookAt; //Look at object rather than look forward

    public GameObject enemy; //Need to change for EventManager
    public Transform lastKnownEnemyPosition;

    private AIWaypointController aiwc;
    private AIStateController aisc;

    public NavMeshAgent agent;

    public float patrolMaxSpeed = 2.5f;
    public float patrolAcceleration = 3f;

    public float searchMaxSpeed = 3f;
    public float searchAcceleration = 3.5f;

    public float combatMaxSpeed = 3.5f;
    public float combatAcceleration = 4f;

    public float maxTurnSpeed;

    bool waiting = false;

    private Vector3 currentWaypoint;
    private Transform guardLocation;


    float nextWaitTime;

    //GameObject gun; // Need to get forward point from it. Then rotate the character such that the barrel will be pointing forward
    //GameObject melee;



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

        switch(aisc.currentAlertState)
        {
            case AIStateController.AIAlertState.Unaware:
                agent.speed = patrolMaxSpeed;
                agent.acceleration = patrolAcceleration;
                break;
            case AIStateController.AIAlertState.Suspicious:
                agent.speed = searchMaxSpeed;
                agent.acceleration = searchAcceleration;
                break;
            case AIStateController.AIAlertState.PlayerEvaded:
                agent.speed = searchMaxSpeed;
                agent.acceleration = searchAcceleration;
                break;
            case AIStateController.AIAlertState.Aware:
                agent.speed = combatMaxSpeed;
                agent.acceleration = combatAcceleration;
                break;
            case AIStateController.AIAlertState.InCombat:
                agent.speed = combatMaxSpeed;
                agent.acceleration = combatAcceleration;
                break;

        }
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

        float angleToNextWaypoint = calculateWaypointAngles(aiwc.currentWaypoint.position);
        //coIn = StartCoroutine(RotateTowardsWaypoint(angleToNextWaypoint));

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

    public void AwareSearchMovement(Vector3 location)
    {
        InvestigateSuspicion(location);
    }

    public void AwareActivateButtonMovement()
    {

    }

    public void IncombatMovement()
    {

    }

    public void PlayerEvadedGuardMovement()
    {
        GuardPosition();
    }

    public void PlayerEvadedSearchMovement()
    {
        
        FollowWaypoint();
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
            StopAllCoroutines();
            //StopCoroutine(co);
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

    public void MeleeCombat()
    {

        agent.destination = transform.position;

        if (co != null)
        {
            waiting = false;
            StopAllCoroutines();
            co = null;
        }

    }

    public void GunCombat()
    {
        agent.destination = transform.position;

        if (co != null)
        {
            waiting = false;
            StopAllCoroutines();
            co = null;
        }

    }

    public void FollowWaypoint()
    {
        if(co == null)
        {
            co = WaitAtWaypointCoroutine();
            //aiwc.GetNextWaypoint();
        }
        if (waiting == false && aiwc.currentWaypoint.position != currentWaypoint)
        {
            //StopAllCoroutines();
            StartCoroutine(WaitAtWaypointCoroutine());
        }

    }

    public void InvestigateSuspicion(Vector3 location)
    {
        agent.updateRotation = true;
        if (co != null)
        {
            waiting = false;
            StopAllCoroutines();
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
