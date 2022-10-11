using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerIlluminationHUD : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        EventManager.StartListening(EventManager.EventType.LightIntensity, PlayerLightIntensity);
    }

    // Update is called once per frame
    void Update()
    {
        //update some graphic with the value of the player light intensity
        //change color to white if player is not illuminated enough to be spotted by a distant enemy
        //change color to yellow if player is illuminated enough to be spotted a distant enemy
        //change color to red if they are spotted by an AI or Camera
        //Camera icon if detected by camera, Eye if detected by AI
        
    }

    void PlayerLightIntensity(Dictionary<string, object> message)
    {
        //get the player's light intensity and then store it
    }

    //message recieve - player detected by AI

    //message recieve - player detected by camera
}
