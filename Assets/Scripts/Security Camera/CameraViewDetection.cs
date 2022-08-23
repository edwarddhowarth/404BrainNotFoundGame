using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraViewDetection : MonoBehaviour
{
    float cameraFOV;
    GameObject player;
    Vector3 initalCameraRotation;
    float cameraAspectRatio;

    bool playerInView;

    // Start is called before the first frame update
    void Start()
    {
        cameraFOV = GetComponent<Camera>().fieldOfView;
        cameraAspectRatio = GetComponent<Camera>().aspect;
        player = GameObject.FindGameObjectWithTag("Player");

        initalCameraRotation = transform.rotation.eulerAngles;
        playerInView = false;
    }

    // Update is called once per frame
    void Update()
    {
        /*
        if (cameraAspectRatio >= 1.7f)
        {
            Debug.Log("16:9");
        }
        else if (cameraAspectRatio >= 1.6)
        {
            Debug.Log("16:10");
        }
        else if (cameraAspectRatio >=1.5)
        {
            Debug.Log("3:2");
        }
        else if( cameraAspectRatio >=1.3)
        {
            Debug.Log("4:3");
        }
        else
        {
            Debug.Log("1:1");
            
        }
        */



        playerInView = false;
        RaycastHit hit;
        Vector2 playerXZ = new Vector2(player.transform.position.x, player.transform.position.z);
        Vector2 playerYZ = new Vector2(player.transform.position.y, player.transform.position.z);
        Vector2 cameraXZ = new Vector2(transform.position.x, transform.position.z);
        Vector2 cameraYZ = new Vector2(transform.position.y, transform.position.z);
        float cameraYAngleRelToStart = transform.rotation.eulerAngles.y - initalCameraRotation.y;
        float cameraXAngleRelToStart = transform.rotation.eulerAngles.z - initalCameraRotation.z;
        //Debug.Log(AngleBetweenVector2(playerXZ, cameraXZ));
        //Debug.Log(AngleBetweenVector2(playerYZ, cameraYZ));
        //Debug.Log("Angle from Camera to Player: " + Vector3.Angle(player.transform.position - transform.position, transform.forward));
        //Debug.Log("cam pos: " + transform.position + "\nplayer pos: " + player.transform.position);
        //Debug.DrawRay(transform.position, player.transform.position - transform.position, Color.white);

        //Debug.Log("Camera Y Angle Degrees: " + cameraYAngleRelToStart);
        //Debug.Log("Camera X Angle Degrees: " + cameraXAngleRelToStart);
        Vector3 playerDirection = (player.transform.position - transform.position).normalized; //The direction from the enemy that faces towards the player in a Vector3 form
        //Debug.DrawRay(transform.position, new Vector3(player.transform.position.x, 0f , player.transform.position.z) * Vector3.Distance(transform.position, new Vector3(0f, player.transform.position.z, player.transform.position.y)), Color.green);

        //Debug.DrawRay(transform.position, new Vector3(0f,player.transform.position.z, player.transform.position.y) * Vector3.Distance(transform.position, new Vector3(0f, player.transform.position.z, player.transform.position.y)), Color.green);


        if (Vector3.Angle(player.transform.position - transform.position, transform.forward) < cameraFOV/2)
            if (Physics.Raycast(transform.position, playerDirection, out hit, Mathf.Infinity))
            {
                if (hit.collider.tag == "Player")
                {
                    //Debug.DrawRay(transform.position, playerDirection * Vector3.Distance(transform.position, player.transform.position), Color.green);
                    playerInView = true;
                }
            }

    }


    private float AngleBetweenVector2(Vector2 v1, Vector2 v2)
    {
        Vector2 diference = v1 - v2;
        float sign = (v2.y < v1.y) ? -1.0f : 1.0f;
        return Vector2.Angle(Vector2.right, diference) * sign;
    }



}