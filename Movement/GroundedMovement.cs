using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class GroundedMovement : AIMovement
{
    private bool moveWanderRight = true;
    private float raycastDistance = 0.5f;
    private float groundRaycastDistance = 0.02f;
    public override void Move(bool canMove)
    {
        // Debug.Log("Moving");
        bool hasTarget = (target != null);
        bool isGrounded = CheckGroundedState();
        bool canUseAgent = canMove && isGrounded && agent != null;
        agent.enabled = canUseAgent;
        if (!canUseAgent)
        {
            // Debug.Log("disabled agent");
            return;
        }
        if (target == null)
        {
            // Debug.Log("Wandering");
            Wander();
        }
        else if (target != null)
        {
            // Debug.Log("Chasing");
            ChaseTarget();
        }
    }


    private bool CheckGroundedState()
    {
        Bounds bounds = collider.bounds;
        Vector3 leftCornerPos = new Vector3(bounds.min.x, bounds.min.y, bounds.center.z); // leftBottom
        bool leftCorner = Physics2D.Raycast(leftCornerPos, Vector3.down, groundRaycastDistance, LayerMask.GetMask("MainGround"));
        Vector3 rightCornerPos = new Vector3(bounds.max.x, bounds.min.y, bounds.center.z); // rightBottom
        bool rightCorner = Physics2D.Raycast(rightCornerPos, Vector3.down, groundRaycastDistance, LayerMask.GetMask("MainGround"));

        return leftCorner || rightCorner;
    }

    public override void ReceiveTarget(GameObject newTarget)
    {
        target = newTarget;
    }

    private void ChaseTarget()
    {
        // if (moveSpeed == 0f)
        // {
        //     agent.speed = moveSpeed;
        //     return;
        // }
        // Debug.Log("pre alter: " + agent.speed);
        Vector3 targetPos = target.transform.position;
        Vector3 selfPos = transform.position;
        agent.speed = moveSpeed;

        // Debug.Log("post alter: " + agent.speed);
        
        // Determine movement direction
        bool moveRight = selfPos.x < targetPos.x;
        float moveAmount = moveRight ? 2f : -2f;
        
        // Only move if path is clear
        if (!MustChangeDirection(moveRight))
        {
            selfPos.x += moveAmount;
        }

        agent.SetDestination(selfPos);
    }

    private void Wander()
    {

        if (MustChangeDirection(moveWanderRight))
        {
            agent.velocity = Vector3.zero;
            // Debug.Log("switched");
            moveWanderRight = !moveWanderRight; // Change direction
        }
        agent.speed = wanderSpeed;

        if (currentMovement == WanderState.Wandering)
        {
            // wandering
            ChangeTargetDestination(moveWanderRight);
            wanderTimeLeft -= Time.deltaTime;
            wanderTimeLeft = Mathf.Max(0f, wanderTimeLeft);
            if (wanderTimeLeft == 0f)
            {
                currentMovement = WanderState.WanderIdling;
                wanderIdleTimeLeft = Random.Range(wanderIdleTime.x, wanderIdleTime.y);

                float randFloat = Random.Range(0f, 1f);
                bool switchWanderSide = (randFloat <= wanderIdleChangesDirection);
                if (switchWanderSide)
                {
                    moveWanderRight = !moveWanderRight; // change direction here too
                }
            }
        }
        else
        {
            // wander idling
            Vector3 selfPos = transform.position;
            agent.SetDestination(selfPos);
            wanderIdleTimeLeft -= Time.deltaTime;
            wanderIdleTimeLeft = Mathf.Max(0f, wanderIdleTimeLeft);
            if (wanderIdleTimeLeft == 0f)
            {
                currentMovement = WanderState.Wandering;
                wanderTimeLeft = Random.Range(wanderIdleFrequency.x, wanderIdleFrequency.y);
            }
        }

    }

    private void ChangeTargetDestination(bool rightSide)
    {
        Vector3 targetPos = gameObject.transform.position;
        if (rightSide)
        {
            targetPos.x += 2f;
        }
        else
        {
            targetPos.x -= 2f;
        }
        agent.SetDestination(targetPos);
    }

    // tells us if we have ground or an obstacle in the direction we're moving in
    private bool MustChangeDirection(bool rightSide)
    {
        Bounds bounds = collider.bounds;
        Vector3 colliderCorner = new Vector3(bounds.min.x, bounds.min.y, bounds.center.z); // leftBottom
        Vector3 sideDirection = Vector2.left;
        Vector3 currentPos = transform.position;
        Vector3 sidePosition = new Vector3(bounds.min.x, currentPos.y, currentPos.z); // left side
        if (rightSide)
        {
            colliderCorner = new Vector2(bounds.max.x, bounds.min.y); // rightBottom
            sidePosition = new Vector2(bounds.max.x, currentPos.y); // right side
            sideDirection = Vector2.right;
        }

        // Raycast down to check if there's ground below
        float groundCheckDistance = groundRaycastDistance;
        bool hasGround = Physics2D.Raycast(colliderCorner, Vector3.down, groundCheckDistance, LayerMask.GetMask("MainGround"));

        // Raycast horizontally to check for obstacles
        float obstacleCheckDistance = raycastDistance;
        bool hasObstacle = Physics2D.Raycast(sidePosition, sideDirection, obstacleCheckDistance, LayerMask.GetMask("MainGround"));

        // Can wander if there's ground ahead and no obstacles
        // Debug.Log("grounded: " + hasGround + "       obstacle: " + hasObstacle);
        return !hasGround || hasObstacle;
    }
    
    private void OnDrawGizmosSelected()
    {
        if (collider == null) return;

        Bounds bounds = collider.bounds;
        Vector2 currentPos = transform.position;

        // Draw left side checks
        Vector2 leftBottomCorner = new Vector2(bounds.min.x, bounds.min.y);
        Vector2 leftSidePosition = new Vector2(bounds.min.x, currentPos.y);

        // Ground check (down from bottom corner)
        Gizmos.color = Color.blue;
        Gizmos.DrawRay(leftBottomCorner, Vector2.down * 0.2f);

        // Obstacle check (horizontal from side at current height)
        Gizmos.color = Color.magenta;
        Gizmos.DrawRay(leftSidePosition, Vector2.left * 0.5f);

        // Draw right side checks
        Vector2 rightBottomCorner = new Vector2(bounds.max.x, bounds.min.y);
        Vector2 rightSidePosition = new Vector2(bounds.max.x, currentPos.y);

        // Ground check (down from bottom corner)
        Gizmos.color = Color.blue;
        Gizmos.DrawRay(rightBottomCorner, Vector2.down * 0.2f);

        // Obstacle check (horizontal from side at current height)
        Gizmos.color = Color.magenta;
        Gizmos.DrawRay(rightSidePosition, Vector2.right * 0.5f);
    }
}
