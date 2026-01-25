using UnityEngine;
using System.Collections.Generic;
using System.Linq;

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
        // Feste Magnetwerte (aus React-Version übernommen)
        int[,] userDefinedGrid = new int[8, 8]
        {
            { 4, 1, 2, 3, 3, 2, 1, 4 },
            { 3, 4, 1, 2, 2, 1, 4, 3 },
            { 2, 3, 4, 1, 1, 4, 3, 2 },
            { 1, 2, 3, 4, 4, 3, 2, 1 },
            { 4, 3, 2, 1, 1, 2, 3, 4 },
            { 3, 2, 1, 4, 4, 1, 2, 3 },
            { 2, 1, 4, 3, 3, 4, 1, 2 },
            { 1, 4, 3, 2, 2, 3, 4, 1 }
        };

        gridValues = userDefinedGrid;
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

        // Spieler 1 (oben, Reihe 0)
        for (int c = 0; c < columns; c++)
        {
            if (c == shogunColumn)
                SpawnP1Shogun(0, c);
            else
                SpawnP1Normal(0, c);
        }

        // Spieler 2 (unten, Reihe 7)
        for (int c = 0; c < columns; c++)
        {
            if (c == shogunColumn)
                SpawnP2Shogun(7, c);
            else
                SpawnP2Normal(7, c);
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
        foreach (Piece p in pieces.Values)
        {
            if (p.row == row && p.column == column)
                return p;
        }
        return null;
    }

    public int GetPlayerPieceCount(int owner)
    {
        int count = 0;
        foreach (Piece p in pieces.Values)
        {
            if (p.owner == owner)
                count++;
        }
        return count;
    }

    public Piece GetShogun(int owner)
    {
        foreach (Piece p in pieces.Values)
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
            new int[] { -1, 0 },   // oben
            new int[] { 0, 1 },    // rechts
            new int[] { 1, 0 },    // unten
            new int[] { 0, -1 }    // links
        };

        // Gerade Züge (0 Richtungswechsel)
        foreach (var dir in directions)
        {
            for (int step = 1; step <= range; step++)
            {
                int newRow = piece.row + dir[0] * step;
                int newCol = piece.column + dir[1] * step;

                if (!IsValidPosition(newRow, newCol))
                    break;

                Piece targetPiece = GetPieceAt(newRow, newCol);
                if (targetPiece != null && targetPiece.owner == piece.owner)
                    break;  // Eigenes Piece blockiert

                List<Position> path = BuildPath(piece.row, piece.column, newRow, newCol);
                moves.Add(new ValidMove { from = new Position(piece.row, piece.column), to = new Position(newRow, newCol), path = path });

                if (targetPiece != null)
                    break;  // Gegner blockiert, aber dieses Feld ist erlaubt
            }
        }

        // L-förmige Züge (1 Richtungswechsel)
        for (int leg1 = 1; leg1 < range; leg1++)
        {
            int leg2 = range - leg1;

            for (int i = 0; i < 4; i++)
            {
                int[] dir1 = directions[i];
                int[] dir2_left = directions[(i + 3) % 4];  // 90° links
                int[] dir2_right = directions[(i + 1) % 4]; // 90° rechts

                // Versuche beide 90°-Drehungen
                int[][] dir2Options = new int[][] { dir2_left, dir2_right };

                foreach (var dir2 in dir2Options)
                {
                    int midRow = piece.row + dir1[0] * leg1;
                    int midCol = piece.column + dir1[1] * leg1;

                    if (!IsValidPosition(midRow, midCol))
                        continue;

                    Piece midPiece = GetPieceAt(midRow, midCol);
                    if (midPiece != null && midPiece.owner == piece.owner)
                        continue;  // Eigenes Piece blockiert

                    if (midPiece != null && midPiece.owner != piece.owner)
                        continue;  // Gegner blockiert Weg

                    // Zweiter Schenkel
                    int endRow = midRow + dir2[0] * leg2;
                    int endCol = midCol + dir2[1] * leg2;

                    if (!IsValidPosition(endRow, endCol))
                        continue;

                    Piece endPiece = GetPieceAt(endRow, endCol);
                    if (endPiece != null && endPiece.owner == piece.owner)
                        continue;  // Eigenes Piece blockiert

                    // Weg ist frei (endPiece darf Gegner sein)
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

            // Piece bewegen
            piece.row = move.to.row;
            piece.column = move.to.column;

            // Prüfe: bin ich in Check?
            bool inCheck = IsPlayerInCheck(piece.owner);

            // Undo
            piece.row = oldRow;
            piece.column = oldCol;

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

    public bool IsValidPosition(int row, int column)
    {
        return row >= 0 && row < rows && column >= 0 && column < columns;
    }

    // Gibt alle bedrohten Felder eines Spielers zurück
    public HashSet<string> GetThreatenedCells(int attackingPlayer)
    {
        HashSet<string> threats = new HashSet<string>();

        foreach (Piece piece in pieces.Values)
        {
            if (piece.owner == attackingPlayer)
            {
                List<ValidMove> moves = CalculateRawMoves(piece);
                foreach (ValidMove move in moves)
                {
                    threats.Add($"{move.to.row},{move.to.column}");
                }
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
        foreach (Piece piece in pieces.Values)
        {
            if (piece.owner == player)
            {
                List<ValidMove> legalMoves = CalculateValidMoves(piece);
                if (legalMoves.Count > 0)
                    return true;
            }
        }
        return false;
    }

    // === UI / INTERACTION ===

    public void OnTileClicked(int row, int column)
    {
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

    public void SelectPiece(Piece piece)
    {
        if (selectedPiece != null)
        {
            selectedPiece.SetSelected(false);
        }

        selectedPiece = piece;
        selectedPiece.SetSelected(true);
        validMoves = CalculateValidMoves(piece);

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
