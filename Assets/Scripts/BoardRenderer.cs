using System.Collections.Generic;
using UnityEngine;

public class BoardRenderer : MonoBehaviour
{
    public GameManager gameManager;
    public GameObject tilePrefab;
    public float tileSpacing = 1f;

    // Dictionary um Tiles zu speichern und später zu highlighten
    private Dictionary<string, GameObject> tiles = new Dictionary<string, GameObject>();

    // Speichern aktuell gehighlighteter Ziele
    private List<GameObject> highlightedTiles = new List<GameObject>();

    // NEU: Speichern der Original-Farben pro Tile
    private Dictionary<string, Color> originalTileColors = new Dictionary<string, Color>();

    void Start()
    {
        if (gameManager == null)
        {
            Debug.LogError("BoardRenderer: Kein GameManager zugewiesen! Bitte im Inspector setzen.");
            return;
        }

        if (tilePrefab == null)
        {
            Debug.LogError("BoardRenderer: Kein tilePrefab zugewiesen! Bitte im Inspector setzen.");
            return;
        }

        CreateBoardVisuals();
    }

    void CreateBoardVisuals()
    {
        int rows = gameManager.rows;
        int columns = gameManager.columns;

        float xOffset = -(columns - 1) * tileSpacing / 2f;
        float yOffset = -(rows - 1) * tileSpacing / 2f;

        for (int r = 0; r < rows; r++)
        {
            for (int c = 0; c < columns; c++)
            {
                float x = c * tileSpacing + xOffset;
                float y = (rows - 1 - r) * tileSpacing + yOffset;

                Vector3 tilePosition = new Vector3(x, y, 0f);

                GameObject tile = Instantiate(tilePrefab, tilePosition, Quaternion.identity);
                tile.transform.parent = this.transform;

                string key = $"{r},{c}";
                tiles[key] = tile;

                // Tile-Script konfigurieren
                Tile tileScript = tile.GetComponent<Tile>();
                if (tileScript != null)
                {
                    tileScript.row = r;
                    tileScript.column = c;
                    tileScript.gameManager = gameManager;
                }

                // Wert aus dem Board holen
                int value = gameManager.board[r, c];

                // HYBRID: SpriteRenderer (2D) ODER MeshRenderer (3D)
                SpriteRenderer sr = tile.GetComponent<SpriteRenderer>();
                MeshRenderer mr = tile.GetComponent<MeshRenderer>();

                // Color tileColor = value == 4 ? Color.red : Color.gray;
                // Color tileColor = new Color(1, 1, 1, 0);  // Transparent
                Color tileColor = Color.white;  // Alle Tiles weiß/neutral

                
                if (sr != null)
                {
                    // 2D-Version
                    sr.color = tileColor;
                    sr.tag = value == 4 ? "red" : "gray";
                    originalTileColors[key] = tileColor;
                }
                else if (mr != null)
                {
                    // 3D-Version
                    mr.material.color = tileColor;
                    mr.tag = value == 4 ? "red" : "gray";
                    originalTileColors[key] = tileColor;
                }
            }
        }
    }

    public Vector3 GetWorldPosition(int row, int column)
    {
        int rows = gameManager.rows;
        int columns = gameManager.columns;

        float xOffset = -(columns - 1) * tileSpacing / 2f;
        float yOffset = -(rows - 1) * tileSpacing / 2f;

        float x = column * tileSpacing + xOffset;
        float y = (rows - 1 - row) * tileSpacing + yOffset;

        return new Vector3(x, y, 0f);
    }

    // HYBRID: Highlighting für 2D + 3D
    public void HighlightValidMoves(List<ValidMove> validMoves)
    {
        ClearHighlights();

        foreach (ValidMove move in validMoves)
        {
            string key = $"{move.to.row},{move.to.column}";

            if (tiles.ContainsKey(key))
            {
                GameObject targetTile = tiles[key];
                Color highlightColor = new Color(0, 1, 0, 0.7f); // Grün

                SpriteRenderer sr = targetTile.GetComponent<SpriteRenderer>();
                MeshRenderer mr = targetTile.GetComponent<MeshRenderer>();

                if (sr != null)
                {
                    // 2D-Version
                    sr.color = highlightColor;
                    highlightedTiles.Add(targetTile);
                }
                else if (mr != null)
                {
                    // 3D-Version
                    mr.material.color = highlightColor;
                    highlightedTiles.Add(targetTile);
                }
            }
        }
    }

    // HYBRID: Clear Highlights für 2D + 3D
    public void ClearHighlights()
    {
        foreach (GameObject tile in highlightedTiles)
        {
            // Original-Farbe aus Dictionary holen
            string key = null;
            foreach (var kvp in tiles)
            {
                if (kvp.Value == tile)
                {
                    key = kvp.Key;
                    break;
                }
            }

            if (key != null && originalTileColors.ContainsKey(key))
            {
                Color originalColor = originalTileColors[key];

                SpriteRenderer sr = tile.GetComponent<SpriteRenderer>();
                MeshRenderer mr = tile.GetComponent<MeshRenderer>();

                if (sr != null)
                {
                    // 2D-Version
                    sr.color = originalColor;
                }
                else if (mr != null)
                {
                    // 3D-Version
                    mr.material.color = originalColor;
                }
            }
        }
        highlightedTiles.Clear();
    }
}
