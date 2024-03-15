using System.Collections;
using System.Collections.Generic;
using System.Security.Cryptography;
using UnityEngine;

public class PawnBehaviour : MonoBehaviour
{


    // Start is called before the first frame update
    void Start()
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

    // Update is called once per frame
    void Update()
    {
        
    }

    private void movePawn(int x, int z, int boardPiece, string terrainName)
    {
        Debug.Log("I recieved Message:: Position:\tBoardPiece " + boardPiece + "\tCords (" + x + ", " + z + ")" +
            "\n\t    TerrainType:\t" + terrainName);
    }
}
