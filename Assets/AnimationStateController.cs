using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AnimationStateController : MonoBehaviour
{
    Animator animator;
    float velocityZ = 0.0f;
    float velocityX = 0.0f;
    public float acceleration = 2.0f;
    public float deceleration = 2.0f;
    public float maxWalkVelocity = 0.5f;
    public float maxRunVelocity = 2.0f;

    //increse performance
    int VelocityZHash;
    int VelocityXHash;


    // Start is called before the first frame update
    void Start()
    {
        //set reference for animator
        animator = GetComponent<Animator>();

        //increse performance
        VelocityZHash = Animator.StringToHash("Velocity Z");
        VelocityXHash = Animator.StringToHash("Velocity X");
    }

    //handles acceleration and deceleration
    void changeVelocity(bool forwardPressed, bool leftPressed, bool rightPressed, float currentMaxVelocity)
    {
        //if player presses forward, increase velocity in z direction
        if (forwardPressed && velocityZ < currentMaxVelocity)
        {
            velocityZ += Time.deltaTime * acceleration;
        }

        if (leftPressed && velocityX > -currentMaxVelocity)
        {
            velocityX -= Time.deltaTime * acceleration;
        }
        if (rightPressed && velocityX < currentMaxVelocity)
        {
            velocityX += Time.deltaTime * acceleration;
        }

        //decrease velocityZ
        if (!forwardPressed && velocityZ > 0.0f)
        {
            velocityZ -= Time.deltaTime * deceleration;
        }
        //increase velocityX if left is not pressed adn velocityX is < 0
        if (!leftPressed && velocityX < 0.0f)
        {
            velocityX += Time.deltaTime * deceleration;
        }

        //decreases velocityX if right is not pressed adn velocityX is > 0
        if (!rightPressed && velocityX > 0.0f)
        {
            velocityX -= Time.deltaTime * deceleration;
        }

    }

    //handles reset Velocity
    void lockOrResetVelocity(bool forwardPressed, bool leftPressed, bool rightPressed, bool runPressed, float currentMaxVelocity)
    {
        //reset VelocityZ
        if (!forwardPressed && velocityZ < 0.0f)
        {
            velocityZ = 0.0f;
        }


        //reset velocityX
        if (!leftPressed && !rightPressed && velocityX != 0 && (velocityX > -0.05f && velocityX < 0.05f))
        {
            velocityX = 0.0f;
        }

        //lock forward
        if (forwardPressed && runPressed && velocityZ > currentMaxVelocity)
        {
            velocityZ = currentMaxVelocity;
        }
        else
        {
            if (forwardPressed && velocityZ > currentMaxVelocity)
            {
                velocityZ -= Time.deltaTime * deceleration;
            }
            else
            {
                if (velocityZ > currentMaxVelocity && velocityZ < (currentMaxVelocity + 0.05f))
                {
                    velocityZ = currentMaxVelocity;
                }
                else
                {
                    if (forwardPressed && velocityZ < currentMaxVelocity && velocityZ > (currentMaxVelocity - 0.05f))
                    {
                        velocityZ = currentMaxVelocity;
                    }
                }
            }
        }

        //lock left
        if (leftPressed && runPressed && velocityX < -currentMaxVelocity)
        {
            velocityX = -currentMaxVelocity;
        }
        else
        {
            if (leftPressed && velocityX < -currentMaxVelocity)
            {
                velocityX -= Time.deltaTime * deceleration;
            }
            else
            {
                if (velocityX < -currentMaxVelocity && velocityX > (-currentMaxVelocity + 0.05f))
                {
                    velocityX = -currentMaxVelocity;
                }
                else
                {
                    if (leftPressed && velocityX > -currentMaxVelocity && velocityX < (-currentMaxVelocity - 0.05f))
                    {
                        velocityX = -currentMaxVelocity;
                    }
                }
            }
            //lock right
            if (rightPressed && runPressed && velocityX > currentMaxVelocity)
            {
                velocityX = currentMaxVelocity;
            }
            else
            {
                if (rightPressed && velocityX > currentMaxVelocity)
                {
                    velocityX -= Time.deltaTime * deceleration;
                }
                else
                {
                    if (velocityX > currentMaxVelocity && velocityX < (currentMaxVelocity + 0.05f))
                    {
                        velocityX = currentMaxVelocity;
                    }
                    else
                    {
                        if (rightPressed && velocityX < currentMaxVelocity && velocityX > (currentMaxVelocity - 0.05f))
                        {
                            velocityX = currentMaxVelocity;
                        }
                    }
                }
            }
        }
    }
    // Update is called once per frame
    void Update()
    {
        //get key input from player
        bool forwardPressed = Input.GetKey(KeyCode.W);
        bool leftPressed = Input.GetKey(KeyCode.A);
        bool rightPressed = Input.GetKey(KeyCode.D);
        bool runPressed = Input.GetKey(KeyCode.LeftShift);

        //set current maxVelocity
        float currentMaxVelocity = runPressed ? maxRunVelocity : maxWalkVelocity;

        //handle changed in velocity
        changeVelocity(forwardPressed, leftPressed, rightPressed, currentMaxVelocity);
        lockOrResetVelocity(forwardPressed, leftPressed, rightPressed, runPressed, currentMaxVelocity);


        //set the parameters to our local variable vaules
        animator.SetFloat(VelocityZHash, velocityZ);
        animator.SetFloat(VelocityXHash, velocityX);



    }
}
                   
