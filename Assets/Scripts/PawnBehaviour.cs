using GeneralEnumerations;
using System.Collections;
using System.Collections.Generic;
using System.Security.Cryptography;
//using UnityEditor.ShaderKeywordFilter;
using UnityEngine;

public class PawnBehaviour : MonoBehaviour
{
    [SerializeField] Vector3 offset = new Vector3 (1.46f, -0.42f, 1f);
    [SerializeField] BoardPiece currentBoardPiece;
    [SerializeField] int xCurrentPos;
    [SerializeField] int zCurrentPos;
    public bool isActivePawn;

    private ErrorMsg movePawn(int x, int z, HexGrid boardPiece, string terrainName, string cardType, int cardPower)
    {
        Vector3 centre = HexMetrics.Center(boardPiece.HexSize, x, z, boardPiece.Orientation) + boardPiece.gridOrigin;
        
        ErrorMsg errCode = checkIfCanMove(centre, x, z, boardPiece, terrainName, cardType, cardPower);
        if (errCode == ErrorMsg.OK)
        {
            transform.position = centre + offset;
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
        if (!isActivePawn)
        {
            //Debug.Log("not active pawn" + this.name);
            return ErrorMsg.NOT_ACTIVE_PAWN;
        }

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
        GameObject Pawns = GameObject.Find("Pawns");
        PawnBehaviour pawnBehaviour;
        foreach (Transform pawn in Pawns.transform)
        {
            pawnBehaviour = pawn.GetComponent<PawnBehaviour>();
            if(pawnBehaviour != this)
            {
                if (pawnBehaviour.xCurrentPos == x
                    && pawnBehaviour.zCurrentPos == z
                    && pawnBehaviour.currentBoardPiece == boardPiece.BoardPieceLetter)
                {
                    Debug.Log("Field is occupied by player " + pawn.name +" the caller is " + this.name);
                    return ErrorMsg.FIELD_OCCUPIED;
                }
            }
        }

        //check if selected card matches the field and its power
        if (cardType != terrainName)
        {
            Debug.Log("Chosen card " + cardType + " does not match the chosen field " + terrainName);
            return ErrorMsg.CARD_NOT_MATCHING;
        }
        

        return ErrorMsg.OK;
    }
}
