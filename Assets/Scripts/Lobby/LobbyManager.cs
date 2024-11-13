using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Unity.Netcode;
using Unity.Services.Authentication;
using Unity.Services.Core;
using Unity.Services.Lobbies;
using Unity.Services.Lobbies.Models;
using Unity.Services.Relay;
using Unity.VisualScripting;
using UnityEditor;
using UnityEngine;
using static LobbyManager.PlayerColor;

public class LobbyManager : MonoBehaviour
{
    public static LobbyManager Instance { get; private set; }

    public const string KEY_PLAYER_NAME = "PlayerName";
    public const string KEY_PLAYER_COLOR = "Color";
    public const string KEY_START_GAME = "StartGame";
    public const int MAX_PLAYERS = 4;

    private Lobby hostLobby;
    private Lobby joinedLobby;
    public static Lobby lobbyBeforeGame;
    private float heartbeatTimer;
    private float lobbyUpdateTimer;
    private string playerName;
    public bool startedTheGame;

    public event EventHandler<LobbyEventArgs> OnJoinedLobby;
    public event EventHandler<LobbyEventArgs> OnJoinedLobbyUpdate;

    public class LobbyEventArgs : EventArgs
    {
        public Lobby lobby;
    }

    public enum PlayerColor
    {
        Red,
        Purple,
        White,
        Blue,
    }

    private void Awake()
    {
        startedTheGame = false;
        // Singleton Creation
        if (Instance == null) {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else {
            Destroy(gameObject);
        }
    }

    void Update()
    {
        HandleLobbyHeartbeat();
        HandleLobbyPollForUpdates();
    }

    public async Task Authenticate(string playerName)
    {
        this.playerName = playerName;
        InitializationOptions initializationOptions = new InitializationOptions();
        initializationOptions.SetProfile(playerName);

        await UnityServices.InitializeAsync(initializationOptions);

        AuthenticationService.Instance.SignedIn += () => {
            // do nothing
            Debug.Log("Signed in! " + AuthenticationService.Instance.PlayerId + " " + playerName);
        };

        await AuthenticationService.Instance.SignInAnonymouslyAsync();
    }

    private async void HandleLobbyHeartbeat()
    {
        if (hostLobby != null)
        {
            heartbeatTimer -= Time.deltaTime;
            if (heartbeatTimer < 0f)
            {
                float heartbeatTimerMax = 15;
                heartbeatTimer = heartbeatTimerMax;
                await LobbyService.Instance.SendHeartbeatPingAsync(hostLobby.Id);
            }
        }
    }

    private async void HandleLobbyPollForUpdates()
    {
        if (joinedLobby != null)
        {
            lobbyUpdateTimer -= Time.deltaTime;
            if (lobbyUpdateTimer < 0f)
            {
                float lobbyUpdateTimerMax = 1.1f;
                lobbyUpdateTimer = lobbyUpdateTimerMax;
                Lobby lobby = await LobbyService.Instance.GetLobbyAsync(joinedLobby.Id);
                joinedLobby = lobby;
                OnJoinedLobbyUpdate?.Invoke(this, new LobbyEventArgs { lobby = joinedLobby });
                if (joinedLobby.Data[KEY_START_GAME].Value != "0")
                {
                    // Start Game for Clients in Lobby
                    // Host is not polling for updates anymore (because joinedLobby for host is null)
                    // so it won't get executed by host
                    if (!IsLobbyHost())
                    {
                        RelayManager.Instance.JoinRelay(joinedLobby.Data[KEY_START_GAME].Value);
                    }

                    lobbyBeforeGame = joinedLobby;
                    joinedLobby = null;
                }
            }
        }
    }

    public async Task CreateLobby(string lobbyName)
    {
        try
        {
            Player player = GetPlayer();
            int maxPlayers = MAX_PLAYERS;
            CreateLobbyOptions createLobbyOptions = new CreateLobbyOptions();
            createLobbyOptions.IsPrivate = true;
            createLobbyOptions.Player = player;
            createLobbyOptions.Data = new Dictionary<string, DataObject>
            {
                { KEY_START_GAME, new DataObject(DataObject.VisibilityOptions.Member, "0") },
            };
            hostLobby = await LobbyService.Instance.CreateLobbyAsync(lobbyName, maxPlayers, createLobbyOptions);
            joinedLobby = hostLobby;
            Debug.Log("Created Lobby! " + hostLobby.Name + ", maxPlayers: " + hostLobby.MaxPlayers + ", LobbyCode: " +
                      hostLobby.LobbyCode);
            PrintPlayers(hostLobby);
            UpdatePlayerCharacter(GetRandomPlayerColor());
            OnJoinedLobby?.Invoke(this, new LobbyEventArgs { lobby = joinedLobby });
        }
        catch (LobbyServiceException e)
        {
            Debug.Log(e);
        }
    }

    public Lobby GetJoinedLobby()
    {
        return joinedLobby;
    }

    public Lobby GetLobbyBeforeGame()
    {
        return lobbyBeforeGame;
    }

    public Player GetPlayerByIdAfterGameStart(string playerId)
    {
        // we are checking lobbyBefore game instead joinedLobby because
        // joined lobby is set to null after game start 
        // so the lobby would not be updated anymore
        if (lobbyBeforeGame == null)
        {
            Debug.LogError("Lobby is not initialized.");
            return null;
        }
        
        foreach (var player in lobbyBeforeGame.Players)
        {
            if (player.Id == playerId)
            {
                return player;
            }
        }
        
        Debug.LogError("Player with ID " + playerId + " not found in the lobby.");
        return null;
    }
    
    public Player GetPlayerById(string playerId)
    {
        // we are checking lobbyBefore game instead joinedLobby because
        // joined lobby is set to null after game start 
        // so the lobby would not be updated anymore
        if (joinedLobby == null)
        {
            Debug.LogError("Lobby is not initialized.");
            return null;
        }
        
        foreach (var player in joinedLobby.Players)
        {
            if (player.Id == playerId)
            {
                return player;
            }
        }
        return null;
    }

    public bool IsLobbyHost()
    {
        return joinedLobby != null && joinedLobby.HostId == AuthenticationService.Instance.PlayerId;
    }

    private async void ListLobbies()
    {
        try
        {
            // Getting information about every lobby
            QueryLobbiesOptions queryLobbiesOptions = new QueryLobbiesOptions
            {
                Count = 25,
                Filters = new List<QueryFilter>
                {
                    new QueryFilter(QueryFilter.FieldOptions.AvailableSlots, "0", QueryFilter.OpOptions.GT)
                },
                Order = new List<QueryOrder>
                {
                    new QueryOrder(false, QueryOrder.FieldOptions.Created)
                }
            };
            QueryResponse queryResponse = await Lobbies.Instance.QueryLobbiesAsync(queryLobbiesOptions);
            Debug.Log("Lobbies found: " + queryResponse.Results.Count);
            foreach (Lobby lobby in queryResponse.Results)
            {
                Debug.Log(lobby.Name + ", maxPlayers: " + lobby.MaxPlayers);
            }
        }
        catch (LobbyServiceException e)
        {
            Debug.Log(e);
        }
    }

    public async Task JoinLobbyByCode(string lobbyCode)
    {
        try
        {
            Player player = GetPlayer();
            JoinLobbyByCodeOptions joinLobbyByCodeOptions = new JoinLobbyByCodeOptions
            {
                Player = player
            };
            Lobby lobby = await Lobbies.Instance.JoinLobbyByCodeAsync(lobbyCode, joinLobbyByCodeOptions);
            joinedLobby = lobby;
            Debug.Log("Joined Lobby with code " + lobbyCode);
            PrintPlayers(joinedLobby);
            UpdatePlayerCharacter(GetRandomPlayerColor());
            OnJoinedLobby?.Invoke(this, new LobbyEventArgs { lobby = lobby });
        }
        catch (LobbyServiceException e)
        {
            Debug.Log(e);
        }

    }

    // Quick Join without lobby id or lobby code but if you want you can add the filters just like in query
    private async void QuickJoinLobby()
    {
        try
        {
            await LobbyService.Instance.QuickJoinLobbyAsync();
            Debug.Log("Quick Joined any Lobby");
        }
        catch (LobbyServiceException e)
        {
            Debug.Log(e);
        }

    }

    private Player GetPlayer()
    {
        return new Player(AuthenticationService.Instance.PlayerId, null, new Dictionary<string, PlayerDataObject>
        {
            { KEY_PLAYER_NAME, new PlayerDataObject(PlayerDataObject.VisibilityOptions.Public, playerName) },
            {
                KEY_PLAYER_COLOR,
                new PlayerDataObject(PlayerDataObject.VisibilityOptions.Public, Red.ToString())
            },
        });
    }

    private void PrintPlayers(Lobby lobby)
    {
        foreach (Player player in lobby.Players)
        {
            Debug.Log(player.Id + " " + player.Data["PlayerName"].Value);
        }
    }

    private async void UpdatePlayerName(string newPlayerName)
    {
        try
        {
            playerName = newPlayerName;
            await LobbyService.Instance.UpdatePlayerAsync(joinedLobby.Id, AuthenticationService.Instance.PlayerId,
                new UpdatePlayerOptions
                {
                    Data = new Dictionary<string, PlayerDataObject>
                    {
                        { "PlayerName", new PlayerDataObject(PlayerDataObject.VisibilityOptions.Member, playerName) }
                    }
                });
            OnJoinedLobbyUpdate?.Invoke(this, new LobbyEventArgs { lobby = joinedLobby });
        }
        catch (LobbyServiceException e)
        {
            Debug.Log(e);
        }

    }

    public async void UpdatePlayerCharacter(PlayerColor playerColor)
    {
        if (joinedLobby != null)
        {
            try
            {
                UpdatePlayerOptions options = new UpdatePlayerOptions();

                options.Data = new Dictionary<string, PlayerDataObject>()
                {
                    {
                        KEY_PLAYER_COLOR, new PlayerDataObject(
                            visibility: PlayerDataObject.VisibilityOptions.Public,
                            value: playerColor.ToString())
                    }
                };

                string playerId = AuthenticationService.Instance.PlayerId;
                Lobby lobby = await LobbyService.Instance.UpdatePlayerAsync(joinedLobby.Id, playerId, options);
                joinedLobby = lobby;

                OnJoinedLobbyUpdate?.Invoke(this, new LobbyEventArgs { lobby = joinedLobby });
            }
            catch (LobbyServiceException e)
            {
                Debug.Log(e);
            }
        }
    }

    public async Task LeaveLobby()
    {
        PlayerColor colorOfLeavingPlayer =
            System.Enum.Parse<PlayerColor>(GetPlayerById(AuthenticationService.Instance.PlayerId).Data[KEY_PLAYER_COLOR].Value);
        try
        {
            await LobbyService.Instance.RemovePlayerAsync(joinedLobby.Id, AuthenticationService.Instance.PlayerId);
        }
        catch (LobbyServiceException e)
        {
            Debug.Log(e);
        }
        finally
        {
            // stop lobby updates
            CancelInvoke(nameof(HandleLobbyPollForUpdates));
            joinedLobby = null;
            
            if (AuthenticationService.Instance.IsSignedIn)
            {
                AuthenticationService.Instance.SignOut();
                Debug.Log("Player signed out.");
            }
        }
    }

    private async void KickPlayerOut()
    {
        try
        {
            await LobbyService.Instance.RemovePlayerAsync(joinedLobby.Id, AuthenticationService.Instance.PlayerId);
        }
        catch (LobbyServiceException e)
        {
            Debug.Log(e);
        }
    }

    public async void MigrateLobbyHost()
    {
        try
        {
            hostLobby = await Lobbies.Instance.UpdateLobbyAsync(hostLobby.Id, new UpdateLobbyOptions
            {
                HostId = joinedLobby.Players[1].Id,
            });
            joinedLobby = hostLobby;
        }
        catch (LobbyServiceException e)
        {
            Debug.Log(e);
        }
    }
    
    public async Task MigrateLobbyHostAndLeave()
    {
        if (IsLobbyHost())
        {
            try
            {
                if (joinedLobby.Players.Count > 1)
                {
                    // Assign new host
                    string newHostId = joinedLobby.Players.First(player => player.Id != AuthenticationService.Instance.PlayerId).Id;
                
                    hostLobby = await Lobbies.Instance.UpdateLobbyAsync(joinedLobby.Id, new UpdateLobbyOptions
                    {
                        HostId = newHostId
                    });
                    Debug.Log($"Host migrated to player {newHostId}");
                }
            }
            catch (LobbyServiceException e)
            {
                Debug.LogError($"Error migrating host: {e}");
                return; 
            }
        }
        await LeaveLobby();
    }

    private async void DeleteLobby()
    {
        try
        {
            await LobbyService.Instance.DeleteLobbyAsync(joinedLobby.Id);
        }
        catch (LobbyServiceException e)
        {
            Debug.Log(e);
        }
    }

    public string ValidateName(string name)
    {
        char[] arrName = name.Where(c => (char.IsLetterOrDigit(c) ||
                                          c == '-' ||
                                          c == '_')).ToArray();
        name = new string(arrName);
        if (name.Length > 29)
        {
            name = name.Substring(0, 20);
        }
        
        return name;
    }

    public async void StartGame()
    {
        if (IsLobbyHost())
        {
            try
            {
                // Host creating Relay 
                // Start Game for Host
                Debug.Log("StartGame");
                lobbyBeforeGame = joinedLobby;
                string relayCode = await RelayManager.Instance.CreateRelay();

                Lobby lobby = await Lobbies.Instance.UpdateLobbyAsync(joinedLobby.Id, new UpdateLobbyOptions
                {
                    Data = new Dictionary<string, DataObject>
                    {
                        { KEY_START_GAME, new DataObject(DataObject.VisibilityOptions.Member, relayCode) }
                    }
                });
                GameLoop.Instance.CanStartGame = true;
                joinedLobby = lobby;
            }
            catch (LobbyServiceException e)
            {
                Debug.Log(e);
            }
        }
    }
    

    private PlayerColor GetRandomPlayerColor()
    {
        List<PlayerColor> availableColors = new List<PlayerColor>
        {
            PlayerColor.Red,
            PlayerColor.Purple,
            PlayerColor.White,
            PlayerColor.Blue
        };
        if (joinedLobby != null)
        {
            foreach (var player in joinedLobby.Players)
            {
                
                availableColors.Remove(System.Enum.Parse<PlayerColor>(player.Data[KEY_PLAYER_COLOR].Value));
            }
        }
        int randomIndex = UnityEngine.Random.Range(0, availableColors.Count); 
        PlayerColor assignedColor = availableColors[randomIndex];
        
        return assignedColor;
    }
}