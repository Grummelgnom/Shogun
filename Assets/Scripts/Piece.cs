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

    // Neu: Text für Range-Anzeige
    private TextMesh rangeText;

    void Start()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        if (spriteRenderer != null)
        {
            originalColor = spriteRenderer.color;
        }

        // Range-Text erstellen
        CreateRangeDisplay();
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

        // Text aktualisieren
        UpdateRangeDisplay();
    }

    // Erstellt die Range-Anzeige als TextMesh
    void CreateRangeDisplay()
    {
        // Neues GameObject als Kind erstellen
        GameObject textObj = new GameObject("RangeText");
        textObj.transform.parent = this.transform;
        textObj.transform.localPosition = new Vector3(0, 0, -0.1f);  // Leicht vor dem Piece

        // TextMesh Component hinzufügen
        rangeText = textObj.AddComponent<TextMesh>();
        rangeText.text = range.ToString();
        rangeText.fontSize = 50;
        rangeText.color = Color.white;
        rangeText.anchor = TextAnchor.MiddleCenter;
        rangeText.alignment = TextAlignment.Center;
        rangeText.characterSize = 0.1f;

        // MeshRenderer für Sichtbarkeit
        MeshRenderer mr = textObj.GetComponent<MeshRenderer>();
        if (mr != null)
        {
            mr.sortingOrder = 10;  // Über allem anderen
        }
    }

    // Aktualisiert die Range-Anzeige
    void UpdateRangeDisplay()
    {
        if (rangeText != null)
        {
            rangeText.text = range.ToString();
        }
    }
}
