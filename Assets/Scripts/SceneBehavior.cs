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
        SceneManager.LoadScene("TutorialScene");
        Debug.Log("Entering tutorial scene...");
    }
}
