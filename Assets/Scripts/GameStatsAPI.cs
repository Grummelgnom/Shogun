using UnityEngine;
using UnityEngine.Networking;
using System.Collections;

/// <summary>
/// Datenstruktur für ein Spiel - wird zu JSON konvertiert
/// </summary>
[System.Serializable]
public class GameStat
{
    public string gameMode;           // "PvP", "PvAI", "AIvAI"
    public int winner;                // 1 oder 2
    public int totalMoves;            // Anzahl Züge
    public int gameDuration;          // Sekunden
    public int player1PiecesLeft;     // Verbleibende Steine Spieler 1
    public int player2PiecesLeft;     // Verbleibende Steine Spieler 2
    public int aiDepth;               // KI-Schwierigkeit (0 = kein AI)
}

/// <summary>
/// Kommunikation mit der PHP-API
/// </summary>
public class GameStatsAPI : MonoBehaviour
{
    // WICHTIG: Hier deine lokale API-URL eintragen!
    private const string API_URL = "http://localhost/shogun/api.php";

    /// <summary>
    /// Speichert Spiel-Statistiken in der Datenbank
    /// Wird vom GameManager nach Spielende aufgerufen
    /// </summary>
    public void SaveGameStats(GameStat stats)
    {
        StartCoroutine(PostGameStats(stats));
    }

    /// <summary>
    /// Sendet POST-Request an API
    /// Coroutine = läuft asynchron, blockiert nicht das Spiel
    /// </summary>
    private IEnumerator PostGameStats(GameStat stats)
    {
        // 1. GameStat-Objekt zu JSON-String konvertieren
        string json = JsonUtility.ToJson(stats);
        Debug.Log("📤 Sende an API: " + json);

        // 2. HTTP-Request vorbereiten
        using (UnityWebRequest request = new UnityWebRequest(API_URL, "POST"))
        {
            // JSON-String in Bytes umwandeln
            byte[] bodyRaw = System.Text.Encoding.UTF8.GetBytes(json);

            // Request konfigurieren
            request.uploadHandler = new UploadHandlerRaw(bodyRaw);
            request.downloadHandler = new DownloadHandlerBuffer();
            request.SetRequestHeader("Content-Type", "application/json");

            // 3. Request senden und auf Antwort warten
            yield return request.SendWebRequest();

            // 4. Antwort verarbeiten
            if (request.result == UnityWebRequest.Result.Success)
            {
                Debug.Log("✅ Statistik gespeichert! Antwort: " + request.downloadHandler.text);
            }
            else
            {
                Debug.LogError("❌ Fehler beim Speichern: " + request.error);
                Debug.LogError("Details: " + request.downloadHandler.text);
            }
        }
    }

    /// <summary>
    /// Lädt letzte 10 Spiele aus der Datenbank (optional)
    /// Kann später für Statistik-Menü verwendet werden
    /// </summary>
    public void LoadStats()
    {
        StartCoroutine(GetGameStats());
    }

    private IEnumerator GetGameStats()
    {
        using (UnityWebRequest request = UnityWebRequest.Get(API_URL))
        {
            yield return request.SendWebRequest();

            if (request.result == UnityWebRequest.Result.Success)
            {
                Debug.Log("📊 Statistiken geladen: " + request.downloadHandler.text);
                // Hier könntest du die Daten parsen und in UI anzeigen
            }
            else
            {
                Debug.LogError("❌ Fehler beim Laden: " + request.error);
            }
        }
    }
}
