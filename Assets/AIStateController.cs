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

    public int ViewDistance = 50;


    [Range(0,1)]
    public float LightDetectionThreshold;

    public float VisualSuspicionTime = 3f;
    public float VisualIdentifyTime = 3f;
    public float TimeTillEvaded = 5f;

    AIWaypointController aiwc;
    AIMovementController aimc;

    Vector3 playerLocation;
    float playerLightIntensity;

    
    bool playerInLoS; // AI sees player but not identifed as hostile yet (Unaware to Suspicious)
    bool PlayerIdentified; // AI has identfied the Player (Suspicious to Unaware)
    bool suspiciousSoundHeard; // AI has heard suspicious sounds emminating from the player or player actions

    float suspicionTimer;
    float identifyTimer;
    float awareTimer;


    Collider[] players;
    LayerMask playerMask = 1 << 3;

    GameObject player;
    int maxPlayers = 100;

    Transform head;

    public enum AIType
    {
        Guard,
        Soldier,
        VatSoldier,
        NPC,
    }
    public enum AIActionState
    {
        Guard, // Standing in place
        Patrol, // Walking through waypoints
        Search, // Looking for the player based on a given position
        ActivateButton, // Activating a button that does something (alarm, sentry)
        Attack // attacking the player (shoot or hit)
    }

    public enum AIAlertState
    {
        Unaware, // Doesnt know the player is around at all
        Suspicious, // Doesnt know player is around but has noticed things
        PlayerEvaded, // AI was aware and saw player but lost vision
        Aware, // Can see player and going to do something (approach or sound alarm)
        InCombat // Engaged in combat with the player
    }

    //Stores the current action and alert state
    [HideInInspector]
    public AIActionState currentActionState = AIActionState.Guard;
    [HideInInspector]
    public AIAlertState currentAlertState = AIAlertState.Unaware;

    public AIType currentAIType = AIType.Guard;

    bool triggered = false;

    // Start is called before the first frame update
    void Start()
    {
        aiwc = GetComponent<AIWaypointController>();
        aimc = GetComponent<AIMovementController>();


        EventManager.StartListening(EventManager.EventType.PlayerDetectedByCamera, PlayerDetectedByCamera);
        EventManager.StartListening(EventManager.EventType.ObjectLightIntensity, PlayerLightIntensity);


        if (transform.GetChild(0) != null &&
            transform.GetChild(0).GetChild(0) != null &&
            transform.GetChild(0).GetChild(0).GetChild(5) != null &&
            transform.GetChild(0).GetChild(0).GetChild(5).GetChild(0) != null &&
            transform.GetChild(0).GetChild(0).GetChild(5).GetChild(0).GetChild(0) != null &&
            transform.GetChild(0).GetChild(0).GetChild(5).GetChild(0).GetChild(0).GetChild(0) != null &&
            transform.GetChild(0).GetChild(0).GetChild(5).GetChild(0).GetChild(0).GetChild(0).GetChild(0) != null)
        {
            head = transform.GetChild(0).GetChild(0).GetChild(5).GetChild(0).GetChild(0).GetChild(0).GetChild(0);
        }
        else
        {
            head = null;
        }
    }

    // Update is called once per frame
    void Update()
    {
        VisionUpdate(); 
        HearingUpdate(); 
        //Check if the player is in vision and if so, go to suspicious state and start a "Vision" timer. If in vision and in a suspicious state increase counter and once it reaches 3 seconds, go to aware. If no vision but suspicious sound, go to suspicious state
        AlertStateUpdate(); 
        
        ActionStateUpdate();
           

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

    // Check if Player is in line of sight and if the player is illuminated enough.
    private void VisionUpdate()
    {
        RaycastHit hit;
        foreach(Collider playable in players)
        {
            if (playable != null && playable.gameObject && playable.tag == "Player")
            {
                player = playable.gameObject;
            }
        }
        if (player != null)
        {
            Vector3 playerDirection = (player.transform.position - head.position).normalized; //The direction from the enemy that faces towards the player in a Vector3 form
                                                                                                   //Debug.DrawRay(transform.position, new Vector3(player.transform.position.x, 0f , player.transform.position.z) * Vector3.Distance(transform.position, new Vector3(0f, player.transform.position.z, player.transform.position.y)), Color.green);


            if (Vector3.Angle(player.transform.position - head.position, head.forward) < fieldOfView / 2)
                if (Physics.Raycast(head.position, playerDirection, out hit, Mathf.Infinity))
                {
                    if (hit.collider.tag == "Player" && Vector3.Distance(head.position, hit.collider.transform.position) < ViewDistance)
                    {
                        //Debug.DrawRay(head.position, playerDirection * Vector3.Distance(head.position, player.transform.position), Color.green);
                        if(playerLightIntensity > LightDetectionThreshold)
                        {
                            playerInLoS = true;
                        }
                        
                    }
                }
            player = null;
        }

        
        // AI is unaware of the player
        if(currentAlertState == AIAlertState.Unaware)
        {
            //AI can see player but not has not recognised them
            if (playerInLoS) 
            {
                if (suspicionTimer < VisualSuspicionTime + 1)
                {
                    suspicionTimer += Time.deltaTime; // increase suspicion
                }

            }
            else
            {
                if (suspicionTimer > VisualSuspicionTime)
                {
                    suspicionTimer -= Time.deltaTime; // decrease suspicion
                }

            }
        }

        // AI is suspicious is and trying to identify the player
        if(currentAlertState == AIAlertState.Suspicious)
        {
            //Confirmed an unknown in vision and is now attempting to identify
            if (playerInLoS)
            {
                if (identifyTimer < VisualIdentifyTime + 1)
                {
                    identifyTimer += Time.deltaTime; // increase identification
                }
            }
            else
            {
                if (identifyTimer > VisualIdentifyTime)
                {
                    identifyTimer -= Time.deltaTime; // decrease identification
                }
            }
        }

        // AI is aware of the player and they are in line of sight
        // If the AI loses LoS, a timer counts down until the AI considers the player lost
        if (currentAlertState == AIAlertState.Aware)
        {
            if (playerInLoS)
            {
                if(awareTimer < TimeTillEvaded+1 )
                {
                    awareTimer += Time.deltaTime * 2;
                }
                
            }
            else
            {
                if (awareTimer > (-0.1f))
                {
                    awareTimer -= Time.deltaTime;
                }
            }
        }


    }

    // Checks if the AI can hear sounds and if they are suspicious sounds, stop and face the sound and if they keep hearing the sound, investigate.
    private void HearingUpdate()
    {
        suspiciousSoundHeard = false;
    }

    private void AlertStateUpdate()
    {
        // unaware AI sees unknown movement/person and becomes suspicious
        if(currentAlertState == AIAlertState.Unaware)
        {
            if(playerInLoS && suspicionTimer > VisualSuspicionTime)
            {
                currentAlertState = AIAlertState.Suspicious;
            }
        }

        // suspicious AI identifies the unknown as a hostile and becomes aware
        // Else they look at the player if they are in view to identify
        if(currentAlertState == AIAlertState.Suspicious)
        {
            if (playerInLoS && identifyTimer > VisualIdentifyTime)
            {
                currentAlertState = AIAlertState.Aware;
            }
            else if (playerInLoS && identifyTimer < VisualIdentifyTime)
            {
                aimc.lookAt = true;
            }
        }
       
    }

    private void ActionStateUpdate()
    {

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

        //Debug.Log("player being checked for detection");
        if (message["objectName"] is string && message["objectTag"] is string && message["objectIntensity"] is float && message["objectLocation"] is Vector3)
        {
            string name = (string)message["objectName"];
            string tag = (string)message["objectTag"];
            float intensity = (float)message["objectIntensity"];
            Vector3 location = (Vector3)message["objectLocation"];
            //Debug.Log("intensity: " + (float)message["playerIntensity"] + "\nPlayer Detection Script Angle: " + Vector3.Angle((Vector3)message["playerLocation"] - transform.position, transform.forward));
            if (tag == "Player")
            {
                playerLightIntensity = intensity;
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

    //Callback for Security Camera detecting Player
    private void PlayerDetectedByCamera(Dictionary<string, object> message)
    {
        //Debug.Log("player being checked for detection");
        if (message["playerLocation"] is Vector3 && message["cameraLocation"] is Vector3)
        {
            //NEED TO DECIDE
            // should we check how far the AI is away from the camera and get the closest
            // OR
            // Add a AIManagerScript to check from the position of the Camera which are the closest AI and then message both of them to search - best idea as the camera doesn't know about the AI or Manager, just that its saying its detected the player

            playerLocation = (Vector3)message["playerLocation"]; //stores the location of the player locally
            //cameraLocation = (Vector3)message["cameraLocation"];
        }

    }

    //Callback for Group AI Member detecting Player



    // Checks for the player within the AI's vision sphere
    private void FixedUpdate()
    {
        int numColliders = Physics.OverlapSphereNonAlloc(transform.position, ViewDistance, players, playerMask);

        if (numColliders > maxPlayers)
        {
            Debug.LogError(gameObject.name + " Has too many objects in the scene to illuminate");
        }
    }




}
