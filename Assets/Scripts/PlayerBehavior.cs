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

    void Start()
    {
        // Initialize player at a starting position
        targetPosition = new Vector2(0, 0);
        transform.position = targetPosition;
        // Verify that a tile exists at the starting position
        if (FindTileAtPosition(targetPosition) == null)
        {
            Debug.LogError("No tile found at starting position " + targetPosition + "; please adjust the player's initial position.");
        }
        else
        {
            Debug.Log("Player started at " + targetPosition);
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
}