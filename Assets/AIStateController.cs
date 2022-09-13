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

    [Range(0,1)]
    public float LightDetectionThreshold;

    AIWaypointController aiwc;
    AIMovementController aimc;

    Vector3 playerLocation;
    float playerLightIntensity; // NEED TO FIX CALLBACKS SINCE THE DICT FORMAT IS CHANGED
    enum AIActionState
    {
        Guard, // Standing in place
        Patrol, // Walking through waypoints
        Search, // Looking for the player based on a given position
        ActivateButton, // Activating a button that does something (alarm, sentry)
        Attack // attacking the player (shoot or hit)
    }

    enum AIAlertState
    {
        Unaware, // Doesnt know the player is around at all
        Suspicious, // Doesnt know player is around but has noticed things
        PlayerEvaded, // AI was aware and saw player but lost vision
        Aware, // Can see player and going to do something (approach or sound alarm)
        InCombat // Engaged in combat with the player
    }
    
    //Stores the current action and alert state
    AIActionState currentActionState = AIActionState.Guard;
    AIAlertState currentAlertState = AIAlertState.Unaware;

    bool triggered = false;

    // Start is called before the first frame update
    void Start()
    {
        aiwc = GetComponent<AIWaypointController>();
        aimc = GetComponent<AIMovementController>();


        EventManager.StartListening(EventManager.EventType.PlayerDetected, PlayerDetectedByOthers);
        EventManager.StartListening(EventManager.EventType.ObjectLightIntensity, PlayerLightIntensity);

    }

    // Update is called once per frame
    void Update()
    {

        switch(currentAlertState)
        {
            case AIAlertState.Unaware:
                UnawareStateUpdate();
                break;
            case AIAlertState.Suspicious:
                SuspiciousStateUpdate();
                break;
            case AIAlertState.PlayerEvaded:
                PlayerEvadedStateUpdate();
                break;
            case AIAlertState.Aware:
                AwareStateUpdate();
                break;
            case AIAlertState.InCombat:
                InCombatStateUpdate();
                break;

        }



        playerLightIntensity = 0f;
    }


    //Check if player can be detected
    //check if AI is suspicious
    //Continue ActionState Guard or Patrol
    public void UnawareStateUpdate()
    {
        currentAlertState = CheckIfPlayerDetected(); // Update alert state aware if player is detected on this frame
        currentAlertState = CheckForSuspiciousActions(); // update alert state to suspicious if AI is suspicious on this frame

        // if the alert state didn't change, continue with action (guard, patrol)

        //else we end the method and go to the update method for the new alert state


    }


    //Check if player can be detected
    //Continue ActionState Guard, Patrol or Search
    public void SuspiciousStateUpdate()
    {
        currentAlertState = CheckIfPlayerDetected();

        // if the alert state didn't change, continue with action (guard, search)

        //else we end the method and go to the update method for the new alert state
    }

    //Check if player can be detected
    //Continue ActionState Guard, Patrol or Search
    public void PlayerEvadedStateUpdate()
    {
        currentAlertState = CheckIfPlayerDetected();
    }

    //Continue ActionState Attack or Active Button
    //Has methods for either approaching the player to attack or doing another action
    public void AwareStateUpdate()
    {

    }

    //Continue ActionState Attack
    //Has methods for attacking
    public void InCombatStateUpdate()
    {

    }

    //Check if the AI is suspicious of the player (sound or disturbance)

    private AIAlertState SuspiciousOfPlayer()
    {
        if(SuspiciousSoundHeard() || SuspiciousAction())
        {
            return AIAlertState.Suspicious;
        }

        return currentAlertState;
    }

    private bool SuspiciousSoundHeard()
    {
        return true;
    }

    private bool SuspiciousAction()
    {
        return true;
    }

    //AI was aware of the player but now has lost them
    private AIAlertState HasPlayerEvaded()
    {
        if (currentAlertState == AIAlertState.Aware && (PlayerInFieldOfView() && playerLightIntensity < LightDetectionThreshold || !PlayerInFieldOfView()))
        {
            return AIAlertState.PlayerEvaded;
        }
        return currentAlertState;
    }
    

    // May need to get some type of callback from the player about sounds. A method to determine if the footstep sound was loud enough for the AI to hear and if so, go to the location it was heard in a suspicious state.
    //Interactable objects may also need to have a bool to store whether it has been used by the player and if the AI can see the object changing or changed, investigate it.
    private AIAlertState CheckForSuspiciousActions()
    {
        if(SuspiciousAction() || SuspiciousSoundHeard())
        {
            return AIAlertState.Suspicious;
        }


        return currentAlertState;
    }

    //Check if AI has Detected the player
    private AIAlertState CheckIfPlayerDetected()
    {
        if(PlayerInFieldOfView() && playerLightIntensity > LightDetectionThreshold)
        {
            if(currentAlertState != AIAlertState.Aware && currentAlertState != AIAlertState.InCombat)
            {
                return AIAlertState.Aware;
            }
        }
        return currentAlertState;
    }


    //Check if player is in field of view

    private bool PlayerInFieldOfView()
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
                    return true;
                }
            }
        }

        return false;
    }

    //Callback for the player's light intensity
    private void PlayerLightIntensity(Dictionary<string, object> message)
    {

        if (message["playerIntensity"] is float)
        {
            playerLightIntensity =  (float)message["playerIntensity"];
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
    private void PlayerDetectedByOthers(Dictionary<string, object> message)
    {
        //Debug.Log("player being checked for detection");
        if (message["location"] is Vector3)
        {
            playerLocation = (Vector3)message["location"]; //stores the location of the player locally
        }

    }


    //Callback for Security Camera detecting Player





}
