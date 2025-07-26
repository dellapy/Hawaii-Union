using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.SceneManagement;

[System.Obsolete]
public class TileBehavior : MonoBehaviour
{
    private SpriteRenderer spriteRenderer;

    public bool isRevealed = false;
    public bool isFlagged = false;

    public Sprite tileSprite;
    public Sprite revealedSprite;
    public Sprite flagSprite;
    public Sprite mineSprite;

    private TextMeshPro textMesh;

    [HideInInspector] public int adjacentMines = 0;

    public static bool isGameOver = false;
    public GameObject gameOverPanel;
    public TextMeshProUGUI mineCountText;
    [HideInInspector] public static int totalMines;
    [HideInInspector] public static int defusedMines = 0;

    void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        textMesh = GetComponentInChildren<TextMeshPro>();
        if (textMesh == null)
        {
            Debug.LogError("TextMeshPro component not found on " + gameObject.name, gameObject);
        }
        if (gameOverPanel == null)
        {
            Debug.LogError("GameOverPanel not assigned on " + gameObject.name + "; please assign in inspector.", gameObject);
        }
        else
        {
            gameOverPanel.SetActive(false);
        }
        if (mineCountText == null)
        {
            Debug.LogError("MineCountText not assigned on " + gameObject.name + "; please assign in inspector.", gameObject);
        }

        CalculateAdjacentMines();
        if (gameObject.CompareTag("Mine"))
        {
            totalMines++;
        }
        UpdateMineCountText();
    }

    private void CalculateAdjacentMines()
    {
        adjacentMines = 0;
        Vector2[] offsets = DefineNeighborOffsets();
        Vector2 currentPos = new Vector2(Mathf.Round(transform.position.x), Mathf.Round(transform.position.y));

        foreach (Vector2 offset in offsets)
        {
            Vector2 neighborPos = currentPos + offset;
            GameObject neighbor = FindTileAtPosition(neighborPos);
            if (neighbor != null && neighbor.CompareTag("Mine"))
            {
                adjacentMines++;
            }
        }
        Debug.Log($"Tile at {currentPos} has {adjacentMines} adjacent mines.");
    }

    void OnMouseDown() // Handles left-click
    {
        if (isGameOver || isRevealed) // Block interaction
        {
            return;
        }
            if (isFlagged)
            {
                
                PlayerEnergy playerEnergy = FindObjectOfType<PlayerBehavior>().GetComponent<PlayerEnergy>();
                
                if (NeighborsRevealedOrFound() && playerEnergy.GetEnergy() >= playerEnergy.energyCost)
                {
                    isFlagged = false;
                    isRevealed = true;
                    RevealTile();
                    playerEnergy.SubtractEnergy();
                    if (gameObject.CompareTag("Mine"))
                {
                    // Defuse mine
                    gameObject.tag = "Untagged";
                    defusedMines++;
                    UpdateMineCountText();
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
                    GameOver();
                    UpdateMineCountText();
                }
                else
                {
                    RevealTile();
                    Debug.Log("Tile revealed: " + transform.position + ", Adjacent Mines: " + adjacentMines);
                }
            }
    }

    void OnMouseOver() // Handles right-click
    {
        if (isGameOver || isRevealed) // Block interaction
        {
            return;
        }
        if (Input.GetMouseButtonDown(1))
            {
                isFlagged = !isFlagged;
                spriteRenderer.sprite = isFlagged ? flagSprite : tileSprite; // Revert to default sprite if unflagged
                textMesh.text = ""; // Clear text when flagging/unflagging
                Debug.Log(isFlagged ? "Tile flagged" : "Tile unflagged");
            }
    }

    public void GameOver()
    {
        isGameOver = true;
        if (gameOverPanel != null)
        {
            gameOverPanel.SetActive(true);
        }
        Debug.Log("Game over state activated: player movement and tile interactions disabled.");
    }

    public void RestartGame()
    {
        isGameOver = false;
        totalMines = 0;
        defusedMines = 0;
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    public void RevealTile()
    {
        if (!isFlagged)
        {
            spriteRenderer.sprite = revealedSprite;
            if (textMesh != null)
            {
                textMesh.text = adjacentMines > 0 ? adjacentMines.ToString() : ""; // Show number if > 0
            }
        }
    }

        private void UpdateMineCountText()
    {
        if (mineCountText == null || isGameOver)
        {
            if (mineCountText != null) mineCountText.text = "Game Over";
            return;
        }
        mineCountText.text = $"Defused: {defusedMines}/{totalMines}";
    }

    private bool NeighborsRevealedOrFound()
    {
        Vector2[] offsets = DefineNeighborOffsets();
        Debug.Log("Checking neighbors for tile at " + transform.position);

        foreach (Vector2 offset in offsets)
        {
            Vector2 neighborPos = new Vector2(transform.position.x, transform.position.y) + offset;
            GameObject neighbor = FindTileAtPosition(neighborPos);
            if (neighbor != null)
            {
                TileBehavior neighborTile = neighbor.GetComponent<TileBehavior>();
                if (neighborTile != null)
                {
                    if (!neighborTile.isRevealed && !neighborTile.isFlagged)
                    {
                        Debug.Log("Neighbor at " + neighborPos + " is neither revealed nor flagged.");
                        return false; // Neighbor is neither revealed nor flagged
                    }
                    else
                    {
                        Debug.Log("Neighbor at " + neighborPos + " is " + (neighborTile.isRevealed ? "revealed" : "flagged"));
                    }
                }
            }
            else
            {
                Debug.Log("No neighbor found at " + neighborPos + "; treating as valid (edge case).");
            }
        }

        Debug.Log("All neighbors at " + transform.position + " are revealed or flagged.");
        return true; // All neighbors are either revealed, flagged, or non-existent (edge of board)
    }

    private GameObject FindTileAtPosition(Vector2 pos)
    {
        pos = new Vector2(Mathf.Round(pos.x), Mathf.Round(pos.y));
        GameObject[] allTiles = GameObject.FindObjectsOfType<GameObject>(); // Get all GameObjects
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

    private void UpdateNeighborMineCounts()
    {
        Vector2[] offsets = DefineNeighborOffsets();
        Debug.Log("Updating neighbor mine counts for tile at " + transform.position);

        PlayerBehavior player = FindObjectOfType<PlayerBehavior>();
        Vector2 playerPos = player != null ? new Vector2(Mathf.Round(player.transform.position.x), Mathf.Round(player.transform.position.y)) : Vector2.zero;
        foreach (Vector2 offset in offsets)
        {
            Vector2 neighborPos = new Vector2(transform.position.x, transform.position.y) + offset;
            GameObject neighbor = FindTileAtPosition(neighborPos);
            if (neighbor != null)
            {
                TileBehavior neighborTile = neighbor.GetComponent<TileBehavior>();
                if (neighborTile != null)
                {
                    neighborTile.adjacentMines = Mathf.Max(0, neighborTile.adjacentMines - 1);
                    Debug.Log("Updated neighbor at " + neighborPos + " (Tag: " + neighbor.tag + "): adjacentMines = " + neighborTile.adjacentMines);
                    if (neighborTile.isRevealed && neighborTile.textMesh != null)
                    {
                        neighborTile.textMesh.text = neighborTile.adjacentMines > 0 ? neighborTile.adjacentMines.ToString() : "";
                    }

                    if (player != null && playerPos == neighborPos)
                    {
                        player.RefreshAdjacentMinesText();
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