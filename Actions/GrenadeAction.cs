using System;
using System.Collections.Generic;
using UnityEngine;

public class GrenadeAction : BaseAction
{

    [SerializeField] private Transform grenadeProjectilePrefab;
    private int maxThrowDistance = 7;

    private void Update()
    {
        if (!isActive)
        {
            return;
        }
    }
    public override string GetActionName()
    {
        return "Grenade";
    }

    public override EnemyAIAction GetEnemyAIAction(GridPosition gridPosition)
    {
        return new EnemyAIAction
        {
            gridPosition = gridPosition,
            actionValue = 0,
        };
    }

    public override List<GridPosition> GetValidActionGridPositionList()
    {
        List<GridPosition> validGridPositionList = new List<GridPosition>();

        GridPosition unitGridPosition = unit.GetGridPosition();

        for (int x = -maxThrowDistance; x <= maxThrowDistance; x++)
        {
            for (int z = -maxThrowDistance; z <= maxThrowDistance; z++)
            {
                GridPosition offsetGridPosition = new GridPosition(x, z);
                GridPosition testGridPosition = unitGridPosition + offsetGridPosition;

                if (!GridSystemHex.Instance.IsValidGridPosition(testGridPosition))
                {
                    continue;
                }

                if (GridSystemHex.Instance.GetPathLength(unitGridPosition, testGridPosition) > maxThrowDistance)
                {
                    continue;
                }

                // Debug.Log(testGridPosition);
                validGridPositionList.Add(testGridPosition);
            }
        }

        return validGridPositionList;
    }

    public override void TakeAction(GridPosition gridPosition, Action onActionComplete)
    {
        Transform grenadeProjectileTransform = Instantiate(grenadeProjectilePrefab, unit.GetWorldPosition(), Quaternion.identity);
        GrenadeProjectile grenadeProjectile = grenadeProjectileTransform.GetComponent<GrenadeProjectile>();
        grenadeProjectile.Setup(gridPosition, OnGrenadeBehaviorComplete);

        Debug.Log("GrenadeAction");
        ActionStart(onActionComplete);
    }

    private void OnGrenadeBehaviorComplete()
    {
        ActionComplete();
    }

    public override int GetActionRange()
    {
        return maxThrowDistance;
    }
}
