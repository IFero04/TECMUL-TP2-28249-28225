using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class scr_WaveSpawner : MonoBehaviour
{
    [System.Serializable]
    public class WaveContent
    {
        [SerializeField][NonReorderable] GameObject[] monsterSpawner;

        public GameObject[] GetMosterSpawnList()
        {
            return monsterSpawner;
        }
    }

    [SerializeField][NonReorderable] WaveContent[] waves;
    public float mult = 1;
    public float spawnRange = 50;
    public int enemiesKilled;
    public float countdown = 30f;
    public float timeToNextWave = 45;
    public bool canSpawn = true;

    public int currentWaveIndex = 0;
    public List<GameObject> currentWave;

    void Update()
    {
        if (spawnRange < 15)
            spawnRange = 15;

        timeToNextWave -= Time.deltaTime;
        if (timeToNextWave <= 0 && canSpawn)
        {
            canSpawn = false;
            countdown = 30f;
            SpawnWave();
            currentWaveIndex++;
        }
            
        countdown -= Time.deltaTime;

        if ((currentWave.Count == 0 || countdown <= 0) && canSpawn == false)
        {
            if (currentWaveIndex >= waves.Length)
            {
                currentWaveIndex = 0;
                mult += 0.15f;
                spawnRange /= mult;
            }

            if (countdown <= 0)
            {
                currentWave.Clear();
            }

            canSpawn = true;
            timeToNextWave = 5;
        }
    }

    public void SpawnWave()
    {
        for (int i = 0; i < waves[currentWaveIndex].GetMosterSpawnList().Length; i++)
        {
            GameObject newspawn = Instantiate(waves[currentWaveIndex].GetMosterSpawnList()[i], FindSpawnLoc(), Quaternion.identity);
            currentWave.Add(newspawn);

            scr_Enemy monster = newspawn.GetComponent<scr_Enemy>();
            monster.SetSpawner(this);
        }
    }

    private Vector3 FindSpawnLoc()
    {
        Vector3 SpawnPsos;

        float xLoc = Random.Range(-spawnRange, spawnRange) + transform.position.x;
        float zLoc = Random.Range(-spawnRange, spawnRange) + transform.position.z;
        float yLoc = +transform.position.y;

        SpawnPsos = new Vector3(xLoc, yLoc, zLoc);
        
        if (Physics.Raycast(SpawnPsos, Vector3.down, 5) || xLoc > 10 || xLoc < -10 || zLoc > 10 || zLoc < -10)
        {
            return SpawnPsos;
        }
        else
        {
            return FindSpawnLoc();
        }
    }
}
