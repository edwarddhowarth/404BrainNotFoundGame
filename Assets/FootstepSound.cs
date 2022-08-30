using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FootstepSound : MonoBehaviour
{
    public AudioClip footstep;
    AudioSource footstepSource;

    // Start is called before the first frame update
    void Start()
    {
        footstepSource = GetComponent<AudioSource>();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void OnTriggerEnter(Collider other)
    {
        Debug.Log(other.tag);
        footstepSource.PlayOneShot(footstep);
        
    }
}

