using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class SpawnManager : MonoBehaviour
{
    public static SpawnManager Instance;

    [SerializeField] private List<GameObject> _spawnLocations;

    private void Awake()
    {
        // If there is an instance, and it's not me, delete myself.

        if (Instance != null && Instance != this)
        {
            Destroy(this);
        }
        else
        {
            Instance = this;
        }
    }

    public Vector3 SetSpawnLocation()
    {
        int spawnLocationIndex = Random.Range(0, _spawnLocations.Count);

        return _spawnLocations[spawnLocationIndex].transform.position;
    }
}
