using UnityEngine;
using UnityEngine.UI;
using Yarn.Unity;

public class DialogueTrigger : MonoBehaviour
{
    public static DialogueTrigger Instance { get; private set; }
    public DialogueRunner dialogueRunner;
    public Button button;
    public GameObject dialogueBackground;
    public GameObject dialoguePanel;
    public bool isInDialogue { get; private set; }

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

        if (button == null)
        {
            Debug.LogError("Button not assigned in GameManager; please assign in Inspector.", gameObject);
        }
        else
        {
            button.gameObject.SetActive(false);
        }
        if (dialogueBackground == null)
        {
            Debug.LogError("DialogueBackground not assigned in GameManager; please assign in Inspector.", gameObject);
        }
        else
        {
            dialogueBackground.SetActive(false);
        }
        if (dialoguePanel == null)
        {
            Debug.LogError("DialoguePanel not assigned in GameManager; please assign in Inspector.", gameObject);
        }
        else
        {
            dialoguePanel.SetActive(false);
        }
    }

    public void InteractWithNPC(NPCBehavior.NPC goat)
    {
        if (button != null)
        {
            button.gameObject.SetActive(true);
        }
        if (dialogueBackground != null)
        {
            dialogueBackground.SetActive(true);
        }
        if (dialoguePanel != null)
        {
            dialoguePanel.SetActive(true);
        }
        dialogueRunner.StartDialogue(goat.ToString());
        isInDialogue = true;
    }

    public void DisablePanel()
    {
        if (button != null)
        {
            button.gameObject.SetActive(false);
        }
        if (dialogueBackground != null)
        {
            dialogueBackground.SetActive(false);
        }
        if (dialoguePanel != null)
        {
            dialoguePanel.SetActive(false);
        }
        isInDialogue = false;
    }
}
