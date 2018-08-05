using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class FallingPlatform : RaycastController{

    public float gravityScale = 2;
    public CollisionDetectionMode2D collisionDetection = CollisionDetectionMode2D.Continuous;
    public RigidbodyType2D rigidbodyType = RigidbodyType2D.Kinematic;
    public LayerMask passengerMask;

    Rigidbody2D rb;
    bool hasBeenHit;

    // Use this for initialization
    public override void Start()
    {
        base.Start();
        rb = GetComponent<Rigidbody2D>();
        rb.gravityScale = gravityScale;
        rb.collisionDetectionMode = collisionDetection;
        rb.bodyType = rigidbodyType;
        hasBeenHit = false;
    }

    void Update()
    {
        UpdateRaycastOrigins();

        float rayLength = 2 * skinWidth;

        for (int i = 0; i < verticalRayCount; i++)
        {
            Vector2 rayOrigin = raycastOrigins.topLeft;
            rayOrigin += Vector2.right * (verticalRaySpacing * i);
            RaycastHit2D hit = Physics2D.Raycast(rayOrigin, Vector2.up, rayLength, passengerMask);
            Debug.DrawRay(rayOrigin, Vector2.up, Color.green);

            //Push passenger on to platform
            if (hit && hasBeenHit==false)
            {
                Debug.Log("Hit detected");
                hasBeenHit = true;
                PlatformManager.Instance.StartCoroutine("SpawnPlatform", new Vector3(transform.position.x, transform.position.y,transform.position.z));
                Invoke("DropPlatform", 0.75f);
                Destroy(gameObject, 2f);
            }
        }
    }

    void DropPlatform()
    {
        rb.isKinematic = false;
    }
}
