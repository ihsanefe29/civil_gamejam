
using UnityEngine;

public class MainMenuController : MonoBehaviour
{
    [Header("Main Menu")]
    [SerializeField] private GameObject startButton;
    [SerializeField] private GameObject howToPlayButton;
    [SerializeField] private GameObject playButton;
    [SerializeField] private GameObject returnButton;

    [Header("How To Play")]
    [SerializeField] private GameObject howToPlayPanel;
    [SerializeField] private GameObject[] pages;
    [SerializeField] private GameObject nextButton;
    [SerializeField] private GameObject previousButton;

    private int currentPage = 0;

    private void Start()
    {
        ShowStart();
    }

    public void ShowStart()
    {
        startButton.SetActive(true);
        howToPlayButton.SetActive(false);
        playButton.SetActive(false);
        returnButton.SetActive(false);
        howToPlayPanel.SetActive(false);
    }

    public void OpenMainOptions()
    {
        startButton.SetActive(false);
        howToPlayButton.SetActive(true);
        playButton.SetActive(true);
        returnButton.SetActive(true);
        howToPlayPanel.SetActive(false);
    }

    public void OpenHowToPlay()
    {
        startButton.SetActive(false);
        howToPlayButton.SetActive(false);
        playButton.SetActive(false);
        returnButton.SetActive(true);
        howToPlayPanel.SetActive(true);

        ShowPage(0);
    }

    private void ShowPage(int index)
    {
        if (index < 0 || index >= pages.Length)
            return;

        currentPage = index;

        for (int i = 0; i < pages.Length; i++)
        {
            pages[i].SetActive(i == currentPage);
        }

        previousButton.SetActive(currentPage > 0);
        nextButton.SetActive(currentPage < pages.Length - 1);
    }

    public void NextPage()
    {
        ShowPage(currentPage + 1);
    }

    public void PreviousPage()
    {
        ShowPage(currentPage - 1);
    }

    public void Return()
    {
        if (howToPlayPanel.activeSelf)
        {
            OpenMainOptions();
        }
        else
        {
            ShowStart();
        }
    }
}
