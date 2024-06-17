using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class CreateGameUI : MonoBehaviour
{
    public static CreateGameUI Instance { get; private set; }
    
    [SerializeField] private TextMeshProUGUI GameNameInput;
    //[SerializeField] private Dropdown boardDropdown;
    [SerializeField] private TextMeshProUGUI PlayerNameInput;
    [SerializeField] private Button CreateGameButon;

    private string GameName;
    private string PlayerName;
    
    private void Awake()
    {
        Instance = this;
        CreateGameButon.onClick.AddListener(async () => {
            ValidateNames();
            await LobbyManager.Instance.Authenticate(PlayerName);
            await LobbyManager.Instance.CreateLobby(GameName);
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
        GameName = GameNameInput.text;
        PlayerName = PlayerNameInput.text;
        char[] arrGameName = GameName.Where(c => (char.IsLetterOrDigit(c) || 
                                     char.IsWhiteSpace(c) || 
                                     c == '-' || 
                                     c == '_')).ToArray(); 
        GameName = new string(arrGameName);
        char[] arrPlayerName = PlayerName.Where(c => (char.IsLetterOrDigit(c) || 
                                          char.IsWhiteSpace(c) || 
                                          c == '-' || 
                                          c == '_')).ToArray(); 
        PlayerName = new string(arrPlayerName);
    }
}
