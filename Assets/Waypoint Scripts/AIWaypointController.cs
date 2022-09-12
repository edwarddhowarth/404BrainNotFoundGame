using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

//Handles getting waypoints
//need to add a way to easily change between waypoints.

[RequireComponent(typeof(CapsuleCollider))]
public class AIWaypointController : MonoBehaviour
{
    //Stores reference to waypoint
    [SerializeField]
    private Waypoints waypoints;

    Collider collider;

    private Transform _currentWaypoint;
    public Transform currentWaypoint
    {
        get { return _currentWaypoint; }
    }

    // Start is called before the first frame update
    void Start()
    {
        collider = GetComponent<Collider>();
        GetClosestWaypoint();
    }

    // Update is called once per frame
    void Update()
    {
        float biggestLength; //The longest axis of the objects collider

        if (collider.bounds.size.x > collider.bounds.size.y && collider.bounds.size.x > collider.bounds.size.z)
        {
            biggestLength = collider.bounds.size.x;
        }
        else if (collider.bounds.size.y > collider.bounds.size.x && collider.bounds.size.y > collider.bounds.size.z)
        {
            biggestLength = collider.bounds.size.y;
        }
        else if (collider.bounds.size.z > collider.bounds.size.x && collider.bounds.size.z > collider.bounds.size.y)
        {
            biggestLength = collider.bounds.size.z;
        }
        else
        {
            biggestLength = collider.bounds.size.x + collider.bounds.size.y + collider.bounds.size.z;
        }


        if(Vector3.Distance(transform.position, currentWaypoint.position) < biggestLength)
        {
            GetNextWaypoint();
        }
    }


    public void GetNextWaypoint()
    {
        _currentWaypoint = waypoints.GetNextWaypoint(_currentWaypoint);
    }

    public void GetClosestWaypoint()
    {
        _currentWaypoint = waypoints.GetClosestWaypoint(transform);

    }

    public void ChangeWaypointGroup(string name)
    {
        GameObject wp = WaypointManager.GetWaypointGroupByName(name);
        if(wp != null )
        {
            waypoints = wp.GetComponent<Waypoints>();
        }
        
    }


}
