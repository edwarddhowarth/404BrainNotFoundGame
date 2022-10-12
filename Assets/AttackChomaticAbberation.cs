using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering.PostProcessing;

public class AttackChomaticAbberation : MonoBehaviour
{
    GameObject camera;
    PostProcessVolume vol;
    ChromaticAberration CA;
    float startingCAValue;
    float CACounter;
    bool hit;
    // Start is called before the first frame update
    void Start()
    {
        camera = transform.parent.gameObject;
        vol = camera.GetComponent<PostProcessVolume>();
        vol.profile.TryGetSettings( out CA);
        startingCAValue = CA.intensity.value;

    }

    // Update is called once per frame
    void Update()
    {
        if(hit)
        {
            CA.intensity.value = CA.intensity.value - Time.deltaTime * 2f;

            if(CA.intensity.value <= startingCAValue)
            {
                hit = false;
                CA.intensity.value = startingCAValue;
            }
        }
        
    }

    public void onHitBullet()
    {
        CA.intensity.value = 1f;
        hit = true;
    }
}
