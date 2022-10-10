using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AttackMusic : MonoBehaviour
{

    AudioSource source;
    public AudioClip main;
    public AudioClip tail;
    bool playerEngaged;
    bool wasEngaged;
    float countdown = 0f;
    float escapeCooldown = 0f;
    // Start is called before the first frame update
    void Start()
    {
        source = GetComponent<AudioSource>();
        EventManager.StartListening(EventManager.EventType.AIEngaged, PlayerEngaged);
    }

    // Update is called once per frame
    void Update()
    {
        // IF player is not engaged, and hasn't recently been in combat and countdown, do nothing
        if (!playerEngaged && !wasEngaged)
        {
            if(countdown <= 0f)
            {
                countdown = 0f;
            }

        }
        else if (playerEngaged && !wasEngaged) // If the player hasn't just been in combat and has just been engaged
        {
            
            
            

            if(escapeCooldown <= 0f)
            {
                Debug.Log("Player Engaged for first time");
                countdown = 1f;
                source.loop = true;
                source.clip = main;
                source.Play();
                wasEngaged = true;
            }
            else if(escapeCooldown >= -.1f)
            {
                escapeCooldown -= Time.deltaTime;
            }


        }
        // player no longer being engaged but was just in combat and still in countdown
        else if (!playerEngaged && wasEngaged) 
        {
            if (countdown > -.2f)
            {
                countdown -= Time.deltaTime;
            }

           // Debug.Log("calc: " + (source.timeSamples / 44100.0f));

            if(countdown <= 0f && ((source.timeSamples / 44100.0f) % 3.30f < .01f))
            {
                Debug.Log("Stopping music");
                wasEngaged = false;
                source.Stop();
                source.clip = tail;
                source.loop = false;
                source.Play();
                

                escapeCooldown = 1f;
            }
        }


        playerEngaged = false;





    }

    private void PlayerEngaged(Dictionary<string, object> message)
    {
        if (message["AIEngaging"] is bool)
        {
            playerEngaged = true;
        }

    }

}
