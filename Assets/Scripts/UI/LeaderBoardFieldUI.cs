using System;
using TMPro;
using Unity.Services.Lobbies.Models;
using UnityEngine;
using UnityEngine.UI;

public class LeaderBoardFieldUI : MonoBehaviour
{
    [SerializeField] public int id;
    [SerializeField] private TextMeshProUGUI playerNameText;
    [SerializeField] public Image characterImage;

    public void UpdateLeaderField(PawnData pawnData) {
        playerNameText.text = pawnData.PlayerName;
        characterImage.sprite = LobbyAssets.Instance.GetSprite(pawnData.PawnColor);
    }
}