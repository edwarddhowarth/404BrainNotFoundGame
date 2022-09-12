using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//Controls the animations for the AI and for rotations such as rotating spine/head so they are looking at the player/object

// http://answers.unity.com/answers/181890/view.html for detirmining angle
[RequireComponent(typeof(Animator))]
public class AIAnimationController : MonoBehaviour
{
    Vector3 movementDirection;
    Vector3 lookDir;

    Vector3 previousFramePostion;

    Animator animator;
    AIMovementController aimc;

    bool isStrafing;

    int Strafing;

    int Speed;
    int LookMoveAngle;

    Transform spine;
    Transform neck;

    // Start is called before the first frame update
    void Start()
    {
        previousFramePostion = transform.position;

        animator = GetComponent<Animator>();
        aimc = GetComponent<AIMovementController>();

        Strafing = Animator.StringToHash("isStrafing");
        Speed = Animator.StringToHash("Speed");
        LookMoveAngle = Animator.StringToHash("Look Move Angle");

        spine = transform.GetChild(0).GetChild(0).GetChild(5).GetChild(0);
        neck = transform.GetChild(0).GetChild(0).GetChild(5).GetChild(0).GetChild(0).GetChild(0);

    }

    // Update is called once per frame
    void Update()
    {
        movementDirection = (transform.position - previousFramePostion).normalized;

        lookDir = transform.forward;

        //Debug.Log("Movement Direction: " + movementDirection + "\nLook Direction: " + lookDir);

        float angle = Vector3.Angle(lookDir, movementDirection);

        Vector3 cross = Vector3.Cross(lookDir, movementDirection);

        if (cross.y < 0)
            angle = -angle;

        //Debug.Log(angle);

        Debug.DrawRay(transform.position, lookDir * 100 ,Color.red);

        Debug.DrawRay(transform.position, movementDirection * 100, Color.blue);


        angle += 180;

        if(angle > 0)
        {
            angle = angle/360;
        }

        angle -= .5f;

        if(angle > .6 || angle < .4)
        {
            isStrafing = true;
        }
        else
        {
            isStrafing = false;
        }

        

        float speed = (transform.position - previousFramePostion).magnitude / Time.deltaTime / 12;

        animator.SetBool(Strafing, isStrafing);
        animator.SetFloat(Speed, speed);
        animator.SetFloat(LookMoveAngle, angle);

        

        previousFramePostion = transform.position;
    }

    private void OnAnimatorIK(int layerIndex)
    {
        
    }

    private void LateUpdate()
    {
        if (aimc.lookAt)
        {

            if(Vector3.Angle(transform.forward, aimc.enemy.transform.position) < 70)
            {
                Quaternion playerDirection = Quaternion.LookRotation((aimc.enemy.transform.position - transform.position).normalized, Vector3.up);
                neck.rotation = playerDirection;
                Debug.DrawLine(neck.position, (aimc.enemy.transform.position - neck.position).normalized * 100, Color.black);
                Debug.Log((aimc.enemy.transform.position - neck.position).normalized);
            }
            else if(Vector3.Angle(transform.forward, aimc.enemy.transform.position) < 100)
            {
                Quaternion playerDirection = Quaternion.LookRotation((aimc.enemy.transform.position - transform.position).normalized, Vector3.up);
                spine.rotation = playerDirection;
                Debug.DrawLine(spine.position, (aimc.enemy.transform.position - spine.position).normalized * 100, Color.black);
                Debug.Log((aimc.enemy.transform.position - spine.position).normalized);

                playerDirection = Quaternion.LookRotation((aimc.enemy.transform.position - transform.position).normalized, Vector3.up);
                neck.rotation = playerDirection;
                Debug.DrawLine(neck.position, (aimc.enemy.transform.position - neck.position).normalized * 100, Color.black);
                Debug.Log((aimc.enemy.transform.position - neck.position).normalized);
            }

            
        }
    }

}
