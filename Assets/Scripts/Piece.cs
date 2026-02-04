using UnityEngine;
using System.Collections;

public class Piece : MonoBehaviour
{
    [Header("Game Data")]
    public int owner; // 1 = P1, 2 = P2
    public PieceType pieceType;
    public int row;
    public int column;
    public int range;
    public string id;
    public GameManager gameManager;

    [Header("Visual")]
    private SpriteRenderer spriteRenderer;
    private MeshRenderer meshRenderer;
    private Color originalColor;
    private bool isSelected = false;

    [Header("Range Display")]
    private TextMesh rangeText;

    [Header("Smooth Movement")]
    public float moveSpeed = 8f;
    public float hoverHeight = 0.3f;
    private bool isMoving = false;

    [Header("Board Reference")]
    public BoardRenderer boardRenderer;

    [Header("Hint System")]
    private bool isHinted = false;
    private Color hintColor = new Color(0.2f, 1f, 0.2f, 1f);

    void Start()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        if (spriteRenderer != null)
        {
            originalColor = spriteRenderer.color;
        }
        else
        {
            meshRenderer = GetComponent<MeshRenderer>();
            if (meshRenderer != null)
            {
                originalColor = meshRenderer.material.color;
            }
        }

        // BoardRenderer automatisch finden falls nicht zugewiesen
        if (boardRenderer == null && gameManager != null)
        {
            boardRenderer = gameManager.boardRenderer;
        }

        CreateRangeDisplay();
    }

    void OnMouseDown()
    {
        if (gameManager == null) return;
        if (this == null || gameObject == null) return;

        if (!gameManager.IsPieceActive(this))
        {
            Debug.LogWarning($"Klick auf veraltete Figur {id} ignoriert!");
            return;
        }

        gameManager.OnTileClicked(row, column);
    }

    public void SetSelected(bool selected)
    {
        isSelected = selected;
        UpdateVisualState();
    }

    public void SetHintHighlight(bool active)
    {
        isHinted = active;
        UpdateVisualState();
    }

    private void UpdateVisualState()
    {
        if (spriteRenderer != null)
        {
            if (isHinted)
                spriteRenderer.color = hintColor;
            else if (isSelected)
                spriteRenderer.color = new Color(1f, 0.6f, 0f); // Orange statt Weiß
            else
                spriteRenderer.color = originalColor;
        }
        else if (meshRenderer != null)
        {
            if (isHinted)
                meshRenderer.material.color = hintColor;
            else if (isSelected)
                meshRenderer.material.color = new Color(1f, 0.6f, 0f); // Orange statt Weiß
            else
                meshRenderer.material.color = originalColor;
        }
    }


    public void UpdateRange(int interpretedGridValue, PieceType type)
    {
        if (type == PieceType.Shogun)
        {
            range = (interpretedGridValue >= 3) ? 2 : 1;
        }
        else
        {
            range = Mathf.Clamp(interpretedGridValue, 1, 4);
        }

        Debug.Log($"Piece {id} - Range aktualisiert: {range} (basierend auf GridValue {interpretedGridValue})");
        UpdateRangeDisplay();
    }

    void CreateRangeDisplay()
    {
        GameObject textObj = new GameObject("RangeText");
        textObj.transform.parent = this.transform;

        if (spriteRenderer != null)
        {
            textObj.transform.localPosition = new Vector3(0, 0, -0.1f);
        }
        else
        {
            textObj.transform.localPosition = new Vector3(0, 0, -0.5f);
        }

        textObj.transform.localRotation = Quaternion.identity;

        rangeText = textObj.AddComponent<TextMesh>();
        rangeText.text = range.ToString();
        rangeText.fontSize = 50;
        rangeText.color = Color.white;
        rangeText.anchor = TextAnchor.MiddleCenter;
        rangeText.alignment = TextAlignment.Center;
        rangeText.characterSize = 0.1f;

        MeshRenderer mr = textObj.GetComponent<MeshRenderer>();
        if (mr != null)
        {
            mr.sortingOrder = 10;
        }
    }

    void UpdateRangeDisplay()
    {
        if (rangeText != null)
        {
            rangeText.text = range.ToString();
        }
    }

    public IEnumerator MoveTo(int targetRow, int targetCol)
    {
        if (this == null || gameObject == null || boardRenderer == null)
        {
            yield break;
        }

        isMoving = true;

        float speed = OptionsManager.Instance != null ? OptionsManager.Instance.globalMoveSpeed : moveSpeed;
        float hover = OptionsManager.Instance != null ? OptionsManager.Instance.globalHoverHeight : hoverHeight;

        Vector3 startPos = transform.position;
        Vector3 endPos = boardRenderer.GetWorldPosition(targetRow, targetCol);

        if (spriteRenderer != null)
        {
            endPos.z = -0.1f;
        }
        else
        {
            endPos.z = startPos.z;
        }

        Vector3 midPos = (startPos + endPos) / 2f;
        midPos.z -= hover;

        float duration = 1.0f / speed;
        float elapsed = 0f;

        while (elapsed < duration / 2f)
        {
            if (this == null || gameObject == null)
            {
                yield break;
            }

            elapsed += Time.deltaTime;
            float t = elapsed / (duration / 2f);
            transform.position = Vector3.Lerp(startPos, midPos, t);
            yield return null;
        }

        elapsed = 0f;
        while (elapsed < duration / 2f)
        {
            if (this == null || gameObject == null)
            {
                yield break;
            }

            elapsed += Time.deltaTime;
            float t = elapsed / (duration / 2f);
            transform.position = Vector3.Lerp(midPos, endPos, t);
            yield return null;
        }

        if (this != null && gameObject != null)
        {
            transform.position = endPos;
        }

        isMoving = false;
    }

    public bool IsMoving()
    {
        return isMoving;
    }

    void OnDestroy()
    {
        StopAllCoroutines();
    }
}
