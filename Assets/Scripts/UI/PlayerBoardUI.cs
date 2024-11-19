using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PlayerBoardUI : MonoBehaviour
{
    public static PlayerBoardUI Instance { get; private set; }
    
    [SerializeField] private List<Transform> playerBoardFieldUIList;
    private int numberOfPlayers;
    
    private void Awake()
    {
        Instance = this;
        numberOfPlayers = LobbyManager.Instance.GetLobbyBeforeGame().Players.Count;
        for(int i = LobbyManager.MAX_PLAYERS - 1; i >=0 ; i--)
        {
            if (i >= numberOfPlayers) {
                Destroy(playerBoardFieldUIList[i].gameObject); 
                playerBoardFieldUIList.RemoveAt(i);
            }
        }
    }

    public void UpdatePlayers()
    {
        for(int i = 0; i < numberOfPlayers; i++)
        {
            playerBoardFieldUIList[i].gameObject.SetActive(true);
            playerBoardFieldUIList[i].GetComponent<PlayerBoardFieldUI>().UpdatePlayer(BoardSingleton.instance.PawnsData[i]);
        }
        ShowActivePlayer();
    }
    
    public void ChangeActivePlayer(int activePlayerID) {
        if (activePlayerID == 0) {
            playerBoardFieldUIList[numberOfPlayers - 1].GetComponent<PlayerBoardFieldUI>().SetNotActive();
        }
        else {
            playerBoardFieldUIList[activePlayerID - 1].GetComponent<PlayerBoardFieldUI>().SetNotActive();
        }
        playerBoardFieldUIList[activePlayerID].GetComponent<PlayerBoardFieldUI>().SetActive();
        ShowActivePlayer();
    }

    public void ShowActivePlayer()
    {
        foreach (var playerField in playerBoardFieldUIList)
        {
            PlayerBoardFieldUI playerBoardFieldUI = playerField.GetComponent<PlayerBoardFieldUI>();
            if (playerBoardFieldUI.IsActive()) {
                playerBoardFieldUI.ShowActive();
            }
            else {
                playerBoardFieldUI.ShowNotActive();
            }
        }
    }
}