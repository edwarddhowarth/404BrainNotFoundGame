using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AIManager : MonoBehaviour
{
    private static AIManager aiManager;

    private List<GameObject> selectedAIList;
    public static AIManager instance
    {
        get
        {
            if(!aiManager)
            {
                aiManager = FindObjectOfType(typeof(AIManager)) as AIManager;
                if (!aiManager)
                {
                    Debug.LogError("There needs to be one active AIManager script on a GameObject in your scene.");
                }
                else
                {
                    aiManager.Init();

                    //  Sets this to not be destroyed when reloading scene
                    DontDestroyOnLoad(aiManager);
                }
            }
            return aiManager;
        }
    }

    void Init()
    {
        selectedAIList = new List<GameObject>();
        EventManager.StartListening(EventManager.EventType.PlayerDetectedByCamera, PlayerDetectedByCamera);
    }


    private void PlayerDetectedByCamera(Dictionary<string, object> message)
    {

    }

}
