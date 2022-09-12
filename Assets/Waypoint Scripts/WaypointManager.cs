using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//Put onto an empty object
//Can create empty waypoint groups as a child of this object in the inspector
//Waypoint groups can then add waypoints.

public class WaypointManager : MonoBehaviour
{
    // Start is called before the first frame update

    private static WaypointManager waypointManager;

    private List<GameObject> waypointGroupsList;
    public static WaypointManager instance
    {
        get
        {
            if (!waypointManager)
            {
                waypointManager = FindObjectOfType(typeof(WaypointManager)) as WaypointManager;

                if (!waypointManager)
                {
                    Debug.LogError("There needs to be one active Waypoint script on a GameObject in your scene.");
                }
                else
                {
                    waypointManager.Init();

                    //  Sets this to not be destroyed when reloading scene
                    DontDestroyOnLoad(waypointManager);
                }
            }
            return waypointManager;
        }
    }

    void Init()
    {
        if (waypointGroupsList == null)
        {
            waypointGroupsList = new List<GameObject>();
        }
    }

    private void Start()
    {
        foreach(Transform t in transform)
        {
            instance.waypointGroupsList.Add(t.gameObject);
        }
    }

    public static GameObject GetWaypointGroupByName(string name)
    {
        GameObject group = instance.waypointGroupsList.Find(x => x.name.Contains(name));
        
        if(group != null)
        {
            return group;
        }
        else
        {
            return null;
        }
    }

    public void CreateWaypointList()
    {
        GameObject newWaypointList = new GameObject("New Waypoint Group");

        newWaypointList.AddComponent<Waypoints>();
        newWaypointList.transform.parent = transform;
    }
}
