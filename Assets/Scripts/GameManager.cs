using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

// Enum für die Piece-Typen (Soldat oder Shogun)
public enum PieceType
{
    Soldier,    // Normaler Soldat/Bauer
    Shogun      // Der Anführer
}

public enum GameMode
{
    TwoPlayer,
    VsAI
}

public class GameManager : MonoBehaviour
{
    [Header("Board")]
    public int rows = 8;
    public int columns = 8;

    [Header("Prefabs")]
    public GameObject p1NormalPrefab;
    public GameObject p1ShogunPrefab;
    public GameObject p2NormalPrefab;
    public GameObject p2ShogunPrefab;

    [Header("Refs")]
    public BoardRenderer boardRenderer;

    // Magnetfeld-Werte (feste 8x8 Grid)
    public int[,] gridValues = new int[8, 8];

    // board-Array (aktuell gleich gridValues)
    public int[,] board = new int[8, 8];

    // Alle Pieces in der Szene
    private Dictionary<string, Piece> pieces = new Dictionary<string, Piece>();
    private bool isAnimating = false; // Blockiert Input während Animation


    [Header("Game State")]
    public GameMode mode = GameMode.TwoPlayer;
    public int currentPlayer = 1; // 1 = P1, 2 = P2
    private Piece selectedPiece = null;
    private List<ValidMove> validMoves = new List<ValidMove>();
    private int winner = 0;
    private int turnCount = 0;

    [Header("AI (single settings apply to both AIs)")]
    public int aiPlayer = 2;                 // in VsAI: welcher Player ist die KI
    [Range(1, 8)] public int aiDepth = 3;    // wird aus GameSettings gesetzt
    public float aiDelaySeconds = 0.35f;     // wird aus GameSettings gesetzt
    private bool aiIsThinking = false;

    [Header("AI vs AI")]
    public bool aiVsAi = false;              // aus GameSettings.AIVsAI
    public int aiVsAiMaxTurns = 400;         // Notbremse

    // ===== Simulation Undo Stack (Fix) =====
    private struct SimStep
    {
        public Piece mover;
        public int moverRowBefore;
        public int moverColBefore;
        public int moverRangeBefore;

        public Piece captured;        // null wenn kein Schlag
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

        // Menü-Settings übernehmen (wenn GameSettings existiert)
        ApplyGameSettingsFromMenu();

        // Wenn AI vs AI: sofort starten
        if (mode == GameMode.VsAI && aiVsAi && winner == 0 && !aiIsThinking)
            StartCoroutine(AIMakeMove());
    }

    private void ApplyGameSettingsFromMenu()
    {
        // Falls du GameSettings noch nicht im Projekt hast, kannst du diesen Block auskommentieren.
        // (Wir nutzen das, um Stärke/Tempo global für beide KIs zu setzen.)
        mode = GameSettings.SelectedMode;
        aiVsAi = GameSettings.AIVsAI;

        int strength = GameSettings.Clamp1To4(GameSettings.AIStrength);
        int tempo = GameSettings.Clamp1To4(GameSettings.AITempo);

        // Stärke 1..4 -> Depth konservativ (Freeze-Schutz)
        // 1=>2, 2=>3, 3=>4, 4=>5
        aiDepth = 1 + strength;

        // Tempo 1..4 -> Delay (1 langsam, 4 schnell)
        // 1=>0.60s, 2=>0.40s, 3=>0.25s, 4=>0.10s
        switch (tempo)
        {
            case 1: aiDelaySeconds = 0.60f; break;
            case 2: aiDelaySeconds = 0.40f; break;
            case 3: aiDelaySeconds = 0.25f; break;
            case 4: aiDelaySeconds = 0.10f; break;
        }
    }

    // =============================
    // Board Init / Debug
    // =============================
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
            int temp = values[i];
            values[i] = values[j];
            values[j] = temp;
        }

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

    // =============================
    // Spawn
    // =============================
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
            piece.id = "p1-shogun";
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
            piece.id = "p2-shogun";
            pieces[piece.id] = piece;
        }
    }

    // =============================
    // Core helpers
    // =============================
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

    // =============================
    // Move generation
    // =============================
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

        // Gerade Züge: volle Range
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
                    // letzter Schritt: leer oder Gegner
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
                    // Zwischenschritte müssen leer sein
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

        // L-Züge: 90° Turn
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

    // Berechnet legale Züge (ohne in Check geraten)
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

            // Range temporär korrekt setzen, weil sonst Folgeprüfungen falsch sein können
            int interpreted = GetInterpretedGridValue(piece.row, piece.column, piece.owner);
            piece.range = CalcRange(interpreted, piece.pieceType);

            bool inCheck = IsPlayerInCheck(piece.owner);

            // UNDO
            piece.row = oldRow;
            piece.column = oldCol;
            piece.range = oldRange;

            if (removedPiece && targetPiece != null)
                pieces[targetPiece.id] = targetPiece;

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

    // =============================
    // Check / Threat
    // =============================
    public HashSet<string> GetThreatenedCells(int attackingPlayer)
    {
        HashSet<string> threats = new HashSet<string>();

        List<Piece> attackingPieces = new List<Piece>();
        foreach (Piece p in pieces.Values)
            if (p != null && p.owner == attackingPlayer) attackingPieces.Add(p);

        foreach (Piece piece in attackingPieces)
        {
            if (piece == null) continue;

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
        {
            if (piece == null) continue;
            if (CalculateValidMoves(piece).Count > 0) return true;
        }

        return false;
    }

    // =============================
    // UI / Interaction
    // =============================
    public void OnTileClicked(int row, int column)
    {
        if (winner != 0) return;
        if (isAnimating) return;
        // In AI-vs-AI ignorieren wir alle Klicks komplett
        if (mode == GameMode.VsAI && aiVsAi) return;

        // In VsAI: wenn KI dran ist, Klicks ignorieren
        if (mode == GameMode.VsAI && currentPlayer == aiPlayer) return;

        Piece clickedPiece = GetPieceAt(row, column);

        if (clickedPiece != null && clickedPiece.owner == currentPlayer)
        {
            SelectPiece(clickedPiece);
        }
        else if (selectedPiece != null && clickedPiece == null)
        {
            ValidMove move = validMoves.FirstOrDefault(m => m.to.row == row && m.to.column == column);
            if (move != null) StartCoroutine(ExecuteMove(move)); // GEÄNDERT
            else DeselectPiece();
        }
        else if (clickedPiece != null && clickedPiece.owner != currentPlayer)
        {
            if (selectedPiece != null)
            {
                ValidMove move = validMoves.FirstOrDefault(m => m.to.row == row && m.to.column == column);
                if (move != null) StartCoroutine(ExecuteMove(move)); // GEÄNDERT
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

    // GEÄNDERT: ExecuteMove ist jetzt IEnumerator!
    public IEnumerator ExecuteMove(ValidMove move)
    {
        if (winner != 0) yield break;

        Piece movingPiece = GetPieceAt(move.from.row, move.from.column);
        Piece targetPiece = GetPieceAt(move.to.row, move.to.column);

        if (movingPiece == null) yield break;

        movingPiece.row = move.to.row;
        movingPiece.column = move.to.column;

        // Figur bewegen – SMOOTH mit Animation!
        // WICHTIG: yield return wartet, bis Animation fertig ist!
        yield return StartCoroutine(MovePieceAnimated(movingPiece, move.to.row, move.to.column));

        if (targetPiece != null)
        {
            pieces.Remove(targetPiece.id);
            Destroy(targetPiece.gameObject);

            if (targetPiece.pieceType == PieceType.Shogun)
            {
                winner = currentPlayer;
                Debug.Log($"Spieler {winner} GEWINNT! Shogun geschlagen!");
                DeselectPiece();
                yield break;
            }
        }

        int newInterpretedVal = GetInterpretedGridValue(move.to.row, move.to.column, currentPlayer);
        movingPiece.UpdateRange(newInterpretedVal, movingPiece.pieceType);

        DeselectPiece();
        turnCount++;

        int nextPlayer = currentPlayer == 1 ? 2 : 1;

        // Notbremse für AI vs AI
        if (mode == GameMode.VsAI && aiVsAi && turnCount >= aiVsAiMaxTurns)
        {
            winner = nextPlayer; // willkürlich, damit es endet
            Debug.LogWarning($"AI-vs-AI Notbremse: max turns {aiVsAiMaxTurns} erreicht.");
            yield break;
        }

        int nextPlayerCount = GetPlayerPieceCount(nextPlayer);

        if (nextPlayerCount < 3)
        {
            winner = currentPlayer;
            Debug.Log($"Spieler {winner} GEWINNT! Armee des Gegners aufgerieben ({nextPlayerCount} Steine übrig)!");
            yield break;
        }

        if (!HasLegalMoves(nextPlayer))
        {
            bool inCheck = IsPlayerInCheck(nextPlayer);
            winner = currentPlayer;
            Debug.Log(inCheck ? $"Spieler {winner} GEWINNT! CHECKMATE!" : $"Spieler {winner} GEWINNT! Gegner ist TRAPPED!");
            yield break;
        }

        currentPlayer = nextPlayer;

        // Auto-KI Trigger:
        // - VsAI: wenn currentPlayer == aiPlayer
        // - AIvsAI: immer (solange winner==0)
        if (mode == GameMode.VsAI && winner == 0 && !aiIsThinking)
        {
            bool aiTurn = aiVsAi || (currentPlayer == aiPlayer);
            if (aiTurn) StartCoroutine(AIMakeMove());
        }
    }

    // =============================
    // AI
    // =============================
    private IEnumerator AIMakeMove()
    {
        aiIsThinking = true;
        yield return new WaitForSeconds(aiDelaySeconds);

        if (winner != 0)
        {
            aiIsThinking = false;
            yield break;
        }

        int aiToMove = currentPlayer;
        ValidMove best = GetBestMove(aiToMove, aiDepth);

        aiIsThinking = false;

        if (best != null)
            yield return StartCoroutine(ExecuteMove(best)); // GEÄNDERT: yield return!
        else
            Debug.LogWarning("KI: Kein Zug gefunden.");
    }


    private ValidMove GetBestMove(int forPlayer, int depth)
    {
        int opponent = forPlayer == 1 ? 2 : 1;

        int bestScore = int.MinValue;
        ValidMove bestMove = null;

        List<Piece> myPieces = pieces.Values.Where(p => p != null && p.owner == forPlayer).ToList();
        foreach (Piece p in myPieces)
        {
            List<ValidMove> moves = CalculateValidMoves(p);
            foreach (ValidMove m in moves)
            {
                SimulateMove(m, forPlayer);
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
        int terminalScore;
        if (IsTerminal(aiPerspective, out terminalScore)) return terminalScore;
        if (depth <= 0) return EvaluateBoard(aiPerspective);

        int opponent = playerToMove == 1 ? 2 : 1;

        List<Piece> piecesToMove = pieces.Values.Where(p => p != null && p.owner == playerToMove).ToList();
        bool hasAnyMove = false;

        if (maximizing)
        {
            int best = int.MinValue;

            foreach (Piece p in piecesToMove)
            {
                List<ValidMove> moves = CalculateValidMoves(p);
                foreach (ValidMove m in moves)
                {
                    hasAnyMove = true;
                    SimulateMove(m, playerToMove);
                    int score = Minimax(opponent, depth - 1, alpha, beta, false, aiPerspective);
                    UndoMove();

                    best = Mathf.Max(best, score);
                    alpha = Mathf.Max(alpha, score);
                    if (beta <= alpha) break;
                }
                if (beta <= alpha) break;
            }

            if (!hasAnyMove) return EvaluateNoMoves(playerToMove, aiPerspective);
            return best;
        }
        else
        {
            int best = int.MaxValue;

            foreach (Piece p in piecesToMove)
            {
                List<ValidMove> moves = CalculateValidMoves(p);
                foreach (ValidMove m in moves)
                {
                    hasAnyMove = true;
                    SimulateMove(m, playerToMove);
                    int score = Minimax(opponent, depth - 1, alpha, beta, true, aiPerspective);
                    UndoMove();

                    best = Mathf.Min(best, score);
                    beta = Mathf.Min(beta, score);
                    if (beta <= alpha) break;
                }
                if (beta <= alpha) break;
            }

            if (!hasAnyMove) return EvaluateNoMoves(playerToMove, aiPerspective);
            return best;
        }
    }

    private bool IsTerminal(int aiPerspective, out int score)
    {
        score = 0;
        int opp = aiPerspective == 1 ? 2 : 1;

        if (GetShogun(aiPerspective) == null) { score = -100000; return true; }
        if (GetShogun(opp) == null) { score = 100000; return true; }

        if (GetPlayerPieceCount(aiPerspective) < 3) { score = -90000; return true; }
        if (GetPlayerPieceCount(opp) < 3) { score = 90000; return true; }

        return false;
    }

    private int EvaluateNoMoves(int playerToMove, int aiPerspective)
    {
        // In deinem Spiel gewinnt der Gegner sowohl bei Checkmate als auch Trapped
        bool inCheck = IsPlayerInCheck(playerToMove);
        int sign = (playerToMove == aiPerspective) ? -1 : 1;
        return sign * (inCheck ? 80000 : 70000);
    }

    private int EvaluateBoard(int aiPerspective)
    {
        int opp = aiPerspective == 1 ? 2 : 1;

        int score = 0;

        score += GetPlayerPieceCount(aiPerspective) * 200;
        score -= GetPlayerPieceCount(opp) * 200;

        if (IsPlayerInCheck(aiPerspective)) score -= 500;
        if (IsPlayerInCheck(opp)) score += 500;

        return score;
    }

    // ---- Simulation nur Daten (keine Transforms) ----
    private void SimulateMove(ValidMove move, int player)
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

    // =============================
    // Public getters
    // =============================
    public List<ValidMove> GetValidMoves() => validMoves;
    public Piece GetSelectedPiece() => selectedPiece;
    public int GetWinner() => winner;
    public int GetCurrentPlayer() => currentPlayer;
    public int[,] GetGridValues() => gridValues;
    public BoardRenderer GetBoardRenderer() => boardRenderer;

    // =============================
    // ViewSwitcher Support
    // =============================

    [System.Serializable]
    public class PieceData
    {
        public int owner;
        public PieceType pieceType;
        public int row;
        public int column;
        public int range;
        public string id;
    }

    public List<PieceData> GetAllPieceData()
    {
        List<PieceData> data = new List<PieceData>();

        foreach (Piece p in pieces.Values)
        {
            if (p != null)
            {
                data.Add(new PieceData
                {
                    owner = p.owner,
                    pieceType = p.pieceType,
                    row = p.row,
                    column = p.column,
                    range = p.range,
                    id = p.id
                });
            }
        }

        return data;
    }

    public void DestroyAllPieces()
    {
        foreach (Piece p in pieces.Values.ToList())
        {
            if (p != null && p.gameObject != null)
                Destroy(p.gameObject);
        }
        pieces.Clear();
    }

    public void RespawnPieces(List<PieceData> pieceData)
    {
        foreach (PieceData data in pieceData)
        {
            GameObject prefab = null;

            // Prefab auswählen basierend auf Owner + Type
            if (data.owner == 1 && data.pieceType == PieceType.Soldier)
                prefab = p1NormalPrefab;
            else if (data.owner == 1 && data.pieceType == PieceType.Shogun)
                prefab = p1ShogunPrefab;
            else if (data.owner == 2 && data.pieceType == PieceType.Soldier)
                prefab = p2NormalPrefab;
            else if (data.owner == 2 && data.pieceType == PieceType.Shogun)
                prefab = p2ShogunPrefab;

            if (prefab == null)
            {
                Debug.LogError($"Kein Prefab für Owner {data.owner}, Type {data.pieceType}!");
                continue;
            }

            // Spawnen
            Vector3 worldPos = boardRenderer.GetWorldPosition(data.row, data.column);
            worldPos.z = -0.1f;

            GameObject pieceObj = Instantiate(prefab, worldPos, Quaternion.identity);
            Piece piece = pieceObj.GetComponent<Piece>();

            if (piece != null)
            {
                piece.owner = data.owner;
                piece.pieceType = data.pieceType;
                piece.row = data.row;
                piece.column = data.column;
                piece.gameManager = this;
                piece.range = data.range;
                piece.id = data.id;

                // Range-Display aktualisieren
                piece.UpdateRange(data.range, data.pieceType);

                pieces[piece.id] = piece;
            }
        }
    }

    private IEnumerator MovePieceAnimated(Piece piece, int targetRow, int targetCol)
    {
        isAnimating = true;

        // Starte die Bewegungsanimation
        yield return StartCoroutine(piece.MoveTo(targetRow, targetCol));

        // Animation fertig – erlaube wieder Input
        isAnimating = false;
    }

}
