using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PlayerBehavior : MonoBehaviour
{
    private SpriteRenderer spriteRenderer;
    private Vector2 targetPosition;
    private bool canMove = true; // Prevents multiple moves during processing
    private float lastMoveTime;

    [SerializeField] private float moveCooldown = 0.2f; // Small delay to prevent rapid inputs
    [SerializeField] private float moveDuration = 0.3f; // Time to move between tiles

    [SerializeField] private int startX;
    [SerializeField] private int startY;

    public TextMeshProUGUI adjacentMinesText;

    private PlayerControls playerControls;

    public Button finishLevelButton;

    public AudioSource moveAudio;

    void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        playerControls = new PlayerControls();
        
    }

    void OnEnable()
    {
        playerControls.GameControls.Enable();
    }

    void OnDisable()
    {
        playerControls.GameControls.Disable();
    }

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
            InteractWithTile(startTile);
            UpdateAdjacentMinesText(startTile);
        }

        if (adjacentMinesText == null)
        {
            Debug.LogError("AdjacentMinesText not assigned on PlayerBehavior; please assign in inspector.", gameObject);
        }

        playerControls.GameControls.MoveUp.performed += _ => TryMove(Vector2.up);
        playerControls.GameControls.MoveDown.performed += _ => TryMove(Vector2.down);
        playerControls.GameControls.MoveLeft.performed += _ => TryMove(Vector2.left);
        playerControls.GameControls.MoveRight.performed += _ => TryMove(Vector2.right);
    }

    private void TryMove(Vector2 direction)
    {
        if (GameManager.Instance.isGameOver || GameManager.Instance.isLevelComplete || DialogueTrigger.Instance.isInDialogue || !canMove || Time.time < lastMoveTime + moveCooldown)
            return;
        
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
        if (direction == Vector2.right)
        {
            spriteRenderer.flipX = true;
        }
        else if (direction == Vector2.left)
        {
            spriteRenderer.flipX = false;
        }
        moveAudio.Play();
        Debug.Log("Player moved to " + targetPosition);
    }

    private IEnumerator MoveToPosition(Vector2 targetPos, GameObject tile)
    {
        Vector2 startPos = transform.position;
        float elapsedTime = 0f;

        while (elapsedTime < moveDuration)
        {
            if (GameManager.Instance.isGameOver || GameManager.Instance.isLevelComplete)
            {
                canMove = true;
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

        InteractWithTile(tile);
        UpdateAdjacentMinesText(tile);

        if (tile.CompareTag("Goal"))
        {
            TileBehavior tileBehavior = tile.GetComponent<TileBehavior>();
            if (tileBehavior != null && GameManager.Instance.IsGoalTileUnlocked())
            {
                finishLevelButton.gameObject.SetActive(true);
            }
        }
        else
        {
            finishLevelButton.gameObject.SetActive(false);
        }

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

        // If the tile is unrevealed or flagged, reveal it
        if (!tileBehavior.isRevealed)
        {
            if (tile.CompareTag("Mine"))
            {
                // Game over
                tileBehavior.GetComponent<SpriteRenderer>().sprite = tileBehavior.mineSprite;
                Debug.Log("Game Over! Player stepped on a mine at " + tile.transform.position);
                GameManager.Instance.GameOver();
                UpdateAdjacentMinesText(tile);
            }
            else
            {
                // Reveal the tile normally
                tileBehavior.RevealTile();
                Debug.Log("Tile revealed by player at " + tile.transform.position + ", Adjacent Mines: " + tileBehavior.adjacentMines);
            }
        }
    }

    private void UpdateAdjacentMinesText(GameObject tile)
    {
        if (adjacentMinesText == null || GameManager.Instance.isGameOver || GameManager.Instance.isLevelComplete)
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

        if (tileBehavior.wasFalseFlag)
        {
            adjacentMinesText.text = "";
        }
        else
        {
            adjacentMinesText.text = "Adjacent Mines: " + tileBehavior.adjacentMines;
        }
    }

    public void RefreshAdjacentMinesText()
    {
        GameObject currentTile = FindTileAtPosition(transform.position);
        UpdateAdjacentMinesText(currentTile);
    }

    private GameObject FindTileAtPosition(Vector2 pos)
    {
        pos = new Vector2(Mathf.Round(pos.x), Mathf.Round(pos.y));
        if (GridManager.Instance.tileMap.ContainsKey(pos))
        {
            GameObject tile = GridManager.Instance.tileMap[pos];
            Debug.Log("Found tile at " + pos + " with tag " + tile.tag);
            return tile;
        }
        Debug.LogWarning("No tile found at " + pos);
        return null;
    }

    private NPCBehavior FindNPCAtPosition(Vector2 pos)
    {
        pos = new Vector2(Mathf.Round(pos.x), Mathf.Round(pos.y));
        if (GridManager.Instance.npcMap.ContainsKey(pos))
        {
            NPCBehavior npc = GridManager.Instance.npcMap[pos];
            Debug.Log("Found NPC at " + pos);
            return npc;
        }
        return null;
    }
}