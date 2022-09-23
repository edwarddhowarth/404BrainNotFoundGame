using UnityEngine;
using System.Collections;
using UnityEditor;

// https://learn.unity.com/tutorial/editor-scripting#
//Adds button for easy waypoint group creation


[CustomEditor(typeof(WaypointManager))]
public class WaypointManagerEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        WaypointManager script = (WaypointManager)target;

        if(GUILayout.Button("Create Waypoint Group"))
        {
            script.CreateWaypointList();
        }
    }
}
