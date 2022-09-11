using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//Put onto an empty object
//Can create empty waypoint groups as a child of this object in the inspector
//Waypoint groups can then add waypoints.

public class WaypointManager : MonoBehaviour
{
    // Start is called before the first frame update


    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void CreateWaypointList()
    {
        GameObject newWaypointList = new GameObject("New Waypoint Group");

        newWaypointList.AddComponent<Waypoints>();
        newWaypointList.transform.parent = transform;
    }
}
