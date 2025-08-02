using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }
    public GameObject gameOverPanel;
    public GameObject levelCompletePanel;
    public TextMeshProUGUI mineCountText;
    public TextMeshProUGUI requireCountText;
    public int totalMines { get; private set; }
    public int defusedMines { get; private set; }
    public bool isGameOver { get; private set; }
    public bool isLevelComplete { get; private set; }
    [SerializeField] private int requiredMinesToDefuse = 3;

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

        if (gameOverPanel == null)
        {
            Debug.LogError("GameOverPanel not assigned in GameManager; please assign in Inspector.", gameObject);
        }
        else
        {
            gameOverPanel.SetActive(false);
        }
        if (levelCompletePanel == null)
        {
            Debug.LogError("LevelCompletePanel not assigned in GameManager; please assign in Inspector.", gameObject);
        }
        else
        {
            levelCompletePanel.SetActive(false);
        }
        if (mineCountText == null)
        {
            Debug.LogError("MineCountText not assigned in GameManager; please assign in Inspector.", gameObject);
        }

        totalMines = 0;
        defusedMines = 0;
        foreach (GameObject tile in FindObjectsByType<GameObject>(FindObjectsSortMode.None))
        {
            TileBehavior tb = tile.GetComponent<TileBehavior>();
            if (tb != null && tile.CompareTag("Mine"))
            {
                totalMines++;
            }
        }
        UpdateMineCountText();
        requireCountText.text = $"{requiredMinesToDefuse}";
    }

    public void MineDefused()
    {
        defusedMines++;
        UpdateMineCountText();
        if (defusedMines >= requiredMinesToDefuse)
        {
            Debug.Log("Goal tile unlocked!");
        }
    }

    public bool IsGoalTileUnlocked()
    {
        return defusedMines >= requiredMinesToDefuse;
    }

    public void GameOver()
    {
        isGameOver = true;
        if (gameOverPanel != null)
        {
            gameOverPanel.SetActive(true);
        }
        UpdateMineCountText();
        Debug.Log("Game over state activated: player movement and tile interactions disabled.");
    }

    public void LevelComplete()
    {
        isLevelComplete = true;
        if (levelCompletePanel != null)
        {
            levelCompletePanel.SetActive(true);
        }
        UpdateMineCountText();
        Debug.Log("Level complete! Player reached the goal tile.");
    }

    public void RestartGame()
    {
        isGameOver = false;
        isLevelComplete = false;
        totalMines = 0;
        defusedMines = 0;
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    private void UpdateMineCountText()
    {
        if (mineCountText == null || isGameOver || isLevelComplete)
        {
            if (mineCountText != null)
            {
                mineCountText.text = isGameOver ? "Game Over" : "Level Complete";
            }
            return;
        }
        mineCountText.text = $"{defusedMines} / {totalMines}";
    }
}