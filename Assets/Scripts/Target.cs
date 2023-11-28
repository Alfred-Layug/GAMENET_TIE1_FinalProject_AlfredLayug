using ExitGames.Client.Photon;
using Photon.Pun;
using Photon.Realtime;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Target : MonoBehaviourPunCallbacks
{
    public enum RaiseEventsCode
    {
        DestroyEventCode = 1
    }

    [SerializeField] private RoomProgressChecker roomProgressChecker;

    private void Start()
    {
        roomProgressChecker.targets.Add(gameObject);
    }

    private void OnEnable()
    {
        PhotonNetwork.NetworkingClient.EventReceived += DestroyTarget;
    }

    private void OnDisable()
    {
        PhotonNetwork.NetworkingClient.EventReceived -= DestroyTarget;
    }

    public void EliminateTarget()
    {
        string name = photonView.name;

        // event data
        object[] data = new object[] { name };

        RaiseEventOptions raiseEventOptions = new RaiseEventOptions
        {
            Receivers = ReceiverGroup.All,
            CachingOption = EventCaching.AddToRoomCache
        };

        SendOptions sendOptions = new SendOptions
        {
            Reliability = false
        };
        PhotonNetwork.RaiseEvent((byte)RaiseEventsCode.DestroyEventCode, data, raiseEventOptions, sendOptions);
    }

    public void DestroyTarget(EventData photonEvent)
    {
        if (photonEvent.Code == (byte)RaiseEventsCode.DestroyEventCode)
        {
            object[] data = (object[])photonEvent.CustomData;

            string objectName = (string)data[0];

            if (photonView.name == objectName)
            {
                roomProgressChecker.RemoveTarget(gameObject);
                Destroy(gameObject);
            }
        }
    }
}
