using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

// Attach to any object you want to have a light intensity value

public class CurrentLightIntensity : MonoBehaviour, IIlluminable
{
    public float threshold = 0.5f; //Threshold of how lit the object must be before it is detected
    private float lightIntensity;
    public List<float> incommingLight;

    // Start is called before the first frame update
    void Start()
    {
        incommingLight = new List<float>();
        EventManager.StartListening(EventManager.EventType.LightIntensity, addLightSourceIntensity);
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

        Debug.Log("Light on player: " + lightIntensity);

        //Debug.Log("Individual Light Sources intensity on player: " + string.Join(", ", incommingLight));

        incommingLight.Clear();
    }

    public float calcIntensity()
    {
        float intensity = 0;

        if(incommingLight.Count > 0)
        {
            foreach (float i in incommingLight)
            {
                intensity += i;
            }
            //intensity = intensity / incommingLight.Count;
        }
        
        return intensity;
    }

    public void sendIntensity()
    {
        EventManager.TriggerEvent(EventManager.EventType.ObjectLightIntensity, 
            new Dictionary<string, object> {
            { "objectName", gameObject.name },
            { "objectTag", gameObject.tag },
            { "objectIntensity", lightIntensity }, 
            { "objectLocation", transform.position },
            { "threshold", threshold }
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
                incommingLight.Add(intensity);
            }
            
        }
    }
}
