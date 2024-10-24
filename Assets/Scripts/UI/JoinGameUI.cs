using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class JoinGameUI : MonoBehaviour
{
    public static JoinGameUI Instance { get; private set; }
    
    [SerializeField] private TextMeshProUGUI GameCodeInput;
    [SerializeField] private TextMeshProUGUI PlayerNameInput;
    [SerializeField] private Button JoinGameButton;

    private string PlayerName;
    private string GameCode;
    
    private void Awake()
    {
        Instance = this;
        JoinGameButton.onClick.AddListener(async () => {
            ValidateNames();
            await LobbyManager.Instance.Authenticate(PlayerName);
            await LobbyManager.Instance.JoinLobbyByCode(GameCode);
            Hide();
            LobbyUI.Instance.Show();
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
