using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//Call a function which will create a physics object that will act as a bullet
//It will aim this bullet at the player even if the player is not directly infront of the player.



public class AIGunFiring : MonoBehaviour
{

    private GameObject barrelExit;

    // Start is called before the first frame update
    void Start()
    {
        barrelExit = transform.GetChild(0).gameObject;
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void Fire(Transform fireAt)
    {
        Vector3 fireAtDirection = (fireAt.position - transform.position).normalized;

        RaycastHit hit;

        Debug.DrawRay(transform.position, fireAtDirection * Vector3.Distance(transform.position, fireAtDirection), Color.green,2f);

        
    }
}
