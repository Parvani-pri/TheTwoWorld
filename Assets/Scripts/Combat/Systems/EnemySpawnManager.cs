using System.Collections.Generic;
using UnityEngine;

public class EnemySpawnManager : MonoBehaviour
{
    [SerializeField] List<GameObject> normalEnemies;
    [SerializeField] GameObject boss;
    List<GameObject> existingEnemies = new List<GameObject>();
    [SerializeField] int targetSpawnAmount;
    [SerializeField] float spawnInterval = 10f;
    float spawnTimer;
    int spawnCount;
    bool canSpawn;
    bool hasBossSpawned;


    private void Awake()
    {
        hasBossSpawned = false;
        canSpawn = true;
        spawnTimer = 0f;
        spawnCount = 0;
    }

    private void Update()
    {
        if (spawnTimer > 0f)
        {
            spawnTimer -= Time.deltaTime;
        }

        if (canSpawn && spawnCount < targetSpawnAmount && spawnTimer <= 0f)
        {
            print("spawned");
            int spawnIndex = Random.Range(0, normalEnemies.Count);
            existingEnemies.Add(Instantiate(normalEnemies[spawnIndex], Vector3.zero, Quaternion.identity));
            existingEnemies[existingEnemies.Count - 1].SetActive(true);
            spawnTimer = spawnInterval;
            spawnCount++;
        }

        if(spawnCount == targetSpawnAmount && !hasBossSpawned)
        {
            foreach (var enemy in existingEnemies)
            {
                if (enemy != null)
                {
                    return;
                }
            }
            canSpawn = false;
            hasBossSpawned = true;
            boss.SetActive(true);
        }
    }

}
