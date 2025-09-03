using UnityEngine;

public class GridSystemVisualSingle : MonoBehaviour
{
    [SerializeField] private MeshRenderer meshRenderer;
    [SerializeField] private GameObject selectedGameObject;

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
}
