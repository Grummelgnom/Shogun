using UnityEngine;



public class Piece : MonoBehaviour
{
    public int owner;                    // 1 oder 2
    public PieceType pieceType;          // Soldier oder Shogun
    public int row;
    public int column;
    public int range;                    // Aktuelle Bewegungsreichweite (1-4)
    public GameManager gameManager;
    public string id;

    private bool isSelected = false;
    private Color originalColor;
    private SpriteRenderer spriteRenderer;

    void Start()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        if (spriteRenderer != null)
        {
            originalColor = spriteRenderer.color;
        }
    }

    void OnMouseDown()
    {
        if (gameManager != null)
        {
            gameManager.OnTileClicked(row, column);
        }
    }

    public void SetSelected(bool selected)
    {
        isSelected = selected;

        if (spriteRenderer != null)
        {
            if (isSelected)
            {
                spriteRenderer.color = originalColor * 1.5f;
            }
            else
            {
                spriteRenderer.color = originalColor;
            }
        }
    }

    public void UpdateRange(int interpretedGridValue, PieceType type)
    {
        if (type == PieceType.Shogun)
        {
            range = interpretedGridValue >= 3 ? 2 : 1;
        }
        else
        {
            range = Mathf.Clamp(interpretedGridValue, 1, 4);
        }
        Debug.Log($"Piece {id} - Range aktualisiert: {range} (basierend auf GridValue {interpretedGridValue})");
    }
}
