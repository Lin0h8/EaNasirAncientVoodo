using System;
using UnityEngine;
using UnityEngine.AI;

public class EnemyWanderBehaviour : MonoBehaviour
{
    public NavMeshAgent agent;

    public float wanderRadious = 10f;
    public float wanderDelay = 2f;

    public Transform centerPoint;

    float timer = 0f;

    public static Vector3 RandomNavSphere(Vector3 origin, float dist, int layermask)
    {
        Vector3 randomDirection = UnityEngine.Random.insideUnitSphere * dist;
        randomDirection += origin;

        NavMeshHit navHit;
        NavMesh.SamplePosition(randomDirection, out navHit, dist, layermask);

        return navHit.position;
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        timer = wanderDelay;   
    }

    // Update is called once per frame
    void Update()
    {
        timer += Time.deltaTime;

        if (timer >= wanderDelay && !agent.pathPending && agent.remainingDistance < 0.5f)
        {
            Vector3 newPos = RandomNavSphere(centerPoint.position, wanderRadious, NavMesh.AllAreas);
            agent.SetDestination(newPos);
            timer = 0f;
        }
    }


}
