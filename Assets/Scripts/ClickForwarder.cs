using UnityEngine;

public class ClickForwarder : MonoBehaviour
{
    void OnMouseDown()
    {
        // Leite den Klick an den Parent weiter
        Piece parentPiece = GetComponentInParent<Piece>();
        if (parentPiece != null)
        {
            parentPiece.OnMouseDown();
        }
    }
}
