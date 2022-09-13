using ExitGames.Client.Photon;
using Photon.Realtime;
using Photon.Pun;
using UnityEngine;

public class OtherPlayersView : MonoBehaviour, IOnEventCallback
{
    [SerializeField] private PlayerView prefabPlayerView;

    private PlayerView[] playerViews;
    private int playersCount;
    private int ownerID;

    public void OnEvent(EventData photonEvent)
    {
        switch (photonEvent.Code)
        {
            case Core.EVENT_SEND_PLAYERS_CARD:
                object[] data = (object[])photonEvent.CustomData;
                UpdateAllPlayersView((int[])data[2]);
                break;
        }
    }

    private void Awake()
    {
        playersCount = PhotonNetwork.PlayerList.Length;

        for (int i = 0; i < playersCount; i++)
        {
            if (PhotonNetwork.PlayerList[i] == PhotonNetwork.LocalPlayer)
            {
                ownerID = i;
                break;
            }
        }

        GeneratePlayersView(playersCount);
        SetOtherPlayersName(playersCount);
    }

    private void OnEnable()
    {
        PhotonNetwork.NetworkingClient.EventReceived += OnEvent;
    }

    private void OnDisable()
    {
        PhotonNetwork.NetworkingClient.EventReceived += OnEvent;
    }

    private void UpdateAllPlayersView(int[] playerCardsCount)
    {
        for (int i = 1; i < playersCount; i++)
        {
            int id = TakeModifiedPlayerID(i);
            playerViews[i].UpdatePlayerViewCardsCount(playerCardsCount[id]);
        }
    }

    private void GeneratePlayersView(int PlayersCount)
    {
        switch (PlayersCount)
        {
            case 2:
                playerViews = new PlayerView[2];
                playerViews[1] = Instantiate(prefabPlayerView, transform);
                break;

            case 3:
                playerViews = new PlayerView[3];
                playerViews[1] = Instantiate(prefabPlayerView, transform);
                playerViews[1].gameObject.transform.position = new Vector3(-4, 0, 0);
                playerViews[1].gameObject.transform.rotation = Quaternion.Euler(0, 0, 90);

                playerViews[2] = Instantiate(prefabPlayerView, transform);
                playerViews[2].gameObject.transform.position = new Vector3(4, 0, 0);
                playerViews[2].gameObject.transform.rotation = Quaternion.Euler(0, 0, -90);
                break;

            case 4:
                playerViews = new PlayerView[4];
                playerViews[1] = Instantiate(prefabPlayerView, transform);
                playerViews[1].gameObject.transform.position = new Vector3(-4, 0, 0);
                playerViews[1].gameObject.transform.rotation = Quaternion.Euler(0, 0, 90);

                playerViews[2] = Instantiate(prefabPlayerView, transform);

                playerViews[3] = Instantiate(prefabPlayerView, transform);
                playerViews[3].gameObject.transform.position = new Vector3(4, 0, 0);
                playerViews[3].gameObject.transform.rotation = Quaternion.Euler(0, 0, -90);
                break;

        }
    }

    private void SetOtherPlayersName(int PlayersCount)
    {
        for (int i = 1; i < PlayersCount; i++)
        {
            int id = TakeModifiedPlayerID(i);
            playerViews[i].SetPlayerViewPlayerName(PhotonNetwork.PlayerList[id].NickName);
        }
    }

    private int TakeModifiedPlayerID(int i)
    {
        int id = i + ownerID;

        if (id >= playersCount)
        {
            while (id >= playersCount)
            {
                id -= playersCount;
            }
        }

        return id;
    }
}