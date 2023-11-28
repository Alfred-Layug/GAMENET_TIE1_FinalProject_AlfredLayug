using ExitGames.Client.Photon;
using Photon.Pun;
using Photon.Realtime;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityStandardAssets.Characters.FirstPerson;

public class UIManager : MonoBehaviourPunCallbacks
{
    public enum RaiseEventsCode
    {
        WhoWonEventCode = 0
    }

    private void OnEnable()
    {
        PhotonNetwork.NetworkingClient.EventReceived += OnWinEvent;
    }

    private void OnDisable()
    {
        PhotonNetwork.NetworkingClient.EventReceived -= OnWinEvent;
    }

    void OnWinEvent(EventData photonEvent)
    {
        if (photonEvent.Code == (byte)RaiseEventsCode.WhoWonEventCode)
        {
            GetComponent<Shooting>().enabled = false;

            object[] data = (object[])photonEvent.CustomData;

            string nickNameOfWinner = (string)data[0];

            Debug.Log("Winner: " + nickNameOfWinner);

            GameObject winnerText = GameObject.Find("Winner Text");
            winnerText.GetComponent<TextMeshProUGUI>().text = nickNameOfWinner + " has the fastest hands in the West!";
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.GetComponent<RoomProgressChecker>() != null && photonView.IsMine)
        {
            RoomProgressChecker roomProgressChecker = other.GetComponent<RoomProgressChecker>();
            photonView.RPC("AnnounceRoomProgression", RpcTarget.All, photonView.Owner.NickName, roomProgressChecker.roomNumber);
        }

        if (other.GetComponent<WinningTrigger>() != null)
        {
            AnnounceWinner();
        }
    }

    [PunRPC]
    public void AnnounceRoomProgression(string playerName, int roomNumber)
    {
        GameObject roomProgressText = GameObject.Find("Room Progress Text");
        roomProgressText.GetComponent<TextMeshProUGUI>().text = playerName + " has cleared room " + roomNumber + "!";
        StartCoroutine(HideRoomProgressionTextTimer());
    }

    [PunRPC]
    public void HideRoomProgressionText()
    {
        GameObject roomProgressText = GameObject.Find("Room Progress Text");
        roomProgressText.GetComponent<TextMeshProUGUI>().text = "";
    }

    public void AnnounceWinner()
    {
        string nickName = photonView.Owner.NickName;

        // event data
        object[] data = new object[] { nickName };

        RaiseEventOptions raiseEventOptions = new RaiseEventOptions
        {
            Receivers = ReceiverGroup.All,
            CachingOption = EventCaching.AddToRoomCache
        };

        SendOptions sendOptions = new SendOptions
        {
            Reliability = false
        };
        PhotonNetwork.RaiseEvent((byte)RaiseEventsCode.WhoWonEventCode, data, raiseEventOptions, sendOptions);
    }

    private IEnumerator HideRoomProgressionTextTimer()
    {
        yield return new WaitForSeconds(3);
        photonView.RPC("HideRoomProgressionText", RpcTarget.All);
    }
}