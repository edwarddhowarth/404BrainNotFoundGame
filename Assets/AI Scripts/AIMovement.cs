using System.Collections;
using System.Collections.Generic;
using UnityEngine.AI;
using UnityEngine;

public class AIMovement : MonoBehaviour
{
    public Transform goal;
    public GameObject target;
    NavMeshAgent agent;
    // Start is called before the first frame update
    void Start()
    {
        target = GameObject.Find("enemy");
        agent = GetComponent<NavMeshAgent>();
        agent.destination = goal.position;
        agent.updateRotation = false;
    }

    // Update is called once per frame
    void Update()
    {
        Vector3 targetDirection = (target.transform.position - transform.position).normalized;
        Quaternion dir = Quaternion.LookRotation(targetDirection, Vector3.up);
        agent.transform.rotation = Quaternion.Lerp(transform.rotation, dir, 5f * Time.deltaTime);

    }
}
