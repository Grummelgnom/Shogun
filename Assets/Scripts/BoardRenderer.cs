using System.Collections.Generic;
using UnityEngine;  // Unity-Funktionen für GameObjects, Transform, Color, etc.

// Dieses Script ist dafür zuständig, das Board aus dem GameManager
// als sichtbare 2D-Kacheln in der Scene anzuzeigen.
// Es liegt auf dem GameObject "BoardRenderer".
public class BoardRenderer : MonoBehaviour
{
    // Referenz auf den GameManager in der Scene.
    // Im Inspector ziehst du das GameObject "GameManager" hier hinein.
    public GameManager gameManager;

    // Prefab einer einzelnen Kachel.
    // Das ist ein GameObject mit einem SpriteRenderer (z.B. ein kleines Quadrat).
    // Im Inspector ziehst du hier dein Tile-Prefab hinein.
    public GameObject tilePrefab;

    // Abstand zwischen den Kacheln in Welt-Einheiten.
    // 1f bedeutet: Jede Kachel liegt 1 Einheit auseinander.
    // Du kannst den Wert im Inspector verändern, um das Grid größer/kleiner zu machen.
    public float tileSpacing = 1f;

    // Dictionary um Tiles zu speichern und später zu highlighten
    private Dictionary<string, GameObject> tiles = new Dictionary<string, GameObject>();

    // Speichern aktuell gehighlighteter Ziele
    private List<GameObject> highlightedTiles = new List<GameObject>();


    // Wird beim Start der Scene aufgerufen.
    void Start()
    {
        // Prüfen, ob ein GameManager zugewiesen wurde.
        if (gameManager == null)
        {
            Debug.LogError("BoardRenderer: Kein GameManager zugewiesen! Bitte im Inspector setzen.");
            return;  // Abbrechen, damit wir keinen Fehler produzieren.
        }

        // Prüfen, ob ein Tile-Prefab zugewiesen wurde.
        if (tilePrefab == null)
        {
            Debug.LogError("BoardRenderer: Kein tilePrefab zugewiesen! Bitte im Inspector setzen.");
            return;
        }

        // Wenn alles da ist, bauen wir das visuelle Board.
        CreateBoardVisuals();
    }

    // Erstellt für jedes Feld im Board eine sichtbare Kachel.
    void CreateBoardVisuals()
    {
        // Board-Größe aus dem GameManager holen.
        int rows = gameManager.rows;
        int columns = gameManager.columns;

        // Das Board ungefähr um die Weltmitte (0,0) zentrieren.
        // Wir berechnen einen Offset, damit das Grid nicht oben-links anfängt,
        // sondern in der Nähe des Ursprungs.
        float xOffset = -(columns - 1) * tileSpacing / 2f;
        float yOffset = -(rows - 1) * tileSpacing / 2f;

        // Schleife über alle Reihen (von oben nach unten).
        for (int r = 0; r < rows; r++)
        {
            // Schleife über alle Spalten (von links nach rechts).
            for (int c = 0; c < columns; c++)
            {
                // Position dieser Kachel im Raum berechnen.
                // Wichtig: Wir drehen hier die Reihenachse um (rows - 1 - r),
                // damit Reihe 0 oben angezeigt wird, so wie im Konsolenbild.
                float x = c * tileSpacing + xOffset;
                float y = (rows - 1 - r) * tileSpacing + yOffset;

                Vector3 tilePosition = new Vector3(x, y, 0f);

                // Eine neue Kachel aus dem Prefab erzeugen.
                GameObject tile = Instantiate(tilePrefab, tilePosition, Quaternion.identity);

                // Kachel als Kind dieses BoardRenderers setzen,
                // damit im Hierarchy-Fenster alles schön gruppiert ist.
                tile.transform.parent = this.transform;
                tiles[$"{r},{c}"] = tile;
                // Das Tile-Script auf der Kachel konfigurieren.
                Tile tileScript = tile.GetComponent<Tile>();
                if (tileScript != null)
                {
                    tileScript.row = r;
                    tileScript.column = c;
                    tileScript.gameManager = gameManager;
                }

                // Wert direkt aus dem Board an dieser Position [r, c] holen.
                int value = gameManager.board[r, c];

                //// SpriteRenderer suchen, um optional Farbe zu setzen.
                //SpriteRenderer sr = tile.GetComponent<SpriteRenderer>();
                //if (sr != null)
                //{
                //    // Einfache Visualisierung:
                //    // Wert 4 → rot (starke Magneten), sonst grau (schwache/keine Magneten).
                //    if (value == 4)
                //        sr.color = Color.red;
                //    else
                //        sr.color = Color.gray;
                //    sr.tag = value == 4 ? "red" : "gray";
                //}
                //else
                //{
                //    Debug.LogWarning("BoardRenderer: Tile-Prefab hat keinen SpriteRenderer!");
                //}
            }
        }
    }

    // Hilfsfunktion:
    // Rechnet eine Board-Position (row, column) in eine Weltposition um.
    // Diese Funktion wird vom GameManager benutzt, um Figuren an die richtige Stelle zu setzen.
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

    // Highlightet die gültigen Zugziele
    public void HighlightValidMoves(List<ValidMove> validMoves)
    {
        // Alte Highlights löschen
        ClearHighlights();

        // Neue Highlights setzen
        foreach (ValidMove move in validMoves)
        {
            string key = $"{move.to.row},{move.to.column}";

            if (tiles.ContainsKey(key))
            {
                GameObject targetTile = tiles[key];
                SpriteRenderer sr = targetTile.GetComponent<SpriteRenderer>();

                if (sr != null)
                {
                    // Grün = gültiges Zielfeld
                    sr.color = new Color(0, 1, 0, 0.7f);  // Grün mit etwas Transparenz
                    highlightedTiles.Add(targetTile);
                }
            }
        }
    }

    // Löscht alle Highlights
    public void ClearHighlights()
    {
        foreach (GameObject tile in highlightedTiles)
        {
            SpriteRenderer sr = tile.GetComponent<SpriteRenderer>();
            if (sr != null)
            {
                // Original-Farbe zurücksetzen basierend auf Tag
                if (sr.tag == "red")
                    sr.color = Color.red;
                else
                    sr.color = Color.white;
            }
        }
        highlightedTiles.Clear();
    }

}
