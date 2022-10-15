using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerDetected : MonoBehaviour
{
    public GameObject player;
    bool detected = false;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {

        if(detected)
        {
            EventManager.TriggerEvent(EventManager.EventType.PlayerDetectedByCamera,
                        new Dictionary<string, object> { { "playerLocation", player.transform.position },
                            {"cameraLocation", transform.position }
                        });
        }
        
    }

    public void AttackPlayer()
    {
        detected = true;
    }

}
