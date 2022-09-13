using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PlayerData : MonoBehaviour
{
    [SerializeField] private Text textPlayerName;
    [SerializeField] private Text textPlayerID;

    public void Initialize(int playerID, string playerName)
    {
        textPlayerID.text = playerID.ToString();
        textPlayerName.text = playerName;
    }
}
