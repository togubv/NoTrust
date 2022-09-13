using ExitGames.Client.Photon;
using Photon.Pun;
using Photon.Realtime;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Croupier : MonoBehaviour, IOnEventCallback
{
    [SerializeField] private TurnSystem turnSystem;

    public GameObject[] PhotonCard => photonCard;

    private int turnCardID;
    private List<int> turnPool = new List<int>();
    private RaiseEventOptions options;
    private SendOptions sendOptions;
    private PlayerCards[] playerCards;
    private bool[] currentPool = new bool[12];
    private GameObject[] photonCard = new GameObject[53];

    public void OnEvent(EventData photonEvent)
    {
        switch (photonEvent.Code)
        {
            case Core.EVENT_CARDS_MOVE_TO_HEAP:
                MoveToHeapCards((int[])photonEvent.CustomData);
                break;

            case Core.EVENT_ADD_TURN_POOL_CARDS_FOR_PLAYER:
                StartCoroutine(OpenTurnCardAnimation((object[])photonEvent.CustomData, 3.0f));
                break;

            case Core.EVENT_REMOVE_SENT_CARD_FROM_SENDER:
                object[] removedCards = (object[])photonEvent.CustomData;
                RemovePlayerCards((int)removedCards[0], (int[])removedCards[1]);
                SendPlayersCards();
                break;

            case Core.EVENT_ADD_CARDS_TO_TURN_POOL:
                AddCardsToTurnPool((int[])photonEvent.CustomData);
                break;

            case Core.EVENT_REMOVE_SET_FROM_PLAYER:
                object[] setData = (object[])photonEvent.CustomData;
                RemovePlayerCards((int)setData[0], (int[])setData[1]);
                UpdateAndSendToPlayersCardsPool((int[])setData[2]);
                SendPlayersCards();
                break;
        }
    }

    private void Awake()
    {
        if (PhotonNetwork.IsMasterClient)
        {
            options = new RaiseEventOptions { Receivers = ReceiverGroup.All };
            sendOptions = new SendOptions { Reliability = true };

            playerCards = new PlayerCards[PhotonNetwork.PlayerList.Length];

            for (int i = 0; i < playerCards.Length; i++)
            {
                playerCards[i] = new PlayerCards(new List<int>());
            }

            StartCoroutine(DelayedGenerate(1.0f));
        }
    }

    private IEnumerator DelayedGenerate(float delay)
    {
        yield return new WaitForSeconds(delay);

        GeneratePoolCards();
        UpdateAndSendToPlayersCardsPool(null);
        DistributeCard();
        GeneratePhotonCards();
        SendPlayersCards();
    }

    private void OnEnable()
    {
        if (PhotonNetwork.IsMasterClient)
        {
            PhotonNetwork.NetworkingClient.EventReceived += OnEvent;
        }
    }

    private void OnDisable()
    {
        if (PhotonNetwork.IsMasterClient)
        {
            PhotonNetwork.NetworkingClient.EventReceived += OnEvent;
        }
    }

    private void DistributeCard()
    {
        List<int> pool = new List<int>();

        for (int i = 1; i <= 52; i++)
        {
            pool.Add(i);
        }

        while (pool.Count > 0)
        {
            for (int i = 0; i < PhotonNetwork.PlayerList.Length; i++)
            {
                int randomCard = UnityEngine.Random.Range(0, pool.Count);
                playerCards[i].cards.Add(pool[randomCard]);
                pool.RemoveAt(randomCard);

                if (pool.Count < 1)
                {
                    return;
                }
            }
        }
    }

    private void GeneratePoolCards()
    {
        for (int i = 0; i < currentPool.Length; i++)
        {
            currentPool[i] = true;
        }
    }

    private void GeneratePhotonCards()
    {
        GameObject photonObjects = new GameObject("Photon Objects");

        for (int i = 0; i < 13; i++)
        {
            for (int j = 0; j < 4; j++)
            {
                Vector2 position = new Vector2(0, 0);

                int id = i * 4 + j + 1;
                int value = i;
                int suit = j;
                int sprSuit = j + 13;
                int sprValue = i;
                bool sprColor = (j < 2) ? false : true;

                object[] instantiateData = { id, value, suit, sprSuit, sprValue, sprColor };
                GameObject card = PhotonNetwork.InstantiateRoomObject("CardPhoton", position, Quaternion.identity, 0, instantiateData);
                card.transform.SetParent(photonObjects.transform);
                photonCard[i * 4 + j + 1] = card;
                card.gameObject.GetComponent<PhotonView>().RPC("TogglePhotonObject", RpcTarget.AllViaServer, false);
            }
        }
    }

    private void SendPlayersCards()
    {
        int[] playersCardsCount = new int[playerCards.Length];

        for (int i = 0; i < playersCardsCount.Length; i++)
        {
            playersCardsCount[i] = playerCards[i].cards.Count;
        }

        for (int i = 0; i < playerCards.Length; i++)
        {
            int[] convertedCards = playerCards[i].cards.ToArray();
            object[] data = new object[] { (int)i, (int[])convertedCards, (int[])playersCardsCount };
            PhotonNetwork.RaiseEvent(Core.EVENT_SEND_PLAYERS_CARD, data, options, sendOptions);
        }
    }

    private void UpdateAndSendToPlayersCardsPool(int[] removedCard)
    {
        if (removedCard != null)
        {
            for (int i = 0; i < removedCard.Length; i++)
            {
                currentPool[removedCard[i]] = false;
            }
        }

        PhotonNetwork.RaiseEvent(Core.EVENT_UPDATE_AND_SEND_CARDS_POOL, currentPool, options, sendOptions);
    }

    private void RemovePlayerCards(int player, int[] cardsID)
    {
        for (int i = 0; i < cardsID.Length; i++)
        {
            if (playerCards[player].cards.Contains(cardsID[i]))
            {
                int id = playerCards[player].cards.IndexOf(cardsID[i]);
                playerCards[player].cards.RemoveAt(id);
            }
        }
    }

    private void AddPlayerCards(int player, int[] cardsID)
    {
        for (int i = 0; i < cardsID.Length; i++)
        {
            playerCards[player].cards.Add(cardsID[i]);
        }
    }

    private void AddCardsToTurnPool(int[] newCards)
    {
        for (int i = 0; i < newCards.Length; i++)
        {
            if (newCards[i] != 0)
            {
                turnPool.Add(newCards[i]);
            }
        }
    }

    private void AddTurnCardsToPlayer(int player)
    {
        playerCards[player].cards.AddRange(turnPool);
        turnPool = new List<int>();
    }

    private void PlayerTakeAllPoolCards(object[] data)
    {
        int[] newArray = turnPool.ToArray();
        turnPool = new List<int>();
        AddPlayerCards((int)data[0], newArray);
        HideAllPhotonCards();

        for (int i = 0; i < playerCards.Length; i++)
        {
            if (playerCards[i].cards.Count < 1)
            {
                PhotonNetwork.RaiseEvent(Core.EVENT_END_GAME, i, options, sendOptions);
                return;
            }
        }

        SendPlayersCards();
    }

    private void MoveToHeapCards(int[] heapCards)
    {
        for (int i = 0; i < heapCards.Length; i++)
        {
            if (heapCards[i] != 0)
            {
                GameObject card = photonCard[heapCards[i]];
                card.transform.position = new Vector2(UnityEngine.Random.Range(4.0f, 5.5f), UnityEngine.Random.Range(-0.5f, 0.5f));
                card.transform.rotation = Quaternion.Euler(0, 0, UnityEngine.Random.Range(-30, 30));
            }
        }
    }

    private void HideAllPhotonCards()
    {
        for (int i = 1; i < photonCard.Length; i++)
        {
            photonCard[i].gameObject.GetComponent<PhotonView>().RPC("TogglePhotonObject", RpcTarget.AllViaServer, false);
        }
    }

    private IEnumerator OpenTurnCardAnimation(object[] data, float delay)
    {
        PhotonView photon = photonCard[(int)data[2]].gameObject.GetComponent<PhotonView>();

        photon.RPC("ShowCard", RpcTarget.AllViaServer);
        yield return new WaitForSeconds(delay);
        photon.RPC("HideCard", RpcTarget.AllViaServer);
        PlayerTakeAllPoolCards(data);
    }
}
