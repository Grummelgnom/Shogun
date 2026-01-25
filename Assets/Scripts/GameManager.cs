using UnityEngine;  // Unity-Basisfunktionen

public class GameManager : MonoBehaviour
{
    // Anzahl der Reihen (vertikal) im Spielfeld.
    public int rows = 8;

    // Anzahl der Spalten (horizontal) im Spielfeld.
    public int columns = 8;

    // 2D-Array für unser Spielfeld.
    // board[reihe, spalte] speichert eine Zahl (0–4).
    public int[,] board = new int[8, 8];

    // Wird einmal aufgerufen, wenn die Scene startet.
    void Start()
    {
        // Spielfeld initial befüllen.
        InitBoard();

        // Spielfeld in der Konsole anzeigen,
        // damit wir sehen, ob alles so ist, wie wir denken.
        PrintBoardToConsole();
    }

    // Setzt aktuell alle Felder im Board auf 0.
    void InitBoard()
    {
        // Schleife über alle Reihen.
        for (int r = 0; r < rows; r++)
        {
            // Schleife über alle Spalten in dieser Reihe.
            for (int c = 0; c < columns; c++)
            {
                // Feld an Position (r, c) bekommt den Wert 0.
                board[r, c] = 0;
            }
        }

        // Hier kommt später die Logik für die 16 Felder mit Wert 4 dazu.
    }

    // Gibt das komplette Board als Text in der Unity-Konsole aus.
    void PrintBoardToConsole()
    {
        // Wir bauen uns Zeile für Zeile einen Text zusammen.
        string output = "Current Board:\n";

        // Wieder alle Reihen durchgehen.
        for (int r = 0; r < rows; r++)
        {
            // Start einer neuen Zeile im Text.
            string rowText = "";

            // Alle Spalten dieser Reihe durchgehen.
            for (int c = 0; c < columns; c++)
            {
                // Wert an Position (r, c) holen.
                int value = board[r, c];

                // Wert und ein Leerzeichen an den Zeilentext anhängen.
                rowText += value + " ";
            }

            // Die fertige Zeile plus Zeilenumbruch an die Gesamtausgabe anhängen.
            output += rowText + "\n";
        }

        // Jetzt den ganzen Text einmal in der Konsole ausgeben.
        Debug.Log(output);
    }
}
