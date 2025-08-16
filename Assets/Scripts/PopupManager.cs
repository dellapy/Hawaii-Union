using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;

public class PopupManager : MonoBehaviour
{
    public static PopupManager Instance { get; private set; }

    [System.Serializable]
    public class PopupPage
    {
        [TextArea(3, 10)]
        public string content;
        public Sprite image; // Optional image for the page
    }

    [SerializeField] private GameObject popupPanel;
    [SerializeField] private TextMeshProUGUI contentText;
    [SerializeField] private Image contentImage;
    [SerializeField] private Button nextButton;
    [SerializeField] private Button prevButton;
    [SerializeField] private Button closeButton;
    [SerializeField] private TextMeshProUGUI pageNumberText;
    [SerializeField] private List<PopupPage> pages = new List<PopupPage>();

    private int currentPage = 0;

    private void Awake()
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
        // Ensure popup is shown at start
        popupPanel.SetActive(true);

        // Add button listeners
        nextButton.onClick.AddListener(NextPage);
        prevButton.onClick.AddListener(PreviousPage);
        closeButton.onClick.AddListener(ClosePopup);
    }

    public void ShowPopup()
    {
        if (pages.Count == 0) return;

        popupPanel.SetActive(true);
        currentPage = 0;
        UpdatePageContent();
    }

    private void UpdatePageContent()
    {
        // Update text content
        contentText.text = pages[currentPage].content;

        // Update image if available
        if (contentImage != null)
        {
            contentImage.gameObject.SetActive(pages[currentPage].image != null);
            if (pages[currentPage].image != null)
            {
                contentImage.sprite = pages[currentPage].image;
            }
        }

        // Update page number display
        if (pageNumberText != null)
        {
            pageNumberText.text = $"Page {currentPage + 1} of {pages.Count}";
        }

        // Enable/disable navigation buttons
            prevButton.interactable = currentPage > 0;
        nextButton.interactable = currentPage < pages.Count - 1;
    }

    private void NextPage()
    {
        if (currentPage < pages.Count - 1)
        {
            currentPage++;
            UpdatePageContent();
        }
    }

    private void PreviousPage()
    {
        if (currentPage > 0)
        {
            currentPage--;
            UpdatePageContent();
        }
    }

    private void ClosePopup()
    {
        popupPanel.SetActive(false);
    }

    // Optional: Method to add pages programmatically
    public void AddPage(string content, Sprite image = null)
    {
        pages.Add(new PopupPage { content = content, image = image });
    }
}