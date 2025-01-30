using UnityEngine;

public class GridPosition : MonoBehaviour
{
    [SerializeField] int x;
    [SerializeField] int y;

    private void OnMouseUpAsButton()
    {
        GameManager.Instance.ClickGrid(x, y);
    }
}
