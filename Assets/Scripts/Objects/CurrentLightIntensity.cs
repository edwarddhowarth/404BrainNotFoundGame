using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class CurrentLightIntensity : MonoBehaviour
{
    private float lightIntensity;
    public List<float> incommingLight;

    // Start is called before the first frame update
    void Start()
    {
        incommingLight = new List<float>();
        EventManager.StartListening(EventManager.EventType.InLight, addLightSourceIntensity);
    }

    // Update is called once per frame
    void Update()
    {

        lightIntensity = calcIntensity();
        sendIntensity();

        //Debug.Log("Light on player: " + lightIntensity);

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
        EventManager.TriggerEvent(EventManager.EventType.PlayerLightIntensity, 
            new Dictionary<string, object> { 
            { "playerIntensity", lightIntensity }, 
            { "playerLocation", transform.position } 
        });
    }

    public void addLightSourceIntensity(Dictionary<string, object> message)
    {
        if(message["intensity"] is float)
        {
            incommingLight.Add((float)message["intensity"]);
        }
    }
}