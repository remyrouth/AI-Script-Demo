using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SlimeMovement : AIMovement
{
    [Header("Jump Variables")]
    [SerializeField] private float jumpForce = 10f;
    [SerializeField] private Vector2 jumpDelay = new Vector2(1f, 1.5f);
    [SerializeField] private float horizontalForce = 5f;
    [SerializeField] private LayerMask groundLayer; // assign "MainGround" layer here
    [SerializeField] private float raycastDistance = 0.1f;

    [Header("Bounce Player")]
    [SerializeField] private float playerImpactForce = 20f;

    public float currentTimeTillJump = 0f;
    private bool facingRight = false;
    private bool isGrounded = false;

    public override void Move(bool canMove)
    {
        if (!canMove) return;

        currentTimeTillJump -= Time.deltaTime;
        currentTimeTillJump = Mathf.Max(0, currentTimeTillJump);

        if (currentTimeTillJump == 0f && isGrounded)
        {
            RandomizeJump();
            Jump();
        }
        UpdateGroundedState();
        UpdateFaceDirection();
    }

    protected override void InitializeComponents()
    {
        collider = GetComponent<Collider2D>();
        rb = GetComponent<Rigidbody2D>();
        RandomizeJump();

        // Randomize initial facing
        // facingRight = Random.Range(0f, 1f) > 0.5f;
        facingRight = Random.value > 0.5f;
    }

    private void Jump()
    {
        // Debug.Log("Slime jumped");
        // cleared the existing vertical velocity before jumping, then created my own
        rb.velocity = new Vector2(rb.velocity.x, 0f);

        // I added a diagonal force (north-east or north-west depending on facing)
        Vector2 jumpDirection = new Vector2(facingRight ? 1f : -1f, 1f).normalized;
        rb.AddForce(jumpDirection * jumpForce, ForceMode2D.Impulse);
    }

    private void RandomizeJump()
    {
        currentTimeTillJump = Random.Range(jumpDelay.x, jumpDelay.y);
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        // If slime hits player
        if (collision.collider.CompareTag("Player"))
        {
            // Debug.Log("Player hit!");
            Rigidbody2D playerRB = collision.collider.gameObject.GetComponent<Rigidbody2D>();
            if (playerRB != null)
            {
                //  Debug.Log("player rb HIT and impacted");
                // Get the direction from slime to player
                Vector2 impactDirection = (collision.collider.transform.position - transform.position).normalized;

                // then apply force to the player
                playerRB.AddForce(impactDirection * playerImpactForce, ForceMode2D.Impulse);
            }
        }

        // If slime collides with wall while mid-air, reverse direction
        if (!isGrounded && collision.contacts.Length > 0)
        {
            Vector2 normal = collision.contacts[0].normal;

            // Check if hitting a wall horizontally
            if (Mathf.Abs(normal.x) > 0.5f)
            {
                facingRight = !facingRight;
                // Debug.Log("Slime bounced and reversed direction.");
            }
        }
    }

    private void UpdateFaceDirection()
    {
        // Determine facing direction
        Vector2 direction = facingRight ? Vector2.right : Vector2.left;

        // Cast ray forward
        RaycastHit2D hit = Physics2D.Raycast(transform.position, direction, raycastDistance, groundLayer);

        // If airborne and wall is detected
        if (!isGrounded && hit.collider != null)
        {
            // Flip facing direction
            facingRight = !facingRight;

            // Keep current speed, just invert horizontal direction
            rb.velocity = new Vector2(Mathf.Abs(rb.velocity.x) * (facingRight ? 1 : -1), rb.velocity.y);

            // Debug.Log("Slime bounced off wall, kept speed but reversed direction.");
        }
    }


    private void UpdateGroundedState()
    {
        // Raycast downward
        RaycastHit2D hit = Physics2D.Raycast(transform.position, Vector2.down, raycastDistance, groundLayer);

        isGrounded = hit.collider != null;
    }

    private void OnDrawGizmosSelected()
    {
        // Draw raycast down
        Gizmos.color = Color.red;

        // Length should match your raycast check
        float rayLength = raycastDistance;

        // Draw from the slime’s position downward
        Gizmos.DrawLine(transform.position, transform.position + Vector3.down * rayLength);

        // Draw a small sphere at the end for clarity
        Gizmos.DrawSphere(transform.position + Vector3.down * rayLength, 0.02f);
    }

}
