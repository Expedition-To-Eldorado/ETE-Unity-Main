using System.Collections.Generic;
using TMPro;
using Unity.Services.Lobbies.Models;
using UnityEditor.ShaderKeywordFilter;
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
            playerBoardFieldUIList[i].GetComponent<PlayerBoardFieldUI>().UpdatePlayer(
                BoardSingleton.instance.PawnsData[i].Item1, BoardSingleton.instance.PawnsData[i].Item2);
        }
    }
    
    public void ChangeActivePlayer() {
        foreach (var player in playerBoardFieldUIList)
        {
            
        }
    }
}