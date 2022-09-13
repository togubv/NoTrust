using System.Collections;
using TMPro;
using UnityEngine;

public class PlayerView : MonoBehaviour
{
    [SerializeField] private TextMeshPro textmPlayerName;
    [SerializeField] private GameObject prefabCardEmpty;
    
    private int cardsCount;
    private GameObject[] cardsEmpty = new GameObject[52];

    private void Awake()
    {
        GameObject cards = new GameObject("Cards");
        cards.transform.SetParent(transform);

        for (int i = 0; i < 52; i++)
        {
            cardsEmpty[i] = Instantiate(prefabCardEmpty, cards.transform);
            cardsEmpty[i].SetActive(false);
        }
    }

    public void UpdatePlayerViewCardsCount(int playerCardsCount)
    {
        for (int i = 0; i < cardsEmpty.Length; i++)
        {
            cardsEmpty[i].SetActive(false);
        }

        cardsCount = playerCardsCount;
        float x = ((int)(cardsCount / 2)) * -0.3f;

        for (int i = 0; i < cardsCount; i++)
        {
            cardsEmpty[i].SetActive(true);
            cardsEmpty[i].gameObject.transform.position = new Vector2(x + 0.3f * i, cardsEmpty[i].gameObject.transform.position.y);
        }
    }

    public void SetPlayerViewPlayerName(string name)
    {
        textmPlayerName.text = name;
    }
}