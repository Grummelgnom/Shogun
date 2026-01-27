using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

// Enum für die Piece-Typen (Soldat oder Shogun)
public enum PieceType
{
    Soldier,    // Normaler Soldat/Bauer
    Shogun      // Der Anführer
}

public class GameManager : MonoBehaviour
{
    public int rows = 8;
    public int columns = 8;

    public GameObject p1NormalPrefab;
    public GameObject p1ShogunPrefab;
    public GameObject p2NormalPrefab;
    public GameObject p2ShogunPrefab;
    public enum GameMode { TwoPlayer, VsAI }
    public GameMode mode = GameMode.TwoPlayer;  // Inspector-Default

    public BoardRenderer boardRenderer;

    // board-Array korrekt deklarieren
    public int[,] board = new int[8, 8];

    // Magnetfeld-Werte (feste 8x8 Grid)
    public int[,] gridValues = new int[8, 8];

    // ... rest des Codes


    // Alle Pieces in der Szene
    private Dictionary<string, Piece> pieces = new Dictionary<string, Piece>();

    // ... rest des Codes


    // Spielzustand
    public int currentPlayer = 1;  // 1 = P1 (oben), 2 = P2 (unten)
    private Piece selectedPiece = null;
    private List<ValidMove> validMoves = new List<ValidMove>();
    private int winner = 0;  // 0 = keiner, 1 = P1, 2 = P2
    private int turnCount = 0;

    void Start()
    {
        InitBoard();
        PrintBoardToConsole();
        SpawnStartPieces();
    }

    void InitBoard()
    {
        // Versuche mehrere Random-Grids und nimm das beste (wenigste Pärchen)
        int bestScore = int.MaxValue;
        int[,] bestGrid = null;

        int attempts = 500;  // Anzahl Versuche (höher = besseres Ergebnis, aber langsamer)

        for (int attempt = 0; attempt < attempts; attempt++)
        {
            int[,] candidateGrid = GenerateRandomGrid();
            int score = CountOrthogonalPairs(candidateGrid);

            if (score < bestScore)
            {
                bestScore = score;
                bestGrid = candidateGrid;
            }
        }

        // Bestes Grid übernehmen
        gridValues = bestGrid;
        board = bestGrid;

        Debug.Log($"Bestes Grid gefunden mit {bestScore} orthogonalen Pärchen (aus {attempts} Versuchen)");
    }

    // Generiert ein zufälliges 8x8 Grid mit 16×1, 16×2, 16×3, 16×4
    int[,] GenerateRandomGrid()
    {
        int[,] grid = new int[8, 8];

        // Liste mit allen Werten: 16x1, 16x2, 16x3, 16x4
        List<int> values = new List<int>();
        for (int i = 1; i <= 4; i++)
        {
            for (int j = 0; j < 16; j++)
            {
                values.Add(i);
            }
        }

        // Mischen (Fisher-Yates Shuffle)
        for (int i = values.Count - 1; i > 0; i--)
        {
            int j = Random.Range(0, i + 1);
            int temp = values[i];
            values[i] = values[j];
            values[j] = temp;
        }

        // Auf Grid verteilen
        int index = 0;
        for (int r = 0; r < 8; r++)
        {
            for (int c = 0; c < 8; c++)
            {
                grid[r, c] = values[index];
                index++;
            }
        }

        return grid;
    }

    // Zählt, wie viele orthogonale Nachbarn denselben Wert haben
    int CountOrthogonalPairs(int[,] grid)
    {
        int pairCount = 0;

        for (int r = 0; r < 8; r++)
        {
            for (int c = 0; c < 8; c++)
            {
                int value = grid[r, c];

                // Rechts prüfen
                if (c < 7 && grid[r, c + 1] == value)
                    pairCount++;

                // Unten prüfen
                if (r < 7 && grid[r + 1, c] == value)
                    pairCount++;
            }
        }

        return pairCount;
    }


    void PrintBoardToConsole()
    {
        string output = "Grid Values:\n";
        for (int r = 0; r < rows; r++)
        {
            string rowText = "";
            for (int c = 0; c < columns; c++)
            {
                rowText += gridValues[r, c] + " ";
            }
            output += rowText + "\n";
        }
        Debug.Log(output);
    }

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
            Debug.LogError("GameManager: Piece-Prefabs nicht gesetzt!");
            return;
        }

        int shogunColumn = 4;

        // Spieler 2 (oben, Reihe 0)
        for (int c = 0; c < columns; c++)
        {
            if (c == shogunColumn)
                SpawnP2Shogun(0, c);
            else
                SpawnP2Normal(0, c);
        }

        // Spieler 1 (unten, Reihe 7)
        for (int c = 0; c < columns; c++)
        {
            if (c == shogunColumn)
                SpawnP1Shogun(7, c);
            else
                SpawnP1Normal(7, c);
        }
    }

    void SpawnP1Normal(int row, int column)
    {
        Vector3 worldPos = boardRenderer.GetWorldPosition(row, column);
        worldPos.z = -0.1f;

        GameObject pieceObj = Instantiate(p1NormalPrefab, worldPos, Quaternion.identity);
        Piece piece = pieceObj.GetComponent<Piece>();

        if (piece != null)
        {
            piece.owner = 1;
            piece.pieceType = PieceType.Soldier;
            piece.row = row;
            piece.column = column;
            piece.gameManager = this;
            piece.UpdateRange(GetInterpretedGridValue(row, column, 1), PieceType.Soldier);
            piece.id = $"p1-soldier-{row}-{column}";
            pieces[piece.id] = piece;
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
            piece.pieceType = PieceType.Shogun;
            piece.row = row;
            piece.column = column;
            piece.gameManager = this;
            piece.UpdateRange(GetInterpretedGridValue(row, column, 1), PieceType.Shogun);
            piece.id = $"p1-shogun";
            pieces[piece.id] = piece;
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
            piece.pieceType = PieceType.Soldier;
            piece.row = row;
            piece.column = column;
            piece.gameManager = this;
            piece.UpdateRange(GetInterpretedGridValue(row, column, 2), PieceType.Soldier);
            piece.id = $"p2-soldier-{row}-{column}";
            pieces[piece.id] = piece;
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
            piece.pieceType = PieceType.Shogun;
            piece.row = row;
            piece.column = column;
            piece.gameManager = this;
            piece.UpdateRange(GetInterpretedGridValue(row, column, 2), PieceType.Shogun);
            piece.id = $"p2-shogun";
            pieces[piece.id] = piece;
        }
    }

    // === KERNLOGIK ===

    public int GetInterpretedGridValue(int row, int column, int owner)
    {
        int rawVal = gridValues[row, column];
        // Spieler 1: normaler Wert, Spieler 2: invertiert
        return owner == 1 ? rawVal : 5 - rawVal;
    }

    public Piece GetPieceAt(int row, int column)
    {
        // Über ToList() iterieren, um Kopie zu erstellen
        foreach (Piece p in pieces.Values.ToList())
        {
            if (p.row == row && p.column == column)
                return p;
        }
        return null;
    }

    public int GetPlayerPieceCount(int owner)
    {
        int count = 0;
        foreach (Piece p in pieces.Values.ToList())
        {
            if (p.owner == owner)
                count++;
        }
        return count;
    }

    public Piece GetShogun(int owner)
    {
        foreach (Piece p in pieces.Values.ToList())
        {
            if (p.owner == owner && p.pieceType == PieceType.Shogun)
                return p;
        }
        return null;
    }

    // Berechnet Raw Moves: L-förmige Züge mit Pfad-Verfolgung
    public List<ValidMove> CalculateRawMoves(Piece piece)
    {
        List<ValidMove> moves = new List<ValidMove>();

        int range = piece.range;
        if (range <= 0)
            return moves;

        // Vier Richtungen: oben, rechts, unten, links
        int[][] directions = new int[][]
        {
        new int[] { -1, 0 },   // oben (Index 0)
        new int[] { 0, 1 },    // rechts (Index 1)
        new int[] { 1, 0 },    // unten (Index 2)
        new int[] { 0, -1 }    // links (Index 3)
        };

        // === GERADE ZÜGE: VOLLE Range in eine Richtung ===
        foreach (var dir in directions)
        {
            bool blocked = false;
            int finalRow = piece.row;
            int finalCol = piece.column;

            // Jeden Schritt prüfen bis zur vollen Range
            for (int step = 1; step <= range; step++)
            {
                int newRow = piece.row + dir[0] * step;
                int newCol = piece.column + dir[1] * step;

                if (newRow < 0 || newRow >= rows || newCol < 0 || newCol >= columns)
                {
                    blocked = true;
                    break;
                }

                Piece stepPiece = GetPieceAt(newRow, newCol);

                if (step == range)
                {
                    // Letzter Schritt: darf leer oder Gegner sein
                    if (stepPiece != null && stepPiece.owner == piece.owner)
                    {
                        blocked = true;
                        break;
                    }
                    finalRow = newRow;
                    finalCol = newCol;
                }
                else
                {
                    // Zwischenschritte: MÜSSEN leer sein
                    if (stepPiece != null)
                    {
                        blocked = true;
                        break;
                    }
                }
            }

            if (!blocked)
            {
                List<Position> path = BuildPath(piece.row, piece.column, finalRow, finalCol);
                moves.Add(new ValidMove { from = new Position(piece.row, piece.column), to = new Position(finalRow, finalCol), path = path });
            }
        }

        // === L-ZÜGE: Ein 90°-Turn (niemals 180° zurück!) ===
        for (int leg1Length = 1; leg1Length < range; leg1Length++)
        {
            int leg2Length = range - leg1Length;

            for (int dirIndex = 0; dirIndex < 4; dirIndex++)
            {
                int[] firstDir = directions[dirIndex];

                // Nur 90°-Turns erlaubt: links (+3 mod 4) oder rechts (+1 mod 4)
                int[] leftDir = directions[(dirIndex + 3) % 4];   // 90° links
                int[] rightDir = directions[(dirIndex + 1) % 4];  // 90° rechts

                int[][] turnDirections = new int[][] { leftDir, rightDir };

                foreach (var secondDir in turnDirections)
                {
                    // === Prüfe ersten Schenkel ===
                    bool firstLegBlocked = false;
                    int midRow = piece.row;
                    int midCol = piece.column;

                    for (int s = 1; s <= leg1Length; s++)
                    {
                        midRow = piece.row + firstDir[0] * s;
                        midCol = piece.column + firstDir[1] * s;

                        if (midRow < 0 || midRow >= rows || midCol < 0 || midCol >= columns)
                        {
                            firstLegBlocked = true;
                            break;
                        }

                        Piece stepPiece = GetPieceAt(midRow, midCol);

                        // Alle Schritte im ersten Schenkel MÜSSEN leer sein
                        if (stepPiece != null)
                        {
                            firstLegBlocked = true;
                            break;
                        }
                    }

                    if (firstLegBlocked)
                        continue;

                    // === Prüfe zweiten Schenkel ===
                    bool secondLegBlocked = false;
                    int endRow = midRow;
                    int endCol = midCol;

                    for (int s = 1; s <= leg2Length; s++)
                    {
                        endRow = midRow + secondDir[0] * s;
                        endCol = midCol + secondDir[1] * s;

                        if (endRow < 0 || endRow >= rows || endCol < 0 || endCol >= columns)
                        {
                            secondLegBlocked = true;
                            break;
                        }

                        Piece stepPiece = GetPieceAt(endRow, endCol);

                        if (s == leg2Length)
                        {
                            // Letzter Schritt: darf leer oder Gegner sein
                            if (stepPiece != null && stepPiece.owner == piece.owner)
                            {
                                secondLegBlocked = true;
                                break;
                            }
                        }
                        else
                        {
                            // Zwischenschritte: MÜSSEN leer sein
                            if (stepPiece != null)
                            {
                                secondLegBlocked = true;
                                break;
                            }
                        }
                    }

                    if (secondLegBlocked)
                        continue;

                    // Beide Schenkel sind frei → gültiger L-Zug!
                    List<Position> path = BuildPathL(piece.row, piece.column, midRow, midCol, endRow, endCol);
                    moves.Add(new ValidMove { from = new Position(piece.row, piece.column), to = new Position(endRow, endCol), path = path });
                }
            }
        }

        return moves;
    }



    // Berechnet legale Züge (ohne in Check geraten)
    public List<ValidMove> CalculateValidMoves(Piece piece)
    {
        List<ValidMove> rawMoves = CalculateRawMoves(piece);
        List<ValidMove> legalMoves = new List<ValidMove>();

        foreach (ValidMove move in rawMoves)
        {
            // Simuliere den Zug
            Piece targetPiece = GetPieceAt(move.to.row, move.to.column);

            int oldRow = piece.row;
            int oldCol = piece.column;

            // === NEU: Wenn ein Piece geschlagen wird, temporär aus Dictionary entfernen ===
            bool removedPiece = false;
            if (targetPiece != null)
            {
                pieces.Remove(targetPiece.id);
                removedPiece = true;
            }

            // Piece bewegen
            piece.row = move.to.row;
            piece.column = move.to.column;

            // Prüfe: bin ich (der ziehende Spieler) in Check?
            bool inCheck = IsPlayerInCheck(piece.owner);

            // === UNDO: Alles zurücksetzen ===
            piece.row = oldRow;
            piece.column = oldCol;

            // Geschlagenes Piece wieder einfügen
            if (removedPiece && targetPiece != null)
            {
                pieces[targetPiece.id] = targetPiece;
            }

            // Nur legale Züge zurückgeben
            if (!inCheck)
                legalMoves.Add(move);
        }

        return legalMoves;
    }


    public List<Position> BuildPath(int fromRow, int fromCol, int toRow, int toCol)
    {
        List<Position> path = new List<Position>();
        int dr = toRow - fromRow;
        int dc = toCol - fromCol;

        int stepR = dr == 0 ? 0 : dr > 0 ? 1 : -1;
        int stepC = dc == 0 ? 0 : dc > 0 ? 1 : -1;

        int r = fromRow;
        int c = fromCol;

        while (r != toRow || c != toCol)
        {
            if (r != toRow) r += stepR;
            if (c != toCol) c += stepC;
            path.Add(new Position(r, c));
        }

        return path;
    }

    public List<Position> BuildPathL(int fromRow, int fromCol, int midRow, int midCol, int toRow, int toCol)
    {
        List<Position> path = new List<Position>();

        // Erste Etappe
        int dr = midRow - fromRow;
        int dc = midCol - fromCol;
        int stepR = dr == 0 ? 0 : dr > 0 ? 1 : -1;
        int stepC = dc == 0 ? 0 : dc > 0 ? 1 : -1;

        int r = fromRow;
        int c = fromCol;

        while (r != midRow || c != midCol)
        {
            if (r != midRow) r += stepR;
            if (c != midCol) c += stepC;
            path.Add(new Position(r, c));
        }

        // Zweite Etappe
        dr = toRow - midRow;
        dc = toCol - midCol;
        stepR = dr == 0 ? 0 : dr > 0 ? 1 : -1;
        stepC = dc == 0 ? 0 : dc > 0 ? 1 : -1;

        r = midRow;
        c = midCol;

        while (r != toRow || c != toCol)
        {
            if (r != toRow) r += stepR;
            if (c != toCol) c += stepC;
            path.Add(new Position(r, c));
        }

        return path;
    }

    // Gibt alle bedrohten Felder eines Spielers zurück
    public HashSet<string> GetThreatenedCells(int attackingPlayer)
    {
        HashSet<string> threats = new HashSet<string>();

        // Über Kopie iterieren
        List<Piece> attackingPieces = new List<Piece>();
        foreach (Piece p in pieces.Values)
        {
            if (p != null && p.owner == attackingPlayer)
                attackingPieces.Add(p);
        }

        foreach (Piece piece in attackingPieces)
        {
            if (piece == null) continue;

            List<ValidMove> moves = CalculateRawMoves(piece);
            foreach (ValidMove move in moves)
            {
                threats.Add($"{move.to.row},{move.to.column}");
            }
        }

        return threats;
    }

    // Prüft, ob ein Spieler in Check ist
    public bool IsPlayerInCheck(int player)
    {
        Piece shogun = GetShogun(player);
        if (shogun == null)
            return false;

        int opponent = player == 1 ? 2 : 1;
        HashSet<string> threats = GetThreatenedCells(opponent);

        return threats.Contains($"{shogun.row},{shogun.column}");
    }

    // Prüft, ob ein Spieler legale Züge hat
    public bool HasLegalMoves(int player)
    {
        // WICHTIG: Über Kopie iterieren, damit CalculateValidMoves das Dictionary ändern kann
        List<Piece> playerPieces = new List<Piece>();

        foreach (Piece p in pieces.Values)
        {
            if (p != null && p.owner == player)
                playerPieces.Add(p);
        }

        foreach (Piece piece in playerPieces)
        {
            if (piece == null) continue;  // Sicherheitscheck

            List<ValidMove> legalMoves = CalculateValidMoves(piece);
            if (legalMoves.Count > 0)
                return true;
        }

        return false;
    }


    // === UI / INTERACTION ===

    public void OnTileClicked(int row, int column)
    {
        // KI-Check: VsAI && KI dran (Player 2) → Auto-Zug
        if (mode == GameMode.VsAI && currentPlayer == 2)
        {
            StartCoroutine(AIMakeMove());  // Warte 0.5s, dann KI-Zug
            return;
        }

        if (winner != 0)
            return;  // Spiel vorbei

        Piece clickedPiece = GetPieceAt(row, column);

        if (clickedPiece != null && clickedPiece.owner == currentPlayer)
        {
            // Figur des aktuellen Spielers angeklickt → Auswahl
            SelectPiece(clickedPiece);
        }
        else if (selectedPiece != null && clickedPiece == null)
        {
            // Leeres Feld und Piece ausgewählt → Zug versuchen
            ValidMove move = validMoves.FirstOrDefault(m => m.to.row == row && m.to.column == column);
            if (move != null)
            {
                ExecuteMove(move);
            }
            else
            {
                DeselectPiece();
            }
        }
        else if (clickedPiece != null && clickedPiece.owner != currentPlayer)
        {
            // Gegner-Figur: versuche zu schlagen
            if (selectedPiece != null)
            {
                ValidMove move = validMoves.FirstOrDefault(m => m.to.row == row && m.to.column == column);
                if (move != null)
                {
                    ExecuteMove(move);
                }
            }
        }
    }
    private IEnumerator AIMakeMove()
    {
        yield return new WaitForSeconds(0.5f);

        // FIX: .Values.ToList() → Kopie, keine Modifikation!
        var kiPieces = pieces.Values.ToList().Where(p => p != null && p.owner == 2).ToList();

        foreach (Piece p in kiPieces)
        {
            var moves = CalculateValidMoves(p);
            if (moves.Count > 0)
            {
                ExecuteMove(moves[0]);
                yield break;
            }
        }
        Debug.Log("KI: Kein Zug möglich");
    }



    public void SelectPiece(Piece piece)
    {
        if (selectedPiece != null)
        {
            selectedPiece.SetSelected(false);
        }

        selectedPiece = piece;
        selectedPiece.SetSelected(true);
        validMoves = CalculateValidMoves(piece);
        boardRenderer.HighlightValidMoves(validMoves);

        Debug.Log($"Piece ausgewählt: Owner {piece.owner}, Typ {piece.pieceType}, Range {piece.range}, Legale Züge: {validMoves.Count}");
    }

    public void DeselectPiece()
    {
        if (selectedPiece != null)
        {
            selectedPiece.SetSelected(false);
        }
        selectedPiece = null;
        validMoves.Clear();
        boardRenderer.ClearHighlights();
    }

    public void ExecuteMove(ValidMove move)
    {
        Piece movingPiece = GetPieceAt(move.from.row, move.from.column);
        Piece targetPiece = GetPieceAt(move.to.row, move.to.column);

        if (movingPiece == null)
            return;

        // Piece bewegen
        movingPiece.row = move.to.row;
        movingPiece.column = move.to.column;

        Vector3 newWorldPos = boardRenderer.GetWorldPosition(move.to.row, move.to.column);
        newWorldPos.z = -0.1f;
        movingPiece.transform.position = newWorldPos;

        // Wenn Gegner geschlagen
        if (targetPiece != null)
        {
            Debug.Log($"Piece geschlagen: Owner {targetPiece.owner}, Typ {targetPiece.pieceType}");
            pieces.Remove(targetPiece.id);
            Destroy(targetPiece.gameObject);

            // Shogun geschlagen = sofort gewonnen
            if (targetPiece.pieceType == PieceType.Shogun)
            {
                winner = currentPlayer;
                Debug.Log($"Spieler {winner} GEWINNT! Shogun geschlagen!");
                DeselectPiece();
                return;
            }
        }

        // Range neu berechnen basierend auf neuem Feld
        int newInterpretedVal = GetInterpretedGridValue(move.to.row, move.to.column, currentPlayer);
        movingPiece.UpdateRange(newInterpretedVal, movingPiece.pieceType);

        DeselectPiece();
        turnCount++;

        // Spieler wechseln
        int nextPlayer = currentPlayer == 1 ? 2 : 1;

        // Prüfe Endgame-Bedingungen
        int nextPlayerCount = GetPlayerPieceCount(nextPlayer);

        // Army Depleted (< 3 Pieces)
        if (nextPlayerCount < 3)
        {
            winner = currentPlayer;
            Debug.Log($"Spieler {winner} GEWINNT! Armee des Gegners aufgerieben ({nextPlayerCount} Steine übrig)!");
            return;
        }

        // Kein legaler Zug möglich?
        if (!HasLegalMoves(nextPlayer))
        {
            bool inCheck = IsPlayerInCheck(nextPlayer);
            if (inCheck)
            {
                winner = currentPlayer;
                Debug.Log($"Spieler {winner} GEWINNT! CHECKMATE!");
            }
            else
            {
                winner = currentPlayer;
                Debug.Log($"Spieler {winner} GEWINNT! Gegner ist TRAPPED!");
            }
            return;
        }

        // Zug-Wechsel
        currentPlayer = nextPlayer;
        Debug.Log($"Spieler {currentPlayer} ist dran. Turn: {turnCount}");
        // Auto-KI nach P1-Zug
        if (mode == GameMode.VsAI && currentPlayer == 2)
        {
            StartCoroutine(AIMakeMove());
        }

    }

    public List<ValidMove> GetValidMoves()
    {
        return validMoves;
    }

    public Piece GetSelectedPiece()
    {
        return selectedPiece;
    }

    public int GetWinner()
    {
        return winner;
    }

    public int GetCurrentPlayer()
    {
        return currentPlayer;
    }

    public int[,] GetGridValues()
    {
        return gridValues;
    }

    public BoardRenderer GetBoardRenderer()
    {
        return boardRenderer;
    }
}
