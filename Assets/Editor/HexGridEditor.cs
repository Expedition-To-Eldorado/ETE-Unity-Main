using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(HexGrid))]
public class HexGridEditor : Editor
{
    void OnSceneGUI()
    {
        HexGrid hexGrid = (HexGrid)target;

        for (int z = 0; z <= hexGrid.Size * 2; z++)
        {
            for (int x = 0; x <= hexGrid.Size * 2; x++)
            {
                if (x + z >= hexGrid.Size && x + z <= hexGrid.Size * 3)
                {
                    Vector3 centrePosition = HexMetrics.Center(hexGrid.HexSize, x, z, hexGrid.Orientation) + hexGrid.transform.position;
                    //Show the coordinater in a label
                    Handles.Label(centrePosition + Vector3.forward * 0.5f, $"[{x}, {z}]");
                }
            }
        }
    }
}
