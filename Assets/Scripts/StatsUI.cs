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
            textCurrentPlayer.text = $"<color=yellow>*** Spieler {winner} GEWINNT! ***</color>"; // ← Kein Emoji
            gameEnded = true;
        }
        else
        {
            // Farben: P1 = Blau, P2 = Rot
            string playerColor = currentPlayer == 1 ? "#4DA6FF" : "#FF4D4D";
            textCurrentPlayer.text = $"<color={playerColor}>> Spieler {currentPlayer}</color> am Zug"; // ← > statt ▶
        }

        // Turn Count
        textTurnCount.text = $"Zug: {gameManager.turnCount}"; // ← Kein Emoji

        // Piece Count mit Farben
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
                textGameTime.text = $"Zeit: {minutes}:{seconds:00}"; // ← Kein Emoji
            }
        }
    }

}
