using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Photon.Pun;
using Photon.Realtime;

public class NetworkManager : MonoBehaviourPunCallbacks
{
    [Header("Connection Status Panel")]
    public Text _connectionStatusText;

    [Header("Login UI Panel")]
    public InputField _playerNameInput;
    public GameObject _loginUiPanel;

    [Header("Game Options Panel")]
    public GameObject _gameOptionsPanel;

    [Header("Create Room Panel")]
    public GameObject _createRoomPanel;
    public InputField _roomNameInputField;
    public InputField _playerCountInputField;

    [Header("Join Random Room Panel")]
    public GameObject _joinRandomRoomPanel;

    [Header("Show Room List Panel")]
    public GameObject _showRoomListPanel;

    [Header("Inside Room Panel")]
    public GameObject _insideRoomPanel;
    public Text _roomInfoText;
    public GameObject _playerListItemPrefab;
    public GameObject _playerListViewParent;
    public GameObject _startGameButton;

    [Header("Room List Panel")]
    public GameObject _roomListPanel;
    public GameObject _roomItemPrefab;
    public GameObject _roomListParent;

    private Dictionary<string, RoomInfo> _cachedRoomList;
    private Dictionary<string, GameObject> _roomListGameObjects;
    private Dictionary<int, GameObject> _playerListGameObjects;

    #region Unity Functions
    private void Start()
    {
        _cachedRoomList = new Dictionary<string, RoomInfo>();
        _roomListGameObjects = new Dictionary<string, GameObject>();
        ActivatePanel(_loginUiPanel);

        PhotonNetwork.AutomaticallySyncScene = true;
    }

    private void Update()
    {
        _connectionStatusText.text = "Connection status: " + PhotonNetwork.NetworkClientState;
    }
    #endregion

    #region UI Callbacks
    public void OnLoginButtonClicked()
    {
        string playerName = _playerNameInput.text;

        if (string.IsNullOrEmpty(playerName))
        {
            Debug.Log("Player name is invalid!");
        }
        else
        {
            PhotonNetwork.LocalPlayer.NickName = playerName;
            PhotonNetwork.ConnectUsingSettings();
        }
    }

    public void OnCreateRoomButtonClicked()
    {
        string roomName = _roomNameInputField.text;

        if (string.IsNullOrEmpty(roomName))
        {
            roomName = "Room " + Random.Range(1000, 10000);
        }

        RoomOptions roomOptions = new RoomOptions();
        roomOptions.MaxPlayers = (byte)int.Parse(_playerCountInputField.text);

        PhotonNetwork.CreateRoom(roomName, roomOptions);
    }

    public void OnCancelButtonClicked()
    {
        ActivatePanel(_gameOptionsPanel);
    }

    public void OnShowRoomListButtonClicked()
    {
        if (PhotonNetwork.InLobby)
        {
            PhotonNetwork.JoinLobby();
        }

        ActivatePanel(_showRoomListPanel);
    }

    public void OnBackButtonClicked()
    {
        if (PhotonNetwork.InLobby)
        {
            PhotonNetwork.LeaveLobby();
        }
        ActivatePanel(_gameOptionsPanel);
    }

    public void OnLeaveButtonClicked()
    {
        PhotonNetwork.LeaveRoom();
    }

    public void OnJoinRandomRoomClicked()
    {
        ActivatePanel(_joinRandomRoomPanel);
        PhotonNetwork.JoinRandomRoom();
    }

    public void OnStartGameButtonClicked()
    {
        if (PhotonNetwork.IsMasterClient)
        {
            PhotonNetwork.LoadLevel("GameScene");
        }
    }
    #endregion

    #region PUN Callbacks
    public override void OnConnected()
    {
        Debug.Log("Connected to the internet!");
    }

    public override void OnConnectedToMaster()
    {
        Debug.Log(PhotonNetwork.LocalPlayer.NickName + " has connected to Photon servers.");
        ActivatePanel(_gameOptionsPanel);
    }

    public override void OnCreatedRoom()
    {
        Debug.Log(PhotonNetwork.CurrentRoom.Name + " created!");
    }

    public override void OnJoinedRoom()
    {
        Debug.Log(PhotonNetwork.LocalPlayer.NickName + " has joined " + PhotonNetwork.CurrentRoom.Name);
        ActivatePanel(_insideRoomPanel);

        _roomInfoText.text = "Room Name: " + PhotonNetwork.CurrentRoom.Name + " Current Player Count: " +
                PhotonNetwork.CurrentRoom.PlayerCount + "/" + PhotonNetwork.CurrentRoom.MaxPlayers;

        if (_playerListGameObjects == null)
        {
            _playerListGameObjects = new Dictionary<int, GameObject>();
        }

        foreach (Player player in PhotonNetwork.PlayerList)
        {
            GameObject playerItem = Instantiate(_playerListItemPrefab);
            playerItem.transform.SetParent(_playerListViewParent.transform);
            playerItem.transform.localScale = Vector3.one;

            playerItem.transform.Find("PlayerNameText").GetComponent<Text>().text = player.NickName;
            playerItem.transform.Find("PlayerIndicator").gameObject.SetActive(player.ActorNumber == PhotonNetwork.LocalPlayer.ActorNumber);

            _playerListGameObjects.Add(player.ActorNumber, playerItem);
        }
    }

    public override void OnRoomListUpdate(List<RoomInfo> roomList)
    {
        ClearRoomListGameObjects();

        _startGameButton.SetActive(PhotonNetwork.LocalPlayer.IsMasterClient);
        foreach (RoomInfo info in roomList)
        {
            Debug.Log(info.Name);

            if (!info.IsOpen || !info.IsVisible || info.RemovedFromList)
            {
                if (_cachedRoomList.ContainsKey(info.Name))
                {
                    _cachedRoomList.Remove(info.Name);
                }
            }
            else
            {
                //Update existing rooms info
                if (_cachedRoomList.ContainsKey(info.Name))
                {
                    _cachedRoomList[info.Name] = info;
                }
                else
                {
                    _cachedRoomList.Add(info.Name, info);
                }
            }
        }

        foreach (RoomInfo info in _cachedRoomList.Values)
        {
            GameObject listItem = Instantiate(_roomItemPrefab);
            listItem.transform.SetParent(_roomListParent.transform);
            listItem.transform.localScale = Vector3.one;

            listItem.transform.Find("RoomNameText").GetComponent<Text>().text = info.Name;
            listItem.transform.Find("RoomPlayersText").GetComponent<Text>().text = "Player count: " + info.PlayerCount + "/" + info.MaxPlayers;
            listItem.transform.Find("JoinRoomButton").GetComponent<Button>().onClick.AddListener(() => OnJoinRoomClicked(info.Name));

            _roomListGameObjects.Add(info.Name, listItem);
        }
    }

    public override void OnLeftLobby()
    {
        ClearRoomListGameObjects();
        _cachedRoomList.Clear();
    }

    public override void OnPlayerEnteredRoom(Player player)
    {
        _roomInfoText.text = "Room Name: " + PhotonNetwork.CurrentRoom.Name + " Current Player Count: " +
                PhotonNetwork.CurrentRoom.PlayerCount + "/" + PhotonNetwork.CurrentRoom.MaxPlayers;

        GameObject playerItem = Instantiate(_playerListItemPrefab);
        playerItem.transform.SetParent(_playerListViewParent.transform);
        playerItem.transform.localScale = Vector3.one;

        playerItem.transform.Find("PlayerNameText").GetComponent<Text>().text = player.NickName;
        playerItem.transform.Find("PlayerIndicator").gameObject.SetActive(player.ActorNumber == PhotonNetwork.LocalPlayer.ActorNumber);

        _playerListGameObjects.Add(player.ActorNumber, playerItem);
    }

    public override void OnPlayerLeftRoom(Player otherPlayer)
    {
        _startGameButton.SetActive(PhotonNetwork.LocalPlayer.IsMasterClient);

        _roomInfoText.text = "Room Name: " + PhotonNetwork.CurrentRoom.Name + " Current Player Count: " +
                PhotonNetwork.CurrentRoom.PlayerCount + "/" + PhotonNetwork.CurrentRoom.MaxPlayers;
        Destroy(_playerListGameObjects[otherPlayer.ActorNumber]);
        _playerListGameObjects.Remove(otherPlayer.ActorNumber);
    }

    public override void OnLeftRoom()
    {
        foreach (var gameObject in _playerListGameObjects.Values)
        {
            Destroy(gameObject);
        }
        _playerListGameObjects.Clear();
        _playerListGameObjects = null;
        ActivatePanel(_gameOptionsPanel);
    }

    public override void OnJoinRandomFailed(short returnCode, string message)
    {
        Debug.LogWarning(message);

        string roomName = "Room " + Random.Range(1000, 10000);
        RoomOptions roomOptions = new RoomOptions();
        roomOptions.MaxPlayers = 20;

        PhotonNetwork.CreateRoom(roomName, roomOptions);
    }
    #endregion

    #region Private Methods
    private void OnJoinRoomClicked(string roomName)
    {
        if (PhotonNetwork.InLobby)
        {
            PhotonNetwork.LeaveLobby();
        }
        PhotonNetwork.JoinRoom(roomName);
    }

    private void ClearRoomListGameObjects()
    {
        foreach (var item in _roomListGameObjects.Values)
        {
            Destroy(item);
        }

        _roomListGameObjects.Clear();
    }
    #endregion

    #region Public Methods
    public void ActivatePanel(GameObject panelToBeActivated)
    {
        _loginUiPanel.SetActive(panelToBeActivated.Equals(_loginUiPanel));
        _gameOptionsPanel.SetActive(panelToBeActivated.Equals(_gameOptionsPanel));
        _createRoomPanel.SetActive(panelToBeActivated.Equals(_createRoomPanel));
        _joinRandomRoomPanel.SetActive(panelToBeActivated.Equals(_joinRandomRoomPanel));
        _showRoomListPanel.SetActive(panelToBeActivated.Equals(_showRoomListPanel));
        _insideRoomPanel.SetActive(panelToBeActivated.Equals(_insideRoomPanel));
        _roomListPanel.SetActive(panelToBeActivated.Equals(_roomListPanel));
    }
    #endregion
}
