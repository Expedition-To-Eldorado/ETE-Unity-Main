using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HexMetrics
{
    public static float OuterRadius (float hexSize)
    {
        return hexSize;
    }

    public static float InnerRadius(float hexSize)
    {
        return hexSize * 0.866025404f;
    }

    public static Vector3[] Corners(float hexSize, HexOrientation orientation)
    {
        Vector3[] corners = new Vector3[6];
        for (int i = 0; i < 6; i++)
        {
            corners[i] = Corner(hexSize, orientation, i);
        }
        return corners;
    }

    public static Vector3 Corner(float hexSize, HexOrientation orientation, int index)
    {
        float angle = 60f * index;
        if (orientation == HexOrientation.PointyTop)
        {
            angle += 30f;
        }
        Vector3 corner = new Vector3(hexSize * Mathf.Cos(angle * Mathf.Deg2Rad),
            0f,
            hexSize * Mathf.Sin(angle * Mathf.Deg2Rad)
            );

        return corner;
    }

    public static Vector3 Center(float hexSize, int x, int z, HexOrientation orientation)
    {
        Vector3 centrePosition;
        if (orientation == HexOrientation.PointyTop)
        {
            centrePosition.x = (x + z * 0.5f) * (InnerRadius(hexSize) * 2f);
            centrePosition.y = 0f;
            centrePosition.z = z * (OuterRadius(hexSize) * 1.5f);
        }
        else
        {
            centrePosition.x = (x) * (OuterRadius(hexSize) * 1.5f);
            centrePosition.y = 0f;
            centrePosition.z = (z + x * 0.5f) * (InnerRadius(hexSize) * 2f);
        }
        return centrePosition;
    }

    public static Vector2 CubeToAxial(Vector3 cube)
    {
        return new Vector2(cube.x, cube.y);
    }

    public static Vector3 CubeRound(Vector3 frac)
    {
        Vector3 roundedCoordinates = new Vector3();
        int rx = Mathf.RoundToInt(frac.x);
        int ry = Mathf.RoundToInt(frac.y);
        int rz = Mathf.RoundToInt(frac.z);
        float xDiff = Mathf.Abs(rx - frac.x);
        float yDiff = Mathf.Abs(ry - frac.y);
        float zDiff = Mathf.Abs(rz - frac.z);
        if (xDiff > yDiff && xDiff > zDiff)
        {
            rx = -ry - rz;
        }
        else if (yDiff > zDiff)
        {
            ry = -rx - rz;
        }
        else
        {
            rz = -rx - ry;
        }
        roundedCoordinates.x = rx;
        roundedCoordinates.y = ry;
        roundedCoordinates.z = rz;
        return roundedCoordinates;
    }

    public static Vector3 AxialToCube(int q, int r)
    {
        return new Vector3(q, r, -q - r);
    }
    public static Vector3 AxialToCube(float q, float r)
    {
        return new Vector3(q, r, -q - r);
    }
    public static Vector3 AxialToCube(Vector2 axialCoord)
    {
        return AxialToCube(axialCoord.x, axialCoord.y);
    }

    public static Vector2 AxialRound(Vector2 coordinates)
    {
        return CubeToAxial(CubeRound(AxialToCube(coordinates.x, coordinates.y)));
    }

    public static Vector2 CoordinateToAxial(float x, float z, float hexSize, HexOrientation orientation)
    {
        if (orientation == HexOrientation.PointyTop)
        {
            return CoordinateToPointyAxial(x, z, hexSize);
        }
        else
        {
            return CoordinateToFlatAxial(x, z, hexSize);
        }
    }

    private static Vector2 CoordinateToPointyAxial(float x, float z, float hexSize)
    {
        Vector2 poityHexCoordinates = new Vector2();
        poityHexCoordinates.x = (Mathf.Sqrt(3) / 3 * x - 1f / 3 * z) / hexSize;
        poityHexCoordinates.y = (2f / 3 * z) / hexSize;
        return AxialRound(poityHexCoordinates);
    }

    private static Vector2 CoordinateToFlatAxial(float x, float z, float hexSize)
    {
        Vector2 flatHexCoordinates = new Vector2();
        flatHexCoordinates.x = (2f / 3 * x) / hexSize;
        flatHexCoordinates.y = (-1f / 3 * x + Mathf.Sqrt(3) / 3 * z) / hexSize;
        return AxialRound(flatHexCoordinates);
    }
}
