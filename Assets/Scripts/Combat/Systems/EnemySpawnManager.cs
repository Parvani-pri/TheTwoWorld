using System.Collections.Generic;
using UnityEngine;

public class EnemySpawnManager : MonoBehaviour
{
    [SerializeField] List<GameObject> normalEnemies;
    List<GameObject> existingEnemies;
    [SerializeField] int targetSpawnAmount;
    [SerializeField] float spawnInterval = 10f;
    float spawnTimer;
    int spawnCount;
    bool canSpawn;

    private void Awake()
    {
        spawnTimer = 0f;
        spawnCount = 0;
    }

    private void Update()
    {
        if (canSpawn && spawnCount < targetSpawnAmount && spawnTimer <= 0f)
        {
            int spawnIndex = Random.Range(0, normalEnemies.Count);
            existingEnemiesInstantiate
            spawnCount++;
        }
    }

}
