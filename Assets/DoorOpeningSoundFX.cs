using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(AudioSource))]
public class DoorOpeningSoundFX : MonoBehaviour
{

    AudioSource source;
    public AudioClip doorOpeningClip;
    public AudioClip doorOpenedClip;

    // Start is called before the first frame update
    void Start()
    {
        source = GetComponent<AudioSource>();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void doorOpening()
    {
        source.clip = doorOpeningClip;
        source.Play();

    }

    public void doorOpen()
    {
        source.clip = doorOpenedClip;
        source.Play();
    }


}
