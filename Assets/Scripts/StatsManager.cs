using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Networking;
using TMPro;
using System.Collections;
using System.Collections.Generic;

/// <summary>
/// Datenstruktur für API-Antwort mit mehreren Spielen
/// </summary>
[System.Serializable]
public class GamesResponse
{
    public bool success;
    public int count;
    public List<GameStatFromDB> games;
}

/// <summary>
/// Ein einzelnes Spiel aus der Datenbank
/// </summary>
[System.Serializable]
public class GameStatFromDB
{
    public string id;
    public string game_mode;
    public string winner;
    public string total_moves;
    public string game_duration;
    public string player1_pieces_left;
    public string player2_pieces_left;
    public string ai_depth;
    public string created_at;
}

/// <summary>
/// Verwaltet die Statistik-Anzeige im Hauptmenü
/// </summary>
public class StatsManager : MonoBehaviour
{
    [Header("UI References")]
    [Tooltip("Das Haupt-Stats-Panel (wird ein/ausgeblendet)")]
    public GameObject statsPanel;

    [Tooltip("Der Content-Container im Scroll View")]
    public Transform scrollViewContent;

    [Tooltip("Text für die Zusammenfassung unten")]
    public TextMeshProUGUI summaryText;

    [Header("Prefabs")]
    [Tooltip("Prefab für einen einzelnen Spiel-Eintrag")]
    public GameObject gameEntryPrefab;

    [Header("API")]
    private const string API_URL = "http://localhost/shogun/api.php";

    // Gespeicherte Daten
    private List<GameStatFromDB> loadedGames = new List<GameStatFromDB>();

    /// <summary>
    /// Stats-Panel öffnen und Daten laden
    /// </summary>
    public void OpenStats()
    {
        if (statsPanel != null)
        {
            statsPanel.SetActive(true);
            RefreshStats();
        }
    }

    /// <summary>
    /// Stats-Panel schließen
    /// </summary>
    public void CloseStats()
    {
        if (statsPanel != null)
        {
            statsPanel.SetActive(false);
        }
    }

    /// <summary>
    /// Statistiken neu laden (wird von Refresh-Button aufgerufen)
    /// </summary>
    public void RefreshStats()
    {
        StartCoroutine(LoadStatsFromAPI());
    }

    /// <summary>
    /// Lädt Statistiken von der API
    /// </summary>
    private IEnumerator LoadStatsFromAPI()
    {
        summaryText.text = "Lade Statistiken...";

        using (UnityWebRequest request = UnityWebRequest.Get(API_URL))
        {
            yield return request.SendWebRequest();

            if (request.result == UnityWebRequest.Result.Success)
            {
                Debug.Log("📊 API-Antwort: " + request.downloadHandler.text);

                // JSON parsen
                GamesResponse response = JsonUtility.FromJson<GamesResponse>(request.downloadHandler.text);

                if (response != null && response.success)
                {
                    loadedGames = response.games ?? new List<GameStatFromDB>();
                    DisplayGames();
                    UpdateSummary();
                }
                else
                {
                    summaryText.text = "Fehler beim Laden der Daten.";
                }
            }
            else
            {
                Debug.LogError("❌ API-Fehler: " + request.error);
                summaryText.text = "Verbindung zur Datenbank fehlgeschlagen.\n" + request.error;
            }
        }
    }

    /// <summary>
    /// Zeigt alle Spiele in der Scroll View an
    /// </summary>
    private void DisplayGames()
    {
        if (scrollViewContent == null)
        {
            Debug.LogError("❌ scrollViewContent ist NULL! Bitte im Inspector zuweisen!");
            return;
        }

        Debug.Log($"✅ scrollViewContent OK: {scrollViewContent.name}");
        Debug.Log($"📊 Anzahl Spiele: {loadedGames.Count}");

        // Alte Einträge löschen
        foreach (Transform child in scrollViewContent)
        {
            Destroy(child.gameObject);
        }

        if (loadedGames.Count == 0)
        {
            CreateEmptyEntry("Noch keine Spiele gespielt.");
            return;
        }

        // Für jedes Spiel einen Eintrag erstellen
        foreach (GameStatFromDB game in loadedGames)
        {
            if (game == null)
            {
                Debug.LogWarning("⚠️ Spiel-Objekt ist NULL, überspringe...");
                continue;
            }

            Debug.Log($"Erstelle Eintrag für Spiel ID: {game.id}");
            CreateGameEntry(game);
        }

        Debug.Log("✅ Alle Einträge erstellt!");
    }

    /// <summary>
    /// Erstellt einen einzelnen Spiel-Eintrag
    /// </summary>
    private void CreateGameEntry(GameStatFromDB game)
    {
        // Haupt-Container mit Hintergrund
        GameObject entry = new GameObject("GameEntry_" + game.id);
        entry.transform.SetParent(scrollViewContent, false);

        // Layout
        RectTransform rt = entry.AddComponent<RectTransform>();
        rt.sizeDelta = new Vector2(720, 90);

        // Hintergrund mit schöner Farbe
        Image bg = entry.AddComponent<Image>();
        bg.color = new Color(0.15f, 0.15f, 0.2f, 0.95f);

        // === Text Container als Child ===
        GameObject textObj = new GameObject("Text");
        textObj.transform.SetParent(entry.transform, false);

        RectTransform textRt = textObj.AddComponent<RectTransform>();
        textRt.anchorMin = new Vector2(0, 0);
        textRt.anchorMax = new Vector2(1, 1);
        textRt.offsetMin = new Vector2(15, 10);
        textRt.offsetMax = new Vector2(-15, -10);

        TextMeshProUGUI text = textObj.AddComponent<TextMeshProUGUI>();
        text.fontSize = 18;
        text.alignment = TextAlignmentOptions.TopLeft;
        text.color = new Color(0.95f, 0.95f, 0.95f, 1f);
        text.enableWordWrapping = true;

        // === Inhalt formatieren (nur ASCII-Zeichen) ===
        string winnerText = $"<color=#FFD700>Spieler {game.winner}</color>";
        string modeText = GetModeDisplayName(game.game_mode);
        string dateText = FormatDate(game.created_at);

        text.text = $"<size=22><b>{modeText}</b></size>\n" +
                   $"<color=#90EE90>> {winnerText} gewonnen</color>\n" +
                   $"<size=14><color=#CCCCCC>Züge: {game.total_moves}  |  Dauer: {game.game_duration}s  |  {dateText}</color></size>";

        Debug.Log($"✅ Eintrag erstellt: {game.id}");
    }

    /// <summary>
    /// Erstellt Platzhalter-Eintrag wenn keine Daten
    /// </summary>
    private void CreateEmptyEntry(string message)
    {
        GameObject entry = new GameObject("EmptyEntry");
        entry.transform.SetParent(scrollViewContent, false);

        RectTransform rt = entry.AddComponent<RectTransform>();
        rt.sizeDelta = new Vector2(700, 60);

        TextMeshProUGUI text = entry.AddComponent<TextMeshProUGUI>();
        text.fontSize = 18;
        text.alignment = TextAlignmentOptions.Center;
        text.color = Color.gray;
        text.text = message;
    }

    /// <summary>
    /// Aktualisiert die Zusammenfassung unten
    /// </summary>
    private void UpdateSummary()
    {
        if (loadedGames.Count == 0)
        {
            summaryText.text = "<i>Noch keine Statistiken verfügbar.</i>";
            return;
        }

        int totalGames = loadedGames.Count;
        int p1Wins = 0;
        int p2Wins = 0;
        int totalMoves = 0;

        foreach (GameStatFromDB game in loadedGames)
        {
            if (game.winner == "1") p1Wins++;
            else if (game.winner == "2") p2Wins++;

            if (int.TryParse(game.total_moves, out int moves))
            {
                totalMoves += moves;
            }
        }

        float avgMoves = totalGames > 0 ? (float)totalMoves / totalGames : 0;

        // Schönere Formatierung nur mit ASCII-Zeichen
        summaryText.text =
            $"<size=16><color=#FFD700>==============================</color></size>\n" +
            $"<b>Gesamt:</b> {totalGames} {(totalGames == 1 ? "Spiel" : "Spiele")}  |  " +
            $"<b>Durchschn. Züge:</b> <color=#90EE90>{avgMoves:F1}</color>\n" +
            $"<b>Siege:</b> <color=#87CEEB>Spieler 1: {p1Wins}</color>  |  <color=#FF6B6B>Spieler 2: {p2Wins}</color>";
    }

    /// <summary>
    /// Formatiert Spielmodus für Anzeige
    /// </summary>
    private string GetModeDisplayName(string mode)
    {
        switch (mode)
        {
            case "PvP": return "Spieler vs Spieler";
            case "PvAI": return "Spieler vs Computer";
            case "AIvAI": return "Computer vs Computer";
            default: return mode;
        }
    }

    /// <summary>
    /// Formatiert Datum für Anzeige
    /// </summary>
    private string FormatDate(string dateTime)
    {
        // Format: "2026-02-16 09:30:00" -> "16.02.2026 09:30"
        if (string.IsNullOrEmpty(dateTime)) return "Unbekannt";

        try
        {
            string[] parts = dateTime.Split(' ');
            if (parts.Length >= 2)
            {
                string[] dateParts = parts[0].Split('-');
                string[] timeParts = parts[1].Split(':');

                if (dateParts.Length >= 3 && timeParts.Length >= 2)
                {
                    return $"{dateParts[2]}.{dateParts[1]}.{dateParts[0]} {timeParts[0]}:{timeParts[1]}";
                }
            }
        }
        catch
        {
            // Falls Parsing fehlschlägt, Original zurückgeben
        }

        return dateTime;
    }
}
