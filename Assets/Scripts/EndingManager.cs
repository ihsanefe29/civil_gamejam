
using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class EndingManager : MonoBehaviour
{
    [Header("Ending Canvas")]
    [SerializeField] private GameObject endingCanvas;
    [Header("Flood UI")]
    [SerializeField] private FloodTaskUI floodTaskUI;

    [Header("Backgrounds")]
    [SerializeField] private GameObject classroomBackground;
    [SerializeField] private GameObject sleepBackground;
    [SerializeField] private Image fadeOverlay;

    [Header("Ending Panel")]
    [SerializeField] private GameObject endingTextPanel;
    [SerializeField] private GameObject teacherText;

    [Header("Wake Up Frames - Frame_0 to Frame_12")]
    [SerializeField] private GameObject[] frames;

    [Header("Timing")]
    [SerializeField] private float congratulationsTime = 3f;
    [SerializeField] private float fadeTime = 1.5f;
    [SerializeField] private float frameTime = 0.3f;
    [SerializeField] private float classroomTime = 4f;

    private bool started = false;

    private void Awake()
    {
        // Hide all ending elements at the beginning.
        if (classroomBackground != null)
            classroomBackground.SetActive(false);

        if (sleepBackground != null)
            sleepBackground.SetActive(false);

        if (endingTextPanel != null)
            endingTextPanel.SetActive(false);

        if (teacherText != null)
            teacherText.SetActive(false);

        foreach (GameObject frame in frames)
        {
            if (frame != null)
                frame.SetActive(false);
        }

        // FadeOverlay is transparent initially.
        if (fadeOverlay != null)
        {
            fadeOverlay.gameObject.SetActive(true);
            SetFade(0f);
        }
    }
    public void StartEnding()
    {
        if (started)
            return;

        started = true;

        // Canvas starts only when the level is completed.
        if (endingCanvas != null)
            endingCanvas.SetActive(true);

        StartCoroutine(EndingSequence());
    }

    private IEnumerator EndingSequence()
    {
        // 1. Show congratulations for 3 seconds.
        yield return new WaitForSecondsRealtime(
            congratulationsTime
        );

        // 2. Fade to black.
        yield return StartCoroutine(Fade(0f, 1f));

        // 3. Hide the congratulations panel.
        if (floodTaskUI != null)
            floodTaskUI.HideResults();

        // 4. Prepare waking-up frames.
        if (sleepBackground != null)
            sleepBackground.SetActive(true);

        int lastFrame = frames.Length - 1;

        if (lastFrame >= 0 && frames[lastFrame] != null)
            frames[lastFrame].SetActive(true);

        // 5. Reveal the waking-up scene.
        yield return StartCoroutine(Fade(1f, 0f));

        // 6. Show the teacher's message.
        if (teacherText != null)
            teacherText.SetActive(true);

        // 7. Play waking-up frames in reverse.
        for (int i = lastFrame; i >= 0; i--)
        {
            if (i < lastFrame && frames[i + 1] != null)
                frames[i + 1].SetActive(false);

            if (frames[i] != null)
                frames[i].SetActive(true);

            yield return new WaitForSecondsRealtime(frameTime);
        }

        // 8. Keep Frame_0 visible briefly.
        yield return new WaitForSecondsRealtime(1f);

        // 9. Show the classroom.
        if (sleepBackground != null)
            sleepBackground.SetActive(false);

        if (classroomBackground != null)
            classroomBackground.SetActive(true);

        // TeacherText remains visible.

        // 10. Wait before showing the final panel.
        yield return new WaitForSecondsRealtime(classroomTime);

        // 11. Show Final + Replay.
        if (endingTextPanel != null)
            endingTextPanel.SetActive(true);
    }

    private IEnumerator Fade(float from, float to)
    {
        if (fadeOverlay == null)
            yield break;

        float elapsed = 0f;

        while (elapsed < fadeTime)
        {
            elapsed += Time.unscaledDeltaTime;

            float t = Mathf.Clamp01(elapsed / Mathf.Max(0.01f, fadeTime));

            SetFade(Mathf.Lerp(from, to, t));

            yield return null;
        }

        SetFade(to);
    }

    private void SetFade(float alpha)
    {
        if (fadeOverlay == null)
            return;

        Color color = fadeOverlay.color;
        color.a = alpha;
        fadeOverlay.color = color;
    }

    public void ReplayGame()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene("MainMenu");
    }
}
