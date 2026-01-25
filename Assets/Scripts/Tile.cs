using UnityEngine;

// Dieses Script kommt auf JEDE einzelne Kachel.
// Es merkt sich ihre Position im Board (r, c)
// und reagiert auf einen Mausklick.
public class Tile : MonoBehaviour
{
    // Position im Board (Reihe und Spalte).
    public int row;
    public int column;

    // Referenz auf den GameManager, falls wir später dort etwas melden wollen.
    public GameManager gameManager;

    // Diese Funktion wird von Unity aufgerufen,
    // wenn du mit der Maus auf das Objekt klickst (im 2D-Spiel mit Collider).
    void OnMouseDown()
    {
        // Einfacher Debug-Text, damit du siehst, was passiert.
        Debug.Log($"Tile geklickt: Row = {row}, Column = {column}");

        // Später können wir hier z.B. den Zustand im Board ändern
        // oder einen Zug auslösen.
        SpriteRenderer sr = GetComponent<SpriteRenderer>();
        if (sr != null)
        {
            sr.color = Color.yellow; // nur zum Test
        }

    }
}
