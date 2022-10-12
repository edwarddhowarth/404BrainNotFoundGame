using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering.PostProcessing;

public class AttackChomaticAbberation : MonoBehaviour
{
    GameObject camera;
    PostProcessVolume vol;
    ChromaticAberration CA;
    Bloom bloom;
    float startingCAValue;
    float startingBloomValue;
    float CACounter;
    bool hit;
    // Start is called before the first frame update
    void Start()
    {
        camera = transform.parent.gameObject;
        vol = camera.GetComponent<PostProcessVolume>();
        vol.profile.TryGetSettings( out CA);
        vol.profile.TryGetSettings(out bloom);
        startingCAValue = CA.intensity.value;
        startingBloomValue = bloom.intensity.value;

    }

    // Update is called once per frame
    void Update()
    {
        if(hit)
        {
            CA.intensity.value = CA.intensity.value - Time.deltaTime * 2f;
            bloom.threshold.value = bloom.threshold.value + Time.deltaTime * .5f;

            if(CA.intensity.value <= startingCAValue)
            {
                hit = false;
                CA.intensity.value = startingCAValue;
                bloom.intensity.value = startingBloomValue;
            }
        }
        
    }

    public void onHitBullet()
    {
        CA.intensity.value = 1f;
        bloom.threshold.value = .25f;
        hit = true;
    }
}
