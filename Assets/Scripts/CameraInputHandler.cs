using UnityEngine;

public class CameraInputHandler : MonoBehaviour
{
    public GameManager gameManager;

    void Update()
    {
        if (Input.GetMouseButtonDown(0)) // Linke Maustaste
        {
            Debug.Log("Mausklick erkannt!");

            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit;

            Debug.DrawRay(ray.origin, ray.direction * 100, Color.red, 2f); // Strahl sichtbar machen

            if (Physics.Raycast(ray, out hit))
            {
                Debug.Log("Raycast hat etwas getroffen: " + hit.collider.name);

                // Prüfe, ob wir ein Tile getroffen haben
                Tile tile = hit.collider.GetComponent<Tile>();
                if (tile != null)
                {
                    Debug.Log("Tile getroffen: " + tile.row + "," + tile.column);
                    gameManager.OnTileClicked(tile.row, tile.column);
                    return;
                }

                // Prüfe, ob wir eine Piece getroffen haben
                Piece piece = hit.collider.GetComponent<Piece>();
                if (piece != null)
                {
                    Debug.Log("Piece getroffen: " + piece.row + "," + piece.column);
                    gameManager.OnTileClicked(piece.row, piece.column);
                    return;
                }

                Debug.Log("Objekt hat weder Tile noch Piece Script");
            }
            else
            {
                Debug.Log("Raycast hat NICHTS getroffen!");
            }
        }
    }
}
