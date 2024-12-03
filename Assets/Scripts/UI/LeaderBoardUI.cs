using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

public class LeaderBoardUI : MonoBehaviour
{
    public static LeaderBoardUI Instance { get; set; }
    
    [SerializeField] private List<Transform> leaderBoardFieldUIList;
    private int numberOfPlayers;
    
    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else if (Instance != this)
        {
            Destroy(gameObject);
        }
        numberOfPlayers = LobbyManager.Instance.GetLobbyBeforeGame().Players.Count;
        for(int i = LobbyManager.MAX_PLAYERS - 1; i >=0 ; i--)
        {
            if (i >= numberOfPlayers) {
                Destroy(leaderBoardFieldUIList[i].gameObject); 
                leaderBoardFieldUIList.RemoveAt(i);
            }
        }
    }

    public void UpdateLeaderBoard()
    {
        for(int i = 0; i < numberOfPlayers; i++)
        {
            leaderBoardFieldUIList[i].gameObject.SetActive(true);
            leaderBoardFieldUIList[i].GetComponent<LeaderBoardFieldUI>().UpdateLeaderField(BoardSingleton.instance.PlayerLeaderBoard[i]);
        }
    }
}