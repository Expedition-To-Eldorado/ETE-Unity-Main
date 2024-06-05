using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using GeneralEnumerations;
using Unity.Netcode;

[RequireComponent(typeof(MeshFilter), typeof(MeshRenderer), typeof(MeshCollider))]
public class HexGridMeshGenerator : MonoBehaviour
{
    [field:SerializeField] public LayerMask gridLayer { get; private set; }
    [field: SerializeField] public HexGrid hexGrid { get; private set; }
    [field: SerializeField] public Shader hexClickedShader { get; private set; }
    //public Transform explosionTest;
    //public static Action<int, int, HexGrid, string, string, int> MovePawn;
    public delegate ErrorMsg movePawn(int x, int z, HexGrid boardPiece, string terrainName, string cardType, int cardPower);
    public static movePawn MovePawn;

    private void Awake()
    {
        if (hexGrid == null)
        {
            hexGrid = GetComponentInParent<HexGrid>();
        }
        if (hexGrid == null)
        {
            Debug.LogError("HexGridMeshGenerator could not find a HexGrid component in its parent or itself.");
        }
    }

    private void OnEnable()
    {
        MouseController.instance.OnLeftMouseClick += OnLeftMouseClick;
        //MouseController.instance.OnRightMouseClick += OnRightMouseClick;
    }
    
    private void OnDisable()
    {
        MouseController.instance.OnLeftMouseClick -= OnLeftMouseClick;
        //MouseController.instance.OnRightMouseClick -= OnRightMouseClick;
    }

    public void CreateHexMesh()
    {
        CreateHexMesh(hexGrid.Size, hexGrid.HexSize, hexGrid.Orientation, gridLayer);
    }

    public void CreateHexMesh(HexGrid hexGrid, LayerMask layerMask)
    {
        this.hexGrid = hexGrid;
        this.gridLayer = layerMask;
        CreateHexMesh(hexGrid.Size, hexGrid.HexSize, hexGrid.Orientation, layerMask);
    }

    public void CreateHexMesh(int size, float hexSize, HexOrientation orientation, LayerMask layerMask)
    {
        ClearHexGridMesh();
        int numOfHex = (3 * size * size) + (3 * size) + 1;
        Vector3[] vertices = new Vector3[7 * numOfHex];
        int currentHex = 0;
        for (int z = 0; z <= size * 2; z++)
        {
            for (int x = 0; x <= size * 2; x++)
            {
                if (x + z >= size && x + z <= size * 3 && hexGrid.CheckEndPiece(x, z))
                {
                    Vector3 centrePosition = HexMetrics.Center(hexSize, x, z, orientation);
                    vertices[currentHex * 7] = centrePosition;
                    for (int s = 0; s < HexMetrics.Corners(hexSize, orientation).Length; s++)
                    {
                        vertices[currentHex * 7 + s + 1] = centrePosition + HexMetrics.Corners(hexSize, orientation)[s % 6];
                    }
                    currentHex++;
                }

            }
        }

        int[] triangles = new int[3 * 6 * numOfHex];
        int currentHexForTriangles = 0;
        for (int z = 0; z <= size * 2; z++)
        {
            for (int x = 0; x <= size * 2; x++)
            {
                if (x + z >= size && x + z <= size * 3 && hexGrid.CheckEndPiece(x, z))
                {
                    for (int s = 0; s < HexMetrics.Corners(hexSize, orientation).Length; s++)
                    {
                        int cornerIndex = s + 2 > 6 ? s + 2 - 6 : s + 2;
                        triangles[3 * 6 * currentHexForTriangles + s * 3 + 2] = currentHexForTriangles * 7;
                        triangles[3 * 6 * currentHexForTriangles + s * 3 + 1] = currentHexForTriangles * 7 + s + 1;
                        triangles[3 * 6 * currentHexForTriangles + s * 3 + 0] = currentHexForTriangles * 7 + cornerIndex;
                    }
                    currentHexForTriangles++;
                }

            }
        }

        Mesh mesh = new Mesh();
        mesh.name = "Hex Mesh";
        mesh.vertices = vertices;
        mesh.triangles = triangles;
        mesh.RecalculateNormals();
        mesh.RecalculateBounds();
        mesh.Optimize();
        mesh.RecalculateUVDistributionMetrics();

        GetComponent<MeshFilter>().sharedMesh = mesh;
        GetComponent<MeshCollider>().sharedMesh = mesh;

        int gridLayerIndex = GetLayerIndex(layerMask);
        Debug.Log("Layer Index: " + gridLayerIndex);

        gameObject.layer = gridLayerIndex;
    }

    public void ClearHexGridMesh()
    {
        if (GetComponent<MeshFilter>().sharedMesh == null)
            return;
        GetComponent<MeshFilter>().sharedMesh.Clear();
        GetComponent<MeshCollider>().sharedMesh.Clear();
    }

    private int GetLayerIndex(LayerMask layerMask)
    {
        int layerMaskValue = layerMask.value;
        Debug.Log("Layer Mask Value: " + layerMaskValue);
        for (int i = 0; i < 32; i++)
        {
            if(((1 << i) & layerMaskValue) != 0)
            {
                Debug.Log("Layer Index Loop: " + i);
                return i;
            }
        }
        return 0;
    }

    private void OnLeftMouseClick(RaycastHit hit)
    {
        HexGrid grid = hit.transform.GetComponent<HexGrid>();
        Debug.Log("Hit object: " + hit.transform.name + " at position " + hit.point);
        float localX = hit.point.x - hit.transform.position.x;
        float localZ = hit.point.z - hit.transform.position.z;
        int x = (int)HexMetrics.CoordinateToAxial(localX, localZ, hexGrid.HexSize, hexGrid.Orientation).x;
        int z = (int)HexMetrics.CoordinateToAxial(localX, localZ, hexGrid.HexSize, hexGrid.Orientation).y;
        TerrainType terrain = BoardSingleton.instance.TerrainTypes[BoardSingleton.instance.Pieces[grid.BoardPiece][z][x]];
        Debug.Log("Position:\tBoardPiece " + grid.BoardPiece + "\tCords (" + x + ", " + z + ")" +
            "\n\t    TerrainType:\t" + terrain.name);
        //Im not sure if this should be called in here. 
        //This delegate call is for debug only for the time being!!!
        ErrorMsg errcode = (ErrorMsg)(MovePawn?.Invoke(x, z, grid, terrain.name, "Jungle1", 1));
        Debug.Log(errcode.ToString());
    }

    /*private void OnRightMouseClick(RaycastHit hit)
    {
    
    }*/
}
