using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class WinningTrigger : MonoBehaviour
{
    private void OnTriggerEnter(Collider other)
    {
        if (other.GetComponent<PhotonView>() != null)
        {
            GetComponent<BoxCollider>().enabled = false;
        }
    }
}