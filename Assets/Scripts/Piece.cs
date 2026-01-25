using UnityEngine;

// Dieses Script beschreibt eine einzelne Figur auf dem Brett.
public class Piece : MonoBehaviour
{
    // Welchem Spieler gehört diese Figur? 1 oder 2.
    public int owner;

    // Ist das der Shogun (Hauptfigur) oder eine normale Figur?
    public bool isShogun;

    // Aktuelle Position auf dem Board (r = Reihe, c = Spalte).
    public int row;
    public int column;

    // Referenz auf den GameManager, um Züge zu melden.
    public GameManager gameManager;

    // Später: Hier können wir Logik einbauen,
    // z.B. Hervorheben, wenn ausgewählt, oder erlaubte Züge anzeigen.
}
