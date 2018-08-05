using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(BoxCollider2D))]
public class RaycastController : MonoBehaviour {

    public LayerMask collisionMask;

    public const float skinWidth = .015f;
    public int horizontalRayCount = 4; //How many rays shot left and right
    public int verticalRayCount = 4; //How many rays shot up and down

    //Spacing between rays
    [HideInInspector] public float horizontalRaySpacing;
    [HideInInspector] public float verticalRaySpacing;

    [HideInInspector] public BoxCollider2D collider;
    public RaycastOrigins raycastOrigins;

    //It is virtual so inheritied class can overide this and call the base function
    public virtual void Start()
    {
        collider = GetComponent<BoxCollider2D>();
        CalculateRaySpacing();

    }

    //Method for updating the corner values. Get the bounds from the collider class
    public void UpdateRaycastOrigins()
    {
        Bounds bounds = collider.bounds;
        bounds.Expand(skinWidth * -2); //mulitply by -2 shrinks the skin width so it is inside the collider

        raycastOrigins.bottomLeft = new Vector2(bounds.min.x, bounds.min.y);
        raycastOrigins.bottomRight = new Vector2(bounds.max.x, bounds.min.y);
        raycastOrigins.topLeft = new Vector2(bounds.min.x, bounds.max.y);
        raycastOrigins.topRight = new Vector2(bounds.max.x, bounds.max.y);
    }

    //Used to calulate even ray spacing based on number of rays you want size of your collider. Math no one wants to do
    public void CalculateRaySpacing()
    {
        //Get skin bounds
        Bounds bounds = collider.bounds;
        bounds.Expand(skinWidth * -2);

        //Need have at least 2 hori and vert rays
        horizontalRayCount = Mathf.Clamp(horizontalRayCount, 2, int.MaxValue);
        verticalRayCount = Mathf.Clamp(verticalRayCount, 2, int.MaxValue);

        //Do calc
        horizontalRaySpacing = bounds.size.y / (horizontalRayCount - 1);
        verticalRaySpacing = bounds.size.x / (verticalRayCount - 1);
    }

    //Used to easily get corner values
    public struct RaycastOrigins
    {
        public Vector2 topLeft, topRight;
        public Vector2 bottomLeft, bottomRight;

    }
    //      TL----TR    Diagram for the Dyslexic
    //      |      |
    //      |      |
    //      BL----BR
}
