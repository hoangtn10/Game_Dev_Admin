using System.Collections;
using System.Collections.Generic;
using UnityEngine;
//Note: collision detection is handled in the Controller 
//Note: for an object to have dectiable collision in must be apart of the collision LayerMask. Whatever layer Collision Mask is set to detect, will activate collision.



public class Controller2D : RaycastController {
    float maxClimbAngle = 80;
    float maxDescendAngle = 75;

    public CollisionInfo collisions;

    public override void Start()
    {
        base.Start();
        collisions.faceDir = 1;
    }

    //Check collisions then move the object
    public void Move(Vector3 velocity, bool standingOnPlatform = false)
    {
        UpdateRaycastOrigins(); //Update Ray position
        collisions.Reset();     //Reset all collision flags
        collisions.velocityOld = velocity;

        //Set diriction charecter is facing
        if(velocity.x != 0)
        {
            collisions.faceDir = (int) Mathf.Sign(velocity.x);
        }
        if (velocity.y < 0)
        {
            DescendSlope(ref velocity);
        }
        //Allway check hotizontal collision for wall sliding
        HorizontalCollisions(ref velocity);
        if (velocity.y != 0)
        {
            VerticalCollisions(ref velocity);
        }
        transform.Translate(velocity);

        if (standingOnPlatform)
        {
            collisions.below = true;
        }
    }

    //Method for checking if object colides on the X-axis
    void HorizontalCollisions(ref Vector3 velocity)
    {
        float directionX = collisions.faceDir;
        float rayLength = Mathf.Abs(velocity.x) + skinWidth; //add skinWidth because you are inside

        if(Mathf.Abs(velocity.x) < skinWidth)
        {
            rayLength = 2 * skinWidth;
        }


        for (int i = 0; i < horizontalRayCount; i++)
        {
            Vector2 rayOrigin = (directionX == -1) ? raycastOrigins.bottomLeft : raycastOrigins.bottomRight; //-left Origin = left, +right Origin = right
            rayOrigin += Vector2.up * (horizontalRaySpacing * i);//Get the ray vector
            RaycastHit2D hit = Physics2D.Raycast(rayOrigin, Vector2.right * directionX, rayLength, collisionMask); //Decting a hit using raycast : Raycast(rayposition,raydirection,raylength,layerMask)

            //Draw Raycasts down for debug purposes
            Debug.DrawRay(rayOrigin, Vector2.right * directionX * rayLength, Color.blue);

            //Logic for hit detection
            if (hit)
            {
                //Don't calc if inside
                if (hit.distance == 0)
                {
                    continue;
                }
                //Slope movement and detection
                float slopeAngle = Vector2.Angle(hit.normal, Vector2.up);

                if(i==0 && slopeAngle <= maxClimbAngle)
                {
                    //Maintaning velocity when transtioning from descending slope to vertical slope
                    if (collisions.descendingSlope)
                    {
                        collisions.descendingSlope = false;
                        velocity = collisions.velocityOld;
                    }
                    //Push on the slope when not on slope
                    float distanceToSlopeStart = 0;
                    if(slopeAngle != collisions.slopeAngleOld)
                    {
                        distanceToSlopeStart = hit.distance - skinWidth;
                        velocity.x -= distanceToSlopeStart * directionX;
                    }
                    ClimbSlope(ref velocity, slopeAngle);
                    velocity.x += distanceToSlopeStart * directionX;
                }

                //This get excuted when not on slope
                if (!collisions.climbingSlope || slopeAngle > maxClimbAngle)
                {
                    //Update velocity in the y direction
                    velocity.x = (hit.distance - skinWidth) * directionX;
                    //Change rayLength so it can't hit on lower colliable object
                    rayLength = hit.distance;

                    //Handle sidecollision when on a slope
                    if (collisions.climbingSlope)
                    {
                        velocity.y = Mathf.Tan(collisions.slopeAngle * Mathf.Deg2Rad) * Mathf.Abs(velocity.x);
                    }

                    //assign a left or right collision flag
                    collisions.left = directionX == -1;
                    collisions.right = directionX == 1;
                }
            }
        }
    }

    //Method for checking if object colides on the Y-axis
    void VerticalCollisions(ref Vector3 velocity)
    {
        float directionY = Mathf.Sign(velocity.y); //+ means up -means down
        float rayLength = Mathf.Abs(velocity.y) + skinWidth; //add skinWidth because you are inside

        
        for (int i = 0; i < verticalRayCount; i++)
        {
            Vector2 rayOrigin = (directionY == -1) ? raycastOrigins.bottomLeft : raycastOrigins.topLeft; //down Origin = bottom, up Origin = top
            rayOrigin += Vector2.right * (verticalRaySpacing * i + velocity.x);//Get the ray vector
            RaycastHit2D hit = Physics2D.Raycast(rayOrigin, Vector2.up * directionY, rayLength,collisionMask); //Decting a hit using raycast : Raycast(rayposition,raydirection,raylength,layerMask)

            //Draw Raycasts down for debug purposes
            Debug.DrawRay(rayOrigin,  Vector2.up * directionY * rayLength, Color.blue);

            //Logic for hit detection
            if (hit)
            {
                //Update velocity in the y direction
                velocity.y = (hit.distance - skinWidth) * directionY;
                //Change rayLength so it can't hit on lower colliable object
                rayLength = hit.distance;
                
                //Handle topCollisions when climbing a slope
                if (collisions.climbingSlope)
                {
                    velocity.x = velocity.y / Mathf.Tan(collisions.slopeAngle * Mathf.Deg2Rad) * Mathf.Sign(velocity.x);
                }

                //assign a above or below collision flag
                collisions.below = directionY == -1;
                collisions.above = directionY == 1;
            }
        }

        //Smooth slope to slope transitions
        if (collisions.climbingSlope)
        {
            float directionX = Mathf.Sign(velocity.x);
            rayLength = Mathf.Abs(velocity.x) + skinWidth;
            Vector2 rayOrigin = ((directionX == -1) ? raycastOrigins.bottomLeft : raycastOrigins.bottomRight) + Vector2.up * velocity.y;
            RaycastHit2D hit = Physics2D.Raycast(rayOrigin, Vector2.right * directionX, rayLength, collisionMask);

            if (hit)
            {
                float slopeAngle = Vector2.Angle(hit.normal, Vector2.up);
                if (slopeAngle != collisions.slopeAngle)
                {
                    velocity.x = (hit.distance - skinWidth) * directionX;
                    collisions.slopeAngle = slopeAngle;
                }
            }
        }
    }

    //Calc speed on a slope
    void ClimbSlope(ref Vector3 velocity, float slopeAngle)
    {
        float moveDistance = Mathf.Abs(velocity.x);
        float climbVelocityY = Mathf.Sin(slopeAngle * Mathf.Deg2Rad) * moveDistance;

        //check if you are jumping if not climb the slope
        if (velocity.y <= climbVelocityY)
        {
            velocity.y = climbVelocityY;
            velocity.x = Mathf.Cos(slopeAngle * Mathf.Deg2Rad) * moveDistance * Mathf.Sign(velocity.x);
            collisions.below = true;
            collisions.climbingSlope = true;
            collisions.slopeAngle = slopeAngle;
        }
    }

    //Method for Descending down a slope
    void DescendSlope(ref Vector3 velocity)
    {
        float directionX = Mathf.Sign(velocity.x);
        Vector2 rayOrigin = (directionX == -1) ? raycastOrigins.bottomRight : raycastOrigins.bottomLeft;
        RaycastHit2D hit = Physics2D.Raycast(rayOrigin, -Vector2.up, Mathf.Infinity, collisionMask);

        if (hit)
        {
            float slopeAngle = Vector2.Angle(hit.normal, Vector2.up);
            if (slopeAngle != 0 && slopeAngle <= maxDescendAngle)
            {
                if (Mathf.Sign(hit.normal.x) == directionX)
                {
                    if (hit.distance - skinWidth <= Mathf.Tan(slopeAngle * Mathf.Deg2Rad) * Mathf.Abs(velocity.x))
                    {
                        float moveDistance = Mathf.Abs(velocity.x);
                        float descendVelocityY = Mathf.Sin(slopeAngle * Mathf.Deg2Rad) * moveDistance;
                        velocity.x = Mathf.Cos(slopeAngle * Mathf.Deg2Rad) * moveDistance * Mathf.Sign(velocity.x);
                        velocity.y -= descendVelocityY;

                        collisions.slopeAngle = slopeAngle;
                        collisions.descendingSlope = true;
                        collisions.below = true;
                    }
                }
            }
        }
    }

    //Gives info on where collision occured
    public struct CollisionInfo
    {
        public bool above, below;
        public bool left, right;

        public bool climbingSlope;
        public bool descendingSlope;
        public float slopeAngle, slopeAngleOld;
        public Vector3 velocityOld;
        public int faceDir; //1 is right and -1 is left

        public void Reset()
        {
            above = below = false;
            left = right = false;
            climbingSlope = false;
            descendingSlope = false;
            slopeAngleOld = slopeAngle;
            slopeAngle = 0;
        }

    } 

}
