using UnityEngine;  // Basis-Namespace für Unity-Funktionen (GameObject, MonoBehaviour, Debug, etc.)

// Dieses Script verwaltet die Spiellogik im Hintergrund.
// Es liegt auf dem GameObject "GameManager" in deiner Scene.
public class GameManager : MonoBehaviour
{
    // Anzahl der Reihen im Spielfeld (vertikal).
    // "public" bedeutet: Du kannst diesen Wert im Inspector anpassen.
    public int rows = 8;

    // Anzahl der Spalten im Spielfeld (horizontal).
    public int columns = 8;

    // 2D-Array für unser Spielfeld.
    // board[reihe, spalte]
    // Jeder Eintrag ist eine ganze Zahl (int),
    // die später einen bestimmten Magnet-Zustand beschreibt (0–4).
    public int[,] board = new int[8, 8];

    // Diese Funktion wird von Unity automatisch einmal aufgerufen,
    // wenn die Scene gestartet wird und dieses GameObject aktiv ist.
    void Start()
    {
        // Spielfeld vorbereiten:
        // 1. Alles auf 0 setzen.
        // 2. 16 zufällige Felder mit Wert 4 belegen.
        InitBoard();

        // Das Board als Text in der Konsole ausgeben,
        // damit wir zur Kontrolle sehen, wie es aussieht.
        PrintBoardToConsole();
    }

    // Diese Funktion richtet das Spielfeld ein.
    // Aktuell:
    // - Alle Felder bekommen zuerst den Wert 0.
    // - Danach werden genau 16 verschiedene Felder mit dem Wert 4 gefüllt.
    void InitBoard()
    {
        // Schritt 1: Alles auf 0 setzen.
        for (int r = 0; r < rows; r++)  // r = row (Reihe)
        {
            for (int c = 0; c < columns; c++)  // c = column (Spalte)
            {
                // An der Position [r, c] setzen wir den Wert auf 0.
                board[r, c] = 0;
            }
        }

        // Schritt 2: 16 Felder mit 4 belegen.
        int foursPlaced = 0;  // Zählt, wie viele 4er wir bereits gesetzt haben.

        // Solange wir noch nicht 16 Vierer gesetzt haben,
        // wiederholen wir die Schleife.
        while (foursPlaced < 16)
        {
            // Zufällige Reihe wählen (0 bis rows-1).
            int randomRow = Random.Range(0, rows);

            // Zufällige Spalte wählen (0 bis columns-1).
            int randomColumn = Random.Range(0, columns);

            // Nur wenn das Feld an dieser Stelle noch 0 ist,
            // dürfen wir hier eine 4 hinsetzen.
            if (board[randomRow, randomColumn] == 0)
            {
                // An dieser Position wird eine 4 eingetragen.
                board[randomRow, randomColumn] = 4;

                // Zähler erhöhen: wir haben einen weiteren Vierer gesetzt.
                foursPlaced++;
            }
            // Wenn dort schon eine 4 steht, passiert nichts
            // und die while-Schleife versucht es einfach mit neuen Zufallszahlen erneut.
        }
    }

    // Diese Funktion gibt das komplette Board in der Unity-Konsole aus.
    // Das ist sehr hilfreich zum Debuggen und Verstehen.
    void PrintBoardToConsole()
    {
        // Hier bauen wir einen Text, der das Board als Grid darstellt.
        string output = "Current Board:\n";

        // Schleife über jede Reihe.
        for (int r = 0; r < rows; r++)
        {
            // Text für die aktuelle Zeile.
            string rowText = "";

            // Schleife über jede Spalte in dieser Reihe.
            for (int c = 0; c < columns; c++)
            {
                // Wert an der Position [r, c] holen.
                int value = board[r, c];

                // Wert plus Leerzeichen an die Zeile anhängen.
                rowText += value + " ";
            }

            // Die fertige Zeile plus Zeilenumbruch an den Gesamttext anhängen.
            output += rowText + "\n";
        }

        // Den gesamten Text einmal in der Konsole ausgeben.
        Debug.Log(output);  // In der Console siehst du dann das 8x8-Grid.
    }
}
