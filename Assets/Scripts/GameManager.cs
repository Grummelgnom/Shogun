using UnityEngine;  // Hier holen wir uns Unity-Funktionen, die wir brauchen

// Diese Klasse steuert unser Spiel.
// "MonoBehaviour" bedeutet: Dieses Script kann an ein GameObject gehängt werden
// und Unity kann dann bestimmte Funktionen automatisch aufrufen (z.B. Start).
public class GameManager : MonoBehaviour
{
    // Wie viele Reihen unser Spielfeld haben soll (von oben nach unten).
    public int rows = 8;

    // Wie viele Spalten unser Spielfeld haben soll (von links nach rechts).
    public int columns = 8;

    // Das eigentliche Spielfeld als 2D-Array (Tabelle).
    // Jeder Eintrag ist eine ganze Zahl (int) und steht für einen Zustand:
    // 0, 1, 2, 3 oder 4 (wir werden später festlegen, was genau was bedeutet).
    // board[reihe, spalte]
    public int[,] board = new int[8, 8];

    // Die Start-Funktion wird von Unity einmal aufgerufen,
    // wenn die Scene startet und dieses GameObject aktiv ist.
    void Start()
    {
        // Hier sagen wir: Wenn das Spiel startet, soll das Spielfeld vorbereitet werden.
        InitBoard();
    }

    // Diese Funktion richtet das Spielfeld ein.
    // Im Moment setzen wir einfach alle Werte auf 0.
    void InitBoard()
    {
        // Wir gehen jede Reihe durch.
        for (int r = 0; r < rows; r++)
        {
            // Und in jeder Reihe gehen wir jede Spalte durch.
            for (int c = 0; c < columns; c++)
            {
                // An dieser Stelle (r = Reihe, c = Spalte) setzen wir den Wert auf 0.
                board[r, c] = 0;
            }
        }

        // Später werden wir hier noch Logik ergänzen,
        // um die 16 Felder mit dem Wert 4 zu platzieren.
    }
}
