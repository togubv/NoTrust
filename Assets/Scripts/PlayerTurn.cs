using ExitGames.Client.Photon;
using Photon.Pun;
using Photon.Realtime;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PlayerTurn : MonoBehaviour, IOnEventCallback
{
    [SerializeField] private TurnSystem turnSystem;

    [SerializeField] private Card prefabCard;
    [SerializeField] private CardUI prefabCardPool;

    [SerializeField] private Transform cardsPoolUI;
    [SerializeField] private CardUI turnCardUI;

    public delegate void ShowCardPoolWindowHandler(bool toggle, bool[] pool);
    public event ShowCardPoolWindowHandler ShowCardPoolWindowHandlerEvent;

    public delegate void ToggleButtonThrowHandler(bool toggle);
    public event ToggleButtonThrowHandler ToggleButtonThrowHandlerEvent;

    public delegate void TogglePanelTurnTypeButtonsHandler(bool toggle, bool btnAttack, bool btnDefence);
    public event TogglePanelTurnTypeButtonsHandler TogglePanelTurnTypeButtonsHandlerEvent;

    public delegate void TogglePanelEndGameHandler(string winnerName);
    public event TogglePanelEndGameHandler TogglePanelEndGameHandlerEvent;

    public static LayerMask layerMaskCard = 1 << 8;

    public CardUI[] CardsPool => cardsPool;

    private Camera cameraMain;
    private int[] pickedCards = new int[4];
    private TurnType turnType;

    private List<int> cardsIDs;
    private bool[] cardsPoolIDs = new bool[12];
    private Card[] cards = new Card[53];
    private CardUI[] cardsPool = new CardUI[12];
    private int ownerID;
    private int turnCardID;
    private bool isMyTurn;
    private int[] playersCardsCount;

    private RaiseEventOptions options;
    private SendOptions sendOptions;

    public void OnEvent(EventData photonEvent)
    {
        switch (photonEvent.Code)
        {
            case Core.EVENT_SEND_PLAYERS_CARD:
                object[] data = (object[])photonEvent.CustomData;

                playersCardsCount = (int[])data[2];
                if (ownerID == (int)data[0])
                {
                    UpdatePlayerCards((int[])data[1]);
                }
                break;

            case Core.EVENT_UPDATE_AND_SEND_CARDS_POOL:
                object poolData = photonEvent.CustomData;

                cardsPoolIDs = (bool[])poolData;

                UpdatePoolCards(cardsPoolIDs);

                break;

            case Core.EVENT_UPDATE_AND_SEND_TURN_CARD:
                object turnData = photonEvent.CustomData;

                UpdateTurnCard((int)turnData);

                break;

            case Core.EVENT_UPDATE_TURN:
                object[] newTurnData = (object[])photonEvent.CustomData;

                int currentTurn = (int)newTurnData[0];
                int newTurnType = (int)newTurnData[1];

                if (ownerID == currentTurn)
                {
                    isMyTurn = true;

                    if (CheckForAttackPossibility())
                    {
                        OnPlayerTurn(newTurnType);
                        break;
                    }

                    OnPlayerTurn(4);
                }

                break;

            case Core.EVENT_END_GAME:
                EndGame((int)photonEvent.CustomData);
                break;
        }
    }

    private void Awake()
    {
        options = new RaiseEventOptions { Receivers = ReceiverGroup.All };
        sendOptions = new SendOptions { Reliability = true };

        cameraMain = Camera.main;

        GeneratePlayerCards();
        GenerateCardsPool();

        for (int i = 0; i < PhotonNetwork.PlayerList.Length; i++)
        {
            if (PhotonNetwork.PlayerList[i] == PhotonNetwork.LocalPlayer)
            {
                ownerID = i;
                break;
            }
        }
    }

    private void OnEnable()
    {
        PhotonNetwork.NetworkingClient.EventReceived += OnEvent;
    }

    private void OnDisable()
    {
        PhotonNetwork.NetworkingClient.EventReceived += OnEvent;
    }

    private void Update()
    {
        if (Input.GetMouseButtonDown(0) && isMyTurn)
        {
            ClickToCard();
        }
    }

    private void OnPlayerTurn(int typeID)
    {
        SetTurnLayer(typeID);

        if (typeID == 0)
        {
            TogglePanelTurnTypeButtonsHandlerEvent?.Invoke(false, true, true);
            return;
        }

        if (typeID == 4)
        {
            Debug.Log("LAST TURN");
            TogglePanelTurnTypeButtonsHandlerEvent?.Invoke(true, false, true);
            return;
        }

        TogglePanelTurnTypeButtonsHandlerEvent?.Invoke(true, true, true);
    }

    private bool CheckForAttackPossibility()
    {
        if (cardsIDs.Count < 1)
        {
            return false;
        }
        return true;
    }

    public void ClickToTurnTypeAttack()
    {
        SetTurnLayer(1);
    }

    public void ClickToTurnTypeDefence()
    {
        SetTurnLayer(3);
    }

    public void ClickToButtonThrow()
    {
        switch (turnType)
        {
            case TurnType.First:
                ShowCardPoolWindowHandlerEvent?.Invoke(true, cardsPoolIDs);
                SetTurnLayer(2);
                break;

            default:
                EndTurnAttack(-1);
                break;
        }
    }

    private void ClickToCard()
    {
        Vector2 mousePos = cameraMain.ScreenToWorldPoint(Input.mousePosition);

        RaycastHit2D hit = Physics2D.Raycast(mousePos, Vector2.zero, Mathf.Infinity, layerMaskCard);

        if (hit)
        {
            hit.collider.gameObject.TryGetComponent<Card>(out Card cardTable);
            hit.collider.gameObject.TryGetComponent<CardUI>(out CardUI cardUI);

            if (cardTable != null)
            {
                if (turnType == TurnType.Photon)
                {
                    EndTurnDefence(cardTable.value, cardTable.id);
                    return;
                }

                PrePickCardsForDrop(cardTable);
                UpdateButtonThrow();
                return;
            }

            if (cardUI != null)
            {
                EndTurnAttack(cardUI.value);
                ShowCardPoolWindowHandlerEvent?.Invoke(false, null);
                return;
            }
        }
    }

    private void PrePickCardsForDrop(Card card)
    {
        if (card == null)
            return;

        for (int i = 0; i < pickedCards.Length; i++)
        {
            if (pickedCards[i] == card.id)
            {
                pickedCards[i] = 0;
                ToggleClickedCardVisibility(card.id, false);
                return;
            }
        }

        for (int i = 0; i < pickedCards.Length; i++)
        {
            if (pickedCards[i] == 0)
            {
                pickedCards[i] = card.id;
                ToggleClickedCardVisibility(card.id, true);
                return;
            }
        }
    }

    private void EndTurnAttack(int turnCardID)
    {
        isMyTurn = false;

        DeactivatePickedCards(pickedCards);

        int[] newPickedCards = pickedCards;

        pickedCards = new int[4];

        UpdateButtonThrow();

        object[] turnData = new object[] { ownerID, newPickedCards, turnCardID, -1, -1};

        PhotonNetwork.RaiseEvent(Core.EVENT_END_TURN_ATTACK, turnData, options, sendOptions);       
    }

    private void EndTurnDefence(int openedCardValue, int openedCardID)
    {
        isMyTurn = false;

        object[] turnData = new object[] { ownerID, null, -1, openedCardValue, openedCardID };

        PhotonNetwork.RaiseEvent(Core.EVENT_END_TURN_DEFENCE, turnData, options, sendOptions);
    }

    private void UpdatePlayerCards(int[] newCards)
    {
        cardsIDs = new List<int>();
        cardsIDs.AddRange(newCards);

        for (int i = 1; i < cards.Length; i++)
        {
            cards[i].gameObject.SetActive(false);
        }

        for (int i = 0; i < cardsIDs.Count; i++)
        {
            cards[cardsIDs[i]].gameObject.SetActive(true);
        }

        SortCards();
        CheckForSetAndRemove();
    }

    private void UpdatePoolCards(bool[] poolCards)
    {
        cardsPoolIDs = poolCards;
    }

    private void UpdateTurnCard(int newTurnCard)
    {
        if (newTurnCard < 0)
        {
            turnCardUI.gameObject.SetActive(false);
            return;
        }

        turnCardID = newTurnCard;

        turnCardUI.gameObject.SetActive(true);

        turnCardUI.SetCardValue(0, turnCardID, 0, 0, turnCardID);
    }

    private void SortCards()
    {
        float x = ((int)(cardsIDs.Count / 2)) * -0.3f;

        for (int i = 0; i < cardsIDs.Count; i++)
        {
            cards[cardsIDs[i]].gameObject.transform.position = new Vector2(x + 0.3f * i, -4);
        }
    }

    private void DeactivatePickedCards(int[] deavtivatedCards)
    {
        for (int i = 0; i < deavtivatedCards.Length; i++)
        {
            if (deavtivatedCards[i] != 0)
            {
                ToggleClickedCardVisibility(deavtivatedCards[i], false);
            }
        }
    }

    private void ToggleClickedCardVisibility(int i, bool toggle)
    {
        Transform transform = cards[i].gameObject.transform;

        if (toggle)
        {
            transform.position = new Vector2(transform.position.x, transform.position.y + 0.5f);
            return;
        }

        transform.position = new Vector2(transform.position.x, transform.position.y - 0.5f);
    }

    private void UpdateButtonThrow()
    {
        int count = 0;

        for (int i = 0; i < pickedCards.Length; i++)
        {
            if (pickedCards[i] != 0)
            {
                count++;
            }
        }

        if (count > 0)
        {
            ToggleButtonThrowHandlerEvent?.Invoke(true);
            return;
        }

        ToggleButtonThrowHandlerEvent?.Invoke(false);
    }

    private void SetTurnLayer(int type)
    {
        switch (type)
        {
            case 0:
                turnType = TurnType.First;
                layerMaskCard = 1 << 8;
                break;

            case 1:
                turnType = TurnType.Player;
                layerMaskCard = 1 << 8;
                break;

            case 2:
                turnType = TurnType.Pool;
                layerMaskCard = 1 << 9;
                break;

            case 3:
                turnType = TurnType.Photon;
                layerMaskCard = 1 << 10;
                break;

            case 4:
                turnType = TurnType.Last;
                break;

            case 5:
                layerMaskCard = 1 << 5;
                break;
        }
    }

    private void GeneratePlayerCards()
    {
        GameObject playerCards = new GameObject("Player Cards");

        for (int i = 0; i < 13; i++)
        {
            for (int j = 0; j < 4; j++)
            {
                Card card = Instantiate(prefabCard, playerCards.transform);
                bool sprCardColor = (j < 2) ? false : true;
                card.Initialize(i * 4 + j + 1, i, j, i, j + 13, sprCardColor);
                cards[i * 4 + j + 1] = card;
                card.gameObject.SetActive(false);
            }
        }
    }

    private void GenerateCardsPool()
    {
        for (int i = 0; i < cardsPool.Length; i++)
        {
            CardUI card = Instantiate(prefabCardPool, cardsPoolUI);
            cardsPool[i] = card;

            card.SetCardValue(i, i, 0, 0, i);
        }
    }

    private void CheckForSetAndRemove()
    {
        List<int> turnCardID = new List<int>();
        List<int> dropCards = new List<int>();

        for (int i = 0; i < cardsIDs.Count; i++)
        {
            List<int> set = new List<int>();

            for (int j = 0; j < cardsIDs.Count; j++)
            {
                if (i != j && cards[cardsIDs[i]].value != 12 && cards[cardsIDs[i]].value == cards[cardsIDs[j]].value)
                {
                    if (set.Contains(cards[cardsIDs[i]].id) == false)
                    {
                        set.Add(cards[cardsIDs[i]].id);
                    }
                    set.Add(cards[cardsIDs[j]].id);
                }
            }

            if (set.Count == 4)
            {
                turnCardID.Add(cards[set[0]].value);
                dropCards.AddRange(set);
            }
        }

        if (turnCardID.Count > 0)
        {
            int[] newTurnCardID = turnCardID.ToArray();
            int[] newDropCards = dropCards.ToArray();
            object[] setData = { ownerID, newDropCards, newTurnCardID };
            PhotonNetwork.RaiseEvent(Core.EVENT_REMOVE_SET_FROM_PLAYER, setData, options, sendOptions);
        }
    }

    private void EndGame(int player)
    {
        SetTurnLayer(5);
        string winnerName = PhotonNetwork.PlayerList[player].NickName;
        TogglePanelEndGameHandlerEvent?.Invoke(winnerName);
    }
}
