using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameUI : MonoBehaviour
{
    [SerializeField] private TurnSystem turnSystem;

    [SerializeField] private GameObject cardPoolWindow;
    [SerializeField] private GameObject panelTurnType;

    private void Start()
    {
        turnSystem.ToggleCardPoolWindowHandlerEvent += ShowCardPoolWindow;
        turnSystem.ConfirmEndTurnHandlerEvent += TogglePanelTurnType;
    }

    public void HidePanelTurnType()
    {
        panelTurnType.SetActive(false);
    }

    private void TogglePanelTurnType(int player, Card[] cards)
    {
        panelTurnType.SetActive(true);
        Core.layerMaskCard = 1 << 0;
    }

    private void ShowCardPoolWindow(bool toggle)
    {
        cardPoolWindow.SetActive(toggle);

        if (toggle)
        {
            Core.layerMaskCard = 1 << 5;
            return;
        }

        Core.layerMaskCard = 1 << 0;
    }
}
