using UnityEngine;
using TMPro;

public class StatsUI : MonoBehaviour
{
    [Header("References")]
    public GameManager gameManager;

    [Header("UI Text Elements")]
    public TextMeshProUGUI textCurrentPlayer;
    public TextMeshProUGUI textTurnCount;
    public TextMeshProUGUI textPieceCount;
    public TextMeshProUGUI textGameTime;

    private float gameStartTime;
    private bool gameEnded = false;

    void Start()
    {
        gameStartTime = Time.time;
        UpdateStats();
    }

    void Update()
    {
        UpdateStats();
    }

    public void UpdateStats()
    {
        if (gameManager == null) return;

        // Current Player & Winner
        int currentPlayer = gameManager.GetCurrentPlayer();
        int winner = gameManager.GetWinner();

        if (winner != 0)
        {
            textCurrentPlayer.text = $"<color=yellow>*** Player {winner} WINS! ***</color>";
            gameEnded = true;
        }
        else
        {
            string playerColor = currentPlayer == 1 ? "#4DA6FF" : "#FF4D4D";
            textCurrentPlayer.text = $"<color={playerColor}>> Player {currentPlayer}</color> to move";
        }

        // Turn Count
        textTurnCount.text = $"Turn: {gameManager.turnCount}";

        // Piece Count
        int p1Count = gameManager.GetPlayerPieceCount(1);
        int p2Count = gameManager.GetPlayerPieceCount(2);
        textPieceCount.text = $"<color=#4DA6FF>P1: {p1Count}</color> | <color=#FF4D4D>P2: {p2Count}</color>";

        // Game Time
        if (textGameTime != null)
        {
            if (!gameEnded)
            {
                float elapsed = Time.time - gameStartTime;
                int minutes = Mathf.FloorToInt(elapsed / 60f);
                int seconds = Mathf.FloorToInt(elapsed % 60f);
                textGameTime.text = $"Time: {minutes}:{seconds:00}";
            }
        }
    }
}