using System;
using TMPro;
using Unity.Services.Lobbies.Models;
using UnityEngine;
using UnityEngine.UI;

public class PlayerBoardFieldUI : MonoBehaviour
{
    [SerializeField] public int Id;
    [SerializeField] private TextMeshProUGUI playerNameText;
    [SerializeField] public Image characterImage;
    [SerializeField] private bool activePlayer;

    // private void Awake()
    // {
    //     if (LobbyManager.Instance.GetLobbyBeforeGame().Players.Count >= Id)
    //     {
    //         
    //     }
    // }

    public void UpdatePlayer(string playerName, LobbyManager.PlayerColor playerColor) {
        playerNameText.text = playerName;
        characterImage.sprite = LobbyAssets.Instance.GetSprite(playerColor);
    }

    public void SetActive()
    {
        activePlayer = true;
    }

    public void SetNotActive()
    {
        activePlayer = false;
    }
}