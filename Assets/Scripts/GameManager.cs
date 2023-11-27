using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class GameManager : MonoBehaviour
{
    public GameObject _playerPrefab;

    private void Start()
    {
        if (PhotonNetwork.IsConnectedAndReady)
        {
            StartCoroutine(DelayedPlayerSpawn());
        }
    }

    private IEnumerator DelayedPlayerSpawn()
    {
        yield return new WaitForSeconds(3);
        PhotonNetwork.Instantiate(_playerPrefab.name, SpawnManager.Instance.SetSpawnLocation(), Quaternion.identity);
    }
}
