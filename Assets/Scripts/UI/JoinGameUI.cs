using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.Services.Authentication;
using Unity.Services.Lobbies;
using UnityEngine;
using UnityEngine.UI;

public class JoinGameUI : MonoBehaviour
{
    public static JoinGameUI Instance { get; private set; }
    
    [SerializeField] private TextMeshProUGUI GameCodeInput;
    [SerializeField] private TextMeshProUGUI PlayerNameInput;
    [SerializeField] private Button JoinGameButton;
    [SerializeField] private Button ReturnButton;

    private string PlayerName;
    private string GameCode;
    
    private void Awake()
    {
        Instance = this;
        JoinGameButton.onClick.AddListener(async () => {
            try
            {
                ValidateNames();
                await LobbyManager.Instance.Authenticate(PlayerName);
                await LobbyManager.Instance.JoinLobbyByCode(GameCode);
                LobbyUI.Instance.Show();
            }
            catch(Exception e)
            {
                Debug.LogException(e);
            }
            Hide();
        });
        ReturnButton.onClick.AddListener(async () => {
            Hide();
        });
        
        Hide();
    }
    
    private void Hide() {
        gameObject.SetActive(false);
    }
    
    public void Show() {
        gameObject.SetActive(true);
    }
    
    private void ValidateNames()
    {
        GameCode = LobbyManager.Instance.ValidateName(GameCodeInput.text);
        PlayerName = LobbyManager.Instance.ValidateName(PlayerNameInput.text);
    }
}
