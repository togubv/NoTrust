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

    public void SetCardValue(int id, int value, int suit, int spriteSuit, int spriteValue)
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
        image[0].color = new Color(1, 1, 1, 1);
    }

    public void DisableCard()
    {
        boxCollider.enabled = false;
        image[0].color = new Color(0.5f, 0.5f, 0.5f, 0.5f);
    }

    private void SetCardSprites()
    {
        for (int i = 1; i < image.Length; i++)
        {
            if (i % 2 != 0)
            {
                image[i].sprite = sprites[13];
                continue;
            }

            image[i].sprite = sprites[spriteValue];
        }
    }
}
