using UnityEngine;
using System.Collections;
using UnityEditor;

// https://learn.unity.com/tutorial/editor-scripting#
//Adds button for easier addition of nodes to a waypoint

[CustomEditor(typeof(Waypoints))]
public class WaypointsEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        Waypoints script = (Waypoints)target;

        if (GUILayout.Button("Add Node to Waypoint Group"))
        {
            script.AddToWaypoint();
        }
    }


}
