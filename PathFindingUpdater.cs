using System;
using UnityEngine;

public class PathFindingUpdater : MonoBehaviour
{
    private void Start()
    {
        DestructableCrate.OnAnyDestroyed += DestructableCrate_OnAnyDestroyed;
    }

    private void DestructableCrate_OnAnyDestroyed(object sender, EventArgs e)
    {
        DestructableCrate destructableCrate = sender as DestructableCrate;
        GridSystemHex.Instance.SetWalkableGridPosition(destructableCrate.GetGridPosition(), true);
    }
}
