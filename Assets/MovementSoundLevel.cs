using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MovementSoundLevel : MonoBehaviour
{
    Invector.vCharacterController.vThirdPersonController vCharacter;
    float soundLevel;
    Rigidbody rb;
    // Start is called before the first frame update
    void Start()
    {
        vCharacter = GetComponent<Invector.vCharacterController.vThirdPersonController>();
        rb = GetComponent<Rigidbody>();
    }

    // Update is called once per frame
    void Update()
    {
        if(rb.velocity.magnitude > .5f)
        {
            soundLevel = 1f;
            if (vCharacter.isSprinting)
            {
                soundLevel = 2f;
            }
            if (vCharacter.isCrouching)
            {
                soundLevel = .2f;
            }
            if (vCharacter.isRolling)
            {
                soundLevel = 1.3f;
            }
        }
        else
        {
            soundLevel = 0f;
        }
       

        sendSoundLevel();
    }

    public void sendSoundLevel()
    {
        EventManager.TriggerEvent(EventManager.EventType.PlayerSoundLevel,
            new Dictionary<string, object> {
                { "soundLevel", soundLevel }
        });
    }
}
