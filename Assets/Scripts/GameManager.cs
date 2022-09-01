using Photon.Pun;
using Photon.Realtime;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviourPunCallbacks
{
    [SerializeField] private GameObject menuPause;
    [SerializeField] private GameObject buttonMenu;

    public void ToggleMenuPause()
    {
        if (menuPause.activeInHierarchy)
        {
            menuPause.SetActive(false);
            buttonMenu.SetActive(true);
            return;
        }

        menuPause.SetActive(true);
        buttonMenu.SetActive(false);
    }

    public void Leave()
    {
        PhotonNetwork.LeaveRoom();
    }

    public override void OnLeftRoom()
    {
        SceneManager.LoadScene(0);
    }

    public override void OnPlayerEnteredRoom(Player newPlayer)
    {
        Debug.LogFormat($"Player {0} entered room", newPlayer.NickName);
    }

    public override void OnPlayerLeftRoom(Player otherPlayer)
    {
        Debug.LogFormat($"Player {0} left room", otherPlayer.NickName);
    }
}
