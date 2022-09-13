using ExitGames.Client.Photon;
using Photon.Pun;
using Photon.Realtime;
using System.Collections;
using UnityEngine;

public class TurnSystem : MonoBehaviour, IOnEventCallback
{
    [SerializeField] private Croupier croupier;
    [SerializeField] private CardUI turnCard;

    private int[] currentAttackCards = new int[4]; 
    private RaiseEventOptions options;
    private SendOptions sendOptions;
    private int playersCount;
    private int currentTurn;
    private int turnCardID;

    public void OnEvent(EventData photonEvent)
    {
        switch (photonEvent.Code)
        {
            case Core.EVENT_END_TURN_ATTACK:
                object[] turnDataAttack = (object[])photonEvent.CustomData;
                GetTurnData((int)turnDataAttack[0], (int[])turnDataAttack[1], (int)turnDataAttack[2], (int)turnDataAttack[3], (int)turnDataAttack[4]);
                break;

            case Core.EVENT_END_TURN_DEFENCE:
                object[] turnDataDefence = (object[])photonEvent.CustomData;
                GetTurnData((int)turnDataDefence[0], (int[])turnDataDefence[1], (int)turnDataDefence[2], (int)turnDataDefence[3], (int)turnDataDefence[4]);
                break;
        }
    }

    private void Awake()
    {
        if (PhotonNetwork.IsMasterClient)
        {
            options = new RaiseEventOptions { Receivers = ReceiverGroup.All };
            sendOptions = new SendOptions { Reliability = true };
            playersCount = PhotonNetwork.PlayerList.Length;
            StartCoroutine(DelayedUpdateTurn(2.0f));
        }

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

    private IEnumerator DelayedUpdateTurn(float delay)
    {
        yield return new WaitForSeconds(delay);
        UpdateTurn(0, 0);
    }

    private void UpdateTurn(int type, int step)
    {
        currentTurn += step;

        if (currentTurn > playersCount - 1)
        {
            currentTurn = 0;
        }

        if (currentTurn < 0)
        {
            currentTurn = playersCount - 1;
        }

        object[] data = new object[] { currentTurn, type };

        PhotonNetwork.RaiseEvent(Core.EVENT_UPDATE_TURN, data, options, sendOptions);
    }


    private void GetTurnData(int sender, int[] sentCards, int sentTurnCard, int openCardValue, int openCardID)
    {
        int currentPlayer = sender;
        int previousPlayer;

        if (sender - 1 < 0) { previousPlayer = playersCount - 1; }
        else { previousPlayer = sender - 1; }

        if (sentCards != null)
        {
            SetSentAttackCards(sentCards);
            RemoveSentAttackCardsFromSender(currentPlayer, sentCards);
        }

        if (sentTurnCard > -1)
        {
            UpdateAndSendToPlayersTurnCard(sentTurnCard);
        }

        if (openCardValue > -1)
        {
            if (turnCardID == openCardValue)
            {
                AddTurnPoolCardsToPlayer(currentPlayer, openCardValue, openCardID);
                StartCoroutine(OpenCardAnimation(3.0f, 0, 1));
            }

            else
            {
                AddTurnPoolCardsToPlayer(previousPlayer, openCardValue, openCardID);
                StartCoroutine(OpenCardAnimation(3.0f, 0, 0));
            }
            return;
        }

        UpdateTurn(1, 1);
    }



    private void SetSentAttackCards(int[] attackCards)
    {
        for (int i = 0; i < currentAttackCards.Length; i++)
        {
            if (currentAttackCards[i] != 0)
            {
                AttackCardsMoveToHeap(currentAttackCards);
                break;
            }
        }

        for (int i = 0; i < attackCards.Length; i++)
        {
            currentAttackCards[i] = attackCards[i];

            if (attackCards[i] != 0)
            {
                GameObject photonCard = croupier.PhotonCard[attackCards[i]];
                photonCard.gameObject.GetComponent<PhotonView>().RPC("TogglePhotonObject", RpcTarget.AllViaServer, true);
                photonCard.gameObject.GetComponent<PhotonView>().RPC("HideCard", RpcTarget.AllViaServer);
                photonCard.gameObject.transform.rotation = Quaternion.identity;
                photonCard.gameObject.transform.position = new Vector2(i * 1.2f - 2.0f, 0);
            }
        }

        PhotonNetwork.RaiseEvent(Core.EVENT_ADD_CARDS_TO_TURN_POOL, attackCards, options, sendOptions);
    }

    private void AttackCardsMoveToHeap(int[] attackCards)
    {
        PhotonNetwork.RaiseEvent(Core.EVENT_CARDS_MOVE_TO_HEAP, attackCards, options, sendOptions);
    }

    private void AddTurnPoolCardsToPlayer(int player, int openedCard, int openedCardID)
    {
        object[] data = { player, openedCard, openedCardID };
        PhotonNetwork.RaiseEvent(Core.EVENT_ADD_TURN_POOL_CARDS_FOR_PLAYER, data, options, sendOptions);
    }

    private void RemoveSentAttackCardsFromSender(int player, int[] cards)
    {
        object[] removedCards = new object[] { player, cards };
        PhotonNetwork.RaiseEvent(Core.EVENT_REMOVE_SENT_CARD_FROM_SENDER, removedCards, options, sendOptions);
    }

    private void UpdateAndSendToPlayersTurnCard(int cardID)
    {
        turnCardID = cardID;

        object data = (int)turnCardID;
        PhotonNetwork.RaiseEvent(Core.EVENT_UPDATE_AND_SEND_TURN_CARD, data, options, sendOptions);
    }

    private IEnumerator OpenCardAnimation(float delay, int type, int step)
    {
        yield return new WaitForSeconds(delay);
        UpdateAndSendToPlayersTurnCard(-1);
        UpdateTurn(type, step);
    }
}
