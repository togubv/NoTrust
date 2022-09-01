using UnityEngine;

public class Card : GameCard
{
    private SpriteRenderer[] spriteR;   //  [0] - shirt, [1] - suit, [2] - value, [3] - mini suit, [4] - mini value
    private bool _color;

    private void OnEnable()
    {
        if (spriteR == null)
        {
            spriteR = GetComponentsInChildren<SpriteRenderer>();
        }

        RefreshSprites();
    }

    public void SetCardValue(int id, int value, int suit, Sprite spriteSuit, Sprite spriteValue, bool color)
    {
        _id = id;
        _value = value;
        _suit = suit;
        _spriteSuit = spriteSuit;
        _spriteValue = spriteValue;
        _color = color;

        if (spriteR != null)
        {
            RefreshSprites();
        }
    }

    public void RefreshSprites()
    {
        SetCardSprites();

        if (_color)
        {
            SetCardColor(Color.red);
            return;
        }

        SetCardColor(Color.black);
    }

    public void ShowCard()
    {
        for (int i = 1; i < spriteR.Length; i++)
        {
            spriteR[i].enabled = true;
        }
    }

    public void HideCard()
    {
        for (int i = 1; i < spriteR.Length; i++)
        {
            spriteR[i].enabled = false;
        }
    }

    private void SetCardColor(Color color)
    {
        for (int i = 1; i < spriteR.Length; i++)
        {
            spriteR[i].color = color;
        }
    }

    private void SetCardSprites()
    {
        for (int i = 1; i < spriteR.Length; i++)
        {
            if (i % 2 != 0)
            {
                spriteR[i].sprite = _spriteSuit;
                continue;
            }

            spriteR[i].sprite = _spriteValue;
        }
    }
}
