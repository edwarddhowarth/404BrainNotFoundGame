using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//Call a function which will create a physics object that will act as a bullet
//It will aim this bullet at the player even if the player is not directly infront of the player.


[RequireComponent(typeof(AudioSource))]
public class AIGunFiring : MonoBehaviour
{

    private GameObject barrelExit;

    public GameObject Bullet;
    AudioSource gunShot;

    // Start is called before the first frame update
    void Start()
    {
        barrelExit = transform.GetChild(0).gameObject;
        gunShot = GetComponent<AudioSource>();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void Fire(Transform fireAt)
    {
        Vector3 Aim = fireAt.position + new Vector3(0, .2f, 0);
        Vector3 fireAtDirection = Aim - barrelExit.transform.position;
        Quaternion dir = Quaternion.LookRotation(fireAtDirection, Vector3.up);
        Quaternion fADQ = Quaternion.Euler(fireAtDirection);
        Instantiate(Bullet, barrelExit.transform.position, barrelExit.transform.rotation);
        gunShot.Play();

        RaycastHit hit;


        Debug.DrawRay(barrelExit.transform.position, fireAtDirection * Vector3.Distance(barrelExit.transform.position, fireAtDirection), Color.green,2f);

        
    }
}
