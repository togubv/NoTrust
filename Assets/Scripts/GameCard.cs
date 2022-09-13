using UnityEngine;

public class GameCard : MonoBehaviour
{
    [SerializeField] protected Sprite[] sprites;

    public int _id;
    public int _value;
    public int _suit;
    public int _spriteSuit;
    public int _spriteValue;
    
    public int id => _id;
    public int value => _value;
    public int suit => _suit;
    public int spriteSuit => _spriteSuit;
    public int spriteValue => _spriteValue;
}