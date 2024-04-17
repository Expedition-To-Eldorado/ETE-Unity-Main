using System.Collections;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;

public class BoardSingleton : MonoBehaviour
{
    public static BoardSingleton instance { get; private set; }
    private static TerrainType[] AllTerrains;
    public List<TerrainType> TerrainTypes = new List<TerrainType>();
    public List<List<List<int>>> Pieces = new List<List<List<int>>>();

    public void Awake()
    {
        if (!gameObject.activeSelf)
        {
            Debug.LogWarning("BoardSingleton GameObject is inactive. Ensure it is active in the scene.");
            return;
        }
        // If there is an instance, and it's not me, delete myself.
        if (instance != null && instance != this)
        {
            Destroy(this);
        }
        else
        {
            instance = this;
        }
        //SetTerrainTypes();
        SetBoardPieces();
    }
    private void SetTerrainTypes()
    {
        //Creating Array with all ScriptableObjects with type TerrainTypes
        AllTerrains = (TerrainType[])Resources.FindObjectsOfTypeAll(typeof(TerrainType));

        //Adding all TerrainTypes to TerrainTypes List according to ID value
        for (int i = 0; i < AllTerrains.Length; i++)
        {
            TerrainType terrain = AllTerrains.SingleOrDefault(x => x.ID == i);
            TerrainTypes.Add(terrain);
        }
    }

    public enum BoardPiece
    {
        A, B, C, D, E, F, G, H, I, J, K, M, N
    }

    private void SetBoardPieces()
    {
        // A boardPiece = 0
        Pieces.Add(new List<List<int>> {
                            new List<int> { -1, -1, -1, 18, 19, 20, 21 },
                            new List<int> { -1, -1,  1,  1,  1,  1,  1 },
                            new List<int> { -1,  1,  1,  4,  1,  8,  1 },
                            new List<int> {  1,  4,  1,  8,  1,  4,  1 },
                            new List<int> {  1,  0,  1,  1,  1,  1, -1 },
                            new List<int> {  8,  0,  1,  1,  4, -1, -1 },
                            new List<int> {  1,  12,  1,  1, -1, -1, -1 }
                        });
        // B boardPiece = 1
        Pieces.Add(new List<List<int>> {
                            new List<int> { -1, -1, -1, 18, 19, 20, 21 },
                            new List<int> { -1, -1,  1,  1,  1,  1,  1 },
                            new List<int> { -1,  1,  1,  1,  4,  1,  1 },
                            new List<int> {  1,  1,  8,  1,  1,  0,  8 },
                            new List<int> {  1,  1,  4,  1,  4, 12, -1 },
                            new List<int> {  1,  1,  4,  1,  8, -1, -1 },
                            new List<int> {  8,  1,  1,  1, -1, -1, -1 }
                        });
        // C boardPiece = 2
        Pieces.Add(new List<List<int>> {
                            new List<int> { -1, -1, -1,  1,  1,  8,  8 },
                            new List<int> { -1, -1,  4, 15,  1,  4,  8 },
                            new List<int> { -1,  4, 15,  8,  8,  4,  4 },
                            new List<int> {  8,  4, 15,  0,  8, 15, 15 },
                            new List<int> {  8,  8,  4,  4, 15,  8, -1 },
                            new List<int> {  1,  4, 15,  8,  8, -1, -1 },
                            new List<int> {  1,  1, 15, 15, -1, -1, -1 }
                        });
        // D boardPiece = 3
        Pieces.Add(new List<List<int>> {
                            new List<int> { -1, -1, -1,  1,  1,  1,  2 },
                            new List<int> { -1, -1,  1,  8,  8,  8,  1 },
                            new List<int> { -1,  1,  8,  8,  9,  8,  1 },
                            new List<int> {  2,  8,  9,  0,  1,  1,  2 },
                            new List<int> {  1,  8,  1,  0,  6,  1, -1 },
                            new List<int> {  1,  1,  6,  4,  0, -1, -1 },
                            new List<int> {  2,  1,  0, 10, -1, -1, -1 }
                        });
        // E boardPiece = 4
        Pieces.Add(new List<List<int>> {
                            new List<int> { -1, -1, -1,  0, 15, 15,  1 },
                            new List<int> { -1, -1, 15,  0,  2,  9,  1 },
                            new List<int> { -1,  1, 15, 17, 15,  0,  1 },
                            new List<int> {  1,  2,  3,  8,  1,  2, 15 },
                            new List<int> { 15,  1,  0,  8,  8,  1, -1 },
                            new List<int> {  1,  2,  1,  0,  4, -1, -1 },
                            new List<int> { 12,  4,  4,  4, -1, -1, -1 }
                        });
        // F boardPiece = 5
        Pieces.Add(new List<List<int>> {
                            new List<int> { -1, -1, -1, 12,  2, 15, 13 },
                            new List<int> { -1, -1,  1,  3,  1,  9,  9 },
                            new List<int> { -1, 15,  4, 16,  1,  1, 15 },
                            new List<int> { 15,  4,  5,  0,  2,  1, 15 },
                            new List<int> { 15,  1,  9, 10,  8, 15, -1 },
                            new List<int> {  2,  1,  0,  8,  8, -1, -1 },
                            new List<int> {  1,  0,  0,  8, -1, -1, -1 }
                        });
        // G boardPiece = 6
        Pieces.Add(new List<List<int>> {
                            new List<int> { -1, -1, -1,  1,  1,  4,  0 },
                            new List<int> { -1, -1,  1,  2,  5,  0,  4 },
                            new List<int> { -1,  1,  4,  5,  7,  5,  1 },
                            new List<int> {  1,  0, 15,  6,  5,  2,  1 },
                            new List<int> {  1,  4,  5, 15,  4,  1, -1 },
                            new List<int> {  1,  2,  4,  0,  1, -1, -1 },
                            new List<int> { 12,  1,  1,  1, -1, -1, -1 }
                        });
        // H boardPiece = 7
        Pieces.Add(new List<List<int>> {
                            new List<int> { -1, -1, -1,  1,  2,  2,  2 },
                            new List<int> { -1, -1,  4,  1,  1,  1,  2 },
                            new List<int> { -1,  5,  5,  4,  4,  1,  2 },
                            new List<int> {  6,  0,  5,  5,  4,  1,  1 },
                            new List<int> {  5,  5,  4,  4,  8,  9, -1 },
                            new List<int> {  4,  8,  8,  8,  9, -1, -1 },
                            new List<int> {  8,  9,  9,  9, -1, -1, -1 }
                        });
        // I boardPiece = 8
        Pieces.Add(new List<List<int>> {
                            new List<int> { -1, -1, -1,  9,  9,  8,  1 },
                            new List<int> { -1, -1,  9,  8,  8,  1,  1 },
                            new List<int> { -1,  5, 17,  0,  0,  2,  2 },
                            new List<int> {  4,  5,  1,  2, 14,  0,  0 },
                            new List<int> {  4,  5,  1,  0,  2,  1, -1 },
                            new List<int> {  4,  1,  0,  1,  1, -1, -1 },
                            new List<int> {  1,  1,  1,  1, -1, -1, -1 }
                        });
        // J boardPiece = 9
        Pieces.Add(new List<List<int>> {
                            new List<int> { -1, -1, -1, 16, 15, 15, 15 },
                            new List<int> { -1, -1,  4,  0, 16, 16, 15 },
                            new List<int> { -1,  4,  5,  1,  1, 16, 16 },
                            new List<int> {  4,  5,  1, 12,  2,  8,  8 },
                            new List<int> {  4,  5,  2,  1,  9,  8, -1 },
                            new List<int> {  4,  4,  9,  0,  8, -1, -1 },
                            new List<int> {  4,  8,  8,  8, -1, -1, -1 }
                        });
        // K boardPiece = 10
        Pieces.Add(new List<List<int>> {
                            new List<int> { -1, -1, -1, 12,  1,  1,  2 },
                            new List<int> { -1, -1,  2,  1,  2,  1,  2 },
                            new List<int> { -1,  2,  7,  1,  3,  1,  2 },
                            new List<int> {  1,  1,  3,  1,  3,  1,  1 },
                            new List<int> {  2,  1,  3,  1, 10,  2, -1 },
                            new List<int> {  2,  1,  2,  1,  2, -1, -1 },
                            new List<int> {  2,  1,  1, 12, -1, -1, -1 }
                        });
        // L boardPiece = 11
        Pieces.Add(new List<List<int>> {
                            new List<int> { -1, -1, -1,  2,  2,  1,  0 },
                            new List<int> { -1, -1,  5, 13,  5,  1,  1 },
                            new List<int> { -1,  1,  2,  2,  1,  2,  1 },
                            new List<int> {  2,  1,  0,  1,  0,  1,  2 },
                            new List<int> {  2,  1,  1,  3,  1,  2, -1 },
                            new List<int> {  1,  8,  8,  3,  1, -1, -1 },
                            new List<int> {  8, 12, 12,  3, -1, -1, -1 }
                        });
        // M boardPiece = 12
        Pieces.Add(new List<List<int>> {
                            new List<int> { -1, -1, -1,  8,  8,  1,  1 },
                            new List<int> { -1, -1,  1,  1,  1, 16,  1 },
                            new List<int> { -1,  0,  0,  0,  0, 16,  1 },
                            new List<int> {  0,  1,  1, 16,  1,  1,  0 },
                            new List<int> {  1,  5,  1,  1,  8,  0, -1 },
                            new List<int> {  1,  7,  0,  0, 11, -1, -1 },
                            new List<int> {  1,  1,  1, 12, -1, -1, -1 }
                        });
        // N boardPiece = 13
        Pieces.Add(new List<List<int>> {
                            new List<int> { -1, -1, -1,  1,  1,  4,  4 },
                            new List<int> { -1, -1,  1,  1,  5,  5,  8 },
                            new List<int> { -1,  1,  2,  1,  6,  8,  8 },
                            new List<int> {  1,  1,  8,  7,  8,  1,  1 },
                            new List<int> {  1,  8,  6,  1,  2,  1, -1 },
                            new List<int> {  8,  8,  5,  1,  1, -1, -1 },
                            new List<int> {  8,  4,  4,  1, -1, -1, -1 }
                        });
    }
}
