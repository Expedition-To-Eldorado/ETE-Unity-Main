using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.Netcode;
using UnityEngine;
using GeneralEnumerations;
using IngameDebugConsole;
using Unity.Services.Authentication;
using Unity.Services.Lobbies.Models;

public class PlayerNetwork : NetworkBehaviour
{
    //Networking fields
    [SerializeField] private MeshRenderer meshRenderer;
    [SerializeField] private List<Material> PawnMaterials;
    public string PlayerName;
    public LobbyManager.PlayerColor PlayerColor;
    public static Action UseCard;
    public static Action clearMultipleChosenCards;
    public static Action burnMultipleCards;
    private int numberOfPlayers;
    private NetworkVariable<LobbyManager.PlayerColor> playerColor = new NetworkVariable<LobbyManager.PlayerColor>();

    //PawnMoving
    [SerializeField] Vector3 offset = new Vector3 (1.46f, 0.55f, 1f);
    
    private void Awake()
    {
        meshRenderer = GetComponentInChildren<MeshRenderer>();
    }

    public override void OnNetworkSpawn()
    {
        // making sure that code is executed only for the owner of the script
        if (IsOwner)
        {
            transform.position = new Vector3(32f + (int)OwnerClientId*10, 0.55f, -4.3f);
            Player player = LobbyManager.Instance.GetPlayerByIdAfterGameStart(AuthenticationService.Instance.PlayerId);
            PlayerName = player.Data[LobbyManager.KEY_PLAYER_NAME].Value;
            PlayerColor = System.Enum.Parse<LobbyManager.PlayerColor>(player.Data[LobbyManager.KEY_PLAYER_COLOR].Value);
            SetPlayerColorServerRpc(PlayerColor);
            Debug.Log("PLAYER NAME: " + player.Data[LobbyManager.KEY_PLAYER_NAME].Value + "PLAYER COLOR: "+ (int)PlayerColor);
            SetPlayerInfoServerRpc(PlayerName, PlayerColor, new ServerRpcParams());
            numberOfPlayers = LobbyManager.Instance.GetLobbyBeforeGame().Players.Count;
            if ((int)OwnerClientId == numberOfPlayers - 1)
            {
                System.Threading.Thread.Sleep(2000);
                NotifyServerToSendAllPlayerDataServerRpc();
            }
        }
        playerColor.OnValueChanged += OnPlayerColorChanged;
        OnPlayerColorChanged(LobbyManager.PlayerColor.Red, playerColor.Value);
    }
    
    private void OnEnable()
    {
        HexGridMeshGenerator.MovePawn += movePawn;
    }

    private void OnDisable()
    {
        HexGridMeshGenerator.MovePawn -= movePawn;
    }
    
    private void OnPlayerColorChanged(LobbyManager.PlayerColor oldColor, LobbyManager.PlayerColor newColor)
    {
        meshRenderer.material = PawnMaterials[(int)newColor];
    }
    
    private void movePawn(int x, int z, HexGrid boardPiece, string terrainName, CardBehaviour card, int noOfChosenCards)
    {
        Debug.Log("am i host? " + IsHost);

        if (!IsOwner) return;

        Debug.Log("Moving pawn, caller: " + OwnerClientId);

        Vector3 centre = HexMetrics.Center(boardPiece.HexSize, x, z, boardPiece.Orientation) + boardPiece.gridOrigin;
        int terrainPower = terrainName[terrainName.Length - 1] - '0';
        terrainName = terrainName.Substring(0, terrainName.Length - 1);

        ErrorMsg errCode;
        if (card == null)
        {
            errCode = checkIfCanMove(centre, x, z, boardPiece, terrainName, null);
        }
        else
        {
            errCode = checkIfCanMove(centre, x, z, boardPiece, terrainName, card.Typ);
        }

        if (errCode == ErrorMsg.OK 
            || errCode == ErrorMsg.BURN_CARD 
            || errCode == ErrorMsg.DISCARD_CARD
            || errCode == ErrorMsg.END_GAME)
        {
            LeanTween.move(this.gameObject, centre + offset, 0.5f).setEase(LeanTweenType.easeInOutQuint);
            SendCordsServerRpc(x, z, boardPiece.BoardPieceLetter, new ServerRpcParams());
        }
        else
        {
            return;
        }

        if (errCode == ErrorMsg.END_GAME)
        {
            GameLoop.PlayerPhase = Phase.GAME_WON;
            Debug.Log("Game ended");
            SendPlayerLeaderBoardServerRpc(new ServerRpcParams());
        }

        if (errCode == ErrorMsg.OK)
        {
            //i thought that there is no need for overcomplicating this card
            if(card.NameOfCard == "Tubylec")
            {
                card.leftPower = 0;
            }
            else
            {
                card.leftPower -= terrainPower;
            }

            if (card.leftPower <= 0)
            {
                UseCard?.Invoke();
            }
        }

        if (errCode == ErrorMsg.DISCARD_CARD)
        {
            clearMultipleChosenCards?.Invoke();
        }
        else if (errCode == ErrorMsg.BURN_CARD)
        {
            burnMultipleCards?.Invoke();
        }

        Debug.Log(errCode);
        return;
    }

    private ErrorMsg checkIfCanMove(Vector3 centre, int x, int z, HexGrid boardPiece, string terrainName, string cardType)
    {
        //check if the field is adjacent to the current field
        float distance = Vector3.Distance(centre, transform.position - offset);
        float acceptableDist = boardPiece.HexSize * Mathf.Sqrt(3) * 1.2f;
        if (distance > acceptableDist)
        {
            Debug.Log("the distance is too long " + distance + " > " + acceptableDist);
            return ErrorMsg.DIST_TOO_LONG;
        }
        
        //check if the field is not a mountain
        //useless much?
        if (terrainName == "Mountains")
        {
            Debug.Log("Player can not move to mountains");
            return ErrorMsg.FIELD_IS_MNTN;
        }

        if (terrainName == "Camp")
        {
            Debug.Log("Burning card");
            return ErrorMsg.BURN_CARD;
        }

        if (terrainName == "Discard")
        {
            Debug.Log("Discard card field");
            return ErrorMsg.DISCARD_CARD;
        }

        if (terrainName == "EndJungle" && (cardType == "Jungle" || cardType == "All"))
        {
            Debug.Log("You won the game");
            return ErrorMsg.END_GAME;
        }

        if (terrainName == "EndRiver" && (cardType == "River" || cardType == "All"))
        {
            Debug.Log("You won the game");
            return ErrorMsg.END_GAME;
        }
        
        //check if any other player is on the selected field
        if (!MouseController.instance.checkIfNotOccupiedPosition(new PawnPosition(boardPiece.BoardPieceLetter, new Vector2(x, z))))
        {
            Debug.Log("Field is occupied by player, the caller is " + this.name);
            return ErrorMsg.FIELD_OCCUPIED;
        }

        //if hydroplane is being used for first time
        if(DeckManager.isHydroplaneUsed && DeckManager.hydroplaneField.Equals("")){
            DeckManager.hydroplaneField = terrainName;
            Debug.Log("hydroplane on " + terrainName + " was used");
            return ErrorMsg.OK;
        }

        //hydroplane is used on a different field
        if (DeckManager.isHydroplaneUsed && !DeckManager.hydroplaneField.Equals(terrainName))
        {
            Debug.Log("hydroplane on " + terrainName + " cannot be used because hydroplane field is: " + DeckManager.hydroplaneField);
            DeckManager.isHydroplaneUsed = false;
            return ErrorMsg.CARD_NOT_MATCHING;
        }

        //check if selected card matches the field and its power
        if (cardType != terrainName && cardType != "All")
        {
            Debug.Log("Chosen card " + cardType + " does not match the chosen field " + terrainName);
            return ErrorMsg.CARD_NOT_MATCHING;
        }

        return ErrorMsg.OK;
    }

    [ServerRpc(RequireOwnership = false)]
    public void SendPlayerLeaderBoardServerRpc(ServerRpcParams serverRpcParams)
    {
        int SenderId = (int)serverRpcParams.Receive.SenderClientId;
        UpdatePlayerLeaderBoardClientRpc(SenderId);
    }

    [ClientRpc]
    public void UpdatePlayerLeaderBoardClientRpc(int playerId)
    {
        Debug.Log("A player has won: " + playerId);
        int numOfPlayers = LobbyManager.Instance.GetLobbyBeforeGame().Players.Count;
        BoardSingleton.instance.PlayerLeaderBoard.Add(BoardSingleton.instance.PawnsData[playerId]);

        if (BoardSingleton.instance.PlayerLeaderBoard.Count >= numOfPlayers - 1)
        {
            //TODO -> Implement showing leaderboard
            GameLoop.PlayerPhase = Phase.GAME_ENDED;
            GameLoop.updateText();
            int idOfLastPlayer = WhoIsTheLastPlayer(numOfPlayers);
            BoardSingleton.instance.PlayerLeaderBoard.Add(BoardSingleton.instance.PawnsData[idOfLastPlayer]);
            PlayerBoardUI.Instance.gameObject.SetActive(false);
            ActivateLeaderBoardUI();
        }

    }

    private int WhoIsTheLastPlayer(int numOfPlayers)
    {
        if (numOfPlayers <= 1)
        {
            return 0;
        }
        List<int> playersIds = new List<int>();
        for (int i = 0; i < numOfPlayers; i++)
        {
            playersIds.Add(i);
        }
        for (int i = 0; i < BoardSingleton.instance.PlayerLeaderBoard.Count; i++)
        {
            int index = BoardSingleton.instance.PawnsData.FindIndex
                (a => a.PawnColor == BoardSingleton.instance.PlayerLeaderBoard[i].PawnColor);
            playersIds.Remove(index);
        }
        Debug.Log(playersIds);
        return playersIds[0];
    }
    
    private void ActivateLeaderBoardUI()
    {
        if (LeaderBoardUI.Instance == null)
        {
            // Znajdź obiekt w scenie, nawet jeśli jest nieaktywny.
            LeaderBoardUI leaderBoard = FindObjectOfType<LeaderBoardUI>(true); // true oznacza, że szuka obiektów nieaktywnych.
            if (leaderBoard != null)
            {
                LeaderBoardUI.Instance = leaderBoard; // Ręczne ustawienie instancji.
            }
            else
            {
                Debug.LogError("LeaderBoardUI object not found in the scene!");
                return;
            }
        }

        LeaderBoardUI.Instance.gameObject.SetActive(true);
        LeaderBoardUI.Instance.UpdateLeaderBoard();
    }

    [ServerRpc(RequireOwnership = false)]
    public void SendCordsServerRpc(int x, int z, BoardPiece boardPiece, ServerRpcParams serverRpcParams)
    {
        // Some Client sent data about their new position to the serwer
        int SenderId = (int)serverRpcParams.Receive.SenderClientId;
        BoardSingleton.instance.PawnPositions[SenderId] = new PawnPosition(boardPiece, new Vector2(x, z));
        UpdateCordsClientRpc(x, z, boardPiece, SenderId, NetworkManager.Singleton.ConnectedClients.Count);
    }

    [ClientRpc]
    private void UpdateCordsClientRpc(int x, int z, BoardPiece boardPiece, int SenderId, int NumberOfClients)
    {
        // Sending data about new position of some client to all clients
        BoardSingleton.instance.PawnPositions[SenderId] = new PawnPosition(boardPiece, new Vector2(x, z));
    }
    
    [ServerRpc]
    public void SetPlayerInfoServerRpc(string name, LobbyManager.PlayerColor color, ServerRpcParams serverRpcParams)
    {
        int SenderId = (int)serverRpcParams.Receive.SenderClientId;
        BoardSingleton.instance.PawnsData[SenderId] = new PawnData(name, color);
    }
    
    [ClientRpc]
    private void UpdateAllPlayersInfoClientRpc(PawnData[] pawnsData)
    {
        int numberofplayers = LobbyManager.Instance.GetLobbyBeforeGame().Players.Count;
        for (var i = 0; i < numberofplayers; i++)
        {
            BoardSingleton.instance.PawnsData[i] = pawnsData[i];
        }
        PlayerBoardUI.Instance.UpdatePlayers();
    }

    [ServerRpc]
    private void NotifyServerToSendAllPlayerDataServerRpc()
    {
        UpdateAllPlayersInfoClientRpc(BoardSingleton.instance.PawnsData.ToArray());
    }
    
    [ServerRpc]
    private void SetPlayerColorServerRpc(LobbyManager.PlayerColor color)
    {
        playerColor.Value = color;
    }
}
