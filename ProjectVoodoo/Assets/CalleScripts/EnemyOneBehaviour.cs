using UnityEngine;
using UnityEngine.AI;
public class EnemyOneBehaviour : MonoBehaviour
{
    public NavMeshAgent agent;
    public Transform pointA;
    public Transform pointB;

    public float detectionRadious = 5f;

    public Transform playerPosition;

    public bool isLockedOnPlayer = false;

    Transform currentTarget;

    void Start()
    {
        currentTarget = pointA;
        agent.SetDestination(currentTarget.position);
    }

    // Update is called once per frame
    void Update()
    {
        float distanceToPlayer = Vector3.Distance(transform.position, playerPosition.position);

        if (distanceToPlayer < detectionRadious)
        {
            isLockedOnPlayer = true;
            currentTarget.position = playerPosition.position;
            agent.SetDestination(currentTarget.position);

            agent.speed = 4;
        }
        else
        {
            isLockedOnPlayer = false;

            agent.speed = 2;

            if (!agent.pathPending && agent.remainingDistance < 0.5f)
            {
                currentTarget = currentTarget == pointA ? pointB : pointA;
                agent.SetDestination(currentTarget.position);
            }
        }
    }
}
