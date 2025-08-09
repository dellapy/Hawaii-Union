using TMPro;
using UnityEngine;

public class TileBehavior : MonoBehaviour
{
    private SpriteRenderer spriteRenderer;
    [HideInInspector] public bool isFlagged = false;
    [HideInInspector] public bool isGrass = false;
    [HideInInspector] public bool isRevealed = false;
    [HideInInspector] public bool wasFalseFlag = false;

    public Sprite tileSprite;
    public Sprite revealedSprite;
    public Sprite revealed1Sprite;
    public Sprite revealed2Sprite;
    public Sprite revealed3Sprite;
    public Sprite flagSprite;
    public Sprite mineSprite;
    public Sprite goalSprite;
    public Sprite grassSprite;

    public int adjacentMines = 0; // Only change for debugging

    private GameObject flagObject;
    public float flagOffset = 0.5f;

    void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        
        if (!GetComponent<Collider2D>())
        {
            BoxCollider2D collider = gameObject.AddComponent<BoxCollider2D>();
            collider.size = new Vector2(1f, 1f);
        }
        if (gameObject.CompareTag("Goal"))
        {
            spriteRenderer.sprite = goalSprite;
            isGrass = false;
        }
        else if (gameObject.CompareTag("Grass"))
        {
            spriteRenderer.sprite = grassSprite;
            isGrass = true;
        }

        // Flag child object setup
        flagObject = new GameObject("Flag");
        flagObject.transform.SetParent(transform);
        flagObject.transform.localPosition = Vector3.up * flagOffset;
        SpriteRenderer flagRenderer = flagObject.AddComponent<SpriteRenderer>();
        flagRenderer.sprite = flagSprite;
        flagRenderer.sortingOrder = 1;
        Billboard billboard = flagObject.AddComponent<Billboard>();
        billboard.billboardType = Billboard.BillboardType.Euler;
        flagObject.SetActive(false); // Disabled by default

        CalculateAdjacentMines();
    }

    private void CalculateAdjacentMines()
    {
        adjacentMines = 0;
        Vector2[] offsets = DefineNeighborOffsets();
        Vector2 currentPos = new Vector2(Mathf.Round(transform.position.x), Mathf.Round(transform.position.y));

        foreach (Vector2 offset in offsets)
        {
            Vector2 neighborPos = currentPos + offset;
            GameObject neighbor = GridManager.Instance.tileMap.ContainsKey(neighborPos) ? GridManager.Instance.tileMap[neighborPos] : null;
            if (neighbor != null && neighbor.CompareTag("Mine"))
            {
                adjacentMines++;
            }
        }
        Debug.Log($"Tile at {currentPos} has {adjacentMines} adjacent mines.");
    }

        void OnMouseDown() // Handles left-click
    {
        if (GameManager.Instance.isGameOver || isRevealed) // Block interaction
        {
            return;
        }
        if (gameObject.CompareTag("Goal") || gameObject.CompareTag("Grass"))
        {
            Debug.Log($"Cannot interact with {gameObject.tag} tile via mouse click.");
            return;
        }
        if (isFlagged)
        {
            PlayerEnergy playerEnergy = FindFirstObjectByType<PlayerBehavior>().GetComponent<PlayerEnergy>();
            if (NeighborsRevealedOrFound() && playerEnergy.GetEnergy() >= playerEnergy.energyCost)
            {
                isFlagged = false;
                flagObject.SetActive(false);
                isRevealed = true;
                playerEnergy.SubtractEnergy();
                if (gameObject.CompareTag("Mine"))
                {
                    // Defuse mine
                    gameObject.tag = "Untagged";
                    GameManager.Instance.MineDefused();
                    UpdateNeighborMineCounts();
                    RevealTile();
                    Debug.Log("Mine removed at " + transform.position);
                }
                else
                {
                    // False flag: clear flag and reveal without showing adjacentMines
                    wasFalseFlag = true;
                    RevealTile();
                    Debug.Log("False flag revealed at " + transform.position + ", Adjacent Mines: " + adjacentMines);
                }
            }
            else
            {
                Debug.Log("Cannot click flagged tile at " + transform.position + ": not all neighbors are revealed, flagged, or a goal space, or insufficient energy.");
            }
        }
            else
            {
                isRevealed = true;
                if (gameObject.CompareTag("Mine"))
                {
                    spriteRenderer.sprite = mineSprite;
                    Debug.Log("Game Over! Mine clicked.");
                    GameManager.Instance.GameOver();
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
        if (GameManager.Instance.isGameOver || isRevealed || CompareTag("Goal")) // Block interaction
        {
            return;
        }
        if (Input.GetMouseButtonDown(1))
        {
            isFlagged = !isFlagged;
            flagObject.SetActive(isFlagged);
            Debug.Log(isFlagged ? "Tile flagged" : "Tile unflagged");
        }
    }


    public void RevealTile()
    {
        if (!CompareTag("Goal"))
        {
            switch (adjacentMines)
            {
                case 1:
                    spriteRenderer.sprite = revealed1Sprite;
                    break;
                case 2:
                    spriteRenderer.sprite = revealed2Sprite;
                    break;
                case 3:
                    spriteRenderer.sprite = revealed3Sprite;
                    break;
                default:
                    spriteRenderer.sprite = revealedSprite;
                    break;
            }
        }
        flagObject.SetActive(false);
        if (isGrass)
        {
            isGrass = false; // Grass is collected
            PlayerEnergy playerEnergy = FindFirstObjectByType<PlayerBehavior>().GetComponent<PlayerEnergy>();
            if (playerEnergy != null)
            {
                playerEnergy.AddEnergy();
                Debug.Log("Grass collected at " + transform.position + ", energy recharged.");
            }
            gameObject.tag = "Untagged"; // Revert to normal tile
        }
    }

    public bool NeighborsRevealedOrFound()
    {
        Vector2[] offsets = DefineNeighborOffsets();
        Debug.Log("Checking neighbors for tile at " + transform.position);

        foreach (Vector2 offset in offsets)
        {
            Vector2 neighborPos = new Vector2(transform.position.x, transform.position.y) + offset;
            GameObject neighbor = GridManager.Instance.tileMap.ContainsKey(neighborPos) ? GridManager.Instance.tileMap[neighborPos] : null;
            if (neighbor != null)
            {
                TileBehavior neighborTile = neighbor.GetComponent<TileBehavior>();
                if (neighborTile != null)
                {
                    if (!neighborTile.isRevealed && !neighborTile.isFlagged && !neighborTile.CompareTag("Goal") && !neighborTile.isGrass)
                    {
                        Debug.Log("Neighbor at " + neighborPos + " is neither revealed nor flagged.");
                        return false;
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

        Debug.Log("All neighbors at " + transform.position + " are revealed, flagged, goal, or grass.");
        return true;
    }

    public void UpdateNeighborMineCounts()
    {
        Vector2[] offsets = DefineNeighborOffsets();
        Debug.Log("Updating neighbor mine counts for tile at " + transform.position);

        PlayerBehavior player = FindFirstObjectByType<PlayerBehavior>();
        Vector2 playerPos = player != null ? new Vector2(Mathf.Round(player.transform.position.x), Mathf.Round(player.transform.position.y)) : Vector2.zero;
        foreach (Vector2 offset in offsets)
        {
            Vector2 neighborPos = new Vector2(transform.position.x, transform.position.y) + offset;
            GameObject neighbor = GridManager.Instance.tileMap.ContainsKey(neighborPos) ? GridManager.Instance.tileMap[neighborPos] : null;
            if (neighbor != null)
            {
                TileBehavior neighborTile = neighbor.GetComponent<TileBehavior>();
                if (neighborTile != null && !neighborTile.CompareTag("Goal") && !neighborTile.CompareTag("Grass"))
                {
                    neighborTile.adjacentMines = Mathf.Max(0, neighborTile.adjacentMines - 1);
                    Debug.Log("Updated neighbor at " + neighborPos + " (Tag: " + neighbor.tag + "): adjacentMines = " + neighborTile.adjacentMines);
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

    public GameObject GetFlagObject()
    {
        return flagObject;
    }
}