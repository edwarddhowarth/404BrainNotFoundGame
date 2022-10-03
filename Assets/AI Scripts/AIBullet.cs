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

    private void OnTriggerEnter(Collider other)
    {
        if(other.gameObject.GetComponent<Invector.vIDamageReceiver>() != null)
        {
            Invector.vDamage damage = new Invector.vDamage(25);
            other.gameObject.GetComponent<Invector.vIDamageReceiver>().TakeDamage(damage);
            
        }
        else
        {
            
        }
        Destroy(transform.gameObject);
    }
}
