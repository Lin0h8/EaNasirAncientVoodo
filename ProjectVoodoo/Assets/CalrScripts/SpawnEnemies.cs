using System;
using UnityEngine;

public class SpawnEnemies : MonoBehaviour
{
    public GameObject enemy;
    public GameObject spawnPoint;

    public float spawnDelay = 1f;
    public float lastSpawnTime;

    [Header("Count")]
    public int enemyCount = 0;
    public int spawnCount;
    public int maxEnemyCount = 6;

    // Update is called once per frame
    void Update()
    {
        if (Time.time >= spawnDelay + lastSpawnTime)
        {
            if (enemyCount < maxEnemyCount)
            {
                SpawnEnemy();
                lastSpawnTime = Time.time;
            }
        }   
    }

    private void SpawnEnemy()
    {
        Instantiate(enemy, spawnPoint.transform.position, spawnPoint.transform.rotation);
        enemyCount++;
    }
}
