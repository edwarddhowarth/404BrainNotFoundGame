using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PanningTest : MonoBehaviour
{
    // Start is called before the first frame update
    AudioSource test;
    void Start()
    {
        test = GetComponent<AudioSource>();
    }

    // Update is called once per frame
    void Update()
    {
        //test.panStereo;
    }
}
