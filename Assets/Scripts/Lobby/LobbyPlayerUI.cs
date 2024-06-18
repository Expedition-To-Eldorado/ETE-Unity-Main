using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.Services.Lobbies.Models;
using UnityEngine;
using UnityEngine.UI;

public class LobbyPlayerUI : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI playerNameText;
    [SerializeField] private Image characterImage;
    private Player player;

    public void UpdatePlayer(Player player) {
        this.player = player;
        playerNameText.text = player.Data[LobbyManager.KEY_PLAYER_NAME].Value;
        LobbyManager.PlayerColor playerColor =
            System.Enum.Parse<LobbyManager.PlayerColor>(player.Data[LobbyManager.KEY_PLAYER_COLOR].Value);
        characterImage.sprite = LobbyAssets.Instance.GetSprite(playerColor);
    }
}
