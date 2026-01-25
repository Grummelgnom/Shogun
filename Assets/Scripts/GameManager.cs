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
    // Setzt alle Felder im Board auf 0
    // und verteilt anschließend genau 16 zufällige Felder mit dem Wert 4.
    void InitBoard()
    {
        // 1. Schritt: Alles auf 0 setzen.
        for (int r = 0; r < rows; r++)
        {
            for (int c = 0; c < columns; c++)
            {
                board[r, c] = 0;
            }
        }

        // 2. Schritt: Genau 16 Felder mit der 4 belegen.
        // Wir zählen mit, wie viele 4er wir schon gesetzt haben.
        int foursPlaced = 0;

        // Solange wir noch nicht 16 Stück gesetzt haben, wiederholen wir die Schleife.
        while (foursPlaced < 16)
        {
            // Zufällige Reihe zwischen 0 und rows-1.
            int randomRow = Random.Range(0, rows);

            // Zufällige Spalte zwischen 0 und columns-1.
            int randomColumn = Random.Range(0, columns);

            // Nur wenn an dieser Stelle noch eine 0 steht,
            // dürfen wir hier eine 4 setzen.
            if (board[randomRow, randomColumn] == 0)
            {
                board[randomRow, randomColumn] = 4;

                // Wir erhöhen den Zähler, weil wir eine weitere 4 gesetzt haben.
                foursPlaced++;
            }
            // Wenn dort schon eine 4 steht, machen wir einfach eine neue Runde,
            // bis wir 16 verschiedene Felder gefunden haben.
        }
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
