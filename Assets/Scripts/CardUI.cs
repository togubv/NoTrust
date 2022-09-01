using UnityEngine;
using UnityEngine.UI;

public class CardUI : GameCard
{
    private Image[] image;   //  [0] - shirt, [1] - suit, [2] - value, [3] - mini suit, [4] - mini value

    private BoxCollider2D boxCollider;

    private void OnEnable()
    {
        if (image == null)
        {
            image = GetComponentsInChildren<Image>();
        }

        if (boxCollider == null)
        {
            boxCollider = GetComponent<BoxCollider2D>();
        }

        RefreshSprites();
    }

    public void RefreshSprites()
    {
        SetCardSprites();
    }

    public void SetCardValue(int id, int value, int suit, Sprite spriteSuit, Sprite spriteValue, bool color)
    {
        _id = id;
        _value = value;
        _suit = suit;
        _spriteSuit = spriteSuit;
        _spriteValue = spriteValue;
        
        if (image != null)
        {
            RefreshSprites();
        }
    }

    public void EnableCard()
    {
        boxCollider.enabled = true;

        for (int i = 1; i < image.Length; i++)
        {
            Color color = image[i].color;
            image[i].color = new Color(color.r, color.g, color.b, 50);
        }
    }

    public void DisableCard()
    {
        boxCollider.enabled = false;

        for (int i = 1; i < image.Length; i++)
        {
            Color color = image[i].color;
            image[i].color = new Color(color.r, color.g, color.b, 255);
        }
    }

    private void SetCardColor(Color color)
    {
        for (int i = 1; i < image.Length; i++)
        {
            image[i].color = color;
        }
    }

    private void SetCardSprites()
    {
        for (int i = 1; i < image.Length; i++)
        {
            if (i % 2 != 0)
            {
                image[i].sprite = _spriteSuit;
                continue;
            }

            image[i].sprite = _spriteValue;
        }
    }
}
