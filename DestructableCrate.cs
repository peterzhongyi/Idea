using System;
using Unity.Mathematics;
using UnityEngine;

public class DestructableCrate : MonoBehaviour
{
    public static event EventHandler OnAnyDestroyed;

    [SerializeField] private Transform crateDestroedPrefab;

    private GridPosition gridPosition;

    private void Start()
    {
        gridPosition = LevelGrid.Instance.GetGridPosition(transform.position);
    }

    public GridPosition GetGridPosition()
    {
        return gridPosition;
    }

    public void Damage()
    {
        Transform crateDestroyedTransform = Instantiate(crateDestroedPrefab, transform.position, transform.rotation);
        ApplyExplosionToChildren(crateDestroyedTransform, 150f, transform.position, 10f);
        Destroy(gameObject);

        OnAnyDestroyed?.Invoke(this, EventArgs.Empty);
    }

    private void ApplyExplosionToChildren(Transform root, float explosionForce,
                                        Vector3 explosionPosition, float explosionRange)
    {
        foreach (Transform child in root)
        {
            if (child.TryGetComponent<Rigidbody>(out Rigidbody childRigidbody))
            {
                childRigidbody.AddExplosionForce(explosionForce, explosionPosition, explosionRange);
            }

            ApplyExplosionToChildren(child, explosionForce, explosionPosition, explosionRange);
        }
    }
}
