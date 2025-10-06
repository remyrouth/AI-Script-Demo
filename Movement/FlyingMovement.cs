using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FlyingMovement : AIMovement
{
    [SerializeField] private Vector3 wanderPoint;
    [SerializeField] float wanderRadius = 5f;

    void Start()
    {
        base.InitializeComponents();
    }

    public override void Move(bool canMove)
    {
        Debug.Log("Flying movement active");
        // Vector3 selfPos = transform.position;
        // agent.SetDestination(selfPos);
        if (canMove)
        {
            agent.SetDestination(wanderPoint);
            agent.speed = wanderSpeed;
            UpdateWanderPoint();
        }
        else
        {
            Vector3 selfPos = transform.position;
            agent.SetDestination(selfPos);
        }

    }


    public override void ReceiveTarget(GameObject newTarget)
    {
        target = newTarget;
    }


    private void Wander()
    {

    }

    private void UpdateWanderPoint()
    {
        Vector2 selfPos = new Vector2(transform.position.x, transform.position.y);
        Vector2 wanderPointV2 = new Vector2(wanderPoint.x, wanderPoint.y);
        float distance = Vector3.Distance(selfPos, wanderPointV2);
        float targetDistance = agent.stoppingDistance;
        // Debug.Log("stoppingDistance: " + targetDistance.ToString());

        if (distance <= targetDistance || wanderPoint == new Vector3(0f, 0f, 0f))
        {
            // Debug.Log("Choosing new bc of distance: " + distance.ToString());
            wanderPoint = ChooseWanderPoint();
        }
    }
    
    /// <summary>
    /// Gets a random reachable point on the NavMesh within a given radius from the origin.
    /// </summary>
    public Vector3 ChooseWanderPoint()
    {
        for (int i = 0; i < 30; i++) // Try up to 30 times
        {
            // Pick a random direction inside a sphere
            Vector3 randomDirection = Random.insideUnitSphere * wanderRadius;
            randomDirection += (Vector3)startPosition;

            // Check if the point is on the NavMesh
            if (UnityEngine.AI.NavMesh.SamplePosition(randomDirection, out UnityEngine.AI.NavMeshHit navHit, 2f, UnityEngine.AI.NavMesh.AllAreas))
            {
                // Debug.Log("on nav mesh");
                // Check if agent can reach it
                UnityEngine.AI.NavMeshPath path = new UnityEngine.AI.NavMeshPath();
                if (base.agent.CalculatePath(navHit.position, path) && path.status == UnityEngine.AI.NavMeshPathStatus.PathComplete)
                {
                    // Debug.Log("Returned + iterartion: " + i.ToString());
                    // Debug.Log("found pos");
                    return navHit.position; // Valid, reachable point
                }
            }
        }

        // If no point found, return original position
        return (Vector3)startPosition;
    }

}
