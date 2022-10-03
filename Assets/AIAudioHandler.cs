using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(AudioSource))]
public class AIAudioHandler : MonoBehaviour
{
    AudioSource source;
    public AudioClip stoneFootstepClip;
    public AudioClip metalFootstepClip;
    // Start is called before the first frame update
    void Start()
    {
        source = GetComponent<AudioSource>();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void metalFootstep()
    {


    }
    public void stoneFootstep()
    {
        source.clip = stoneFootstepClip;
        source.Play();

    }

}
