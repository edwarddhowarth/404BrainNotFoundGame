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

    private Vector3 startRot = new Vector3(-90f, 90f, 90f);

    private Quaternion originalRot;

    // Start is called before the first frame update
    void Start()
    {
        barrelExit = transform.GetChild(0).GetChild(0).gameObject;
        gunShot = GetComponent<AudioSource>();

    }

    // Update is called once per frame
    void Update()
    {
        

    }

    public void Fire(Transform fireAt)
    {
        transform.rotation = originalRot;
        Debug.DrawRay(transform.position, transform.up * 100f, Color.cyan, 2f);
        Vector3 Aim = fireAt.position + new Vector3(0, 1f, 0);
        Vector3 fireAtDirection = Aim - barrelExit.transform.position;
        Quaternion dir = Quaternion.LookRotation(fireAtDirection, Vector3.up);
        Quaternion fADQ = Quaternion.Euler(fireAtDirection);


        Vector3 FADG = Aim - transform.position;
        Quaternion gunDir = Quaternion.LookRotation(FADG, Vector3.up);
        transform.rotation = gunDir;
        Instantiate(Bullet, barrelExit.transform.position, barrelExit.transform.rotation);
        Debug.DrawRay(barrelExit.transform.position, fireAtDirection * Vector3.Distance(barrelExit.transform.position, fireAtDirection), Color.green, 2f);
        gunShot.Play();
        //transform.rotation = originalRot;

        RaycastHit hit;


        

        
    }
}
