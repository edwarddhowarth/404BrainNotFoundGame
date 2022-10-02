using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AIBullet : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        transform.position = transform.position + (transform.up * 1f);
    }
}
