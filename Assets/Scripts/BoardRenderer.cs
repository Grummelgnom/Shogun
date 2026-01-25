using UnityEngine;  // Unity-Funktionen (GameObject, Transform, Color, etc.)

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

        // Wir berechnen einen Offset, damit das Board ungefähr um die Weltmitte (0,0) herum zentriert ist.
        // Ohne das würde das Grid oben rechts wegdriften.
        float xOffset = -(columns - 1) * tileSpacing / 2f;
        float yOffset = -(rows - 1) * tileSpacing / 2f;

        // Schleife über alle Reihen.
        for (int r = 0; r < rows; r++)
        {
            // Schleife über alle Spalten.
            for (int c = 0; c < columns; c++)
            {
                // Position der Kachel berechnen.
                // Wir drehen hier die Reihenachse um (rows - 1 - r),
                // damit die Darstellung mit dem Konsolen-Output besser zusammenpasst
                // (Reihe 0 oben, Reihe rows-1 unten).
                float x = c * tileSpacing + xOffset;
                float y = (rows - 1 - r) * tileSpacing + yOffset;
                Vector3 tilePosition = new Vector3(x, y, 0f);

                // Eine neue Kachel aus dem Prefab erzeugen.
                GameObject tile = Instantiate(tilePrefab, tilePosition, Quaternion.identity);
                tile.transform.parent = this.transform;

                // Tile-Script auf der Kachel holen (muss am Prefab hängen)
                Tile tileScript = tile.GetComponent<Tile>();
                if (tileScript != null)
                {
                    tileScript.row = r;
                    tileScript.column = c;
                    tileScript.gameManager = gameManager;
                }


                // Die Kachel als Kind dieses BoardRenderers setzen,
                // damit im Hierarchy-Fenster alles schön aufgeräumt ist.
                tile.transform.parent = this.transform;

                // Wert direkt aus dem Board holen
                int value = gameManager.board[r, c];

                // SpriteRenderer der Kachel holen
                SpriteRenderer sr = tile.GetComponent<SpriteRenderer>();

                if (sr != null)
                {
                    // Test: Logge Position und Wert in die Konsole
                    Debug.Log($"Tile ({r},{c}) hat Wert {value}");

                    // Wenn Wert 4 → rot, sonst grau
                    if (value == 4)
                    {
                        sr.color = Color.red;
                    }
                    else
                    {
                        sr.color = Color.gray;
                    }
                }

            }
        }
    }
}
