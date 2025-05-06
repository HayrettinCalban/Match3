using UnityEngine;
using System.Collections;

public class Tile : MonoBehaviour
{
    public int xIndex;
    public int yIndex;

    public int tileType;

    private BoardManager boardManager;
    private SpriteRenderer spriteRenderer;

    void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        if (spriteRenderer == null)
        {
            Debug.LogError("Tile prefab'ında SpriteRenderer bulunamadı! Görünüm çalışmayabilir.");
        }
        if (GetComponent<Collider2D>() == null)
        {
            Debug.LogError($"Tile prefab'ı '{gameObject.name}' üzerinde Collider2D bulunamadı! Tıklama çalışmayabilir.");
        }
    }

    public void Initialize(int x, int y, BoardManager manager)
    {
        xIndex = x;
        yIndex = y;
        boardManager = manager;
    }

    private void OnMouseDown()
    {
        if (boardManager != null && !boardManager.IsProcessingMove())
        {
            boardManager.TileClicked(this);
        }
    }

    public IEnumerator MoveToPosition(Vector2 targetPosition, float duration)
    {
        Vector2 startPosition = transform.position;
        float elapsed = 0f;

        while (elapsed < duration)
        {
            transform.position = Vector2.Lerp(startPosition, targetPosition, elapsed / duration);
            elapsed += Time.deltaTime;
            yield return null;
        }
        transform.position = targetPosition;
    }
}