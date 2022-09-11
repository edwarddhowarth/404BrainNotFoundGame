using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

//Using Code from MetalStorm Games - Unity Basics - Waypoint path system in unity https://www.youtube.com/watch?v=EwHiMQ3jdHw

//Create using WaypointManager
//Once created, you can add waypoints using a button in the inspector

public class Waypoints : MonoBehaviour
{
    public bool ShowLoop = true;

    [SerializeField]
    private bool displaySequence = true;

    private int _waypoints = 0;
    public int waypoints { 
        get { return _waypoints; } 
    }


#if UNITY_EDITOR
    private void OnDrawGizmos()
    { 
        if(transform.childCount > 0)
        {
            //Draw Spheres
            foreach (Transform t in transform)
            {
                Gizmos.color = Color.blue;
                Gizmos.DrawWireSphere(t.position, 1f);

            }

            //Colors starting waypoint green
            Gizmos.color = Color.green;
            Gizmos.DrawWireSphere(transform.GetChild(0).position, 1f);

            //Colors last waypoint magenta
            Gizmos.color = Color.magenta;
            Gizmos.DrawWireSphere(transform.GetChild(transform.childCount - 1).position, 1f);

            //Draws red lines from first to last.
            Gizmos.color = Color.red;
            for (int i = 0; i < transform.childCount - 1; i++)
            {
                Gizmos.DrawLine(transform.GetChild(i).position, transform.GetChild(i + 1).position);
            }

            //draws a line from the last to first waypoints
            if (ShowLoop)
            {
                Gizmos.DrawLine(transform.GetChild(transform.childCount - 1).position, transform.GetChild(0).position);
            }

            //Displays the sequence number of each waypoint
            if (displaySequence)
            {
                for (int i = 0; i < transform.childCount; i++)
                {
                    GUIStyle style = new GUIStyle();
                    style.fontSize = 20;
                    Handles.Label(transform.GetChild(i).position + new Vector3(0, 3f, 0), i.ToString(), style);
                }
            }
        }
       

    }

    public void AddToWaypoint()
    {
        
        GameObject newWaypoint = new GameObject("Waypoint " + _waypoints);
        newWaypoint.transform.parent = transform;
        _waypoints++;
    }
#endif

    public Transform GetNextWaypoint(Transform currentWaypoint)
    {
        //Waypoint not set, return first
        if(currentWaypoint == null)
        {
            return transform.GetChild(0);
        }
        
        //Get waypoint index and return the next in sequence.
        if(currentWaypoint.GetSiblingIndex() < transform.childCount -1)
        {
            return transform.GetChild(currentWaypoint.GetSiblingIndex() + 1);
        }
        else //Last waypoint given, return first.
        {
            return transform.GetChild(0);
        }
    }

    public Transform GetClosestWaypoint(Transform objectTransform)
    {
        if (objectTransform == null)
        {
            return transform.GetChild(0);
        }

        Transform waypoint = transform.GetChild(0);

        foreach(Transform t in transform)
        {
            if(Vector3.Distance(objectTransform.position, t.position) < Vector3.Distance(objectTransform.position, waypoint.position))
            {
                waypoint = t;
            }
        }

        return waypoint;
    }



}
