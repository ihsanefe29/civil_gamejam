
using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class LevelIntroController : MonoBehaviour
{
    [Header("Intro Canvas")]
    [SerializeField] private GameObject levelIntroCanvas;

    [Header("Cover")]
    [SerializeField] private GameObject coverBackground;
    [SerializeField] private GameObject levelCover;
    [SerializeField] private float coverDuration = 3f;

    [Header("Welcome Panel")]
    [SerializeField] private GameObject welcomePanel;
    [SerializeField] private GameObject storyPage;
    [SerializeField] private GameObject controlsPage;

    [Header("Buttons")]
    [SerializeField] private Button nextButton;
    [SerializeField] private Button previousButton;

    private int currentPage = 0;
    private bool levelStarted = false;

    private IEnumerator Start()
    {
        // Pause gameplay during introduction.
        Time.timeScale = 0f;

        levelIntroCanvas.SetActive(true);

        // Display only the cover for 3 seconds.
        coverBackground.SetActive(true);
        levelCover.SetActive(true);
        welcomePanel.SetActive(false);

        // Real-time waiting works while the game is paused.
        yield return new WaitForSecondsRealtime(coverDuration);

        // Keep the tree cover behind the welcome panel.
        coverBackground.SetActive(true);
        levelCover.SetActive(true);
        welcomePanel.SetActive(true);

        ShowPage(0);
    }

    private void ShowPage(int page)
    {
        currentPage = page;

        storyPage.SetActive(page == 0);
        controlsPage.SetActive(page == 1);

        // Hide previous button on the first page.
        previousButton.gameObject.SetActive(page > 0);
    }

    public void NextPage()
    {
        if (levelStarted) return;

        if (currentPage == 0)
        {
            ShowPage(1);
        }
        else
        {
            StartLevel();
        }
    }

    public void PreviousPage()
    {
        if (currentPage > 0)
        {
            ShowPage(currentPage - 1);
        }
    }

    private void StartLevel()
    {
        if (levelStarted) return;

        levelStarted = true;

        // Hide the entire introduction UI.
        levelIntroCanvas.SetActive(false);

        // Resume gameplay and earthquake timer.
        Time.timeScale = 1f;

        Debug.Log("Level 1 started!");
    }

    private void OnDestroy()
    {
        // Restore time if this scene is unloaded during intro.
        Time.timeScale = 1f;
    }
}
