using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;



//Angle Script
//http://answers.unity.com/comments/1202706/view.html

//Currently it only gets the intensity for the player object
//Really it needs to first find out what is in range of the light and what type of objects should be calculated
//Then it should calculate the light intensity and then send a dictionary with 2 values
//The name of the object and the intensity

//NEED
//name of object for message
//Position to calculate raycast

//Probably a oversized box collider based on the range and diameter of the point light that only IIlluminable objects will trigger
//Or just do a sphere collider since we just need to get information about IIlluminable Location and dont care about precision

//I assume performance is fine for heaps of box colliders/Sphere only looking for a specific object overlap
//We only care about checking if it is possible to be illuminated and not if they actually are which is what the raycast implement does
//We only want to know if there is an IIlluminable within range

//Raycast requires a direction to cast to and we dont know the player location without getting it first
//Raycast is also much more expensive compared to a box/sphere collider so you cant just cast everywhere and try to find an IIlluminable


//As the Collider works on fixed time, its not going to update as much as the other scripts and so there will be times where IIlluminable not be illuminated and thus break detection
//Should check if IIlluminable is inside and then store a reference
//Then when the collider updates again, check if the IIlluminable is still inside and if not, remove from reference list.

/*
 * Uses a sphere collider to 
 */

public class LightIntensityCalc : MonoBehaviour
{
    // Start is called before the first frame update
    Light lighting;
    //GameObject player;

    //public List<GameObject> illuminables; //Stores a reference to all objects that can be illuminated that are currently within range of the light
    Collider[] illuminables;
    const int maxColliders = 100;
    private int numColliders; // Holds result of OverlapSphereNonAlloc
    LayerMask illuminateMask = 1 << 3;


    float intensity;
    void Start()
    {
        //player = GameObject.FindGameObjectWithTag("Player");
        lighting = GetComponent<Light>();
        //illuminables = new List<GameObject>();
        illuminables = new Collider[maxColliders];
        
    }

    // Update is called once per frame
    void Update()
    {
        foreach(Collider o in illuminables)
        {
            if(o != null && o.gameObject && o.gameObject.GetComponent<IIlluminable>() != null)
            {
                Vector3 objectDirection = (o.transform.position - lighting.transform.position).normalized; //The direction from the enemy that faces towards the player in a Vector3 form
                RaycastHit hit;


                if (lighting.type == LightType.Spot)
                {
                    if (Vector3.Angle(o.transform.position - transform.position, transform.forward) < lighting.spotAngle && Physics.Raycast(transform.position, objectDirection, out hit, Mathf.Infinity))
                    {
                        if (hit.collider.name == o.name)
                        {
                            //Debug.Log("drawing ray");
                            //Debug.DrawRay(lighting.transform.position, playerDirection * Vector3.Distance(lighting.transform.position, player.transform.position), Color.yellow);
                            intensity = lighting.intensity * (1 / Mathf.Pow(Vector3.Distance(lighting.transform.position, o.transform.position), 2));
                            //Debug.Log(lighting.type);
                            //Debug.Log(intensity);
                            EventManager.TriggerEvent(EventManager.EventType.LightIntensity, new Dictionary<string, object> { { "name", o.name }, { "intensity", intensity } });
                            Debug.Log(o.name + " intensity at: " + intensity);
                        }


                    }
                }
            }
           
        }

            
        }


    // Need to clear all illuminables before the next OnTriggerStay so we can ensure that the references held are still within the collider
    // Fixed update happens before OnTriggerStay and both happen before Update
    // BUT fixedUpdate and Time.fixedDeltaTime only happen every 0.02 (Can decrease it but it will break everything and increase the cost of physics)
    // So we need to clear during the start of FixedUpdate, get illuminables just after in OnTriggerStay and then during the time between FixedUpdate, keep calculating the illuminable items list in update
    //Alternative would be to just send it and then tell the calc to average it out but that doesn't really solve the issue
    // The best solution would be one that worked in Update would require storing all references so this is the best.
    // https://docs.unity3d.com/Manual/ExecutionOrder.html
    private void FixedUpdate()
    {
        int numColliders = Physics.OverlapSphereNonAlloc(lighting.transform.position, lighting.range, illuminables, illuminateMask);
        foreach(Collider o in illuminables)
        {
            if(o != null)
            {
                Debug.Log(numColliders + " " + o.name + " " + o.GetInstanceID());
            }
            
        }
        
        if(numColliders > maxColliders)
        {
            Debug.LogError(gameObject.name + " Has too many objects in the scene to illuminate");
        }

    }





}



