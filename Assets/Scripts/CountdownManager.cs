using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class CountdownManager : MonoBehaviourPunCallbacks
{
    public TextMeshProUGUI timerText;

    public float timeToStart = 3;

    private void Start()
    {
        if (GameObject.Find("Timer Text").GetComponent<TextMeshProUGUI>() != null)
        {
            timerText = GameObject.Find("Timer Text").GetComponent<TextMeshProUGUI>();
        }

        GetComponent<Shooting>().enabled = false;
    }

    private void Update()
    {
        if (PhotonNetwork.IsMasterClient)
        {
            if (timeToStart > 0)
            {
                timeToStart -= Time.deltaTime;
                photonView.RPC("SetTime", RpcTarget.AllBuffered, timeToStart);
            }
            else if (timeToStart < 0)
            {
                photonView.RPC("StartRace", RpcTarget.AllBuffered);
            }
        }
    }

    [PunRPC]
    public void SetTime(float time)
    {
        if (timerText != null)
        {
            if (time > 0)
            {
                timerText.text = time.ToString("F1");
            }
            else
            {
                timerText.text = "";
            }
        }
    }

    [PunRPC]
    public void StartRace()
    {
        GetComponent<PlayerMovementController>()._isMovementEnabled = true;
        GetComponent<Shooting>().enabled = true;
        this.enabled = false;
    }
}