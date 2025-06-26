using TMPro;
using UnityEngine;

public class TileBehavior : MonoBehaviour
{
    private SpriteRenderer spriteRenderer;

    private bool isRevealed = false;
    private bool isFlagged = false;

    public Sprite tileSprite;
    public Sprite revealedSprite;
    public Sprite flagSprite;
    public Sprite mineSprite;

    private TextMeshPro textMesh;

    public int adjacentMines = 0;

    void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        textMesh = GetComponentInChildren<TextMeshPro>();
        if (textMesh == null)
        {
            Debug.LogError("TextMeshPro component not found on " + gameObject.name, gameObject);
        }
    }

    void OnMouseDown() // Handles left-click
    {
        if (!isRevealed && !isFlagged)
        {
            isRevealed = true;
            if (gameObject.CompareTag("Mine"))
            {
                spriteRenderer.sprite = mineSprite;
                textMesh.text = ""; // Clear text for mines
                Debug.Log("Game Over! Mine clicked.");
                // Game logic for loss here
            }
            else
            {
                spriteRenderer.sprite = revealedSprite;
                textMesh.text = adjacentMines > 0 ? adjacentMines.ToString() : ""; // Show number if > 0
                Debug.Log("Tile revealed: " + transform.position + ", Adjacent Mines: " + adjacentMines);
            }
        }
    }

    void OnMouseOver() // Handles right-click
    {
        if (Input.GetMouseButtonDown(1) && !isRevealed)
        {
            isFlagged = !isFlagged;
            spriteRenderer.sprite = isFlagged ? flagSprite : tileSprite; // Revert to default sprite if unflagged
            textMesh.text = ""; // Clear text when flagging/unflagging
            Debug.Log(isFlagged ? "Tile flagged" : "Tile unflagged");
        }
    }
}