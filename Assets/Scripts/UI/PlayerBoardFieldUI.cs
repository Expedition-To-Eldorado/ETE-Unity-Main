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
    [SerializeField] public bool activePlayer;

    public void UpdatePlayer(PawnData pawnData) {
        playerNameText.text = pawnData.PlayerName;
        characterImage.sprite = LobbyAssets.Instance.GetSprite(pawnData.PawnColor);
    }

    public bool IsActive()
    {
        return activePlayer;
    }

    public void SetActive()
    {
        activePlayer = true;
    }
    
    public void SetNotActive()
    {
        activePlayer = false;
    }

    public void ShowActive()
    {
        GameObject background = gameObject.transform.GetChild(0).gameObject;
        Image img =  background.GetComponent<Image>();
        img.color = new Color(img.color.r, img.color.g, img.color.b, 0.5f);
    }

    public void ShowNotActive()
    {
        GameObject background = gameObject.transform.GetChild(0).gameObject;
        Image img =  background.GetComponent<Image>();
        img.color = new Color(img.color.r, img.color.g, img.color.b, 0f);
    }
}