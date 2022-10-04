using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraViewConsole : MonoBehaviour
{
    public GameObject player;
    public GameObject security_1;

    Camera playerCam;
    Camera security_1Cam;
    // Start is called before the first frame update
    void Start()
    {
        if(player && security_1)
        {
            playerCam = player.GetComponent<Camera>();
            security_1Cam = security_1.GetComponent<Camera>();
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void viewSecurityCamera()
    {
        playerCam.enabled = false;
        security_1Cam.enabled = true;
    }

    public void returnToPlayer()
    {
        security_1Cam.enabled = false;
        playerCam.enabled = true;
    }

}
