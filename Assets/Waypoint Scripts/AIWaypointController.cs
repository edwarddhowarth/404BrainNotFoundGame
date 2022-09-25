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
    public List<Waypoints> waypoints;

    private Waypoints workingWaypoint;

    Collider collider;

    private Transform _currentWaypoint;
    public Transform currentWaypoint
    {
        get { return _currentWaypoint; }
    }

    private Waypoints workingPosition;

    private Transform _singlePosition;
    public Transform singlePosition
    {
        get { return _singlePosition; }
    }

    // Start is called before the first frame update
    void Start()
    {
        workingPosition = waypoints[0];

        workingWaypoint = waypoints[0];
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

    public Transform GetPosition(int pos)
    {
        return workingPosition.transform.GetChild(pos).transform;
    }


    public void GetNextWaypoint()
    {
        _currentWaypoint = workingWaypoint.GetNextWaypoint(_currentWaypoint);
    }

    public void GetClosestWaypoint()
    {
        _currentWaypoint = workingWaypoint.GetClosestWaypoint(transform);

    }

    public void ChangeWaypointGroup(string name)
    {
        GameObject wp = WaypointManager.GetWaypointGroupByName(name);
        if(wp != null )
        {
            workingWaypoint = wp.GetComponent<Waypoints>();
        }
        
    }


}
