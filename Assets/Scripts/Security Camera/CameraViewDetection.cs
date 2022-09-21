using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraViewDetection : MonoBehaviour
{
    [HideInInspector]
    public float cameraFOV;

    [HideInInspector]
    public float MaximumDetectionDistance;
    [HideInInspector]
    public float MinimumLightDetection;

    Collider[] players;
    LayerMask playerMask = 1 << 3;

    GameObject player;
    int maxPlayers = 100;



    [HideInInspector]
    public bool playerInFieldOfView;

    Camera cam;

    // Start is called before the first frame update
    void Start()
    {
        cam = GetComponent<Camera>();
        cameraFOV = cam.fieldOfView;
        playerInFieldOfView = false;
        players = new Collider[maxPlayers];
    }

    // Update is called once per frame
    void Update()
    {
        cameraFOV = cam.fieldOfView; //FOV may change during runtime
        //Debug.Log(LayerMask.LayerToName(3));

        playerInFieldOfView = false;
        RaycastHit hit;
      
        
        //Debug.DrawRay(transform.position, new Vector3(0f,player.transform.position.z, player.transform.position.y) * Vector3.Distance(transform.position, new Vector3(0f, player.transform.position.z, player.transform.position.y)), Color.green);

        //Debug.DrawRay(transform.position, Quaternion.Euler(30f, -0f, 0) * transform.forward * 100, Color.green);
        //Debug.Log(cameraFOV / 2);

        foreach (Collider playable in players)
        {
            if(playable != null && playable.gameObject && playable.tag == "Player")
            {
                player = playable.gameObject;
            }
        }
        if(player != null)
        {
            Vector3 playerDirection = (player.transform.position - transform.position).normalized; //The direction from the enemy that faces towards the player in a Vector3 form
                                                                                                   //Debug.DrawRay(transform.position, new Vector3(player.transform.position.x, 0f , player.transform.position.z) * Vector3.Distance(transform.position, new Vector3(0f, player.transform.position.z, player.transform.position.y)), Color.green);


            if (Vector3.Angle(player.transform.position - transform.position, transform.forward) < cameraFOV / 2)
                if (Physics.Raycast(transform.position, playerDirection, out hit, Mathf.Infinity))
                {
                    if (hit.collider.tag == "Player" && Vector3.Distance(transform.position, hit.collider.transform.position) < MaximumDetectionDistance)
                    {
                        //Debug.DrawRay(transform.position, playerDirection * Vector3.Distance(transform.position, player.transform.position), Color.green);
                        playerInFieldOfView = true;
                    }
                }
            player = null;
        }

        

    }

    private void FixedUpdate()
    {
        int numColliders = Physics.OverlapSphereNonAlloc(transform.position, MaximumDetectionDistance, players, playerMask);

        if (numColliders > maxPlayers)
        {
            Debug.LogError(gameObject.name + " Has too many objects in the scene to illuminate");
        }
    }




}
