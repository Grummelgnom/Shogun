using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Networking;
using TMPro;
using System.Collections;
using System.Collections.Generic;

[System.Serializable]
public class GamesResponse
{
    public bool success;
    public int count;
    public List<GameStatFromDB> games;
}

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
    public string winner_name; // NEU - nur Gewinnername
}

public class StatsManager : MonoBehaviour
{
    [Header("UI References")]
    public GameObject statsPanel;
    public Transform scrollViewContent;
    public TextMeshProUGUI summaryText;

    [Header("Visual Effects")]
    public GameObject blurOverlay;
    public Button[] menuButtons;

    [Header("Edit Dialog - NUR GEWINNER")]
    public GameObject editDialog;
    public TMP_InputField winnerInput; // Nur 1 Input!

    [Header("API")]
    private const string API_URL = "http://localhost/shogun/api.php";

    private List<GameStatFromDB> loadedGames = new List<GameStatFromDB>();
    private int currentEditingGameId = -1;

    public void OpenStats()
    {
        if (statsPanel != null)
        {
            statsPanel.SetActive(true);
            RefreshStats();
        }

        if (blurOverlay != null)
        {
            blurOverlay.SetActive(true);
        }

        SetMenuButtonsInteractable(false);
    }

    public void CloseStats()
    {
        if (statsPanel != null)
        {
            statsPanel.SetActive(false);
        }

        if (blurOverlay != null)
        {
            blurOverlay.SetActive(false);
        }

        SetMenuButtonsInteractable(true);
    }

    private void SetMenuButtonsInteractable(bool interactable)
    {
        if (menuButtons == null || menuButtons.Length == 0) return;

        foreach (Button btn in menuButtons)
        {
            if (btn != null)
            {
                btn.interactable = interactable;
            }
        }
    }

    public void RefreshStats()
    {
        StartCoroutine(LoadStatsFromAPI());
    }

    private IEnumerator LoadStatsFromAPI()
    {
        summaryText.text = "Lade Statistiken...";

        using (UnityWebRequest request = UnityWebRequest.Get(API_URL))
        {
            yield return request.SendWebRequest();

            if (request.result == UnityWebRequest.Result.Success)
            {
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

    private void DisplayGames()
    {
        if (scrollViewContent == null) return;

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

        foreach (GameStatFromDB game in loadedGames)
        {
            if (game == null) continue;
            CreateGameEntry(game);
        }
    }

    private void CreateGameEntry(GameStatFromDB game)
    {
        GameObject entry = new GameObject("GameEntry_" + game.id);
        entry.transform.SetParent(scrollViewContent, false);

        RectTransform rt = entry.AddComponent<RectTransform>();
        rt.sizeDelta = new Vector2(720, 90);

        Image bg = entry.AddComponent<Image>();
        bg.color = new Color(0.15f, 0.15f, 0.2f, 0.95f);

        // === TEXT ===
        GameObject textObj = new GameObject("Text");
        textObj.transform.SetParent(entry.transform, false);

        RectTransform textRt = textObj.AddComponent<RectTransform>();
        textRt.anchorMin = new Vector2(0, 0);
        textRt.anchorMax = new Vector2(1, 1);
        textRt.offsetMin = new Vector2(15, 10);
        textRt.offsetMax = new Vector2(-110, -10);

        TextMeshProUGUI text = textObj.AddComponent<TextMeshProUGUI>();
        text.fontSize = 18;
        text.alignment = TextAlignmentOptions.TopLeft;
        text.color = new Color(0.95f, 0.95f, 0.95f, 1f);
        text.enableWordWrapping = true;

        // === GEWINNER-NAME (editierbar) ===
        string winnerName = string.IsNullOrEmpty(game.winner_name)
            ? $"Spieler {game.winner}"
            : game.winner_name;

        string modeText = GetModeDisplayName(game.game_mode);
        string dateText = FormatDate(game.created_at);

        text.text = $"{modeText}\n" +
                    $"> <color=#4CAF50>{winnerName}</color> gewonnen\n" +
                    $"Züge: {game.total_moves} | Dauer: {game.game_duration}s | {dateText}";

        // === EDIT-BUTTON ===
        GameObject editBtn = new GameObject("EditButton");
        editBtn.transform.SetParent(entry.transform, false);

        RectTransform editBtnRt = editBtn.AddComponent<RectTransform>();
        editBtnRt.anchorMin = new Vector2(1, 0.5f);
        editBtnRt.anchorMax = new Vector2(1, 0.5f);
        editBtnRt.pivot = new Vector2(1, 0.5f);
        editBtnRt.anchoredPosition = new Vector2(-60, 0);
        editBtnRt.sizeDelta = new Vector2(40, 40);

        Image editBtnImg = editBtn.AddComponent<Image>();
        editBtnImg.color = new Color(0.2f, 0.6f, 0.8f, 0.8f);

        Button editBtnComponent = editBtn.AddComponent<Button>();

        GameObject editTextObj = new GameObject("Text");
        editTextObj.transform.SetParent(editBtn.transform, false);

        RectTransform editTextRt = editTextObj.AddComponent<RectTransform>();
        editTextRt.anchorMin = Vector2.zero;
        editTextRt.anchorMax = Vector2.one;
        editTextRt.offsetMin = Vector2.zero;
        editTextRt.offsetMax = Vector2.zero;

        TextMeshProUGUI editBtnText = editTextObj.AddComponent<TextMeshProUGUI>();
        editBtnText.text = "✎";
        editBtnText.fontSize = 24;
        editBtnText.fontStyle = FontStyles.Bold;
        editBtnText.alignment = TextAlignmentOptions.Center;
        editBtnText.color = Color.white;

        int gameIdToEdit = int.Parse(game.id);
        editBtnComponent.onClick.AddListener(() => {
            OpenEditDialog(game);
        });

        // === DELETE-BUTTON ===
        GameObject deleteBtn = new GameObject("DeleteButton");
        deleteBtn.transform.SetParent(entry.transform, false);

        RectTransform btnRt = deleteBtn.AddComponent<RectTransform>();
        btnRt.anchorMin = new Vector2(1, 0.5f);
        btnRt.anchorMax = new Vector2(1, 0.5f);
        btnRt.pivot = new Vector2(1, 0.5f);
        btnRt.anchoredPosition = new Vector2(-10, 0);
        btnRt.sizeDelta = new Vector2(40, 40);

        Image btnImg = deleteBtn.AddComponent<Image>();
        btnImg.color = new Color(0.8f, 0.2f, 0.2f, 0.8f);

        Button btn = deleteBtn.AddComponent<Button>();

        GameObject btnTextObj = new GameObject("Text");
        btnTextObj.transform.SetParent(deleteBtn.transform, false);

        RectTransform btnTextRt = btnTextObj.AddComponent<RectTransform>();
        btnTextRt.anchorMin = Vector2.zero;
        btnTextRt.anchorMax = Vector2.one;
        btnTextRt.offsetMin = Vector2.zero;
        btnTextRt.offsetMax = Vector2.zero;

        TextMeshProUGUI btnText = btnTextObj.AddComponent<TextMeshProUGUI>();
        btnText.text = "X";
        btnText.fontSize = 24;
        btnText.fontStyle = FontStyles.Bold;
        btnText.alignment = TextAlignmentOptions.Center;
        btnText.color = Color.white;

        int gameIdToDelete = int.Parse(game.id);
        btn.onClick.AddListener(() => {
            DeleteGame(gameIdToDelete);
        });
    }

    /// <summary>
    /// Öffnet Edit-Dialog - NUR für Gewinner-Namen
    /// </summary>
    private void OpenEditDialog(GameStatFromDB game)
    {
        if (editDialog == null)
        {
            Debug.LogError("❌ editDialog ist NULL!");
            return;
        }

        currentEditingGameId = int.Parse(game.id);

        // Input vorausfüllen
        if (winnerInput != null)
        {
            string currentName = string.IsNullOrEmpty(game.winner_name)
                ? $"Spieler {game.winner}"
                : game.winner_name;

            winnerInput.text = currentName;
        }

        editDialog.SetActive(true);
        Debug.Log($"✎ Edit-Dialog geöffnet für Spiel ID: {currentEditingGameId}");
    }

    /// <summary>
    /// Speichert den neuen Gewinner-Namen
    /// </summary>
    public void SaveEditedName()
    {
        if (currentEditingGameId == -1) return;

        string newName = winnerInput != null ? winnerInput.text : "";

        if (string.IsNullOrEmpty(newName))
        {
            Debug.LogWarning("⚠️ Name ist leer!");
            return;
        }

        StartCoroutine(UpdateWinnerName(currentEditingGameId, newName));
    }

    private IEnumerator UpdateWinnerName(int gameId, string winnerName)
    {
        string json = "{\"id\":" + gameId + ",\"winnerName\":\"" + winnerName + "\"}";

        Debug.Log("📝 PUT Request: " + json);

        using (UnityWebRequest request = new UnityWebRequest(API_URL, "PUT"))
        {
            byte[] bodyRaw = System.Text.Encoding.UTF8.GetBytes(json);
            request.uploadHandler = new UploadHandlerRaw(bodyRaw);
            request.downloadHandler = new DownloadHandlerBuffer();
            request.SetRequestHeader("Content-Type", "application/json");

            yield return request.SendWebRequest();

            if (request.result == UnityWebRequest.Result.Success)
            {
                Debug.Log("✅ Name aktualisiert!");
                CloseEditDialog();
                RefreshStats();
            }
            else
            {
                Debug.LogError("❌ Fehler: " + request.error);
            }
        }
    }

    public void CloseEditDialog()
    {
        if (editDialog != null)
        {
            editDialog.SetActive(false);
        }

        currentEditingGameId = -1;
    }

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

    private void UpdateSummary()
    {
        if (loadedGames.Count == 0)
        {
            summaryText.text = "Noch keine Statistiken verfügbar.";
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

        summaryText.text =
            $"==============================\n" +
            $"Gesamt: {totalGames} {(totalGames == 1 ? "Spiel" : "Spiele")} | " +
            $"Durchschn. Züge: {avgMoves:F1}\n" +
            $"Siege: Spieler 1: {p1Wins} | Spieler 2: {p2Wins}";
    }

    public void DeleteGame(int gameId)
    {
        StartCoroutine(DeleteGameFromAPI(gameId));
    }

    private IEnumerator DeleteGameFromAPI(int gameId)
    {
        string deleteUrl = API_URL + "?id=" + gameId;

        using (UnityWebRequest request = UnityWebRequest.Delete(deleteUrl))
        {
            request.downloadHandler = new DownloadHandlerBuffer();
            yield return request.SendWebRequest();

            if (request.result == UnityWebRequest.Result.Success)
            {
                RefreshStats();
            }
            else
            {
                Debug.LogError($"❌ Fehler beim Löschen: {request.error}");
            }
        }
    }

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

    private string FormatDate(string dateTime)
    {
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
        catch { }

        return dateTime;
    }
}
