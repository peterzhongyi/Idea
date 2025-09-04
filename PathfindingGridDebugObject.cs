using TMPro;
using UnityEngine;

public class PathfindingGridDebugObject : GridDebugObject
{
    [SerializeField] private TextMeshPro gCostText;
    [SerializeField] private TextMeshPro hCostText;
    [SerializeField] private TextMeshPro fCostText;
    [SerializeField] private SpriteRenderer isWalkableSpriteRenderer;


    public override void SetGridObject(object gridObject)
    {
        base.SetGridObject(gridObject);
    }

    protected override void Update()
    {
        base.Update();
        gCostText.text = ((GridObject)gridObject).GetGCost().ToString();
        hCostText.text = ((GridObject)gridObject).GetHCost().ToString();
        fCostText.text = ((GridObject)gridObject).GetFCost().ToString();
        isWalkableSpriteRenderer.color = ((GridObject)gridObject).IsWalkable() ? Color.green : Color.red;
    }
}
