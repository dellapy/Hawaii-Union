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
        if (!isRevealed)
        {
            if (isFlagged)
            {
                if (NeighborsRevealedOrFound())
                {
                    isFlagged = false;
                    isRevealed = true;
                    RevealTile();
                    if (gameObject.CompareTag("Mine"))
                    {
                        // Defuse mine
                        gameObject.tag = "Untagged";
                        UpdateNeighborMineCounts();
                        Debug.Log("Mine removed at " + transform.position);
                    }
                    else
                    {
                        // False flag: clear flag and reveal
                        Debug.Log("False flag revealed at " + transform.position + ", Adjacent Mines: " + adjacentMines);
                    }
                }
                else
                {
                    Debug.Log("Cannot click flagged tile at " + transform.position + ": not all neighbors are revealed or flagged.");
                }
            }
            else
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
                    RevealTile();
                    Debug.Log("Tile revealed: " + transform.position + ", Adjacent Mines: " + adjacentMines);
                }
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

    void RevealTile()
    {
        spriteRenderer.sprite = revealedSprite;
        if (textMesh != null)
        {
            textMesh.text = adjacentMines > 0 ? adjacentMines.ToString() : ""; // Show number if > 0
        }
    }

    private bool NeighborsRevealedOrFound()
    {
        Vector2[] offsets = DefineNeighborOffsets();

        foreach (Vector2 offset in offsets)
        {
            Vector2 neighborPos = new Vector2(transform.position.x, transform.position.y) + offset;
            GameObject neighbor = FindTileAtPosition(neighborPos);
            if (neighbor != null)
            {
                TileBehavior neighborTile = neighbor.GetComponent<TileBehavior>();
                if (neighborTile != null && !neighborTile.isRevealed && !neighborTile.isFlagged)
                {
                    return false; // Neighbor is neither revealed nor flagged
                }
            }
        }

        return true; // All neighbors are either revealed or flagged
    }

    private GameObject FindTileAtPosition(Vector2 pos)
    {
        pos = new Vector2(Mathf.Round(pos.x), Mathf.Round(pos.y));
        GameObject[] tiles = GameObject.FindGameObjectsWithTag("Untagged");
        foreach (GameObject tile in tiles)
        {
            Vector2 tilePos = new Vector2(Mathf.Round(tile.transform.position.x), Mathf.Round(tile.transform.position.y));
            if (tilePos == pos)
            {
                Debug.Log("Found Untagged tile at " + pos);
                return tile;
            }
        }
        tiles = GameObject.FindGameObjectsWithTag("Mine");
        foreach (GameObject tile in tiles)
        {
            Vector2 tilePos = new Vector2(Mathf.Round(tile.transform.position.x), Mathf.Round(tile.transform.position.y));
            if (tilePos == pos)
            {
                Debug.Log("Found Mine tile at " + pos);
                return tile;
            }
        }
        Debug.LogWarning("No tile found at " + pos);
        return null;
    }

    private void UpdateNeighborMineCounts()
    {
        Vector2[] offsets = DefineNeighborOffsets();

        foreach (Vector2 offset in offsets)
        {
            Vector2 neighborPos = new Vector2(transform.position.x, transform.position.y) + offset;
            GameObject neighbor = FindTileAtPosition(neighborPos);
            if (neighbor != null)
            {
                TileBehavior neighborTile = neighbor.GetComponent<TileBehavior>();
                if (neighborTile != null && !neighborTile.CompareTag("Mine"))
                {
                    neighborTile.adjacentMines = Mathf.Max(0, neighborTile.adjacentMines - 1);
                    // Update text if neighbor is revealed
                    if (neighborTile.isRevealed && neighborTile.textMesh != null)
                    {
                        neighborTile.textMesh.text = neighborTile.adjacentMines > 0 ? neighborTile.adjacentMines.ToString() : "";
                    }
                }
            }
        }
    }

    private Vector2[] DefineNeighborOffsets()
    {
        Vector2[] offsets = new Vector2[]
        {
            new Vector2(-1, 0), new Vector2(1, 0),
            new Vector2(0, 1), new Vector2(0, -1),
            new Vector2(-1, 1), new Vector2(1, 1),
            new Vector2(-1, -1), new Vector2(1, -1)
        };

        return offsets;
    }
}