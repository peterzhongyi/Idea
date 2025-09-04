using System.Collections.Generic;
using UnityEngine;

public class GridObject : MonoBehaviour
{
    // Visual
    [SerializeField] private MeshRenderer meshRenderer;
    [SerializeField] private GameObject selectedGameObject;

    // General
    private GridPosition gridPosition;
    private List<Unit> unitList;
    private IInteractable interactable;

    // Pathfinding
    private int gCost; // Waling Cost from Start Node
    private int hCost; // Heuristic Cost to reach End Node
    private int fCost; // gCost + hCost
    private GridObject cameFromPathNode;
    private bool isWalkable = true;

    private void Awake()
    {
        gridPosition = GridSystemHex.Instance.GetGridPosition(transform.position);
        unitList = new List<Unit>();
    }

    public override string ToString()
    {
        string unitString = "";
        foreach (Unit unit in unitList)
        {
            unitString += unit + "\n";
        }
        return gridPosition.ToString() + "\n" + unitString;
    }

    public void AddUnit(Unit unit)
    {
        unitList.Add(unit);
    }

    public void RemoveUnit(Unit unit)
    {
        unitList.Remove(unit);
    }

    public List<Unit> GetUnitList()
    {
        return unitList;
    }

    public bool HasAnyUnit()
    {
        return unitList.Count > 0;
    }

    public Unit GetUnit()
    {
        if (HasAnyUnit())
        {
            return unitList[0];
        }
        else
        {
            return null;
        }
    }

    public IInteractable GetInteractable()
    {
        return interactable;
    }

    public void SetInteractable(IInteractable interactable)
    {
        this.interactable = interactable;
    }

    public void Show(Material material)
    {
        meshRenderer.enabled = true;
        meshRenderer.material = material;
    }

    public void Hide()
    {
        meshRenderer.enabled = false;
    }

    public void ShowSelected()
    {
        selectedGameObject.SetActive(true);
    }

    public void HideSelected()
    {
        selectedGameObject.SetActive(false);
    }

    public int GetGCost()
    {
        return gCost;
    }

    public int GetHCost()
    {
        return hCost;
    }

    public int GetFCost()
    {
        return fCost;
    }

    public void SetGCost(int gCost)
    {
        this.gCost = gCost;
    }

    public void SetHCost(int hCost)
    {
        this.hCost = hCost;
    }

    public void CalculateFCost()
    {
        fCost = gCost + hCost;
    }

    public void ResetCameFromPathNode()
    {
        cameFromPathNode = null;
    }

    public void SetCameFromPathNode(GridObject gridObject)
    {
        cameFromPathNode = gridObject;
    }

    public GridObject GetCameFromPathNode()
    {
        return cameFromPathNode;
    }
    
    public GridPosition GetGridPosition()
    {
        return gridPosition;
    }

    public bool IsWalkable()
    {
        return isWalkable;
    }

    public void SetIsWalkable(bool isWalkable)
    {
        this.isWalkable = isWalkable;
    }

}
