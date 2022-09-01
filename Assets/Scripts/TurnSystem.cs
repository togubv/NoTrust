using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TurnSystem : MonoBehaviourPunCallbacks
{
    [SerializeField] private Croupier croupier;

    [SerializeField] private GameObject cardPoolWindow;
    [SerializeField] private Button buttonThrow;
    [SerializeField] private CardUI panelTurnCard;

    public delegate void ConfirmEndTurnHandler(int player, Card[] cards);
    public event ConfirmEndTurnHandler ConfirmEndTurnHandlerEvent;

    public delegate void ToggleCardPoolWindowHandler(bool toggle);
    public event ToggleCardPoolWindowHandler ToggleCardPoolWindowHandlerEvent;

    public delegate void UpdateTurnHandler(int turn);
    public event UpdateTurnHandler UpdateTurnHandlerEvent;

    private Camera mainCamera;
    private Card[] clickedCard;
    private int currentTurn;
    public TurnType turnType;
    public CardUI turnCard;

    public List<Card> turnPool;

    private void Start()
    {
        mainCamera = Camera.main;
        clickedCard = new Card[4];
    }

    private void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            ClickToCard();
        }
    }

    public void ClickToTurnTypeAttack()
    {
        SetTurnType(TurnType.Attack);

        AddCardsToTurnPool(clickedCard);
    }

    public void ClickToTurnTypeDefence()
    {
        SetTurnType(TurnType.Defence);
    }

    public void ClickToButtonThrow()
    {
        switch (turnType)
        {
            case TurnType.First:
                ToggleCardPoolWindowHandlerEvent?.Invoke(true);
                break;

            case TurnType.Attack:
                SetTurnCard(turnCard);
                break;
        }
    }

    private void ClickToCard()
    {
        Vector2 cursor = mainCamera.ScreenToWorldPoint(Input.mousePosition);

        RaycastHit2D hit = Physics2D.Raycast(cursor, Vector2.zero, Mathf.Infinity, Core.layerMaskCard);

        if (hit)
        {
            hit.collider.gameObject.TryGetComponent<Card>(out Card card);
            hit.collider.gameObject.TryGetComponent<CardUI>(out CardUI cardUI);

            if (card != null)
            {
                if (turnType == TurnType.Defence)
                {
                    StartCoroutine(AnimationShowCard(card));
                    return;
                }

                PickCardsForDrop(hit.collider.gameObject);
                UpdateButtonThrow();
                return;
            }
            
            if (cardUI != null)
            {
                turnCard = cardUI;
                SetTurnCard(cardUI);
                return;
            }
        }
    }

    private void SetTurnCard(CardUI cardUI)
    {
        Core.layerMaskCard = 1 << 0;

        ToggleTurnCard(cardUI, true);
        ConfirmEndTurnHandlerEvent?.Invoke(currentTurn, clickedCard);
        HideThrowedCards(clickedCard);

        for (int i = 0; i < clickedCard.Length; i++)
        {
            if (clickedCard[i] != null)
            {
                int id = clickedCard[i].id;
                Card photonCard = croupier.PhotonCards[id];
                photonCard.gameObject.transform.rotation = Quaternion.identity;
                photonCard.gameObject.SetActive(true);
                photonCard.gameObject.transform.position = new Vector2(i * 1.2f - 2.4f, 0);
                photonCard.HideCard();
                AddCardsToTurnPool(clickedCard);
            }
        }

        UpdateButtonThrow();
        ToggleCardPoolWindowHandlerEvent?.Invoke(false);
        UpdateTurn(1);
    }

    private void AddCardsToTurnPool(Card[] cards)
    {
        for (int i = 0; i < cards.Length; i++)
        {
            if (cards[i] != null)
            {
                AddAndMoveClickedCardToTurnPool(cards);
                clickedCard = new Card[4];
                return;
            }
        }
    }

    private void ToggleTurnCard(CardUI cardUI, bool toggle)
    {
        if (toggle)
        {
            panelTurnCard.SetCardValue(cardUI.id, cardUI.value, cardUI.suit, cardUI.spriteSuit, cardUI.spriteValue, false);
            panelTurnCard.gameObject.SetActive(toggle);
            panelTurnCard.RefreshSprites();
            return;
        }

        panelTurnCard.gameObject.SetActive(toggle);
    }

    private void PickCardsForDrop(GameObject go)
    {
        go.TryGetComponent<Card>(out Card card);

        if (card == null)
            return;

        for (int i = 0; i < clickedCard.Length; i++)
        {
            if (card == clickedCard[i])
            {
                clickedCard[i] = null;
                ToggleVisibleClickedCard(card, false);
                return;
            }
        }

        for (int i = 0; i < clickedCard.Length; i++)
        {
            if (clickedCard[i] == null)
            {
                clickedCard[i] = card;
                ToggleVisibleClickedCard(card, true);
                return;
            }
        }
    }

    private void UpdateButtonThrow()
    {
        int count = 0;

        for (int i = 0; i < clickedCard.Length; i++)
        {
            if (clickedCard[i] != null)
            {
                count++;
            }
        }

        if (count > 0)
        {
            buttonThrow.interactable = true;
            return;
        }

        buttonThrow.interactable = false;
    }

    private void ToggleVisibleClickedCard(Card card, bool toggle)
    {
        Transform transform = card.gameObject.transform;

        if (toggle)
        {
            transform.position = new Vector2(transform.position.x, transform.position.y + 0.5f);
            return;
        } 

        transform.position = new Vector2(transform.position.x, transform.position.y - 0.5f);
    }

    private void HideThrowedCards(Card[] cards)
    {
        for (int i = 0; i < cards.Length; i++)
        {
            if (cards[i] != null)
            {
                ToggleVisibleClickedCard(cards[i], false);
                cards[i].gameObject.SetActive(false);
            }
        }
    }

    private void SetTurnType(TurnType type)
    {
        switch (type)
        {
            case TurnType.First:
                Core.layerMaskCard = 1 << 8;
                break;

            case TurnType.Attack:
                Core.layerMaskCard = 1 << 8;
                break;

            case TurnType.Defence:
                Core.layerMaskCard = 1 << 9;
                break;
        }

        turnType = type;
    }

    private void TakeAllCardInPool(Card card)
    {
        AddCardsToTurnPool(clickedCard);

        if (card.value == turnCard.value)
        {
            for (int i = 0; i < turnPool.Count; i++)
            {
                int id = turnPool[i].id;
                croupier.AddPlayerCard(currentTurn, turnPool[i]);
                croupier.PhotonCards[id].gameObject.SetActive(false);
            }

            turnPool = new List<Card>();

            UpdateTurn(1);
            return;
        }

        for (int i = 0; i < turnPool.Count; i++)
        {
            int player = currentTurn - 1;

            if (player < 0)
            {
                player = Core.playersCount - 1;
            }
            croupier.AddPlayerCard(player, turnPool[i]);

            int id = turnPool[i].id;
            croupier.PhotonCards[id].gameObject.SetActive(false);
        }

        turnPool = new List<Card>();
        SetTurnType(TurnType.First);
        UpdateTurn(0);
    }

    private void AddAndMoveClickedCardToTurnPool(Card[] cards)
    {
        foreach (Card card in cards)
        {
            if (card != null)
            {
                int id = card.id;
                turnPool.Add(card);
                card.gameObject.SetActive(false);

                croupier.PhotonCards[id].gameObject.transform.position = new Vector2(Random.Range(5.5f, 6.5f), Random.Range(-0.5f, 0.5f));
                croupier.PhotonCards[id].gameObject.transform.rotation = Quaternion.Euler(0, 0, Random.Range(-30, 30));
                croupier.PhotonCards[id].HideCard();
            }
        }
    }

    private void UpdateTurn(int add)
    {
        int turn = currentTurn + add;

        if (turn >= Core.playersCount)
        {
            turn = 0;
        }

        if (turn < 0)
        {
            turn = Core.playersCount - 1;
        }

        currentTurn = turn;
        UpdateTurnHandlerEvent?.Invoke(turn);
        Debug.Log("Current turn: Player " + turn);
    }

    private IEnumerator AnimationShowCard(Card card)
    {
        card.ShowCard();
        yield return new WaitForSeconds(2.0f);
        TakeAllCardInPool(card);
    }
}
