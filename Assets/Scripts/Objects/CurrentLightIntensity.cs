using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

// Attach to any object you want to have a light intensity value

public class CurrentLightIntensity : MonoBehaviour, IIlluminable
{
    private float lightIntensity;
    private List<float> incomingLight;
    Invector.vCharacterController.vThirdPersonController vCharacter;

    // Start is called before the first frame update
    void Start()
    {
        incomingLight = new List<float>();
        EventManager.StartListening(EventManager.EventType.LightIntensity, addLightSourceIntensity);
        vCharacter = GetComponent<Invector.vCharacterController.vThirdPersonController>();
    }

    public string getName()
    {
        return gameObject.name;
    }

    public Vector3 getCurrentPosition()
    {
        return transform.position;
    }

    // Update is called once per frame
    void Update()
    {

        lightIntensity = calcIntensity();
        sendIntensity();

        //Debug.Log("Light on player: " + lightIntensity);

        //Debug.Log("Individual Light Sources intensity on player: " + string.Join(", ", incommingLight));

        incomingLight.Clear();
    }

    public float calcIntensity()
    {
        float intensity = 0;

        if(incomingLight.Count > 0)
        {
            foreach (float i in incomingLight)
            {
                intensity += i;
            }
            //intensity = intensity / incommingLight.Count;
        }

        float multiplier = 1f;
        if (vCharacter.isCrouching)
        {
            multiplier = .6f;
        }

        if (vCharacter.isSprinting)
        {
            multiplier = 1.3f;
        }

        if(vCharacter.isRolling)
        {
            multiplier = .4f;
        }

        
        return intensity*multiplier;
    }

    public void sendIntensity()
    {
        EventManager.TriggerEvent(EventManager.EventType.ObjectLightIntensity, 
            new Dictionary<string, object> {
            { "objectName", gameObject.name },
            { "objectTag", gameObject.tag },
            { "objectIntensity", lightIntensity }, 
            { "objectLocation", transform.position }
        });
    }

    public void addLightSourceIntensity(Dictionary<string, object> message)
    {
        if(message.Count >= 2 && message["name"] is string && message["intensity"] is float)
        {
            string name = (string)message["name"];
            float intensity = (float)message["intensity"];
            if (name == gameObject.name)
            {
                incomingLight.Add(intensity);
            }
            
        }
    }
}
