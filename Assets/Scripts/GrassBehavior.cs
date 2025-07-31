using UnityEngine;

public class GrassBehavior : MonoBehaviour
{
    private GameObject occupiedTile;

    void Start()
    {
        Vector2 pos = new Vector2(Mathf.Round(transform.position.x), Mathf.Round(transform.position.y));
        transform.position = pos;

        occupiedTile = FindTileAtPosition(pos);
        if (occupiedTile == null)
        {
            Debug.LogError("Grass at " + pos + " is not on a tile! Disabling Grass.", gameObject);
            gameObject.SetActive(false);
            return;
        }

        if (occupiedTile.CompareTag("Mine"))
        {
            Debug.LogError("Grass at " + pos + " is on a mine tile! Disabling Grass.", gameObject);
            gameObject.SetActive(false);
            return;
        }

        Debug.Log("Grass placed at " + pos + " on tile with tag " + occupiedTile.tag);
    }

    public void Collect()
    {
        Debug.Log("Grass collected at " + transform.position);
        gameObject.SetActive(false);

        PlayerEnergy playerEnergy = FindFirstObjectByType<PlayerBehavior>().GetComponent<PlayerEnergy>();
        playerEnergy.AddEnergy();
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