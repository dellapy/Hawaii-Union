using UnityEngine;

public class NPCBehavior : MonoBehaviour
{
    private GameObject occupiedTile;
    public enum NPC
    {
        Wife,
        Uncle,
        Aunt,
        Cousin,
        Kid1,
        Kid2
    }
    public NPC familyMember;

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
        if (gameObject.activeSelf)
        {
            DialogueTrigger dialogueTrigger = GameObject.Find("Managers/Dialogue System").GetComponent<DialogueTrigger>();
            dialogueTrigger.InteractWithNPC(familyMember);
            Debug.Log("NPC collected at " + transform.position);
            gameObject.SetActive(false);
        }
    }

    private GameObject FindTileAtPosition(Vector2 pos)
    {
        pos = new Vector2(Mathf.Round(pos.x), Mathf.Round(pos.y));
        GameObject[] allTiles = FindObjectsByType<GameObject>(FindObjectsSortMode.None);
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