using UnityEngine;
using Yarn.Unity;

public class DialogueTrigger : MonoBehaviour
{
    public static DialogueTrigger Instance { get; private set; }
    public DialogueRunner dialogueRunner;
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
        if (dialoguePanel != null)
        {
            dialoguePanel.SetActive(true);
        }
        dialogueRunner.StartDialogue(goat.ToString());
        isInDialogue = true;
    }

    public void DisablePanel()
    {
        if (dialoguePanel != null)
        {
            dialoguePanel.SetActive(false);
        }
        isInDialogue = false;
    }
}
