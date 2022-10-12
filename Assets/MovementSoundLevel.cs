using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MovementSoundLevel : MonoBehaviour
{
    Invector.vCharacterController.vThirdPersonController vCharacter;
    float soundLevel;
    // Start is called before the first frame update
    void Start()
    {
        vCharacter = GetComponent<Invector.vCharacterController.vThirdPersonController>();
    }

    // Update is called once per frame
    void Update()
    {
        soundLevel = 1f;
        if(vCharacter.isSprinting)
        {
            soundLevel = 2f;
        }
        if(vCharacter.isCrouching)
        {
            soundLevel = .2f;
        }
        if(vCharacter.isRolling)
        {
            soundLevel = 1.3f;
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
