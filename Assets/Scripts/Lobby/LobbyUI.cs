using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.Services.Lobbies;
using Unity.Services.Lobbies.Models;
using UnityEngine;
using UnityEngine.UI;

public class LobbyUI : MonoBehaviour
{
    public static LobbyUI Instance { get; private set; }


    [SerializeField] private Transform playerLobbyTemplate;
    [SerializeField] private Transform container;
    [SerializeField] private TextMeshProUGUI lobbyCodeText;
    //[SerializeField] private TextMeshProUGUI playerCountText;
    //[SerializeField] private Button leaveLobbyButton;
    [SerializeField] private Button startGameButton;

    private void Awake() {
        Instance = this;

        playerLobbyTemplate.gameObject.SetActive(false);
        startGameButton.onClick.AddListener( () => {
            LobbyManager.Instance.StartGame();
        });
        
        // leaveLobbyButton.onClick.AddListener(() => {
        //     LobbyManager.Instance.LeaveLobby();
        // });

        // changeGameModeButton.onClick.AddListener(() => {
        //     LobbyManager.Instance.ChangeGameMode();
        // });
        
        Hide();
    }
    private void Start() {
        LobbyManager.Instance.OnJoinedLobby += UpdateLobby_Event;
        LobbyManager.Instance.OnJoinedLobbyUpdate += UpdateLobby_Event;

        Hide();
    }
    
    private void UpdateLobby_Event(object sender, LobbyManager.LobbyEventArgs e) {
        UpdateLobby();
    }

    private void UpdateLobby() {
        UpdateLobby(LobbyManager.Instance.GetJoinedLobby());
    }
    
    private void UpdateLobby(Lobby lobby) {
        ClearLobby();

        foreach (Player player in lobby.Players) {
            Transform playerSingleTransform = Instantiate(playerLobbyTemplate, container);
            playerSingleTransform.gameObject.SetActive(true);
            LobbyPlayerUI lobbyPlayerSingleUI = playerSingleTransform.GetComponent<LobbyPlayerUI>();
            
            if(!LobbyManager.Instance.IsLobbyHost())
                startGameButton.gameObject.SetActive(false);

            lobbyPlayerSingleUI.UpdatePlayer(player);
        }

        lobbyCodeText.text = lobby.LobbyCode;

        Show();
    }
    
    private void ClearLobby() {
        foreach (Transform child in container) {
            if (child == playerLobbyTemplate) continue;
            Destroy(child.gameObject);
        }
    }
    
    private void Hide() {
        gameObject.SetActive(false);
    }

    public void Show() {
        gameObject.SetActive(true);
    }
}
