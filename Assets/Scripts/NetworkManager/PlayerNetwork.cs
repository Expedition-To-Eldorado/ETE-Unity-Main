using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using GeneralEnumerations;

public class PlayerNetwork : NetworkBehaviour
{
    //Networking fields
    [SerializeField] private List<Material> PawnMaterials;
    private Transform Player;
    private Renderer[] childRenderers;
    private NetworkVariable<int> NumberOfPlayers = new NetworkVariable<int>(0, NetworkVariableReadPermission.Everyone,
        NetworkVariableWritePermission.Server);
        
    //PawnMoving
    [SerializeField] Vector3 offset = new Vector3 (1.46f, 0.55f, 1f);
    [SerializeField] BoardPiece currentBoardPiece;
    [SerializeField] int xCurrentPos;
    [SerializeField] int zCurrentPos;

    private Transform currentPosTransform;
    private Transform movePosTransform;
    //public bool isActivePawn;
    
    public override void OnNetworkSpawn()
    {
        
        // making sure that code is executed only for the owner of the script
        if (IsOwner)
        {
            Player = this.transform;
            if (OwnerClientId >= 5)
            {
                //TODO - Implement Limit of the players to join the game
                //this is not working
                //Destroy(Player.gameObject);
                Player.GetComponent<NetworkObject>().Despawn();
                return;
            }

            childRenderers = Player.GetComponentsInChildren<Renderer>();
            Debug.Log(OwnerClientId);
            childRenderers[0].sharedMaterial = PawnMaterials[(int)OwnerClientId];
            transform.position = new Vector3(32f + (int)OwnerClientId*10, 0.55f, -4.3f);
        }
        
    }

    private void Update()
    {
        
    }

    private void OnEnable()
    {
        HexGridMeshGenerator.MovePawn += movePawn;
        //HexCell.MovePawn += movePawn;
    }

    private void OnDisable()
    {
        HexGridMeshGenerator.MovePawn -= movePawn;
        //HexCell.MovePawn -= movePawn;
    }
    
    
    private ErrorMsg movePawn(int x, int z, HexGrid boardPiece, string terrainName, string cardType, int cardPower, RaycastHit hitCell)
    {
        movePosTransform = hitCell.transform;
        Vector3 centre = HexMetrics.Center(boardPiece.HexSize, x, z, boardPiece.Orientation) + boardPiece.gridOrigin;
        
        ErrorMsg errCode = checkIfCanMove(centre, x, z, boardPiece, terrainName, cardType, cardPower);
        if (errCode == ErrorMsg.OK)
        {
            LeanTween.move(this.gameObject, centre + offset, 0.5f).setEase(LeanTweenType.easeInOutQuint);
            movePosTransform.gameObject.tag = "Occupied";
            if (currentPosTransform && currentPosTransform != movePosTransform)
                currentPosTransform.gameObject.tag = "Untagged";
            currentPosTransform = movePosTransform;
            //transform.position = centre + offset;
            xCurrentPos = x;
            zCurrentPos = z;
            currentBoardPiece = boardPiece.BoardPieceLetter;
        }
        else
        {
            return errCode;
        }
        
        return ErrorMsg.OK;
    }

    //i hate it that it is here MOVE IT SOMEWHERE ELSE FUCKo
    private ErrorMsg checkIfCanMove(Vector3 centre, int x, int z, HexGrid boardPiece, string terrainName, string cardType, int cardPower)
    {
        //check if the pawn is active
        // if (!isActivePawn)
        // {
        //     //Debug.Log("not active pawn" + this.name);
        //     return ErrorMsg.NOT_ACTIVE_PAWN;
        // }

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

        //check if any other player is on the selected field
        //TODO - update it with Flamasters new field status system
        if (movePosTransform.CompareTag("Occupied"))
        {
            Debug.Log("Field is occupied by other player, the caller is: " + this.name);
            return ErrorMsg.FIELD_OCCUPIED;
        }
            
        // GameObject Pawns = GameObject.Find("Pawns");
        // PawnBehaviour pawnBehaviour;
        // foreach (Transform pawn in Pawns.transform)
        // {
        //     pawnBehaviour = pawn.GetComponent<PawnBehaviour>();
        //     if(pawnBehaviour != this)
        //     {
        //         if (pawnBehaviour.xCurrentPos == x
        //             && pawnBehaviour.zCurrentPos == z
        //             && pawnBehaviour.currentBoardPiece == boardPiece.BoardPieceLetter)
        //         {
        //             Debug.Log("Field is occupied by player " + pawn.name +" the caller is " + this.name);
        //             return ErrorMsg.FIELD_OCCUPIED;
        //         }
        //     }
        // }

        //check if selected card matches the field and its power
        if (cardType != terrainName)
        {
            Debug.Log("Chosen card " + cardType + " does not match the chosen field " + terrainName);
            return ErrorMsg.CARD_NOT_MATCHING;
        }
        

        return ErrorMsg.OK;
    }
}
