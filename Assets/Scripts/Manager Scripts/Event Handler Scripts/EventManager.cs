using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//Using code from
//http://bernardopacheco.net/using-an-event-manager-to-decouple-your-game-in-unity
//change from Extiward in the comment section:
//Changed to using an Enum for the event names as it reduces one input from using a String

/*
 * Handles events for all objects going to and from one another.
 * 
 */

public class EventManager : MonoBehaviour
{

    private Dictionary<EventType, Action<Dictionary<string, object>>> eventDictionary;

    private static EventManager eventManager;

    public enum EventType {
        PlayerPosition,
        InCameraView,
        LightIntensity,
        ObjectLightIntensity,
        PlayerDetected,
    }




    public static EventManager instance
    {
        get
        {
            if (!eventManager)
            {
                eventManager = FindObjectOfType(typeof(EventManager)) as EventManager;

                if (!eventManager)
                {
                    Debug.LogError("There needs to be one active EventManager script on a GameObject in your scene.");
                }
                else
                {
                    eventManager.Init();

                    //  Sets this to not be destroyed when reloading scene
                    DontDestroyOnLoad(eventManager);
                }
            }
            return eventManager;
        }
    }

    void Init()
    {
        if (eventDictionary == null)
        {
            eventDictionary = new Dictionary<EventType, Action<Dictionary<string, object>>>();
        }
    }

    public static void StartListening(EventType eventName, Action<Dictionary<string, object>> listener)
    {
        Action<Dictionary<string, object>> thisEvent;

        if (instance.eventDictionary.TryGetValue(eventName, out thisEvent))
        {
            thisEvent += listener;
            instance.eventDictionary[eventName] = thisEvent;
        }
        else
        {
            thisEvent += listener;
            instance.eventDictionary.Add(eventName, thisEvent);
        }
    }

    public static void StopListening(EventType eventName, Action<Dictionary<string, object>> listener)
    {
        if (eventManager == null) return;
        Action<Dictionary<string, object>> thisEvent;
        if (instance.eventDictionary.TryGetValue(eventName, out thisEvent))
        {
            thisEvent -= listener;
            instance.eventDictionary[eventName] = thisEvent;
        }
    }

    public static void TriggerEvent(EventType eventName, Dictionary<string, object> message)
    {
        Action<Dictionary<string, object>> thisEvent = null;
        if (instance.eventDictionary.TryGetValue(eventName, out thisEvent))
        {
            thisEvent.Invoke(message);
        }
    }
}
