using UnityEngine;
using System;
using System.Collections.Generic;

public class GridSystemHex : MonoBehaviour
{
    public static GridSystemHex Instance { get; private set; }

    private const float HEX_VERTICAL_OFFSET_MULTIPLIER = 0.75f;
    private const int MOVE_COST = 1;

    [SerializeField] private int width;
    [SerializeField] private int height;
    [SerializeField] private float cellSize;
    [SerializeField] private Transform gridObjectPrefab;
    [SerializeField] private LayerMask obstacleLayerMask;

    [Serializable]
    public struct GridVisualTypeMaterial
    {
        public GridVisualType gridVisualType;
        public Material material;
    }
    public enum GridVisualType
    {
        White,
        Blue,
        Red,
        Green,
        Yellow
    }
    [SerializeField] private List<GridVisualTypeMaterial> gridVisualTypeMaterialList;

    private GridObject lastSelectedGridObject;

    private GridObject[,] gridObjectArray;

    private void Awake()
    {
        if (Instance != null)
        {
            Debug.LogError("There's more than one GridSystemHex!" + transform + " - " + Instance);
            Destroy(gameObject);
            return;
        }
        Instance = this;

        gridObjectArray = new GridObject[width, height];
        for (int x = 0; x < width; x++)
        {
            for (int z = 0; z < height; z++)
            {
                GridPosition gridPosition = new GridPosition(x, z);
                Vector3 worldPosition = GetWorldPosition(gridPosition);

                Transform gridObjectTransform = Instantiate(gridObjectPrefab, worldPosition, Quaternion.identity);
                gridObjectArray[x, z] = gridObjectTransform.GetComponent<GridObject>();

                // Start from below ground
                float raycastOffsetDistance = 5f;
                if (Physics.Raycast(
                    worldPosition + Vector3.down * raycastOffsetDistance,
                    Vector3.up,
                    raycastOffsetDistance * 2,
                    obstacleLayerMask))
                {
                    gridObjectArray[x, z].SetIsWalkable(false);
                }
            }
        }
    }

    private void Update()
    {
        if (lastSelectedGridObject != null)
        {
            lastSelectedGridObject.HideSelected();
        }

        Vector3 mouseWorldPosition = MouseWorld.GetPosition();
        GridPosition gridPosition = GetGridPosition(mouseWorldPosition);

        if (IsValidGridPosition(gridPosition))
        {
            lastSelectedGridObject = gridObjectArray[gridPosition.x, gridPosition.z];
        }

        if (lastSelectedGridObject != null)
        {
            lastSelectedGridObject.ShowSelected();
        }
    }

    private void Start()
    {
        UnitActionSystem.Instance.OnSelectedActionChanged += UnitActionSystem_OnSelectedActionChanged;

        UpdateGridVisual();

        // for (int x = 0; x < LevelGrid.Instance.GetWidth(); x++)
        // {
        //     for (int z = 0; z < LevelGrid.Instance.GetHeight(); z++)
        //     {
        //         gridSystemVisualSingleArray[x, z].Show(GetGridVisualTypeMaterial(GridVisualType.White));
        //     }
        // }
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

    public GridObject GetGridObject(GridPosition gridPosition)
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

    public void AddUnitAtGridPosition(GridPosition gridPosition, Unit unit)
    {
        GridObject gridObject = GetGridObject(gridPosition);
        gridObject.AddUnit(unit);
    }

    public List<Unit> GetUnitListAtGridPosition(GridPosition gridPosition)
    {
        GridObject gridObject = GetGridObject(gridPosition);
        return gridObject.GetUnitList();
    }

    public void RemoveUnitAtGridPosition(GridPosition gridPosition, Unit unit)
    {
        GridObject gridObject = GetGridObject(gridPosition);
        gridObject.RemoveUnit(unit);
    }

    public void UnitMovedGridPosition(Unit unit, GridPosition fromGridPosition, GridPosition toGridPosition)
    {
        RemoveUnitAtGridPosition(fromGridPosition, unit);
        AddUnitAtGridPosition(toGridPosition, unit);

        UpdateGridVisual();
    }

    public bool HasAnyUnitOnGridPosition(GridPosition gridPosition)
    {
        GridObject gridObject = GetGridObject(gridPosition);
        return gridObject.HasAnyUnit();
    }

    public Unit GetUnitAtGridPosition(GridPosition gridPosition)
    {
        GridObject gridObject = GetGridObject(gridPosition);
        return gridObject.GetUnit();
    }

    public IInteractable GetInteractableAtGridPosition(GridPosition gridPosition)
    {
        GridObject gridObject = GetGridObject(gridPosition);
        return gridObject.GetInteractable();
    }

    public void SetInteractableAtGridPosition(GridPosition gridPosition, IInteractable interactable)
    {
        GridObject gridObject = GetGridObject(gridPosition);
        gridObject.SetInteractable(interactable);
    }

    private void UpdateGridVisual()
    {
        HideAllGridPosition();

        Unit selectedUnit = UnitActionSystem.Instance.GetSelectedUnit();
        Debug.Log("Selected Unit: " + selectedUnit.GetGridPosition());
        BaseAction selectedAction = UnitActionSystem.Instance.GetSelectedAction();
        Debug.Log("Selected Action: " + selectedAction.GetActionName());

        GridVisualType gridVisualType;
        switch (selectedAction)
        {
            default:
            case MoveAction moveAction:
                gridVisualType = GridVisualType.Green;
                break;
            case SpinAction spinAction:
                gridVisualType = GridVisualType.Blue;
                break;
            case ShootAction shootAction:
                gridVisualType = GridVisualType.Red;
                break;
            case GrenadeAction grenadeAction:
                gridVisualType = GridVisualType.Yellow;
                break;
            case SwordAction swordAction:
                gridVisualType = GridVisualType.Red;
                break;
            case InteractAction interactAction:
                gridVisualType = GridVisualType.Blue;
                break;
            case RockAction rockAction:
                gridVisualType = GridVisualType.Yellow;
                break;
        }

        // ShowActionRange(selectedUnit.GetGridPosition(), selectedAction.GetActionRange(), GridVisualType.White);

        ShowGridPositionList(selectedAction.GetValidActionGridPositionList(), gridVisualType);
    }

    public void HideAllGridPosition()
    {
        for (int x = 0; x < GridSystemHex.Instance.GetWidth(); x++)
        {
            for (int z = 0; z < GridSystemHex.Instance.GetHeight(); z++)
            {
                // Debug.Log("Hide grid: " + x + ":" + z);
                gridObjectArray[x, z].Hide();
            }
        }
    }

    private void ShowActionRange(GridPosition gridPosition, int range, GridVisualType gridVisualType)
    {
        List<GridPosition> gridPositionList = new List<GridPosition>();
        for (int x = -range; x <= range; x++)
        {
            for (int z = -range; z <= range; z++)
            {
                GridPosition testGridPosition = gridPosition + new GridPosition(x, z);

                if (!IsValidGridPosition(testGridPosition))
                {
                    continue;
                }

                if (GetPathLength(gridPosition, testGridPosition) > range)
                {
                    continue;
                }

                gridPositionList.Add(testGridPosition);
            }
        }

        ShowGridPositionList(gridPositionList, gridVisualType);
    }

    public void ShowGridPositionList(List<GridPosition> gridPositionList, GridVisualType gridVisualType)
    {
        foreach (GridPosition gridPosition in gridPositionList)
        {
            gridObjectArray[gridPosition.x, gridPosition.z].Show(GetGridVisualTypeMaterial(gridVisualType));
        }
    }

    private Material GetGridVisualTypeMaterial(GridVisualType gridVisualType)
    {
        foreach (GridVisualTypeMaterial gridVisualTypeMaterial in gridVisualTypeMaterialList)
        {
            if (gridVisualTypeMaterial.gridVisualType == gridVisualType)
            {
                return gridVisualTypeMaterial.material;
            }
        }

        Debug.Log("Could not find GridVisualTypeMaterial for GridVisualType: " + gridVisualType);
        return null;
    }

    private void UnitActionSystem_OnSelectedActionChanged(object sender, EventArgs e)
    {
        UpdateGridVisual();
    }

    public List<GridPosition> FindPath(GridPosition startGridPosition, GridPosition endGridPosition, out int pathLength)
    {
        List<GridObject> openList = new();
        List<GridObject> closedList = new();

        GridObject startNode = GetGridObject(startGridPosition);
        GridObject endNode = GetGridObject(endGridPosition);
        openList.Add(startNode);

        for (int x = 0; x < GetWidth(); x++)
        {
            for (int z = 0; z < GetHeight(); z++)
            {
                GridPosition gridPosition = new GridPosition(x, z);
                GridObject pathNode = GetGridObject(gridPosition);

                pathNode.SetGCost(int.MaxValue);
                pathNode.SetHCost(0);
                pathNode.CalculateFCost();
                pathNode.ResetCameFromPathNode();
            }
        }

        startNode.SetGCost(0);
        startNode.SetHCost(CalculateHeuristicDistance(startGridPosition, endGridPosition));
        startNode.CalculateFCost();

        while (openList.Count > 0)
        {
            GridObject currentNode = GetLowestFCostPathNode(openList);

            if (currentNode == endNode)
            {
                pathLength = endNode.GetFCost();
                return CalculatePath(endNode);
            }

            openList.Remove(currentNode);
            closedList.Add(currentNode);

            foreach (GridObject neightborNode in GetNeighborList(currentNode))
            {
                if (closedList.Contains(neightborNode))
                {
                    continue;
                }

                if (!neightborNode.IsWalkable())
                {
                    closedList.Add(neightborNode);
                    continue;
                }

                int tentativeGCost = currentNode.GetGCost() + MOVE_COST;

                if (tentativeGCost < neightborNode.GetGCost())
                {
                    neightborNode.SetCameFromPathNode(currentNode);
                    neightborNode.SetGCost(tentativeGCost);
                    neightborNode.SetHCost(CalculateHeuristicDistance(neightborNode.GetGridPosition(), endGridPosition));
                    neightborNode.CalculateFCost();

                    if (!openList.Contains(neightborNode))
                    {
                        openList.Add(neightborNode);
                    }
                }
            }
        }

        // No path found
        pathLength = 0;
        return null;
    }

    public int CalculateHeuristicDistance(GridPosition gridPositionA, GridPosition gridPositionB)
    {
        return Mathf.RoundToInt(MOVE_COST *
            Vector3.Distance(GetWorldPosition(gridPositionA), GetWorldPosition(gridPositionB)));
    }

    private GridObject GetLowestFCostPathNode(List<GridObject> pathNodeList)
    {
        GridObject lowestFCostPathNode = pathNodeList[0];
        for (int i = 0; i < pathNodeList.Count; i++)
        {
            if (pathNodeList[i].GetFCost() < lowestFCostPathNode.GetFCost())
            {
                lowestFCostPathNode = pathNodeList[i];
            }
        }
        return lowestFCostPathNode;
    }

    private List<GridPosition> CalculatePath(GridObject endNode)
    {
        List<GridObject> pathNodeList = new List<GridObject>();
        pathNodeList.Add(endNode);

        GridObject currentNode = endNode;

        while (currentNode.GetCameFromPathNode() != null)
        {
            pathNodeList.Add(currentNode.GetCameFromPathNode());
            currentNode = currentNode.GetCameFromPathNode();
        }

        pathNodeList.Reverse();

        List<GridPosition> gridPositionList = new List<GridPosition>();
        foreach (GridObject pathNode in pathNodeList)
        {
            gridPositionList.Add(pathNode.GetGridPosition());
        }

        return gridPositionList;
    }

    private List<GridObject> GetNeighborList(GridObject currentNode)
    {
        List<GridObject> neighborList = new List<GridObject>();

        GridPosition gridPosition = currentNode.GetGridPosition();

        if (gridPosition.x - 1 >= 0)
        {
            // Left
            neighborList.Add(gridObjectArray[gridPosition.x - 1, gridPosition.z]);
        }

        if (gridPosition.x + 1 < GetWidth())
        {
            // Right
            neighborList.Add(gridObjectArray[gridPosition.x + 1, gridPosition.z]);
        }

        if (gridPosition.z - 1 >= 0)
        {
            // Down
            neighborList.Add(gridObjectArray[gridPosition.x, gridPosition.z - 1]);
        }

        if (gridPosition.z + 1 < GetHeight())
        {
            // Up
            neighborList.Add(gridObjectArray[gridPosition.x, gridPosition.z + 1]);
        }

        bool oddRow = gridPosition.z % 2 == 1;

        if (oddRow)
        {
            if (gridPosition.x + 1 < GetWidth())
            {
                if (gridPosition.z - 1 >= 0)
                {
                    neighborList.Add(gridObjectArray[gridPosition.x + 1, gridPosition.z - 1]);
                }

                if (gridPosition.z + 1 < GetHeight())
                {
                    neighborList.Add(gridObjectArray[gridPosition.x + 1, gridPosition.z + 1]);
                }
            }
        }
        else
        {
            if (gridPosition.x - 1 >= 0)
            {
                if (gridPosition.z - 1 >= 0)
                {
                    neighborList.Add(gridObjectArray[gridPosition.x - 1, gridPosition.z - 1]);
                }

                if (gridPosition.z + 1 < GetHeight())
                {
                    neighborList.Add(gridObjectArray[gridPosition.x - 1, gridPosition.z + 1]);
                }
            }
        }

        return neighborList;
    }

    public bool IsWalkableGridPosition(GridPosition gridPosition)
    {
        return GetGridObject(gridPosition).IsWalkable();
    }

    public void SetWalkableGridPosition(GridPosition gridPosition, bool IsWalkable)
    {
        GetGridObject(gridPosition).SetIsWalkable(IsWalkable);
    }

    public bool HasPath(GridPosition startGridPosition, GridPosition endGridPosition)
    {
        return FindPath(startGridPosition, endGridPosition, out int pathLength) != null;
    }

    public int GetPathLength(GridPosition startGridPosition, GridPosition endGridPosition)
    {
        FindPath(startGridPosition, endGridPosition, out int pathLength);
        return pathLength;
    }
}
