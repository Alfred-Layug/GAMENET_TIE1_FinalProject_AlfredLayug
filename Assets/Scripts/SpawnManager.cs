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
        int spawnPosition = PhotonNetwork.LocalPlayer.ActorNumber - 1;

        return _spawnLocations[spawnPosition].transform.position;
    }
}