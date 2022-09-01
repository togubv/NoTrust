using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Croupier : MonoBehaviour
{
    [SerializeField] private TurnSystem turnSystem;

    [SerializeField] private Card prefabCard;
    [SerializeField] private CardUI prefabCardUI;
    [SerializeField] private Card prefabCardTurn;
    [SerializeField] private Sprite[] spriteValue;
    [SerializeField] private Sprite[] spriteSuit;
    [SerializeField] private Transform cardsPoolUI;

    public List<Card> cards;
    public List<PlayerCards> playerCards;
    public List<CardUI> cardsPool;
    public List<Card> photonCards;

    public List<Card> PhotonCards => photonCards;

    private void Start()
    {
        turnSystem.ConfirmEndTurnHandlerEvent += ThrowCards;
        turnSystem.UpdateTurnHandlerEvent += SortCardForAllPlayers;

        SetSprites();
        GenerateCards();
        GenerateCardsPool();
        DistributeCard(Core.playersCount);
        SortCardForAllPlayers(0);
    }

    public void RemovePlayerCard(int player, Card card)
    {
        if (playerCards[player].cards.Contains(card))
        {
            playerCards[player].cards.Remove(card);
        }

       //SortCardForPlayer(player);
    }

    public void AddPlayerCard(int player, Card card)
    {
        playerCards[player].cards.Add(card);
        //SortCardForPlayer(player);
    }

    private void ThrowCards(int player, Card[] cards)
    {
        for (int i = 0; i < cards.Length; i++)
        {
            if (cards[i] != null)
            {
                RemovePlayerCard(player, cards[i]);
            }
        }

        //SortCardForPlayer(player);
    }

    private void SetSprites()
    {
        Sprite[] sprites = Resources.LoadAll<Sprite>("Sprites/Card");

        spriteSuit = new Sprite[4];
        spriteValue = new Sprite[sprites.Length - 4];

        for (int i = 0; i < 13; i++)
        {
            spriteValue[i] = sprites[i];
        }

        for (int i = 0; i < 4; i++)
        {
            spriteSuit[i] = sprites[i + spriteValue.Length];
        }
    }

    private void GenerateCards()
    {
        GameObject goCards = new GameObject("Cards");
        GameObject photonObjects = new GameObject("Photon Objects");
        for (int i = 0; i < 13; i++)
        {
            for (int j = 0; j < 4; j++)
            {
                Card card = Instantiate(prefabCard, goCards.transform);
                cards.Add(card);

                Vector2 position = new Vector2(0, 0);
                Card photonCard = Instantiate(prefabCardTurn, position, Quaternion.identity, photonObjects.transform);  // PHOTON
                photonCards.Add(photonCard);
                photonCard.gameObject.SetActive(false);

                if (j < 2)
                {
                    card.SetCardValue(i * 4 + j, i, j, spriteSuit[j], spriteValue[i], false);

                    photonCard.SetCardValue(i * 4 + j, i, j, spriteSuit[j], spriteValue[i], false);
                    continue;
                }

                card.SetCardValue(i * 4 + j, i, j, spriteSuit[j], spriteValue[i], true);

                photonCard.SetCardValue(i * 4 + j, i, j, spriteSuit[j], spriteValue[i], false);
                continue;
            }
        }
    }

    private void GenerateCardsPool()
    {
        for (int i = 0; i < 12; i++)
        {
            CardUI card = Instantiate(prefabCardUI, cardsPoolUI);
            cardsPool.Add(card);

            card.SetCardValue(i, i, 0, spriteSuit[0], spriteValue[i], false);
        }
    }

    private void DistributeCard(int count)
    {
        for (int i = 0; i < count; i++)
        {
            playerCards.Add(new PlayerCards());
            playerCards[i].cards = new List<Card>();
        }

        while (cards.Count > 0)
        {
            for (int i = 0; i < count; i++)
            {
                int randomCard = Random.Range(0, Random.Range(0, cards.Count));
                playerCards[i].cards.Add(cards[randomCard]);
                cards.RemoveAt(randomCard);

                if (cards.Count < 1)
                {
                    return;
                }
            }
        }
    }

    private void SortCardForPlayer(int player)
    {
        for (int i = playerCards[player].cards.Count - 1; i > -1; i--)
        {
            if (playerCards[player].cards[i] == null)
            {
                playerCards[player].cards.RemoveAt(i);
            }
        }

        float x = ((int)(playerCards[player].cards.Count / 2)) * -0.3f;
        float y = 0;

        if (player == 0)
        {
            y = -4;
        }
        else
        {
            y = 4;
        }
        for (int i = 0; i < playerCards[player].cards.Count; i++)
        {
            playerCards[player].cards[i].gameObject.SetActive(true);
            playerCards[player].cards[i].gameObject.transform.position = new Vector2(x + 0.3f * i, y);
        }
    }

    private void SortCardForAllPlayers(int turn)
    {
        for (int i = 0; i < Core.playersCount; i++)
        {
            Debug.Log("SORT PLAYER: " + i);
            SortCardForPlayer(i);
        }
    }
}

[System.Serializable]
public class PlayerCards
{
    public List<Card> cards;
}
