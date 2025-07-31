using System.Collections.Generic;
using UnityEngine;

public class GridManager : MonoBehaviour
{
    public static GridManager Instance { get; private set; }
    public Dictionary<Vector2, GameObject> tileMap = new Dictionary<Vector2, GameObject>();
    public Dictionary<Vector2, NPCBehavior> npcMap = new Dictionary<Vector2, NPCBehavior>();
    public Dictionary<Vector2, GrassBehavior> grassMap = new Dictionary<Vector2, GrassBehavior>();

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
            return;
        }

        tileMap.Clear();
        npcMap.Clear();
        foreach (GameObject obj in FindObjectsByType<GameObject>(FindObjectsSortMode.None))
        {
            TileBehavior tb = obj.GetComponent<TileBehavior>();
            if (tb != null)
            {
                Vector2 pos = new Vector2(Mathf.Round(obj.transform.position.x), Mathf.Round(obj.transform.position.y));
                tileMap[pos] = obj;
            }

            NPCBehavior npc = obj.GetComponent<NPCBehavior>();
            if (npc != null && obj.activeSelf)
            {
                Vector2 pos = new Vector2(Mathf.Round(obj.transform.position.x), Mathf.Round(obj.transform.position.y));
                npcMap[pos] = npc;
            }

            GrassBehavior grass = obj.GetComponent<GrassBehavior>();
            if (grass != null && obj.activeSelf)
            {
                Vector2 pos = new Vector2(Mathf.Round(obj.transform.position.x), Mathf.Round(obj.transform.position.y));
                grassMap[pos] = grass;
            }
        }
        Debug.Log($"GridManager initialized with {tileMap.Count} tiles and {npcMap.Count} NPCs.");
    }
}