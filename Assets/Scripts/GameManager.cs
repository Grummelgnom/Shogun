using UnityEngine;  // Basis-Namespace für Unity-Funktionen (GameObject, MonoBehaviour, Debug, etc.)

// Dieses Script verwaltet die Spiellogik im Hintergrund.
// Es liegt auf dem GameObject "GameManager" in deiner Scene.
public class GameManager : MonoBehaviour
{
    // Anzahl der Reihen im Spielfeld (vertikal).
    public int rows = 8;

    // Anzahl der Spalten im Spielfeld (horizontal).
    public int columns = 8;

    // Vier getrennte Prefabs für Spieler 1/2 und Shogun/Normal.
    public GameObject p1NormalPrefab;
    public GameObject p1ShogunPrefab;
    public GameObject p2NormalPrefab;
    public GameObject p2ShogunPrefab;

    // Referenz auf den BoardRenderer (wird im Inspector gesetzt).
    public BoardRenderer boardRenderer;

    // 2D-Array für unser Spielfeld (Magnet-Layout, 0–4).
    // board[reihe, spalte]
    public int[,] board = new int[8, 8];

    // Diese Funktion wird von Unity automatisch einmal aufgerufen,
    // wenn die Scene gestartet wird und dieses GameObject aktiv ist.
    void Start()
    {
        // 1. Spielfeld vorbereiten (0 + 16 Vierer).
        InitBoard();

        // 2. Zu Kontrolle das Board in der Konsole ausgeben.
        PrintBoardToConsole();

        // 3. Figuren in der Startaufstellung spawnen.
        SpawnStartPieces();
    }

    // Diese Funktion richtet das Spielfeld ein.
    // - Alle Felder bekommen zuerst den Wert 0.
    // - Danach werden genau 16 verschiedene Felder mit dem Wert 4 gefüllt.
    void InitBoard()
    {
        // Schritt 1: Alles auf 0 setzen.
        for (int r = 0; r < rows; r++)
        {
            for (int c = 0; c < columns; c++)
            {
                board[r, c] = 0;
            }
        }

        // Schritt 2: 16 Felder mit 4 belegen.
        int foursPlaced = 0;  // Zählt, wie viele 4er wir bereits gesetzt haben.

        while (foursPlaced < 16)
        {
            int randomRow = Random.Range(0, rows);
            int randomColumn = Random.Range(0, columns);

            if (board[randomRow, randomColumn] == 0)
            {
                board[randomRow, randomColumn] = 4;
                foursPlaced++;
            }
        }
    }

    // Diese Funktion gibt das komplette Board in der Unity-Konsole aus.
    // Das ist sehr hilfreich zum Debuggen und Verstehen.
    void PrintBoardToConsole()
    {
        string output = "Current Board:\n";

        for (int r = 0; r < rows; r++)
        {
            string rowText = "";

            for (int c = 0; c < columns; c++)
            {
                int value = board[r, c];
                rowText += value + " ";
            }

            output += rowText + "\n";
        }

        Debug.Log(output);
    }

    // Richtet die Startaufstellung für beide Spieler ein.
    // Nur erste (oben) und letzte Zeile (unten) werden belegt.
    // Pro Zeile: 7 normale Figuren + 1 Shogun.
    void SpawnStartPieces()
    {
        if (boardRenderer == null)
        {
            Debug.LogError("GameManager: boardRenderer nicht gesetzt!");
            return;
        }

        if (p1NormalPrefab == null || p1ShogunPrefab == null ||
            p2NormalPrefab == null || p2ShogunPrefab == null)
        {
            Debug.LogError("GameManager: Mindestens eines der Piece-Prefabs ist nicht gesetzt!");
            return;
        }

        // Wir gehen von 0-basierten Spalten aus: 0 = ganz links, 7 = ganz rechts.
        // "4. Feld von rechts" → Index 3 (0,1,2,3,4,5,6,7 → 4. von rechts = 3).
        int shogunColumn = 4;

        // Spieler 1 (oben, Reihe 0)
        for (int c = 0; c < columns; c++)
        {
            if (c == shogunColumn)
                SpawnP1Shogun(0, c);    // Shogun oben
            else
                SpawnP1Normal(0, c);    // normale Figuren auf der restlichen ersten Zeile
        }

        // Spieler 2 (unten, Reihe 7)
        for (int c = 0; c < columns; c++)
        {
            if (c == shogunColumn)
                SpawnP2Shogun(7, c);    // Shogun unten, 4. Feld von rechts
            else
                SpawnP2Normal(7, c);    // normale Figuren auf der restlichen letzten Zeile
        }
    }

    // Hilfsfunktionen: instanziieren jeweils den passenden Prefab-Typ.
    // Wir setzen einen kleinen Z-Offset, damit die Figuren sicher vor dem Brett liegen.

    void SpawnP1Normal(int row, int column)
    {
        Vector3 worldPos = boardRenderer.GetWorldPosition(row, column);
        worldPos.z = -0.1f;  // Stück leicht vor das Brett ziehen (abhängig von deiner Kamera)

        GameObject pieceObj = Instantiate(p1NormalPrefab, worldPos, Quaternion.identity);

        Piece piece = pieceObj.GetComponent<Piece>();
        if (piece != null)
        {
            piece.owner = 1;
            piece.isShogun = false;
            piece.row = row;
            piece.column = column;
            piece.gameManager = this;
        }
    }

    void SpawnP1Shogun(int row, int column)
    {
        Vector3 worldPos = boardRenderer.GetWorldPosition(row, column);
        worldPos.z = -0.1f;

        GameObject pieceObj = Instantiate(p1ShogunPrefab, worldPos, Quaternion.identity);

        Piece piece = pieceObj.GetComponent<Piece>();
        if (piece != null)
        {
            piece.owner = 1;
            piece.isShogun = true;
            piece.row = row;
            piece.column = column;
            piece.gameManager = this;
        }
    }

    void SpawnP2Normal(int row, int column)
    {
        Vector3 worldPos = boardRenderer.GetWorldPosition(row, column);
        worldPos.z = -0.1f;

        GameObject pieceObj = Instantiate(p2NormalPrefab, worldPos, Quaternion.identity);

        Piece piece = pieceObj.GetComponent<Piece>();
        if (piece != null)
        {
            piece.owner = 2;
            piece.isShogun = false;
            piece.row = row;
            piece.column = column;
            piece.gameManager = this;
        }
    }

    void SpawnP2Shogun(int row, int column)
    {
        Vector3 worldPos = boardRenderer.GetWorldPosition(row, column);
        worldPos.z = -0.1f;

        GameObject pieceObj = Instantiate(p2ShogunPrefab, worldPos, Quaternion.identity);

        Piece piece = pieceObj.GetComponent<Piece>();
        if (piece != null)
        {
            piece.owner = 2;
            piece.isShogun = true;
            piece.row = row;
            piece.column = column;
            piece.gameManager = this;
        }
    }
}
