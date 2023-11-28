using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RoomProgressChecker : MonoBehaviourPunCallbacks
{
    [SerializeField] private GameObject door;

    public int roomNumber;
    public List<GameObject> targets;

    private void OnTriggerEnter(Collider other)
    {
        if (other.GetComponent<PhotonView>() != null)
        {
            GetComponent<BoxCollider>().enabled = false;
        }
    }

    public void RemoveTarget(GameObject target)
    {
        targets.Remove(target);

        if (targets.Count == 0)
        {
            door.SetActive(false);
        }
    }
}