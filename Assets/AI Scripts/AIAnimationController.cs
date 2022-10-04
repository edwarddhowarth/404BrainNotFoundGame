using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//Controls the animations for the AI and for rotations such as rotating spine/head so they are looking at the player/object


// These could be useful
// https://docs.unity3d.com/ScriptReference/Animator.SetBoneLocalRotation.html Needs to be in LateUpdate
// https://forum.unity.com/threads/animator-setbonelocalrotation-clarification-needed.383680/

// http://answers.unity.com/answers/181890/view.html for detirmining angle
[RequireComponent(typeof(Animator))]
public class AIAnimationController : MonoBehaviour
{
    Vector3 movementDirection;
    Vector3 lookDir;

    Vector3 previousFramePostion;

    Vector3 preUpdateForward;
    Vector3 preUpdatePosition;

    Animator animator;
    AIMovementController aimc;
    AIStateController aisc;

    bool isStrafing;

    

    int Strafing;
    int Speed;
    int LookMoveAngle;
    int Attacking;

    bool attack;

    float lookTime;
    Quaternion currentHeadRotation;
    Quaternion currentSpineRotation;
    bool lookAtStart = false;
    bool returningToCenterHead = false;

    public bool animate = true;
    public bool animateLookAt;
    Transform spine;
    Transform neck;

    // Start is called before the first frame update
    void Start()
    {

        previousFramePostion = transform.position;

        animator = GetComponent<Animator>();
        aimc = GetComponent<AIMovementController>();
        aisc = GetComponent<AIStateController>();

        Strafing = Animator.StringToHash("isStrafing");
        Speed = Animator.StringToHash("Speed");
        LookMoveAngle = Animator.StringToHash("Look Move Angle");
        Attacking = Animator.StringToHash("Attacking");

        // check if the neck and spine objects exist on the AI object
        spine = transform.GetChild(0).GetChild(0).GetChild(5).GetChild(0);
        neck = transform.GetChild(0).GetChild(0).GetChild(5).GetChild(0).GetChild(0).GetChild(0);

        /*
        if (transform.childCount > 1)
        {
            if (transform != null &&
            transform.GetChild(0) != null &&
            transform.GetChild(0).GetChild(0) != null &&
            transform.GetChild(0).GetChild(0).GetChild(5) != null &&
            transform.GetChild(0).GetChild(0).GetChild(5).GetChild(0) != null &&
            transform.GetChild(0).GetChild(0).GetChild(5).GetChild(0).GetChild(0) != null &&
            transform.GetChild(0).GetChild(0).GetChild(5).GetChild(0).GetChild(0).GetChild(0) != null)
            {
                
            }
        }
        else
        {
            spine = null;
            neck = null;
        }
        */


    }

    // Update is called once per frame
    void Update()
    {
        movementDirection = (transform.position - previousFramePostion).normalized;

        lookDir = transform.forward;
        preUpdateForward = lookDir;
        preUpdatePosition = transform.position;

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
        animator.SetBool(Attacking, aisc.InCombatAnimation);

        previousFramePostion = transform.position;
    }

   
    

    private void LateUpdate()
    {
        AnimateLookAt();

    }

    private void AnimateLookAt()
    {
        if (spine && neck && aimc.lookAt)
        {
            if (lookAtStart == false || returningToCenterHead == true) //if we are beginning to look at the object
            {
                currentHeadRotation = neck.rotation; //Get the neck's starting rotation which we will keep updating each frame till look at stops
                currentSpineRotation = spine.rotation;
                lookAtStart = true; //when this is false, 
                returningToCenterHead = false;
                lookTime = 0;
            }


            //aimc.agent.updateRotation = false;
            //Vector3 offset = (transform.rotation.eulerAngles - lookDir).normalized;

            Vector3 targetDirection = (aimc.enemy.transform.position - transform.position).normalized;
            Debug.DrawLine(preUpdatePosition, aimc.enemy.transform.position, Color.green);
            Debug.DrawLine(preUpdatePosition, lookDir * 200, Color.cyan);
            Debug.Log("head angle: " + Vector3.Angle(transform.forward, targetDirection));



            if (Vector3.Angle(lookDir, targetDirection) < 45)
            {
                //return spine to center
                Quaternion centerRotation = Quaternion.Lerp(currentSpineRotation, Quaternion.LookRotation(lookDir, Vector3.up), 6f * Time.deltaTime);
                spine.rotation = centerRotation;
                currentSpineRotation = centerRotation;

                //Turn head
                Quaternion playerDirection = Quaternion.LookRotation(targetDirection, Vector3.up);
                Quaternion newRotation = Quaternion.Lerp(currentHeadRotation, playerDirection, 6f * Time.deltaTime);
                Debug.Log("new rotation: " + newRotation);
                neck.rotation = newRotation;
                currentHeadRotation = newRotation;
                Debug.DrawLine(preUpdatePosition, aimc.enemy.transform.position, Color.black);
                Debug.Log((aimc.enemy.transform.position - neck.position).normalized);
            }
            else if (Vector3.Angle(lookDir, targetDirection) < 90)
            {
                //turn body
                Quaternion playerDirection = Quaternion.LookRotation(targetDirection, Vector3.up);
                Quaternion newRotation = Quaternion.Lerp(currentSpineRotation, playerDirection, 6f * Time.deltaTime);
                Debug.Log("new rotation: " + newRotation);
                spine.rotation = newRotation;
                currentSpineRotation = newRotation;
                Debug.DrawLine(preUpdatePosition, aimc.enemy.transform.position, Color.black);
                Debug.Log((aimc.enemy.transform.position - neck.position).normalized);

                //keep head rotation
                playerDirection = Quaternion.LookRotation(targetDirection, Vector3.up);
                newRotation = Quaternion.Lerp(currentHeadRotation, playerDirection, 6f * Time.deltaTime);
                Debug.Log("new rotation: " + newRotation);
                neck.rotation = newRotation;
                currentHeadRotation = newRotation;

            }
            else
            {

                Quaternion centerRotation = Quaternion.Lerp(currentHeadRotation, Quaternion.LookRotation(lookDir, Vector3.up), 6f * Time.deltaTime);
                /*
                neck.rotation = centerRotation;
                */
                currentHeadRotation = centerRotation;


                centerRotation = Quaternion.Lerp(currentSpineRotation, Quaternion.LookRotation(lookDir, Vector3.up), 6f * Time.deltaTime);
                spine.rotation = centerRotation;
                currentSpineRotation = centerRotation;

                //neck.rotation = currentHeadRotation;
                //spine.rotation = currentSpineRotation;
            }
        }
        else if (animate)
        {

            if (lookAtStart == true && lookTime < 2f)
            {
                lookTime += Time.deltaTime;
                Quaternion newRotation = Quaternion.Lerp(currentHeadRotation, Quaternion.LookRotation(lookDir, Vector3.up), 6f * Time.deltaTime);
                neck.rotation = newRotation;
                currentHeadRotation = newRotation;

                newRotation = Quaternion.Lerp(currentSpineRotation, Quaternion.LookRotation(lookDir, Vector3.up), 6f * Time.deltaTime);
                spine.rotation = newRotation;
                currentSpineRotation = newRotation;




                Debug.Log("Turning back to forward");
                returningToCenterHead = true;

            }
            else
            {
                lookAtStart = false;
                lookTime = 0f;
                returningToCenterHead = false;
            }




        }

        if(spine && aisc.InCombatAnimation)
        {
            Vector3 targetDirection = (aimc.enemy.transform.position - transform.position).normalized;
            Quaternion playerDirection = Quaternion.LookRotation(targetDirection, Vector3.up);
            if (Vector3.Angle(aimc.enemy.transform.position - transform.position, transform.forward) > 15)
            {
                transform.rotation = Quaternion.RotateTowards(transform.rotation, playerDirection, 3f);
            }

            

            
            Quaternion newRotation = Quaternion.Lerp(currentSpineRotation, playerDirection, 6f * Time.deltaTime);
            Debug.Log("new rotation: " + newRotation);
            spine.rotation = newRotation;
            currentSpineRotation = newRotation;
            Debug.DrawLine(preUpdatePosition, aimc.enemy.transform.position, Color.black);
            Debug.Log((aimc.enemy.transform.position - neck.position).normalized);
        }

    }







    /*
   private void OnAnimatorIK(int layerIndex)
   {
       if (animateLookAt && aimc.lookAt)
       {
           if (lookAtStart == false)
           {
               currentHeadRotation = neck.rotation;
               lookAtStart = true;
           }


           //aimc.agent.updateRotation = false;
           //Vector3 offset = (transform.rotation.eulerAngles - lookDir).normalized;

           Vector3 targetDirection = (aimc.enemy.transform.position - transform.position).normalized;
           Vector3 targetDirectionNeck = (aimc.enemy.transform.position - transform.position).normalized;
           Vector3 targetDirectionSpine = (aimc.enemy.transform.position - spine.position).normalized;
           Debug.DrawLine(preUpdatePosition, aimc.enemy.transform.position, Color.green);
           Debug.DrawLine(preUpdatePosition, lookDir * 200, Color.cyan);
           Debug.Log("head angle: " + Vector3.Angle(transform.forward, targetDirection));
           if (Vector3.Angle(lookDir, targetDirection) < 90)
           {
               Quaternion playerDirection = Quaternion.LookRotation(targetDirection, Vector3.up);
               Quaternion newRotation = Quaternion.Lerp(neck.rotation, playerDirection, 10f * Time.deltaTime);

               //animator.SetBoneLocalRotation(HumanBodyBones.Head, playerDirection);
               neck.rotation = playerDirection;
               Debug.Log("Look rotation:" + currentHeadRotation);
               Debug.Log("Look quat:" + playerDirection);
               Debug.Log("Look quat:" + (Quaternion.Inverse(currentHeadRotation) * newRotation));
               Debug.DrawRay(animator.GetBoneTransform(HumanBodyBones.Head).position, targetDirectionNeck * 100f, Color.black);
               currentHeadRotation = newRotation;
           }

           //animator.SetBoneLocalRotation(HumanBodyBones.Head, Quaternion.Inverse(currentHeadRotation));
       }
       else
       {
           lookAtStart = false;
           //neck.rotation = currentHeadRotation;
           //animator.SetBoneLocalRotation(HumanBodyBones.Head, Quaternion.Inverse(currentHeadRotation) * Quaternion.LookRotation(transform.forward, Vector3.up));
       }

   }
   */

}
