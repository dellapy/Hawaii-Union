using UnityEngine;

public class NPCBehavior : MonoBehaviour
{
    private GameObject occupiedTile;

    void Start()
    {
        Vector2 pos = new Vector2(Mathf.Round(transform.position.x), Mathf.Round(transform.position.y));
        transform.position = pos;

        occupiedTile = FindTileAtPosition(pos);
        if (occupiedTile == null)
        {
            Debug.LogError("NPC at " + pos + " is not on a tile! Disabling NPC.", gameObject);
            gameObject.SetActive(false);
            return;
        }

        if (occupiedTile.CompareTag("Mine"))
        {
            Debug.LogError("NPC at " + pos + " is on a mine tile! Disabling NPC.", gameObject);
            gameObject.SetActive(false);
            return;
        }

        TileBehavior tileBehavior = occupiedTile.GetComponent<TileBehavior>();
        if (tileBehavior != null && !tileBehavior.isRevealed)
        {
            tileBehavior.isRevealed = true;
            tileBehavior.RevealTile();
            Debug.Log($"Tile at {pos} revealed under NPC, Adjacent Mines: {tileBehavior.adjacentMines}");
        }

        Debug.Log("NPC placed at " + pos + " on tile with tag " + occupiedTile.tag);
    }

    public void Collect()
    {
        Debug.Log("NPC collected at " + transform.position);
        gameObject.SetActive(false);
        // Add other game logic here
    }

    private GameObject FindTileAtPosition(Vector2 pos)
    {
        pos = new Vector2(Mathf.Round(pos.x), Mathf.Round(pos.y));
        GameObject[] allTiles = GameObject.FindObjectsByType<GameObject>(FindObjectsSortMode.None);
        foreach (GameObject tile in allTiles)
        {
            TileBehavior tileBehavior = tile.GetComponent<TileBehavior>();
            if (tileBehavior != null)
            {
                Vector2 tilePos = new Vector2(Mathf.Round(tile.transform.position.x), Mathf.Round(tile.transform.position.y));
                if (tilePos == pos)
                {
                    return tile;
                }
            }
        }
        return null;
    }
}