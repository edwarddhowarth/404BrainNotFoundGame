using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//Handles the actions the AI is doing and the alert state.
//Will call the appropriate functions/actions depending on the cominbation of the two.

[RequireComponent(typeof(AIMovementController))]
[RequireComponent(typeof(AIWaypointController))]
[RequireComponent(typeof(AIAnimationController))]
public class AIStateController : MonoBehaviour
{
    //Use this to track AI that should be together. Use EventManager to communicate between AI
    public int AIGroup;
    public int fieldOfView = 90;

    AIWaypointController aiwc;
    AIMovementController aimc;

    Vector3 playerLocation;

    enum AIActionState
    {
        Guard,
        Patrol,
        Search,
        ActivateButton,
        Attack
    }

    enum AIAlertState
    {
        Unaware,
        Suspicious,
        PlayerEvaded,
        Aware,
        InCombat
    }
    
    //Stores the current action and alert state
    AIActionState currentActionState;
    AIAlertState currentAlertState;

    bool triggered = false;

    // Start is called before the first frame update
    void Start()
    {
        aiwc = GetComponent<AIWaypointController>();
        aimc = GetComponent<AIMovementController>();


        EventManager.StartListening(EventManager.EventType.PlayerDetected, PlayerDetected);

    }

    // Update is called once per frame
    void Update()
    {

        
        

    }


    //Check if player is in field of view

    private void PlayerInFieldOfView()
    {
        Vector3 playerDirection = (playerLocation - transform.position).normalized;
        RaycastHit hit;

        if (Vector3.Angle(playerLocation - transform.position, transform.forward) < fieldOfView / 2)
        {
            if (Physics.Raycast(transform.position, playerDirection, out hit, Mathf.Infinity))
            {
                if (hit.collider.tag == "Player")
                {
                    Debug.DrawRay(transform.position, playerDirection * Vector3.Distance(transform.position, playerLocation), Color.green);
                    currentAlertState = AIAlertState.InCombat;
                }
            }
        }
            
    }

    /// <summary>
    /// Changes the waypoint group for this AI
    /// </summary>
    /// <param name="name">Name of the waypoint group</param>
    /// <param name="closestNode">Get the closest waypoint node after switching. Else go to start.</param>
    public void ChangeWaypoints(string name, bool closestNode)
    {
        aimc.CancelWaypointMovement();
        aiwc.ChangeWaypointGroup(name);
        if (closestNode)
            aiwc.GetClosestWaypoint();
        else
            aiwc.GetNextWaypoint();
    }

    //Callback for Group AI Member detecting Player
    private void PlayerDetected(Dictionary<string, object> message)
    {
        //Debug.Log("player being checked for detection");
        if (message["location"] is Vector3)
        {
            playerLocation = (Vector3)message["location"];
            currentAlertState = AIAlertState.Aware;
        }

    }


    //Callback for Security Camera detecting Player





}
