using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;



//Angle Script
//http://answers.unity.com/comments/1202706/view.html

public class LightIntensityCalc : MonoBehaviour
{
    // Start is called before the first frame update
    Light lighting;
    GameObject player;



    float intensity;
    void Start()
    {
        player = GameObject.FindGameObjectWithTag("Player");
        lighting = GetComponent<Light>();
    }

    // Update is called once per frame
    void Update()
    {
        Vector3 playerDirection = (player.transform.position - lighting.transform.position).normalized; //The direction from the enemy that faces towards the player in a Vector3 form
        //Debug.Log(lighting.range);
        RaycastHit hit;
        

        if (lighting.type == LightType.Spot)
        {
            if (Vector3.Angle(player.transform.position - transform.position, transform.forward) < lighting.spotAngle)
            {
                //Debug.Log("drawing ray");
                //Debug.DrawRay(lighting.transform.position, playerDirection * Vector3.Distance(lighting.transform.position, player.transform.position), Color.yellow);
                intensity = lighting.intensity * (1 / Mathf.Pow(Vector3.Distance(lighting.transform.position, player.transform.position), 2));
                //Debug.Log(lighting.type);
                //Debug.Log(intensity);
                EventManager.TriggerEvent(EventManager.EventType.InLight, new Dictionary<string, object> { { "intensity", intensity } });

            }
        }
        //Debug.Log("In Angle of Camera");
            
        }
        
    }



 
