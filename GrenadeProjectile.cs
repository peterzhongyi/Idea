using System;
using System.Collections.Generic;
using UnityEngine;

public class GrenadeProjectile : MonoBehaviour
{
    public static event EventHandler OnAnyGrenadeExploded;

    [SerializeField] private Transform grenadeExplodeVfxPrefab;
    [SerializeField] private TrailRenderer trailRenderer;
    [SerializeField] private AnimationCurve arcYAnimationCurve;

    private Vector3 targetPosition;
    private Action onGrenadeBehaviorComplete;
    private float totalDistance;
    private Vector3 positionXZ;

    private void Update()
    {
        Vector3 moveDir = (targetPosition - positionXZ).normalized;

        float moveSpeed = 15f;
        positionXZ += moveDir * moveSpeed * Time.deltaTime;

        float distance = Vector3.Distance(positionXZ, targetPosition);
        float distanceNormalized = 1- distance / totalDistance;

        // Max height dependes on how far the grenade needs to fly.
        float maxHeight = totalDistance / 4f; 
        float positionY = arcYAnimationCurve.Evaluate(distanceNormalized) * maxHeight;
        transform.position = new Vector3(positionXZ.x, positionY, positionXZ.z);

        float reachedTargetDistance = 0.2f;
        if (Vector3.Distance(positionXZ, targetPosition) < reachedTargetDistance)
        {
            float damageRadius = 2f;
            Collider[] colliderArray = Physics.OverlapSphere(targetPosition, damageRadius);
            List<GridObject> affectedGridObjects = new();

            foreach (Collider collider in colliderArray)
            {
                if (collider.TryGetComponent<Unit>(out Unit targetUnit))
                {
                    targetUnit.Damage(30);
                }
                if (collider.TryGetComponent<DestructableCrate>(out DestructableCrate destructableCrate))
                {
                    destructableCrate.Damage();
                }
                if (collider.TryGetComponent<GridObject>(out GridObject gridObject))
                {
                    affectedGridObjects.Add(gridObject);
                }
            }

            OnAnyGrenadeExploded?.Invoke(this, EventArgs.Empty);

            // Clean up trail after hit.
            trailRenderer.transform.parent = null;
            Instantiate(grenadeExplodeVfxPrefab, targetPosition + Vector3.up * 1f, Quaternion.identity);

            foreach (GridObject gridObject in affectedGridObjects)
            {
                gridObject.SetOnFire();
            }

            Destroy(gameObject);

            onGrenadeBehaviorComplete();
        }
    }
    public void Setup(GridPosition targetGridPosition, Action onGrenadeBehaviorComplete)
    {
        this.onGrenadeBehaviorComplete = onGrenadeBehaviorComplete;
        targetPosition = GridSystemHex.Instance.GetWorldPosition(targetGridPosition);

        positionXZ = transform.position;
        positionXZ.y = 0;
        totalDistance = Vector3.Distance(positionXZ, targetPosition);
    }
}
