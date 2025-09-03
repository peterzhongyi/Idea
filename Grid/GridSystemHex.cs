using UnityEngine;
using System;
using System.Collections.Generic;

public class GridSystemHex<TGridObject>
{
    private const float HEX_VERTICAL_OFFSET_MULTIPLIER = 0.75f;
    private int width;
    private int height;
    private float cellSize;
    private TGridObject[,] gridObjectArray;

    public GridSystemHex(int width, int height, float cellSize, Func<GridSystemHex<TGridObject>, GridPosition, TGridObject> createGridObject)
    {
        this.width = width;
        this.height = height;
        this.cellSize = cellSize;

        gridObjectArray = new TGridObject[width, height];
        for (int x = 0; x < width; x++)
        {
            for (int z = 0; z < height; z++)
            {
                GridPosition gridPosition = new GridPosition(x, z);
                gridObjectArray[x, z] = createGridObject(this, gridPosition);
            }
        }

    }

    public Vector3 GetWorldPosition(GridPosition gridPosition)
    {
        // Shift odd rows of hex grids by half of cell size.
        return
            new Vector3(gridPosition.x, 0, 0) * cellSize +
            new Vector3(0, 0, gridPosition.z) * cellSize * HEX_VERTICAL_OFFSET_MULTIPLIER +
            ((gridPosition.z % 2) == 1 ? new Vector3(1, 0, 0) * cellSize * 0.5f : Vector3.zero);
    }

    public GridPosition GetGridPosition(Vector3 worldPosition)
    {
        GridPosition roughXZ = new GridPosition(
            Mathf.RoundToInt(worldPosition.x / cellSize),
            Mathf.RoundToInt(worldPosition.z / cellSize / HEX_VERTICAL_OFFSET_MULTIPLIER)
        );

        bool oddRow = roughXZ.z % 2 == 1;

        // Get all 6 neighbors of the roughtXZ.
        List<GridPosition> neightborGridPositionList = new List<GridPosition>
        {
            // left
            roughXZ + new GridPosition(-1, 0),
            // right
            roughXZ + new GridPosition(1, 0),
            // above
            roughXZ + new GridPosition(0, 1),
            // below
            roughXZ + new GridPosition(0, -1),
            // Diagnal up
            roughXZ + new GridPosition(oddRow ? 1 : -1, 1),
            // Diagnal down
            roughXZ + new GridPosition(oddRow ? 1 : -1, -1),
        };

        GridPosition closedGridPosition = roughXZ;

        foreach (GridPosition neighbor in neightborGridPositionList)
        {
            if (Vector3.Distance(worldPosition, GetWorldPosition(neighbor)) <
                Vector3.Distance(worldPosition, GetWorldPosition(closedGridPosition)))
            {
                closedGridPosition = neighbor;
            }
        }

        return closedGridPosition;
    }

    public void CreateDebugObjects(Transform debugPrefab)
    {
        for (int x = 0; x < width; x++)
        {
            for (int z = 0; z < height; z++)
            {
                GridPosition gridPosition = new GridPosition(x, z);

                Transform debugTransform = GameObject.Instantiate(debugPrefab, GetWorldPosition(gridPosition), Quaternion.identity);
                GridDebugObject gridDebugObject = debugTransform.GetComponent<GridDebugObject>();
                gridDebugObject.SetGridObject(GetGridObject(gridPosition));
            }
        }
    }

    public TGridObject GetGridObject(GridPosition gridPosition)
    {
        return gridObjectArray[gridPosition.x, gridPosition.z];
    }

    public bool IsValidGridPosition(GridPosition gridPosition)
    {
        return gridPosition.x >= 0 &&
                gridPosition.z >= 0 &&
                gridPosition.x < width &&
                gridPosition.z < height;
    }

    public int GetWidth()
    {
        return width;
    }

    public int GetHeight()
    {
        return height;
    }
}
