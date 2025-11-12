using UnityEngine;
using UnityEngine.AI;

public class EnemyAIFSM : MonoBehaviour
{
    // === Component References ===
    public NavMeshAgent agent;

    public Transform playerTarget;
    public Transform patrolCenter;
    public Renderer enemyRenderer;
    public Transform Dungeongen;

    // === Detection Settings ===
    [Header("Detection Settings")]
    public float detectRange = 10f;

    public LayerMask playerLayer; // Layer of the Player object
    public LayerMask obstacleLayer; // Layers that block the raycast (e.g., walls)

    // === Patrol Settings ===
    [Header("Patrol Settings")]
    public float patrolRadius = 15f;

    public float patrolDelay = 3f;

    // === State Management ===
    private enum EnemyState
    { IdlePatrol, FollowPlayer }

    private EnemyState currentState = EnemyState.IdlePatrol;
    private float patrolTimer = 0f;

    // === Visuals (Optional for Debugging) ===
    [Header("Visuals")]
    public Color patrolColor = Color.green;

    public Color followColor = Color.red;

    private void Start()
    {
        // Auto-assign components and initial values
        if (agent == null) agent = GetComponent<NavMeshAgent>();
        if (patrolCenter == null) patrolCenter = transform;
        if (Dungeongen == null) Dungeongen = GameObject.FindWithTag("DungeonGenerator")?.transform;

        // Set initial state and color
        currentState = EnemyState.IdlePatrol;
        patrolTimer = patrolDelay;
        if (enemyRenderer != null) enemyRenderer.material.color = patrolColor;
    }

    private void Update()
    {
        if (Dungeongen.GetComponent<DungeonGenerationScript>().isDone)
        {
            // 1. Always check for player presence
            bool playerDetected = CheckForPlayer();

            // 2. State Transition Logic (FSM)
            switch (currentState)
            {
                case EnemyState.IdlePatrol:
                    if (playerDetected)
                    {
                        // Transition: Patrol -> Follow
                        ChangeState(EnemyState.FollowPlayer);
                    }
                    else
                    {
                        // Stay in Patrol State
                        PatrolUpdate();
                    }
                    break;

                case EnemyState.FollowPlayer:
                    if (!playerDetected)
                    {
                        // Transition: Follow -> Patrol
                        ChangeState(EnemyState.IdlePatrol);
                        // Force immediate patrol movement
                        SetNewPatrolPoint();
                    }
                    else
                    {
                        // Stay in Follow State
                        FollowUpdate();
                    }
                    break;
            }
        }
    }

    // --- State Logic Methods ---

    private void FollowUpdate()
    {
        // The raycast check in CheckForPlayer is the only thing validating the target's position
        agent.SetDestination(playerTarget.position);
    }

    private void PatrolUpdate()
    {
        patrolTimer += Time.deltaTime;

        // Check if agent reached destination AND timer is up
        if (patrolTimer >= patrolDelay && !agent.pathPending && agent.remainingDistance < 0.5f)
        {
            SetNewPatrolPoint();
            patrolTimer = 0f;
        }
    }

    // --- Core Functions ---

    private bool CheckForPlayer()
    {
        if (playerTarget == null) return false;

        Vector3 origin = transform.position;
        Vector3 direction = playerTarget.position - origin;
        float distance = direction.magnitude;

        // First, check if player is within the maximum detection range
        if (distance > detectRange)
        {
            return false;
        }

        RaycastHit hit;

        // Raycast using the detected distance as max distance
        // LayerMask is (Player Layer OR Obstacle Layer) so it will hit either
        if (Physics.Raycast(origin, direction.normalized, out hit, distance, playerLayer | obstacleLayer))
        {
            // Debug Draw: The raycast in the Scene View
            Debug.DrawRay(origin, direction, hit.collider.gameObject == playerTarget.gameObject ? Color.red : Color.yellow);

            // The raycast hit something. Check if that something is the player.
            // If it is, the line of sight is clear.
            return hit.collider.gameObject == playerTarget.gameObject;
        }

        // This case should theoretically only be reached if something goes wrong, but we return false anyway.
        return false;
    }

    private void SetNewPatrolPoint()
    {
        Vector3 newPos = RandomNavSphere(patrolCenter.position, patrolRadius, NavMesh.AllAreas);
        agent.SetDestination(newPos);
    }

    private void ChangeState(EnemyState newState)
    {
        currentState = newState;

        // Apply visual change
        if (enemyRenderer != null)
        {
            enemyRenderer.material.color = (newState == EnemyState.FollowPlayer) ? followColor : patrolColor;
        }
    }

    // --- Helper Function ---

    public static Vector3 RandomNavSphere(Vector3 origin, float dist, int layermask)
    {
        Vector3 randomDirection = UnityEngine.Random.insideUnitSphere * dist;
        randomDirection += origin;

        NavMeshHit navHit;
        NavMesh.SamplePosition(randomDirection, out navHit, dist, layermask);

        return navHit.position;
    }
}