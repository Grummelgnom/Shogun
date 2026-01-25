using UnityEngine;

public class Tile : MonoBehaviour
{
    public int row;
    public int column;
    public GameManager gameManager;

    void OnMouseDown()
    {
        if (gameManager != null)
        {
            gameManager.OnTileClicked(row, column);
        }
    }
}
