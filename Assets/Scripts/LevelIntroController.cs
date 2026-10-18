
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

    [Header("Optional Ending Animation")]
    [SerializeField] private WhaleIntroAnimation whaleAnimation;

    private int currentPage = 0;
    private bool levelStarted = false;
    private bool transitionStarted = false;

    private IEnumerator Start()
    {
        Time.timeScale = 0f;

        levelIntroCanvas.SetActive(true);
        coverBackground.SetActive(true);
        levelCover.SetActive(true);
        welcomePanel.SetActive(false);

        if (whaleAnimation != null)
            whaleAnimation.gameObject.SetActive(false);

        yield return new WaitForSecondsRealtime(coverDuration);

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

        previousButton.gameObject.SetActive(page > 0);
    }

    public void NextPage()
    {
        if (levelStarted || transitionStarted)
            return;

        if (currentPage == 0)
        {
            ShowPage(1);
        }
        else
        {
            if (whaleAnimation != null)
                StartCoroutine(PlayWhaleThenStart());
            else
                StartLevel();
        }
    }

    public void PreviousPage()
    {
        if (levelStarted || transitionStarted)
            return;

        if (currentPage > 0)
            ShowPage(currentPage - 1);
    }
private IEnumerator PlayWhaleThenStart()
{
    transitionStarted = true;

    // Hide intro visuals
    welcomePanel.SetActive(false);
    levelCover.SetActive(false);
    coverBackground.SetActive(false);

    // Play whale animation
    yield return StartCoroutine(
        whaleAnimation.PlayAnimation()
    );

    // Start gameplay after animation
    StartLevel();
}

    private void StartLevel()
    {
        if (levelStarted)
            return;

        levelStarted = true;
        levelIntroCanvas.SetActive(false);
        Time.timeScale = 1f;

        Debug.Log("Level started!");
    }

    private void OnDestroy()
    {
        Time.timeScale = 1f;
    }
}
