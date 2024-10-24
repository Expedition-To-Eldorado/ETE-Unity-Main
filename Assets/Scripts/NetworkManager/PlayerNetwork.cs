using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.Netcode;
using UnityEngine;
using GeneralEnumerations;

public class PlayerNetwork : NetworkBehaviour
{
    //Networking fields
    [SerializeField] private MeshRenderer meshRenderer;
    [SerializeField] private List<Material> PawnMaterials;
    public static Action UseCard;
    public static Action clearMultipleChosenCards;
    public static Action burnMultipleCards;

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
            //Player = this.transform;
            if (OwnerClientId >= 5)
            {
                // Not working :(
                // Destroy(gameObject);
                return;
            }
            transform.position = new Vector3(32f + (int)OwnerClientId*10, 0.55f, -4.3f);
        }
        meshRenderer.material = PawnMaterials[(int)OwnerClientId];
    }

    private void Update()
    {
        
    }

    private void OnEnable()
    {
        HexGridMeshGenerator.MovePawn += movePawn;
    }

    private void OnDisable()
    {
        HexGridMeshGenerator.MovePawn -= movePawn;
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
        }

        if (errCode == ErrorMsg.OK)
        {
            card.leftPower -= terrainPower;
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

        if (terrainName == "EndJungle" && cardType == "Jungle")
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

        //check if selected card matches the field and its power
        if (cardType != terrainName && cardType != "All")
        {
            Debug.Log("Chosen card " + cardType + " does not match the chosen field " + terrainName);
            return ErrorMsg.CARD_NOT_MATCHING;
        }

        return ErrorMsg.OK;
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
        // Print Positions of all clients in game in all clients debug log
        // int i = 0;
        // foreach( var Pawn in BoardSingleton.instance.PawnPositions) {
        //     Debug.Log("Player"+ i + ": " + Pawn.ToString());
        //     i++;
        //     if (i >= NumberOfClients) break;
        // }
    }
    
}
