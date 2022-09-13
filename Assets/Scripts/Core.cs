using UnityEngine;
using System.Collections.Generic;

public class Core : MonoBehaviour
{
    public const byte EVENT_SEND_PLAYERS_CARD = 1;
    public const byte EVENT_UPDATE_AND_SEND_CARDS_POOL = 2;
    public const byte EVENT_UPDATE_AND_SEND_TURN_CARD = 3;
    public const byte EVENT_UPDATE_TURN = 4;
    public const byte EVENT_END_TURN_ATTACK = 5;
    public const byte EVENT_END_TURN_DEFENCE = 6;
    public const byte EVENT_CARDS_MOVE_TO_HEAP = 7;
    public const byte EVENT_ADD_TURN_POOL_CARDS_FOR_PLAYER = 8;
    public const byte EVENT_REMOVE_SENT_CARD_FROM_SENDER = 9;
    public const byte EVENT_ADD_CARDS_TO_TURN_POOL = 10;
    public const byte EVENT_REMOVE_SET_FROM_PLAYER = 11;
    public const byte EVENT_END_GAME = 12;
}

[System.Serializable]
public class PlayerCards
{
    public List<int> cards;

    public PlayerCards(List<int> newList)
    {
        cards = newList;
    }
}

public enum TurnType
{
    First,      //  0
    Player,     //  1
    Pool,       //  2
    Photon,     //  3
    Last        //  4
}
