using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GameUI : MonoBehaviour
{
    [SerializeField] private PlayerTurn playerTurn;
    [SerializeField] private GameObject cardPoolWindow;
    [SerializeField] private GameObject panelTurnTypeButtons;
    [SerializeField] private Button buttonThrow;
    [SerializeField] private Button buttonAttack;
    [SerializeField] private Button buttonDefence;
    [SerializeField] private Text textWinnerName;
    [SerializeField] private GameObject canvasGame;
    [SerializeField] private GameObject canvasEndGame;

    private void Start()
    {
        playerTurn.ShowCardPoolWindowHandlerEvent += ShowCardPoolWindow;
        playerTurn.ToggleButtonThrowHandlerEvent += UpdateButtonThrow;
        playerTurn.TogglePanelTurnTypeButtonsHandlerEvent += TogglePanelTurnType;
        playerTurn.TogglePanelEndGameHandlerEvent += ShowEndGame;
    }

    public void TogglePanelTurnType(bool toggle, bool btnAttack, bool btnDefence)
    {
        if (toggle)
        {
            PlayerTurn.layerMaskCard = 1 << 5;
        }

        panelTurnTypeButtons.SetActive(toggle);
        buttonAttack.interactable = btnAttack;
        buttonDefence.interactable = btnDefence;
    }

    public void TogglePanelTurnTypeForButton(bool toggle)
    {
        panelTurnTypeButtons.SetActive(toggle);
    }

    private void UpdateButtonThrow(bool toggle)
    {
        buttonThrow.interactable = toggle;
    }

    private void ShowCardPoolWindow(bool toggle, bool[] pool)
    {
        if (toggle)
        {
            PlayerTurn.layerMaskCard = 1 << 5;
            cardPoolWindow.SetActive(true);

            for (int i = 0; i < playerTurn.CardsPool.Length; i++)
            {
                playerTurn.CardsPool[i].RefreshSprites();

                if (pool[i])
                {
                    playerTurn.CardsPool[i].EnableCard();
                    continue;
                }

                playerTurn.CardsPool[i].DisableCard();
            }
            return;
        }

        cardPoolWindow.SetActive(false);
    }

    private void ShowEndGame(string winnerName)
    {
        canvasGame.SetActive(false);
        canvasEndGame.SetActive(true);
        textWinnerName.text = winnerName;
    }
}
