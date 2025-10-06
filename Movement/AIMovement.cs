using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class AIMovement : MonoBehaviour
{
    protected UnityEngine.AI.NavMeshAgent agent;
    protected Rigidbody2D rb;
    protected Collider2D collider;
    protected float moveSpeed = 0f;
    protected float wanderSpeed = 0f;
    protected GameObject target;
    protected Vector2 startPosition;

    [Header("Wander Variables")]
    [SerializeField] protected Vector2 wanderIdleTime = new Vector2(1f, 2f);
    [SerializeField] protected Vector2 wanderIdleFrequency = new Vector2(3f, 5f);
    [SerializeField] protected float wanderIdleChangesDirection = 0.25f;
    protected float wanderTimeLeft = 0f;
    protected WanderState currentMovement = WanderState.Wandering;
    protected float wanderIdleTimeLeft = 0f;
    protected enum WanderState
    {
        Wandering,
        WanderIdling
    }
    private void Start()
    {
        InitializeComponents();
    }

    protected virtual void InitializeComponents()
    {
        collider = GetComponent<Collider2D>();
        rb = GetComponent<Rigidbody2D>();
        agent = GetComponent<UnityEngine.AI.NavMeshAgent>();

        wanderIdleTimeLeft = Random.Range(wanderIdleTime.x, wanderIdleTime.y);
        wanderTimeLeft = Random.Range(wanderIdleFrequency.x, wanderIdleFrequency.y);

        if (agent != null)
        {
            // agent.speed = 0f;
            agent.updateRotation = false;       // Disables Y-axis rotation
            agent.updateUpAxis = false;        // Disables X/Z-axis rotation (keeps object upright)
        }

        startPosition = new Vector2(transform.position.x, transform.position.y);
    }

    public void InitializeMovement(float newMoveSpeed, float newWanderSpeed)
    {
        // Debug.Log("Initialized");
        moveSpeed = newMoveSpeed;
        wanderSpeed = newWanderSpeed;
    }

    public virtual void Move(bool canMove)
    {
        // Base implementation can be empty or provide default behavior
    }

    public virtual void ReceiveTarget(GameObject newTarget)
    {
        // Base implementation can be empty or provide default behavior
    }

    public virtual Vector3 GetTargetPos()
    {
        if (agent != null && agent.destination != null)
        {
            return agent.destination;
        }
        else
        {
            return Vector3.zero;
        }
    }

}
