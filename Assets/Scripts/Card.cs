using Photon.Pun;
using UnityEngine;

[RequireComponent(typeof(PhotonView))]
public class Card : GameCard
{
    private SpriteRenderer[] spriteR;   //  [0] - shirt, [1] - suit, [2] - value, [3] - mini suit, [4] - mini value
    private bool _color;
    private PhotonView photonView;
    private BoxCollider2D boxCollider;

    private void Awake()
    {
        boxCollider = GetComponent<BoxCollider2D>();
        photonView = GetComponent<PhotonView>();

        if (photonView.InstantiationData != null)
        {
            _id = (int)photonView.InstantiationData[0];
            _value = (int)photonView.InstantiationData[1];
            _suit = (int)photonView.InstantiationData[2];
            _spriteSuit = (int)photonView.InstantiationData[3];
            _spriteValue = (int)photonView.InstantiationData[4];
            _color = (bool)photonView.InstantiationData[5];

            if (spriteR != null)
            {
                RefreshSprites();
            }
        }
    }

    private void OnEnable()
    {
        if (spriteR == null)
        {
            spriteR = GetComponentsInChildren<SpriteRenderer>();
        }

        RefreshSprites();
    }

    public void Initialize(int id, int value, int suit, int sprValue, int sprSuit, bool color)
    {
        _id = id;
        _value = value;
        _suit = suit;
        _spriteSuit = sprSuit;
        _spriteValue = sprValue;
        _color = color;

        if (spriteR != null)
        {
            RefreshSprites();
        }
    }

    [PunRPC]
    public void TogglePhotonObject(bool toggle)
    {
        boxCollider.enabled = toggle;

        for (int i = 0; i < spriteR.Length; i++)
        {
            spriteR[i].enabled = toggle;
        }
    }


    [PunRPC]
    public void ShowCard()
    {
        for (int i = 1; i < spriteR.Length; i++)
        {
            spriteR[i].enabled = true;
        }
    }

    [PunRPC]
    public void HideCard()
    {
        for (int i = 1; i < spriteR.Length; i++)
        {
            spriteR[i].enabled = false;
        }
    }

    private void RefreshSprites()
    {
        SetCardSprites();

        if (_color)
        {
            SetCardColor(Color.red);
            return;
        }

        SetCardColor(Color.black);
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
                spriteR[i].sprite = sprites[spriteSuit];
                continue;
            }

            spriteR[i].sprite = sprites[spriteValue];
        }
    }
}
