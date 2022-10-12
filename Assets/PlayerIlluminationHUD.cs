using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PlayerIlluminationHUD : MonoBehaviour
{
    float intensity;
    float sound;
    Slider sliderLight;
    List<float> averageIntensity;
    // Start is called before the first frame update
    void Start()
    {
        EventManager.StartListening(EventManager.EventType.ObjectLightIntensity, PlayerLightIntensity);
        
        sliderLight = GetComponent<Slider>();
        averageIntensity = new List<float>();
    }

    // Update is called once per frame
    void Update()
    {
        if(averageIntensity.Count <= 100)
        {
            averageIntensity.Insert(0, intensity);
        }
        else
        {
            averageIntensity.RemoveAt(averageIntensity.Count-1);
            averageIntensity.Insert(0, intensity);
        }
        foreach(float i in averageIntensity)
        {
            intensity += i;
        }

        sliderLight.value = intensity/averageIntensity.Count;

        //update some graphic with the value of the player light intensity
        //change color to white if player is not illuminated enough to be spotted by a distant enemy
        //change color to yellow if player is illuminated enough to be spotted a distant enemy
        //change color to red if they are spotted by an AI or Camera
        //Camera icon if detected by camera, Eye if detected by AI
        
    }

    void PlayerLightIntensity(Dictionary<string, object> message)
    {
        //get the player's light intensity and then store it

        if(message["objectName"] is string && message["objectTag"] is string && message["objectIntensity"] is float && message["objectLocation"] is Vector3)
        {
            if((string)message["objectName"] == "vThirdPersonBasic" && (string)message["objectTag"] == "Player")
            {
                intensity = (float)message["objectIntensity"];
            }
        }
    }

 

    //message recieve - player detected by AI

    //message recieve - player detected by camera
}
