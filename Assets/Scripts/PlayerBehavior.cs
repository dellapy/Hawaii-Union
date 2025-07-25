using System.Collections;
using TMPro;
using UnityEngine;

[System.Obsolete]
public class PlayerBehavior : MonoBehaviour
{
    private Vector2 targetPosition;
    private bool canMove = true; // Prevents multiple moves during processing
    private float lastMoveTime;

    [SerializeField] private float moveCooldown = 0.2f; // Small delay to prevent rapid inputs
    [SerializeField] private float moveDuration = 0.3f; // Time to move between tiles

    [SerializeField] private int startX;
    [SerializeField] private int startY;

    public TextMeshProUGUI adjacentMinesText;

    void Start()
    {
        // Initialize player at a starting position
        targetPosition = new Vector2(startX, startY);
        transform.position = targetPosition;
        // Verify that a tile exists at the starting position
        GameObject startTile = FindTileAtPosition(targetPosition);
        if (startTile == null)
        {
            Debug.LogError("No tile found at starting position " + targetPosition + "; please adjust the player's initial position.");
        }
        else
        {
            Debug.Log("Player started at " + targetPosition);
            UpdateAdjacentMinesText(startTile);
        }
    }

    void Update()
    {
        if (TileBehavior.isGameOver || !canMove || Time.time < lastMoveTime + moveCooldown)
            return;

        Vector2 moveInput = Vector2.zero;

        // Handle input (WASD or arrow keys)
        if (Input.GetKeyDown(KeyCode.W) || Input.GetKeyDown(KeyCode.UpArrow))
            moveInput = Vector2.up;
        else if (Input.GetKeyDown(KeyCode.S) || Input.GetKeyDown(KeyCode.DownArrow))
            moveInput = Vector2.down;
        else if (Input.GetKeyDown(KeyCode.A) || Input.GetKeyDown(KeyCode.LeftArrow))
            moveInput = Vector2.left;
        else if (Input.GetKeyDown(KeyCode.D) || Input.GetKeyDown(KeyCode.RightArrow))
            moveInput = Vector2.right;

        if (moveInput != Vector2.zero)
        {
            TryMove(moveInput);
        }
    }

    private void TryMove(Vector2 direction)
    {
        Vector2 newPosition = targetPosition + direction;

        // Check if there's a tile at the new position
        GameObject tile = FindTileAtPosition(newPosition);

        // If no tile exists, block movement
        if (tile == null)
        {
            Debug.Log("Cannot move to " + newPosition + ": no tile exists");
            return;
        }

        // Move the player
        canMove = false;
        targetPosition = newPosition;
        StartCoroutine(MoveToPosition(targetPosition, tile));
        lastMoveTime = Time.time;
        Debug.Log("Player moved to " + targetPosition);
    }

    private IEnumerator MoveToPosition(Vector2 targetPos, GameObject tile)
    {
        Vector2 startPos = transform.position;
        float elapsedTime = 0f;

        while (elapsedTime < moveDuration)
        {
            if (TileBehavior.isGameOver)
            {
                canMove = true; // Allow coroutine to exit cleanly
                yield break;
            }

            elapsedTime += Time.deltaTime;
            float t = Mathf.Clamp01(elapsedTime / moveDuration);
            transform.position = Vector2.Lerp(startPos, targetPos, t);
            yield return null;
        }

        // Ensure final position is exact
        transform.position = targetPos;
        Debug.Log("Player moved to " + targetPos);

        // Interact with the tile
        InteractWithTile(tile);

        // Update UI text after moving
        UpdateAdjacentMinesText(tile);

        canMove = true;
    }

    private void InteractWithTile(GameObject tile)
    {
        TileBehavior tileBehavior = tile.GetComponent<TileBehavior>();
        if (tileBehavior == null)
        {
            Debug.LogError("Tile at " + tile.transform.position + " has no TileBehavior component");
            return;
        }

        // NPC tiles
        NPCBehavior npc = FindNPCAtPosition(tile.transform.position);
        if (npc != null)
        {
            npc.Collect(); // Collect the NPC
        }

        // Grass tiles
        GrassBehavior grass = FindGrassAtPosition(tile.transform.position);
        if (grass != null)
        {
            grass.Collect(); // Collect the Grass
        }

        // If the tile is unrevealed or flagged, reveal it
        if (!tileBehavior.isRevealed)
        {
            tileBehavior.isRevealed = true;
            if (tile.CompareTag("Mine"))
            {
                // Game over
                tileBehavior.GetComponent<SpriteRenderer>().sprite = tileBehavior.mineSprite;
                tileBehavior.GetComponentInChildren<TextMeshPro>().text = "";
                Debug.Log("Game Over! Player stepped on a mine at " + tile.transform.position);
                tileBehavior.GameOver(); // Call GameOver method
            }
            else
            {
                // Reveal the tile normally
                tileBehavior.RevealTile();
                Debug.Log("Tile revealed by player at " + tile.transform.position + ", Adjacent Mines: " + tileBehavior.adjacentMines);
            }
        }
    }

    public void UpdateAdjacentMinesText(GameObject tile)
    {
        if (adjacentMinesText == null || TileBehavior.isGameOver)
        {
            if (adjacentMinesText != null) adjacentMinesText.text = "";
            return;
        }

        TileBehavior tileBehavior = tile.GetComponent<TileBehavior>();
        if (tileBehavior == null)
        {
            adjacentMinesText.text = "No Tile";
            return;
        }

        if (tile.CompareTag("Mine") && !tileBehavior.isRevealed)
        {
            adjacentMinesText.text = "Mine"; // Don't reveal mine count until revealed
        }
        else
        {
            adjacentMinesText.text = "Adjacent Mines: " + tileBehavior.adjacentMines;
        }
    }

    public void RefreshAdjacentMinesText()
    {
        // Find the tile at the player's current position
        GameObject currentTile = FindTileAtPosition(transform.position);
        UpdateAdjacentMinesText(currentTile);
    }

    private GameObject FindTileAtPosition(Vector2 pos)
    {
        pos = new Vector2(Mathf.Round(pos.x), Mathf.Round(pos.y));
        GameObject[] allTiles = GameObject.FindObjectsOfType<GameObject>();
        foreach (GameObject tile in allTiles)
        {
            TileBehavior tileBehavior = tile.GetComponent<TileBehavior>();
            if (tileBehavior != null)
            {
                Vector2 tilePos = new Vector2(Mathf.Round(tile.transform.position.x), Mathf.Round(tile.transform.position.y));
                if (tilePos == pos)
                {
                    Debug.Log("Found tile at " + pos + " with tag " + tile.tag);
                    return tile;
                }
            }
        }
        Debug.LogWarning("No tile found at " + pos);
        return null;
    }

    private NPCBehavior FindNPCAtPosition(Vector2 pos)
    {
        pos = new Vector2(Mathf.Round(pos.x), Mathf.Round(pos.y));
        GameObject[] allNPCs = GameObject.FindObjectsOfType<GameObject>();
        foreach (GameObject obj in allNPCs)
        {
            NPCBehavior npc = obj.GetComponent<NPCBehavior>();
            if (npc != null && obj.activeSelf)
            {
                Vector2 npcPos = new Vector2(Mathf.Round(obj.transform.position.x), Mathf.Round(obj.transform.position.y));
                if (npcPos == pos)
                {
                    Debug.Log("Found NPC at " + pos);
                    return npc;
                }
            }
        }
        return null;
    }
    
    private GrassBehavior FindGrassAtPosition(Vector2 pos)
    {
        pos = new Vector2(Mathf.Round(pos.x), Mathf.Round(pos.y));
        GameObject[] allGrass = GameObject.FindObjectsOfType<GameObject>();
        foreach (GameObject obj in allGrass)
        {
            GrassBehavior grass = obj.GetComponent<GrassBehavior>();
            if (grass != null && obj.activeSelf)
            {
                Vector2 grassPos = new Vector2(Mathf.Round(obj.transform.position.x), Mathf.Round(obj.transform.position.y));
                if (grassPos == pos)
                {
                    Debug.Log("Found Grass at " + pos);
                    return grass;
                }
            }
        }
        return null;
    }
}