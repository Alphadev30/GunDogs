using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// this script manages the spawn points which the player will be spawned in 
/// </summary>
public class SpawnManager : MonoBehaviour
{
    public static SpawnManager instance;
    public Transform[] spawnPoints;

    private void Awake()
    {
        instance = this;
    }


    void Start()
    {
        foreach(Transform spawnPoint in spawnPoints)
        {
            spawnPoint.gameObject.SetActive(false);
        }
    }

    public Transform GetSpawnPoints()
    {
        return spawnPoints[Random.Range(0, spawnPoints.Length)];
    }

}
