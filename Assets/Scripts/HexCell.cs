using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HexCell : MonoBehaviour
{
    [Header("Cell Properties")]
    [SerializeField] private HexOrientation orientation;
    [field: SerializeField] public HexGrid Grid { get; set; }
    [field: SerializeField] public float HexSize { get; set; }
    [field: SerializeField] public TerrainType TerrainType { get; private set; }
    [field: SerializeField] public Vector3 CubeCoordinates { get; private set; }
    [field:SerializeField] public Vector2 AxialCoordinates { get; set; }
    [field:NonSerialized] public List<HexCell> Neighbours { get; private set; }

    [field:SerializeField] private Transform terrain { get; set; }

    public void SetCoordinates(Vector2 axialCoordinates, HexOrientation orientation)
    {
        this.orientation = orientation;
        AxialCoordinates = axialCoordinates;
        CubeCoordinates = HexMetrics.AxialToCube(axialCoordinates);
    }

    public void SetTerrainType(TerrainType terrainType)
    {
        TerrainType = terrainType;
    }

    public void CreateTerrain()
    {
        if (TerrainType == null)
        {
            Debug.LogError("TerrainType is null");
            return;
        }
        if (Grid == null)
        {
            Debug.LogError("Grid is null");
            return;
        }
        if (HexSize == 0)
        {
            Debug.LogError("HexSize is 0");
            return;
        }
        if (TerrainType.Prefab == null)
        {
            Debug.LogError("TerrainType Prefab is null");
            return;
        }

        Vector3 centrePosition = HexMetrics.Center(
            HexSize,
            (int)AxialCoordinates.x,
            (int)AxialCoordinates.y, orientation
            ) + Grid.transform.position;

        terrain = UnityEngine.Object.Instantiate(
            TerrainType.Prefab,
            centrePosition,
            Quaternion.identity,
            Grid.transform
            );
        terrain.gameObject.layer = LayerMask.NameToLayer("Grid");

        //TODO: Adjust the size of the prefab to the size of the grid cell

        if (orientation == HexOrientation.FlatTop)
        {
            terrain.Rotate(new Vector3(0, 30, 0));
        }
        //Temporary random rotation to make the terrain look more natural
        //int randomRotation = UnityEngine.Random.Range(0, 6);
        terrain.Rotate(new Vector3(0, 60, 0));
    }

    public void SetNeighbours(List<HexCell> neighbours)
    {
        Neighbours = neighbours;
    }
    public void ClearTerrain()
    {
        if (terrain != null)
        {
            UnityEngine.Object.Destroy(terrain.gameObject);
        }
    }
}
