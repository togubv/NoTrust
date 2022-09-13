using Photon.Pun;
using Photon.Realtime;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Lobby : MonoBehaviourPunCallbacks
{
    [SerializeField] private GameObject prefabPlayerData;
    [SerializeField] private GameObject panelMain;
    [SerializeField] private GameObject panelLobby;
    [SerializeField] private GameObject panelPlayerList;
    [SerializeField] private InputField inputFieldName;
    [SerializeField] private GameObject buttonStartGame;
    [SerializeField] private Button buttonCreateRoom;
    [SerializeField] private Button buttonJoinRoom;

    private Dictionary<int, GameObject> playerListEntries;

    public Text LogText;

    private void Start()
    {
        PhotonNetwork.AutomaticallySyncScene = true;
        PhotonNetwork.GameVersion = "1";
        PhotonNetwork.ConnectUsingSettings();
        Log("Connecting...");
    }

    public override void OnConnectedToMaster()
    {
        buttonCreateRoom.interactable = true;
        buttonJoinRoom.interactable = true;
        Log("Connected!");
    }

    public void CreateRoom()
    {
        if (CheckNameInput() == false)
        {
            return;
        }

        PhotonNetwork.LocalPlayer.NickName = inputFieldName.text;
        PhotonNetwork.CreateRoom(null, new RoomOptions { MaxPlayers = 4 });
    }

    public void JoinRandomRoom()
    {
        if (CheckNameInput() == false)
        {
            return;
        }

        PhotonNetwork.LocalPlayer.NickName = inputFieldName.text;
        PhotonNetwork.JoinRandomRoom();        
    }

    public override void OnJoinedRoom()
    {
        if (PhotonNetwork.IsMasterClient)
        {
            buttonStartGame.SetActive(true);
        }

        Log("Joined the room");

        if (playerListEntries == null)
        {
            playerListEntries = new Dictionary<int, GameObject>();
        }

        foreach (Player player in PhotonNetwork.PlayerList)
        {
            GameObject entry = Instantiate(prefabPlayerData, panelPlayerList.transform);
            entry.GetComponent<PlayerData>().Initialize(player.ActorNumber, player.NickName);
            playerListEntries.Add(player.ActorNumber, entry);
        }

        ShowPanelLobby();
    }

    public override void OnPlayerEnteredRoom(Player newPlayer)
    {
        GameObject entry = Instantiate(prefabPlayerData, panelPlayerList.transform);
        entry.GetComponent<PlayerData>().Initialize(newPlayer.ActorNumber, newPlayer.NickName);
        playerListEntries.Add(newPlayer.ActorNumber, entry);
        Log(newPlayer.NickName + " joined the room.");
    }

    public override void OnPlayerLeftRoom(Player otherPlayer)
    {
        Destroy(playerListEntries[otherPlayer.ActorNumber].gameObject);
        playerListEntries.Remove(otherPlayer.ActorNumber);
        Log(otherPlayer.NickName + " left the room.");
    }

    public override void OnLeftRoom()
    {
        foreach (GameObject entry in playerListEntries.Values)
        {
            Destroy(entry.gameObject);
        }

        playerListEntries.Clear();
        playerListEntries = null;

    }

    public void OnStartGameButtonClicked()
    {
        PhotonNetwork.CurrentRoom.IsOpen = false;
        PhotonNetwork.CurrentRoom.IsVisible = false;

        PhotonNetwork.LoadLevel("Game");
    }

    public void OnLeaveGameButtonClicked()
    {
        PhotonNetwork.LeaveRoom();
        ShowPanelMain();
    }

    private void Log(string message)
    {
        Debug.Log(message);
        LogText.text = message;
    }

    private void ShowPanelLobby()
    {
        panelMain.SetActive(false);
        panelLobby.SetActive(true);
    }

    private void ShowPanelMain()
    {
        panelLobby.SetActive(false);
        panelMain.SetActive(true);
    }

    private bool CheckNameInput()
    {
        string playerName = inputFieldName.text;

        if (playerName.Equals("") == true)
        {
            Log("Enter your name!");
            return false;
        }

        return true;
    }
}
