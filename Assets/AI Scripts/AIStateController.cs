using System.Collections;
using System.Collections.Generic;
using UnityEngine;


//Need to allow the AI to go to the player if they have been detected

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

    public float ShootingRange = 10f;
    public float MeleeRange = 2f;


    [Range(0,1)]
    public float LightDetectionThreshold;

    public float InstantDetectionSoundLevel = 5f;

    public float VisualSuspicionTime = 3f;
    public float VisualIdentifyTime = 3f;
    public float TimeTillEvaded = 5f;
    public float EvadedReacquireTime = 3f;

    AIWaypointController aiwc;
    AIMovementController aimc;

    Vector3 playerLocation;
    float playerLightIntensity;
    float playerSoundLevel;
    float playerSoundLevelNormalised;

   

    bool playerInLoS; // AI sees player but not identifed as hostile yet (Unaware to Suspicious)
    bool PlayerIdentified; // AI has identfied the Player (Suspicious to Unaware)
    bool suspiciousSoundHeard; // AI has heard suspicious sounds emminating from the player or player actions
    public bool SCIdentify;


    float suspicionTimer;
    float identifyTimer;
    float ignoreTimer;
    float awareTimer;
    float reacquireTimer;
    float attackCooldownTimer;
    float shootDelayTimer;


    private float gunTimer;
    private float meleeTimer;
    [HideInInspector]
    public bool attack = false;
    public bool finishedAttack = false;
    public bool InCombatAnimation = false;

    private bool CombatStarted = false;


    Vector3 suspiciousLocation;

    Collider[] players;
    LayerMask playerMask = 1 << 3;

    GameObject player;
    int maxPlayers = 10;

    Transform head;

    public enum AIType
    {
        Guard,
        Soldier,
        VatSoldier,
        NPC,
    }

    public enum AIWeapon
    {
        Gun,
        Melee
    }

    public enum AIRole
    {
        Guarding,
        Patrolling,
        Idle
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
    public AIAlertState currentAlertState = AIAlertState.Unaware;

    public AIRole currentAIRole = AIRole.Patrolling;

    public AIType currentAIType = AIType.Guard;

    public AIWeapon currentWeapon = AIWeapon.Gun;

    public float GunFireRate = 3f;
    public float MeleeRate = 1f;
    public GameObject gun;


    bool triggered = false;

    // Start is called before the first frame update
    void Start()
    {
        suspicionTimer = 0f;
        identifyTimer = 0f;
        awareTimer = 0f;
        reacquireTimer = 0f;
        ignoreTimer = VisualSuspicionTime;

        aiwc = GetComponent<AIWaypointController>();
        aimc = GetComponent<AIMovementController>();


        EventManager.StartListening(EventManager.EventType.PlayerDetectedByCamera, PlayerDetectedByCamera);
        EventManager.StartListening(EventManager.EventType.ObjectLightIntensity, PlayerLightIntensity);
        EventManager.StartListening(EventManager.EventType.PlayerSoundLevel, PlayerSoundLevel);

        players = new Collider[maxPlayers];

        head = transform.GetChild(0).GetChild(0).GetChild(5).GetChild(0).GetChild(0).GetChild(0).GetChild(0);

        /*
        if (transform.childCount > 1)
        {
            if (transform.GetChild(0) != null &&
            transform.GetChild(0).GetChild(0) != null &&
            transform.GetChild(0).GetChild(0).GetChild(5) != null &&
            transform.GetChild(0).GetChild(0).GetChild(5).GetChild(0) != null &&
            transform.GetChild(0).GetChild(0).GetChild(5).GetChild(0).GetChild(0) != null &&
            transform.GetChild(0).GetChild(0).GetChild(5).GetChild(0).GetChild(0).GetChild(0) != null &&
            transform.GetChild(0).GetChild(0).GetChild(5).GetChild(0).GetChild(0).GetChild(0).GetChild(0) != null)
            {
               
            }
        }
        else
        {
            head = null;
        }
        */
    }

    // Update is called once per frame
    void FixedUpdate()
    {

        Debug.Log(gameObject.name + " Alert State: " + currentAlertState.ToString());
        Debug.Log("Player Sound Level to: " + gameObject.name + " is : " + playerSoundLevelNormalised);

        int numColliders = Physics.OverlapSphereNonAlloc(transform.position, ViewDistance, players, playerMask);

        if (numColliders > maxPlayers)
        {
            Debug.LogError(gameObject.name + " Has too many objects in the scene to illuminate");
        }

        VisionUpdate(); // Can see Player?
        HearingUpdate(); // Can hear player or their actions??
        //Check if the player is in vision and if so, go to suspicious state and start a "Vision" timer. If in vision and in a suspicious state increase counter and once it reaches 3 seconds, go to aware. If no vision but suspicious sound, go to suspicious state
        ActionStateTimers();
        AlertStateUpdate(); // With new info, update their state
        
        ActionStateUpdate(); // With new alert state, update their action
        

        
           
        

        //playerLightIntensity = 0f;
    }

    private void Update()
    {
        if ((currentAlertState == AIAlertState.Aware || currentAlertState == AIAlertState.InCombat) && CombatStarted)
        {
            EventManager.TriggerEvent(EventManager.EventType.AIEngaged,
                        new Dictionary<string, object> { { "AIEngaging", true }
                        });

        }
        else if((currentAlertState != AIAlertState.Aware && currentAlertState != AIAlertState.InCombat) && CombatStarted)
        {
            CombatStarted = false;

        }



    }

    // Check if Player is in line of sight and if the player is illuminated enough.
    private void VisionUpdate()
    {
        if (head)
        {

            playerInLoS = false;
            player = null;
            RaycastHit hit;
            foreach (Collider playable in players)
            {
                if (playable != null && playable.gameObject && playable.tag == "Player")
                {
                    player = playable.gameObject;
                    Debug.Log(player.name + " player found");
                }
            }
            if (player != null)
            {
                Vector3 playerDirection = ((player.transform.position + new Vector3(0, 1f, 0)) - head.position).normalized; //The direction from the enemy that faces towards the player in a Vector3 form
                                                                                                                            //Debug.DrawRay(transform.position, new Vector3(player.transform.position.x, 0f , player.transform.position.z) * Vector3.Distance(transform.position, new Vector3(0f, player.transform.position.z, player.transform.position.y)), Color.green);

                
                if (Vector3.Angle(player.transform.position - head.position, head.forward) < fieldOfView / 2)
                {
                    
                    if (Physics.Raycast(head.position, playerDirection, out hit, Mathf.Infinity))
                    {
                        
                        Debug.DrawRay(head.position, playerDirection * Vector3.Distance(head.position, player.transform.position), Color.green);
                        if (hit.collider.tag == "Player" && Vector3.Distance(head.position, hit.collider.transform.position) < ViewDistance) // Was the object hit the player?
                        {
                            //Debug.DrawRay(head.position, playerDirection * Vector3.Distance(head.position, player.transform.position), Color.green);
                            Debug.Log("Player light intensity: " + playerLightIntensity);
                            float LightDistanceThresh = LightDetectionThreshold - (2 / Mathf.Pow(Vector3.Distance(transform.position, aimc.enemy.transform.position),2));
                            Debug.Log("Light Distance Adjust: " + LightDistanceThresh);
                            if (playerLightIntensity > LightDistanceThresh) // Is the player illumented enough to be seen
                            {
                                playerInLoS = true;
                                Debug.Log("player in los");
                            }
                            

                        }
                    }
                    else
                    {
                        Debug.Log("ray bad");
                        //Debug.Log("Hit name: " + hit.collider.gameObject.name);
                    }
                }


                if (Vector3.Distance(transform.position, player.transform.position) < 5f) // Player is right next to the AI
                {
                    playerInLoS = true;
                }



            }

        }

    }

    // Checks if the AI can hear sounds and if they are suspicious sounds, stop and face the sound and if they keep hearing the sound, investigate.
    private void HearingUpdate()
    {
        suspiciousSoundHeard = false;
    }



    /*
    * State Timers
    */

    private void ActionStateTimers()
    {

        switch(currentAlertState)
        {
            case AIAlertState.Unaware:
                UnawareStateTimer();
            break;
            case AIAlertState.Suspicious:
                SuspiciousStateTimer();
            break;
            case AIAlertState.PlayerEvaded:
                PlayerEvadedStateTimer();
            break;
            case AIAlertState.Aware:
                AwareStateTimer();
            break;
            case AIAlertState.InCombat:
                InCombatStateTimer();
            break;

        }
    }

    private void UnawareStateTimer()
    {
        if (currentAlertState == AIAlertState.Unaware)
        {
            if (playerSoundLevelNormalised > InstantDetectionSoundLevel)
            {
                suspicionTimer = 10f;
            }
            //AI can see player but not has not recognised them
            if (playerInLoS)
            {
                if (suspicionTimer < VisualSuspicionTime + 1)
                {
                    suspicionTimer += Time.fixedDeltaTime; // increase suspicion
                    
                }

            }
            else
            {
                if (suspicionTimer > (-0.1f))
                {
                    suspicionTimer -= Time.fixedDeltaTime; // decrease suspicion
                }

            }
        }
    }

    private void SuspiciousStateTimer()
    {
        // AI is suspicious is and trying to identify the player
        if (currentAlertState == AIAlertState.Suspicious)
        {
            if (playerSoundLevelNormalised > InstantDetectionSoundLevel)
            {
                identifyTimer = 10f;
            }

            //Confirmed an unknown in vision and is now attempting to identify
            if (playerInLoS)
            {
                if (identifyTimer < VisualIdentifyTime + 1)
                {
                    identifyTimer += Time.fixedDeltaTime; // increase identification
                    ignoreTimer -= Time.fixedDeltaTime;
                }
            }
            else
            {
                if (identifyTimer > (-0.1f))
                {
                    identifyTimer -= Time.fixedDeltaTime; // decrease identification
                    ignoreTimer += Time.fixedDeltaTime;
                }
            }
        }
    }

    private void PlayerEvadedStateTimer()
    {
        if (currentAlertState == AIAlertState.PlayerEvaded)
        {
            if (playerInLoS)
            {
                if (reacquireTimer < EvadedReacquireTime + 1)
                {
                    reacquireTimer += Time.fixedDeltaTime;
                }
            }
            else
            {
                if (reacquireTimer > (-0.1f))
                {
                    reacquireTimer -= Time.fixedDeltaTime;

                }
            }
        }
    }

    private void AwareStateTimer()
    {
        // AI is aware of the player and they are in line of sight
        // If the AI loses LoS, a timer counts down until the AI considers the player lost
        if (currentAlertState == AIAlertState.Aware)
        {
            attackCooldownTimer += Time.fixedDeltaTime;
            if (playerInLoS)
            {
                if (awareTimer < TimeTillEvaded + 1)
                {
                    awareTimer += Time.fixedDeltaTime * 2;
                }

            }
            else
            {
                if (awareTimer > (-0.1f))
                {
                    awareTimer -= Time.fixedDeltaTime;
                }
            }
        }
    }

    private void InCombatStateTimer()
    {
        gunTimer += Time.fixedDeltaTime;
       
    }

    /*
     * Alert States
     */


    private void AlertStateUpdate()
    {

        switch (currentAlertState)
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

    }

    //Check if player can be detected
    //check if AI is suspicious
    //Continue ActionState Guard or Patrol
    public void UnawareStateUpdate()
    {
        //currentAlertState = CheckIfPlayerDetected(); // Update alert state aware if player is detected on this frame
        //currentAlertState = CheckForSuspiciousActions(); // update alert state to suspicious if AI is suspicious on this frame

        // unaware AI sees unknown movement/person and becomes suspicious
        if (SCIdentify)
        {
            currentAlertState = AIAlertState.Suspicious;
            suspicionTimer = 0f;
        }
        else if (currentAlertState == AIAlertState.Unaware)
        {
            if (playerInLoS && suspicionTimer > VisualSuspicionTime)
            {
                currentAlertState = AIAlertState.Suspicious;
                suspicionTimer = 0f;
            }
            aimc.lookAt = false;
        }
        // if the alert state didn't change, continue with action (guard, patrol)

        //else we end the method and go to the update method for the new alert state

        // AI is unaware of the player


    }


    //Check if player can be detected
    //Continue ActionState Guard, Patrol or Search
    public void SuspiciousStateUpdate()
    {

        // suspicious AI identifies the unknown as a hostile and becomes aware
        // Else they look at the player if they are in view to identify
        if(SCIdentify)
        {
            currentAlertState = AIAlertState.Aware;
            identifyTimer = 0f;
            awareTimer = TimeTillEvaded;
            aimc.lookAt = true;
        }
        else
        {
            aimc.lookAt = false;
        }
        if (currentAlertState == AIAlertState.Suspicious)
        {
            if (playerInLoS && identifyTimer > VisualIdentifyTime)
            {
                currentAlertState = AIAlertState.Aware;
                identifyTimer = 0f;
                awareTimer = TimeTillEvaded;
                aimc.lookAt = true;
            }
            else if (playerInLoS && identifyTimer < VisualIdentifyTime)
            {
                aimc.lookAt = true;
            }
            else if (ignoreTimer > VisualSuspicionTime)
            {
                currentAlertState = AIAlertState.Unaware;
                suspicionTimer = VisualSuspicionTime * 0.5f;
                identifyTimer = 0f;
                aimc.lookAt = false;
            }
        }

        // if the alert state didn't change, continue with action (guard, search)

        //else we end the method and go to the update method for the new alert state
    }

    

    //Continue ActionState Attack or Active Button
    //Has methods for either approaching the player to attack or doing another action
    public void AwareStateUpdate()
    {

        if (playerSoundLevelNormalised > InstantDetectionSoundLevel)
        {
            Vector3 targetDirection = (aimc.enemy.transform.position - transform.position).normalized;
            Quaternion playerDirection = Quaternion.LookRotation(targetDirection, Vector3.up);
            Quaternion.RotateTowards(transform.rotation, playerDirection, 3f);
        }

        
        if (currentAlertState == AIAlertState.Aware)
        {
            aimc.lookAt = true; // AI is aware of player so look at them
            /*
            if (playerInLoS)
            {
                
            }
            else
            {
                aimc.lookAt = false;
            }
            */
            
            if (awareTimer <= 0)
            {
                currentAlertState = AIAlertState.PlayerEvaded;
            }
            else
            {
                switch (currentWeapon)
                {
                    case AIWeapon.Gun:
                        Vector3 targetDirection = (aimc.enemy.transform.position - transform.position).normalized;
                        if ((Vector3.Distance(transform.position, aimc.enemy.transform.position) < ShootingRange && Vector3.Angle(transform.forward, targetDirection) < 75 && playerInLoS) || (aimc.cantReachPlayer && playerInLoS))
                        {
                            
                            if (attackCooldownTimer > 2f)
                            {
                                Debug.Log("Is the player inaccessable?: " + aimc.cantReachPlayer);
                                finishedAttack = false;
                                currentAlertState = AIAlertState.InCombat;
                            }

                        }
                        break;
                    case AIWeapon.Melee:
                        if (Vector3.Distance(transform.position, aimc.enemy.transform.position) < MeleeRange && playerInLoS)
                        {
                            currentAlertState = AIAlertState.InCombat;
                        }
                        break;
                }
            }

            
        }

    }

    //Check if player can be detected
    //Continue ActionState Guard, Patrol or Search
    public void PlayerEvadedStateUpdate()
    {

        // Player evaded the AI but AI has reacquired visuals
        if (currentAlertState == AIAlertState.PlayerEvaded)
        {
            if (playerInLoS && reacquireTimer > EvadedReacquireTime)
            {
                currentAlertState = AIAlertState.Aware;
            }

            if(!playerInLoS && reacquireTimer < 0f)
            {
                currentAlertState = AIAlertState.Unaware;
            }
        }
    }

    //Continue ActionState Attack
    //Has methods for attacking
    public void InCombatStateUpdate()
    {


        if (currentAlertState == AIAlertState.InCombat)
        {

        }

    }



    /*
     * Action States
     */


    private void ActionStateUpdate()
    {

        switch (currentAlertState)
        {
            case AIAlertState.Unaware:
                UnawareStateAction();
                break;
            case AIAlertState.Suspicious:
                SuspiciousStateAction();
                break;
            case AIAlertState.PlayerEvaded:
                PlayerEvadedStateAction();
                break;
            case AIAlertState.Aware:
                AwareStateAction();
                break;
            case AIAlertState.InCombat:
                InCombatStateAction();
                break;

        }

    }

    private void UnawareStateAction()
    {
        switch(currentAIRole)
        {
            case AIRole.Guarding:
                aimc.UnawareGuardMovement();
                break;
            case AIRole.Patrolling:
                aimc.UnawarePatrolMovement();
                break;
            case AIRole.Idle:
                aimc.IdleMovement();
                break;
            
        }
    }

    private void SuspiciousStateAction()
    {
        if(player != null)
        {
            if (Vector3.Distance(transform.position, player.transform.position) < 5f)
            {
                suspiciousLocation = player.transform.position;
            }
            else
            {
                suspiciousLocation = transform.position;
            }
            
        }
        else
        {
            suspiciousLocation = transform.position;
        }
        

        switch (currentAIRole)
        {
            case AIRole.Guarding:
                aimc.SuspiciousMovement(suspiciousLocation);
                break;
            case AIRole.Patrolling:
                aimc.SuspiciousMovement(suspiciousLocation);
                break;
            case AIRole.Idle:
                aimc.IdleMovement();
                break;

        }
    }

   

    private void AwareStateAction()
    {
        if (player != null)
        {
            suspiciousLocation = player.transform.position;
        }
        else
        {
            suspiciousLocation = transform.position;
        }

        attack = false;
        if (!playerInLoS && Vector3.Distance(transform.position, aimc.enemy.transform.position) > 0.5f)
        {
            switch (currentAIRole)
            {
                case AIRole.Guarding:
                    aimc.AwareSearchMovement(suspiciousLocation);
                    break;
                case AIRole.Patrolling:
                    aimc.AwareSearchMovement(suspiciousLocation);
                    break;
                case AIRole.Idle:
                    aimc.AwareIdleEvade();
                    break;

            }
        }
        else if(playerInLoS && Vector3.Distance(transform.position, aimc.enemy.transform.position) > 0.5f)
        {
            switch (currentAIRole)
            {
                case AIRole.Guarding:
                    aimc.AwareSearchMovement(suspiciousLocation);
                    break;
                case AIRole.Patrolling:
                    aimc.AwareSearchMovement(suspiciousLocation);
                    break;
                case AIRole.Idle:
                    aimc.AwareIdleEvade();
                    break;

            }
        }
        
    }

    private void PlayerEvadedStateAction()
    {
        attack = false;
        switch (currentAIRole)
        {
            case AIRole.Guarding:
                aimc.PlayerEvadedGuardMovement();
                break;
            case AIRole.Patrolling:
                aimc.PlayerEvadedSearchMovement();
                break;
            case AIRole.Idle:
                aimc.PlayerEvadedIdleMovement();
                break;

        }
    }

    //Create timers to delay the firing of the bullet
    //Animation needs to play for a long enough time for the gun to be pointed at the player
    //once that time has passed, trigger the GunCombat function to spawn the bullet
    //once enough time has passed for the bullet to spawn and leave, exit the firing sequence
    private void firingTimer()
    {

    }

    //Need to calculate the angle on the x axis for height and y axis for left/right rotation
    //so the AI can adjust their body to aim at the player.
    private void InCombatStateAction()
    {
        switch (currentWeapon)
        {
            case AIWeapon.Gun:
                InCombatAnimation = true;

                attack = true;
                aimc.agent.destination = transform.position;


                shootDelayTimer += Time.fixedDeltaTime;


                if (attack && shootDelayTimer > 0.45f && !finishedAttack) // Limits the fire rate and makes the AI attempt 1 attack
                {
                    Debug.Log("angle for gun: " + Vector3.Angle(aimc.enemy.transform.position - gun.transform.GetChild(0).transform.position, gun.transform.GetChild(0).transform.up));
                    if (Vector3.Angle(aimc.enemy.transform.position - gun.transform.GetChild(0).transform.position, gun.transform.GetChild(0).transform.up) < 15f || Vector3.Distance(transform.position, aimc.enemy.transform.position) < 5f)
                    {
                        CombatStarted = true;
                        aimc.GunCombat();
                        finishedAttack = true;
                    }
                    else
                    {
                        Vector3 targetDirection = (aimc.enemy.transform.position - gun.transform.GetChild(0).transform.position).normalized;
                        Quaternion playerDirection = Quaternion.LookRotation(targetDirection, gun.transform.GetChild(0).transform.up);
                        transform.rotation = Quaternion.RotateTowards(transform.rotation, playerDirection, 3f);
                    }
                   
                    
                }
                else if (gunTimer > GunFireRate && finishedAttack) // Once they have attacked, go back to aware state to chase and reset attack
                {
                    currentAlertState = AIAlertState.Aware;
                    gunTimer = 0f;
                    attack = false;
                    attackCooldownTimer = 0f;
                    shootDelayTimer = 0f;
                    finishedAttack = false;
                    InCombatAnimation = false;
                }


                break;
            case AIWeapon.Melee:
                if(meleeTimer < MeleeRate)
                {
                    aimc.MeleeCombat();
                    attack = true;
                    meleeTimer += Time.fixedDeltaTime;
                }
                else
                {
                    currentAlertState = AIAlertState.Aware;
                    meleeTimer = 0f;
                    attack = false;
                }
                break;

        }
    }

    //Check if the AI is suspicious of the player (sound or disturbance)

    /*

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
     */


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

    private void PlayerSoundLevel(Dictionary<string, object> message)
    {
        if(message["soundLevel"] is float)
        {
            playerSoundLevel = (float)message["soundLevel"];
            playerSoundLevelNormalised = playerSoundLevel * (1 / (Vector3.Distance(transform.position, aimc.enemy.transform.position)/9));
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




}
