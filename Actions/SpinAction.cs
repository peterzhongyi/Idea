using System;
using System.Collections.Generic;
using UnityEngine;

public class SpinAction : BaseAction
{
    // public delegate void SpinCompleteDelegate();
    // private SpinCompleteDelegate onSpinComplele;
    private float totalSpinAmount;

    // Update is called once per frame 
    private void Update()
    {
        if (!isActive)
        {
            return;
        }

        float spinAddAmount = 360f * Time.deltaTime;
        transform.eulerAngles += new Vector3(0, spinAddAmount, 0);
        totalSpinAmount += spinAddAmount;
        if (totalSpinAmount > 360f)
        {
            ActionComplete();
        }
    }

    public override void TakeAction(GridPosition gridPosition, Action onActionComplele)
    {
        totalSpinAmount = 0f;

        ActionStart(onActionComplele);
    }

    public override string GetActionName()
    {
        return "Spin";
    }

    public override List<GridPosition> GetValidActionGridPositionList()
    {
        GridPosition unitGridPosition = unit.GetGridPosition();
        return new List<GridPosition>
        {
            unitGridPosition
        };
    }

    public override int GetActionPointsCost()
    {
        return 1;
    }

    public override EnemyAIAction GetEnemyAIAction(GridPosition gridPosition)
    {
        return new EnemyAIAction
        {
            gridPosition = gridPosition,
            actionValue = 0,
        };
    }

    public override int GetActionRange()
    {
        return 0;
    }
}
