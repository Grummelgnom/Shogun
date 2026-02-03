using UnityEngine;

public class ViewSwitcher : MonoBehaviour
{
    [Header("References")]
    public Camera mainCamera;
    public GameManager gameManager;
    public BoardRenderer boardRenderer;

    [Header("2D Prefabs")]
    public GameObject tile2DPrefab;
    public GameObject p1Normal2DPrefab;
    public GameObject p1Shogun2DPrefab;
    public GameObject p2Normal2DPrefab;
    public GameObject p2Shogun2DPrefab;

    [Header("3D Prefabs")]
    public GameObject tile3DPrefab;
    public GameObject p1Normal3DPrefab;
    public GameObject p1Shogun3DPrefab;
    public GameObject p2Normal3DPrefab;
    public GameObject p2Shogun3DPrefab;

    [Header("Camera Settings")]
    public Vector3 camera2DPosition = new Vector3(0, 0, -10);
    public Vector3 camera2DRotation = new Vector3(0, 0, 0);
    public Vector3 camera3DPosition = new Vector3(0, 10, -10);
    public Vector3 camera3DRotation = new Vector3(45, 0, 0);

    [Header("Tile Spacing")]
    public float spacing2D = 1.0f;
    public float spacing3D = 1.15f;

    private bool is3DMode = false;

    void Start()
    {
        // Starte im 2D-Modus
        SwitchTo2D();
    }

    // Öffentliche Methode für UI-Button
    public void ToggleView()
    {
        // Blockiere Wechsel während Animationen oder AI-Zügen
        if (gameManager.isAnimating)
        {
            Debug.Log("⚠️ Warte bis Bewegung abgeschlossen ist!");
            return;
        }

        // Blockiere während AI nachdenkt (VsAI/AIvAI)
        if (gameManager.mode == GameMode.VsAI && gameManager.aiIsThinking)
        {
            Debug.Log("⚠️ Warte bis KI fertig ist!");
            return;
        }

        if (is3DMode)
            SwitchTo2D();
        else
            SwitchTo3D();
    }


    void SwitchTo2D()
    {
        is3DMode = false;
        Debug.Log("Wechsle zu 2D...");

        // Kamera umstellen
        mainCamera.orthographic = true;
        mainCamera.orthographicSize = 5f;
        mainCamera.transform.position = camera2DPosition;
        mainCamera.transform.eulerAngles = camera2DRotation;

        // Prefabs + Spacing umstellen
        boardRenderer.tilePrefab = tile2DPrefab;
        boardRenderer.tileSpacing = spacing2D;
        gameManager.p1NormalPrefab = p1Normal2DPrefab;
        gameManager.p1ShogunPrefab = p1Shogun2DPrefab;
        gameManager.p2NormalPrefab = p2Normal2DPrefab;
        gameManager.p2ShogunPrefab = p2Shogun2DPrefab;

        // Board + Figuren neu aufbauen
        RebuildScene();
    }

    void SwitchTo3D()
    {
        is3DMode = true;
        Debug.Log("Wechsle zu 3D...");

        // Kamera umstellen
        mainCamera.orthographic = false;
        mainCamera.fieldOfView = 60f;
        mainCamera.transform.position = camera3DPosition;
        mainCamera.transform.eulerAngles = camera3DRotation;

        // Prefabs + Spacing umstellen
        boardRenderer.tilePrefab = tile3DPrefab;
        boardRenderer.tileSpacing = spacing3D;
        gameManager.p1NormalPrefab = p1Normal3DPrefab;
        gameManager.p1ShogunPrefab = p1Shogun3DPrefab;
        gameManager.p2NormalPrefab = p2Normal3DPrefab;
        gameManager.p2ShogunPrefab = p2Shogun3DPrefab;

        // Board + Figuren neu aufbauen
        RebuildScene();
    }

    void RebuildScene()
    {
        // Alle Pieces speichern (Position, Owner, Type, Range)
        var pieceData = gameManager.GetAllPieceData();

        // Aktuellen Spielstand speichern
        int currentPlayer = gameManager.GetCurrentPlayer();
        int[,] gridValues = gameManager.GetGridValues();

        // Altes Board + Figuren löschen
        boardRenderer.DestroyBoard();
        gameManager.DestroyAllPieces();

        // Neues Board aufbauen
        boardRenderer.CreateBoardVisuals();

        // Figuren mit gespeicherten Daten neu spawnen
        gameManager.RespawnPieces(pieceData);

        // Spielstand wiederherstellen
        gameManager.currentPlayer = currentPlayer;

        Debug.Log("Scene rebuilt in " + (is3DMode ? "3D" : "2D") + " mode!");
    }
}
