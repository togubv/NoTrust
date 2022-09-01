using UnityEngine;

public class GameCard : MonoBehaviour
{
    public int _id;
    public int _value;
    public int _suit;
    public Sprite _spriteSuit;
    public Sprite _spriteValue;

    public int id => _id;
    public int value => _value;
    public int suit => _suit;
    public Sprite spriteSuit => _spriteSuit;
    public Sprite spriteValue => _spriteValue;
}