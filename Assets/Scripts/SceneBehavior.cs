using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneBehavior : MonoBehaviour
{

    public void doExitGame()
    {
        Debug.Log("Game is exiting");
        Application.Quit();
    }

    public void enterTutorial()
    {
        SceneManager.LoadScene("Tutorial1Scene");
        Debug.Log("Entering tutorial scene...");
    }

    public void enterMainMenu()
    {
        SceneManager.LoadScene("MainMenuScene");
        Debug.Log("Entering main menu scene...");
    }

    public void moveNextScene()
    {
        int nextSceneIndex = SceneManager.GetActiveScene().buildIndex + 1;
        if (SceneManager.sceneCountInBuildSettings > nextSceneIndex)
        {
            SceneManager.LoadScene(nextSceneIndex);
        } 
    }
}
