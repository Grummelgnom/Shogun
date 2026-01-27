using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

// Enum für die Piece-Typen (Soldat oder Shogun)
public enum PieceType
{
    Soldier,
    Shogun
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
    public GameMode mode = GameMode.TwoPlayer;

    public BoardRenderer boardRenderer;

    public int[,] board = new int[8, 8];
    public int[,] gridValues = new int[8, 8];

    private Dictionary<string, Piece> pieces = new Dictionary<string, Piece>();

    public int currentPlayer = 1;
    private Piece selectedPiece = null;
    private List<ValidMove> validMoves = new List<ValidMove>();
    private int winner = 0;
    private int turnCount = 0;

    // ========= AI SETTINGS =========
    [Header("AI")]
    public int aiPlayer = 2;
    [Range(1, 5)] public int aiDepth = 3;      // Depth 3 = sehr viel weniger Freeze-Risiko
    public float aiDelay = 0.35f;              // Optik: "KI denkt nach"
    private bool aiThinking = false;

    // ========= MINIMAX SIM STACK (Undo Fix) =========
    private struct SimStep
    {
        public Piece mover;
        public int moverRowBefore;
        public int moverColBefore;
        public int moverRangeBefore;

        public Piece captured;       // null wenn kein Schlag
        public string capturedId;
        public int capturedRowBefore;
        public int capturedColBefore;
        public int capturedRangeBefore;
    }

    private Stack<SimStep> simStack = new Stack<SimStep>();

    void Start()
    {
        InitBoard();
        PrintBoardToConsole();
        SpawnStartPieces();
    }

    // =======================
    // Board Init
    // =======================
    void InitBoard()
    {
        int bestScore = int.MaxValue;
        int[,] bestGrid = null;
        int attempts = 500;

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

        gridValues = bestGrid;
        board = bestGrid;

        Debug.Log($"Bestes Grid gefunden mit {bestScore} orthogonalen Pärchen (aus {attempts} Versuchen)");
    }

    int[,] GenerateRandomGrid()
    {
        int[,] grid = new int[8, 8];

        List<int> values = new List<int>();
        for (int i = 1; i <= 4; i++)
            for (int j = 0; j < 16; j++)
                values.Add(i);

        for (int i = values.Count - 1; i > 0; i--)
        {
            int j = Random.Range(0, i + 1);
            (values[i], values[j]) = (values[j], values[i]);
        }

        int index = 0;
        for (int r = 0; r < 8; r++)
            for (int c = 0; c < 8; c++)
                grid[r, c] = values[index++];

        return grid;
    }

    int CountOrthogonalPairs(int[,] grid)
    {
        int pairCount = 0;

        for (int r = 0; r < 8; r++)
        {
            for (int c = 0; c < 8; c++)
            {
                int value = grid[r, c];
                if (c < 7 && grid[r, c + 1] == value) pairCount++;
                if (r < 7 && grid[r + 1, c] == value) pairCount++;
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
                rowText += gridValues[r, c] + " ";
            output += rowText + "\n";
        }
        Debug.Log(output);
    }

    // =======================
    // Spawn
    // =======================
    void SpawnStartPieces()
    {
        if (boardRenderer == null)
        {
            Debug.LogError("GameManager: boardRenderer nicht gesetzt!");
            return;
        }

        if (p1NormalPrefab == null || p1ShogunPrefab == null || p2NormalPrefab == null || p2ShogunPrefab == null)
        {
            Debug.LogError("GameManager: Piece-Prefabs nicht gesetzt!");
            return;
        }

        int shogunColumn = 4;

        // Spieler 2 oben (Reihe 0)
        for (int c = 0; c < columns; c++)
        {
            if (c == shogunColumn) SpawnP2Shogun(0, c);
            else SpawnP2Normal(0, c);
        }

        // Spieler 1 unten (Reihe 7)
        for (int c = 0; c < columns; c++)
        {
            if (c == shogunColumn) SpawnP1Shogun(7, c);
            else SpawnP1Normal(7, c);
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

    // =======================
    // Core Helpers
    // =======================
    public int GetInterpretedGridValue(int row, int column, int owner)
    {
        int rawVal = gridValues[row, column];
        return owner == 1 ? rawVal : 5 - rawVal;
    }

    private int CalcRange(int interpretedGridValue, PieceType type)
    {
        if (type == PieceType.Shogun) return interpretedGridValue >= 3 ? 2 : 1;
        return Mathf.Clamp(interpretedGridValue, 1, 4);
    }

    public Piece GetPieceAt(int row, int column)
    {
        foreach (Piece p in pieces.Values.ToList())
        {
            if (p != null && p.row == row && p.column == column)
                return p;
        }
        return null;
    }

    public int GetPlayerPieceCount(int owner)
    {
        int count = 0;
        foreach (Piece p in pieces.Values.ToList())
            if (p != null && p.owner == owner) count++;
        return count;
    }

    public Piece GetShogun(int owner)
    {
        foreach (Piece p in pieces.Values.ToList())
            if (p != null && p.owner == owner && p.pieceType == PieceType.Shogun) return p;
        return null;
    }

    // =======================
    // Moves (deine Logik)
    // =======================
    public List<ValidMove> CalculateRawMoves(Piece piece)
    {
        List<ValidMove> moves = new List<ValidMove>();

        int range = piece.range;
        if (range <= 0) return moves;

        int[][] directions = new int[][]
        {
            new int[] { -1, 0 },
            new int[] { 0, 1 },
            new int[] { 1, 0 },
            new int[] { 0, -1 }
        };

        foreach (var dir in directions)
        {
            bool blocked = false;
            int finalRow = piece.row;
            int finalCol = piece.column;

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

        for (int leg1Length = 1; leg1Length < range; leg1Length++)
        {
            int leg2Length = range - leg1Length;

            for (int dirIndex = 0; dirIndex < 4; dirIndex++)
            {
                int[] firstDir = directions[dirIndex];

                int[] leftDir = directions[(dirIndex + 3) % 4];
                int[] rightDir = directions[(dirIndex + 1) % 4];

                int[][] turnDirections = new int[][] { leftDir, rightDir };

                foreach (var secondDir in turnDirections)
                {
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

                        if (GetPieceAt(midRow, midCol) != null)
                        {
                            firstLegBlocked = true;
                            break;
                        }
                    }

                    if (firstLegBlocked) continue;

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
                            if (stepPiece != null && stepPiece.owner == piece.owner)
                            {
                                secondLegBlocked = true;
                                break;
                            }
                        }
                        else
                        {
                            if (stepPiece != null)
                            {
                                secondLegBlocked = true;
                                break;
                            }
                        }
                    }

                    if (secondLegBlocked) continue;

                    List<Position> path = BuildPathL(piece.row, piece.column, midRow, midCol, endRow, endCol);
                    moves.Add(new ValidMove { from = new Position(piece.row, piece.column), to = new Position(endRow, endCol), path = path });
                }
            }
        }

        return moves;
    }

    public List<ValidMove> CalculateValidMoves(Piece piece)
    {
        List<ValidMove> rawMoves = CalculateRawMoves(piece);
        List<ValidMove> legalMoves = new List<ValidMove>();

        foreach (ValidMove move in rawMoves)
        {
            Piece targetPiece = GetPieceAt(move.to.row, move.to.column);

            int oldRow = piece.row;
            int oldCol = piece.column;
            int oldRange = piece.range;

            bool removedPiece = false;
            if (targetPiece != null)
            {
                pieces.Remove(targetPiece.id);
                removedPiece = true;
            }

            piece.row = move.to.row;
            piece.column = move.to.column;

            // Range temporär updaten (sonst kann Check-Berechnung in Folgezügen schief werden)
            int interpreted = GetInterpretedGridValue(piece.row, piece.column, piece.owner);
            piece.range = CalcRange(interpreted, piece.pieceType);

            bool inCheck = IsPlayerInCheck(piece.owner);

            piece.row = oldRow;
            piece.column = oldCol;
            piece.range = oldRange;

            if (removedPiece && targetPiece != null)
                pieces[targetPiece.id] = targetPiece;

            if (!inCheck) legalMoves.Add(move);
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

    // =======================
    // Threat / Check / Legal
    // =======================
    public HashSet<string> GetThreatenedCells(int attackingPlayer)
    {
        HashSet<string> threats = new HashSet<string>();

        List<Piece> attackingPieces = new List<Piece>();
        foreach (Piece p in pieces.Values)
            if (p != null && p.owner == attackingPlayer) attackingPieces.Add(p);

        foreach (Piece piece in attackingPieces)
        {
            List<ValidMove> moves = CalculateRawMoves(piece);
            foreach (ValidMove move in moves)
                threats.Add($"{move.to.row},{move.to.column}");
        }

        return threats;
    }

    public bool IsPlayerInCheck(int player)
    {
        Piece shogun = GetShogun(player);
        if (shogun == null) return false;

        int opponent = player == 1 ? 2 : 1;
        HashSet<string> threats = GetThreatenedCells(opponent);

        return threats.Contains($"{shogun.row},{shogun.column}");
    }

    public bool HasLegalMoves(int player)
    {
        List<Piece> playerPieces = new List<Piece>();
        foreach (Piece p in pieces.Values)
            if (p != null && p.owner == player) playerPieces.Add(p);

        foreach (Piece piece in playerPieces)
            if (CalculateValidMoves(piece).Count > 0) return true;

        return false;
    }

    // =======================
    // UI / Interaction
    // =======================
    public void OnTileClicked(int row, int column)
    {
        if (winner != 0) return;

        // Wichtig: NICHT hier KI starten – sonst startet sie bei jedem Klick erneut und kann "freezen"
        if (mode == GameMode.VsAI && currentPlayer == aiPlayer)
            return;

        Piece clickedPiece = GetPieceAt(row, column);

        if (clickedPiece != null && clickedPiece.owner == currentPlayer)
        {
            SelectPiece(clickedPiece);
        }
        else if (selectedPiece != null && clickedPiece == null)
        {
            ValidMove move = validMoves.FirstOrDefault(m => m.to.row == row && m.to.column == column);
            if (move != null) ExecuteMove(move);
            else DeselectPiece();
        }
        else if (clickedPiece != null && clickedPiece.owner != currentPlayer)
        {
            if (selectedPiece != null)
            {
                ValidMove move = validMoves.FirstOrDefault(m => m.to.row == row && m.to.column == column);
                if (move != null) ExecuteMove(move);
            }
        }
    }

    public void SelectPiece(Piece piece)
    {
        if (selectedPiece != null) selectedPiece.SetSelected(false);

        selectedPiece = piece;
        selectedPiece.SetSelected(true);
        validMoves = CalculateValidMoves(piece);
        boardRenderer.HighlightValidMoves(validMoves);
    }

    public void DeselectPiece()
    {
        if (selectedPiece != null) selectedPiece.SetSelected(false);
        selectedPiece = null;
        validMoves.Clear();
        boardRenderer.ClearHighlights();
    }

    public void ExecuteMove(ValidMove move)
    {
        Piece movingPiece = GetPieceAt(move.from.row, move.from.column);
        Piece targetPiece = GetPieceAt(move.to.row, move.to.column);

        if (movingPiece == null) return;

        movingPiece.row = move.to.row;
        movingPiece.column = move.to.column;

        Vector3 newWorldPos = boardRenderer.GetWorldPosition(move.to.row, move.to.column);
        newWorldPos.z = -0.1f;
        movingPiece.transform.position = newWorldPos;

        if (targetPiece != null)
        {
            pieces.Remove(targetPiece.id);
            Destroy(targetPiece.gameObject);

            if (targetPiece.pieceType == PieceType.Shogun)
            {
                winner = currentPlayer;
                DeselectPiece();
                return;
            }
        }

        int newInterpretedVal = GetInterpretedGridValue(move.to.row, move.to.column, currentPlayer);
        movingPiece.UpdateRange(newInterpretedVal, movingPiece.pieceType);

        DeselectPiece();
        turnCount++;

        int nextPlayer = currentPlayer == 1 ? 2 : 1;
      
        int nextPlayerCount = GetPlayerPieceCount(nextPlayer);

        // Siegbedingung: Gegner hat nur noch Shogun + 1 Stein (oder weniger)
        if (nextPlayerCount <= 2)
        {
            winner = currentPlayer;
            Debug.Log($"Spieler {winner} GEWINNT! Gegner hat nur noch {nextPlayerCount} Steine (Shogun + max. 1 weiterer).");
            return;
        }


        if (!HasLegalMoves(nextPlayer))
        {
            bool inCheck = IsPlayerInCheck(nextPlayer);
            winner = currentPlayer;

            if (inCheck) Debug.Log($"Spieler {winner} GEWINNT! CHECKMATE!");
            else Debug.Log($"Spieler {winner} GEWINNT! Gegner ist TRAPPED!");

            return;
        }


        currentPlayer = nextPlayer;

        // KI startet nur hier, exakt einmal nach Spielerzug
        if (mode == GameMode.VsAI && currentPlayer == aiPlayer && winner == 0 && !aiThinking)
            StartCoroutine(AIMakeMove());
    }

    // =======================
    // AI (Minimax)
    // =======================
    private IEnumerator AIMakeMove()
    {
        aiThinking = true;
        yield return new WaitForSeconds(aiDelay);

        if (winner != 0) { aiThinking = false; yield break; }
        if (currentPlayer != aiPlayer) { aiThinking = false; yield break; }

        ValidMove best = GetBestMove(aiPlayer, aiDepth);
        if (best != null) ExecuteMove(best);
        else Debug.Log("KI: Kein Zug möglich");

        aiThinking = false;
    }

    private ValidMove GetBestMove(int forPlayer, int depth)
    {
        int opponent = forPlayer == 1 ? 2 : 1;

        int bestScore = int.MinValue;
        ValidMove bestMove = null;

        var myPieces = pieces.Values.Where(p => p != null && p.owner == forPlayer).ToList();

        foreach (Piece p in myPieces)
        {
            var moves = CalculateValidMoves(p);
            foreach (ValidMove m in moves)
            {
                SimulateMove(m);
                int score = Minimax(opponent, depth - 1, int.MinValue, int.MaxValue, false, forPlayer);
                UndoMove();

                if (score > bestScore)
                {
                    bestScore = score;
                    bestMove = m;
                }
            }
        }

        return bestMove;
    }

    private int Minimax(int playerToMove, int depth, int alpha, int beta, bool maximizing, int aiPerspective)
    {
        if (depth <= 0) return EvaluateBoard(aiPerspective);

        int opponent = playerToMove == 1 ? 2 : 1;

        var piecesToMove = pieces.Values.Where(p => p != null && p.owner == playerToMove).ToList();
        bool hasMove = false;

        if (maximizing)
        {
            int best = int.MinValue;

            foreach (Piece p in piecesToMove)
            {
                var moves = CalculateValidMoves(p);
                foreach (ValidMove m in moves)
                {
                    hasMove = true;

                    SimulateMove(m);
                    int score = Minimax(opponent, depth - 1, alpha, beta, false, aiPerspective);
                    UndoMove();

                    best = Mathf.Max(best, score);
                    alpha = Mathf.Max(alpha, score);
                    if (beta <= alpha) break;
                }
                if (beta <= alpha) break;
            }

            if (!hasMove) return EvaluateNoMoves(playerToMove, aiPerspective);
            return best;
        }
        else
        {
            int best = int.MaxValue;

            foreach (Piece p in piecesToMove)
            {
                var moves = CalculateValidMoves(p);
                foreach (ValidMove m in moves)
                {
                    hasMove = true;

                    SimulateMove(m);
                    int score = Minimax(opponent, depth - 1, alpha, beta, true, aiPerspective);
                    UndoMove();

                    best = Mathf.Min(best, score);
                    beta = Mathf.Min(beta, score);
                    if (beta <= alpha) break;
                }
                if (beta <= alpha) break;
            }

            if (!hasMove) return EvaluateNoMoves(playerToMove, aiPerspective);
            return best;
        }
    }

    private int EvaluateNoMoves(int playerToMove, int aiPerspective)
    {
        // In deinem Game: keine Züge => Gegner gewinnt (egal ob Check oder Trapped)
        // Wenn der Spieler ohne Züge die KI ist -> sehr schlecht, sonst sehr gut.
        bool isBadForAI = (playerToMove == aiPerspective);
        return isBadForAI ? -50000 : 50000;
    }

    private int EvaluateBoard(int aiPerspective)
    {
        int opp = aiPerspective == 1 ? 2 : 1;

        // Terminal Checks (schnell)
        if (GetShogun(aiPerspective) == null) return -100000;
        if (GetShogun(opp) == null) return 100000;

        int score = 0;

        score += GetPlayerPieceCount(aiPerspective) * 200;
        score -= GetPlayerPieceCount(opp) * 200;

        if (IsPlayerInCheck(aiPerspective)) score -= 500;
        if (IsPlayerInCheck(opp)) score += 500;

        return score;
    }

    // ---- Simulation (nur Daten, keine Transforms) ----
    private void SimulateMove(ValidMove move)
    {
        Piece mover = GetPieceAt(move.from.row, move.from.column);
        if (mover == null) return;

        Piece captured = GetPieceAt(move.to.row, move.to.column);

        SimStep step = new SimStep
        {
            mover = mover,
            moverRowBefore = mover.row,
            moverColBefore = mover.column,
            moverRangeBefore = mover.range,

            captured = captured,
            capturedId = captured != null ? captured.id : null,
            capturedRowBefore = captured != null ? captured.row : -1,
            capturedColBefore = captured != null ? captured.column : -1,
            capturedRangeBefore = captured != null ? captured.range : -1
        };

        simStack.Push(step);

        if (captured != null)
            pieces.Remove(captured.id);

        mover.row = move.to.row;
        mover.column = move.to.column;

        int interpreted = GetInterpretedGridValue(mover.row, mover.column, mover.owner);
        mover.range = CalcRange(interpreted, mover.pieceType);
    }

    private void UndoMove()
    {
        if (simStack.Count == 0) return;

        SimStep step = simStack.Pop();

        if (step.mover != null)
        {
            step.mover.row = step.moverRowBefore;
            step.mover.column = step.moverColBefore;
            step.mover.range = step.moverRangeBefore;
        }

        if (step.captured != null && !string.IsNullOrEmpty(step.capturedId))
        {
            step.captured.row = step.capturedRowBefore;
            step.captured.column = step.capturedColBefore;
            step.captured.range = step.capturedRangeBefore;

            if (!pieces.ContainsKey(step.capturedId))
                pieces.Add(step.capturedId, step.captured);
        }
    }

    // =======================
    // Getters
    // =======================
    public List<ValidMove> GetValidMoves() => validMoves;
    public Piece GetSelectedPiece() => selectedPiece;
    public int GetWinner() => winner;
    public int GetCurrentPlayer() => currentPlayer;
    public int[,] GetGridValues() => gridValues;
    public BoardRenderer GetBoardRenderer() => boardRenderer;
}
