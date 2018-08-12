using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//When ever you assume an object has a certain componet it is good practice to require that componment i.e the Controller2D in this object
[RequireComponent (typeof (Controller2D))]
[RequireComponent (typeof(Animator))]
public class Player : MonoBehaviour {
    //More intuitive values control gravity, jump speed, x accelration in air and ground
    public float jumpHeight = 4;
    public float timeToJumpApex = .4f;
    public float accelerationTimeAirborne = .2f;
    public float accelerationTimeGrounded = .1f;
    float moveSpeed = 6;

    //Wall sliding and jumping variables
    public Vector2 wallJumpClimb;
    public Vector2 wallJumpOff;
    public Vector2 wallLeap;

    public float wallSlideSpeedMax = 3;
    public float wallStickTime = .25f;
    float timeToWallUnstick;

    //Gravity,jump, movement variables
    float gravity;
    float jumpVelocity;
    float velocityXSmoothing;
    Vector3 velocity;

    //Animator
    Animator animator;
    

    Controller2D controller;

    void Start()
    {
        controller = GetComponent<Controller2D>();

        //setting gravity and jumpspeed
        gravity = -(2 * jumpHeight) / Mathf.Pow(timeToJumpApex, 2);
        jumpVelocity = Mathf.Abs(gravity) * timeToJumpApex;

        //Get animator
        animator = GetComponent<Animator>();
    }

    private void Update()
    {
        Vector2 input = new Vector2(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical"));
        int wallDirX = (controller.collisions.left) ? -1 : 1;

        //Give the player a x velocity. Currently a componect of 
        //input x * moveSpeed
        //Smooth Dampening
        float targetVelocityX = input.x * moveSpeed;
        velocity.x = Mathf.SmoothDamp(velocity.x, targetVelocityX, ref velocityXSmoothing, (controller.collisions.below) ? accelerationTimeGrounded : accelerationTimeAirborne);

        //Start Wall Sliding Logic
        bool wallSliding = false;
        if ((controller.collisions.left || controller.collisions.right) && !controller.collisions.below && velocity.y < 0)
        {
            wallSliding = true;

            if (velocity.y < -wallSlideSpeedMax)
            {
                velocity.y = -wallSlideSpeedMax;
            }

            if (timeToWallUnstick > 0)
            {
                velocityXSmoothing = 0;
                velocity.x = 0;

                if (input.x != wallDirX && input.x != 0)
                {
                    timeToWallUnstick -= Time.deltaTime;
                }
                else
                {
                    timeToWallUnstick = wallStickTime;
                }
            }
            else
            {
                timeToWallUnstick = wallStickTime;
            }

        }

        //reset y velocity when grounded or a ceiling is hit
        if (controller.collisions.above || controller.collisions.below)
        {
            velocity.y = 0;
        }

        //Can jump when space bar is pressed
        if (Input.GetKeyDown(KeyCode.Space))
        {
            if (wallSliding)
            {
                //climbing wall jump
                if (wallDirX == input.x)
                {
                    velocity.x = -wallDirX * wallJumpClimb.x;
                    velocity.y = wallJumpClimb.y;
                }
                //Wall jump off
                else if (input.x == 0)
                {
                    velocity.x = -wallDirX * wallJumpOff.x;
                    velocity.y = wallJumpOff.y;
                }
                //Wall leap off
                else
                {
                    velocity.x = -wallDirX * wallLeap.x;
                    velocity.y = wallLeap.y;
                }
            }
            //normal jump
            if (controller.collisions.below)
            {
                velocity.y = jumpVelocity;
            }
        }

        //Give the player a y velocity. Currently a component of
            //gravity
        velocity.y += gravity * Time.deltaTime;

        //Moves the player based on velocity and a timestep
        controller.Move(velocity * Time.deltaTime);

        //Update the animation
        SetAnimationState();
    }

    //Facing state of player
    private bool isFacingRight = true;
    //Function used to flip the player sprite
    void Flip()
    {
        isFacingRight = !isFacingRight;

        // Multiply the player's x local scale by -1
        Vector3 theScale = transform.localScale;
        theScale.x *= -1;
        transform.localScale = theScale;
    }

    private void SetAnimationState()
    {
        //Set character in right direction
        if(Input.GetAxisRaw("Horizontal") == 1 && !isFacingRight)
        {
            Flip();
        }
        if (Input.GetAxisRaw("Horizontal") == -1 && isFacingRight)
        {
            Flip();
        }

        //Set animation state
        if (Input.GetAxisRaw("Horizontal") != 0)
            animator.SetBool("isWalking", true);
        else
        {
            animator.SetBool("isWalking", false);
        }
    }
}
