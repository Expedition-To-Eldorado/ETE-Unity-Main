using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Threading.Tasks;
using GeneralEnumerations;
public class HexGrid : MonoBehaviour
{
    [field: SerializeField] public HexOrientation Orientation { get; private set; }
    [field: SerializeField] public int Size { get; private set; }
    [field: SerializeField] public int HexSize { get; private set; }
    [field:SerializeField] public int BatchSize { get; private set; }
    [SerializeField] public BoardPiece BoardPieceLetter;
    public int BoardPiece { get; private set; }

    [SerializeField] private List<HexCell> cells = new List<HexCell>();
    private Task<List<HexCell>> hexGenerationTask;
    private Vector3 gridOrigin;
    public event System.Action OnMapInfoGenerated;
    public event System.Action<float> OnCellBatchGenerated;
    public event System.Action OnCellInstancesGenerated;

    private void Awake()
    {
        gridOrigin = transform.position;
        BoardPiece = (int)BoardPieceLetter;
        //Board.SetBoardPieces();
    }

    private void Start()
    {
        hexGenerationTask = Task.Run(()=>GenerateHexCellData());
    }

    private void Update()
    {
        if(hexGenerationTask != null && hexGenerationTask.IsCompleted)
        {
            cells = hexGenerationTask.Result;
            OnMapInfoGenerated?.Invoke();
            StartCoroutine(InstantiateCells());
            hexGenerationTask = null; //Clear the task
        }
    }

    private List<HexCell> GenerateHexCellData()
    {
        Debug.Log("Generating Hex Cell Data");
        List<HexCell> hexCells = new List<HexCell>();
        for (int z = 0; z <= Size * 2; z++)
        {
            for (int x = 0; x <= Size * 2; x++)
            {
                if (x + z >= Size && x + z <= Size * 3)
                {
                    Vector3 centrePosition = HexMetrics.Center(HexSize, x, z, Orientation) + gridOrigin;
                    HexCell cell = new HexCell();
                    cell.SetCoordinates(new Vector2(x, z), Orientation);
                    cell.Grid = this;
                    cell.HexSize = HexSize;
                    TerrainType terrain = BoardSingleton.instance.TerrainTypes[BoardSingleton.instance.Pieces[BoardPiece][z][x]];
                    cell.SetTerrainType(terrain);
                    hexCells.Add(cell);
                }

            }
        }
        return hexCells;
    }

    private IEnumerator InstantiateCells()
    {
        int batchCount = 0;
        int totalBatches = Mathf.CeilToInt(cells.Count / BatchSize);
        for (int i = 0; i < cells.Count; i++)
        {
            cells[i].CreateTerrain();
            if(i % BatchSize == 0)
            {
                batchCount++;
                OnCellBatchGenerated?.Invoke((float)batchCount / totalBatches);
                yield return null;
            }
        }
        OnCellInstancesGenerated?.Invoke();
    }

    private void OnDrawGizmos()
    {
        for(int z = 0; z <= Size * 2; z++)
        {
            for (int x = 0; x <= Size * 2; x++)
            {
                if (x + z >= Size && x + z <= Size*3)
                {
                    Vector3 centrePosition = HexMetrics.Center(HexSize, x, z, Orientation) + transform.position;
                    for (int s = 0; s < HexMetrics.Corners(HexSize, Orientation).Length; s++)
                    {
                        Gizmos.DrawLine(
                            centrePosition + HexMetrics.Corners(HexSize, Orientation)[s % 6],
                            centrePosition + HexMetrics.Corners(HexSize, Orientation)[(s + 1) % 6]
                            );
                    }
                }
                
            }
        }
    }
}

public enum HexOrientation
{
    FlatTop,
    PointyTop
}
