using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Collections;

public class PauseMenu : MonoBehaviour
{
    public static PauseMenu Instance;

    [Header("Panels")]
    public GameObject pausePanel;
    public GameObject savingPanel;

    [Header("Buttons")]
    public Button pauseButton;
    public Button resumeButton;
    public Button saveButton;
    public Button mainMenuButton;

    public float savingDisplayTime = 1.5f;
    private bool isPaused = false;
    private float lastPauseTime = -1f;

    void Awake()
    {
        Instance = this;
        Time.timeScale = 1f;
        isPaused = false;
    }

    void Start()
    {
        if (pausePanel != null) pausePanel.SetActive(false);
        if (savingPanel != null) savingPanel.SetActive(false);

        if (pauseButton != null)
        {
            pauseButton.onClick.RemoveAllListeners();
            pauseButton.onClick.AddListener(TogglePause);
        }
        if (resumeButton != null)
        {
            resumeButton.onClick.RemoveAllListeners();
            resumeButton.onClick.AddListener(Resume);
        }
        if (saveButton != null)
        {
            saveButton.onClick.RemoveAllListeners();
            saveButton.onClick.AddListener(OnSavePressed);
        }
        if (mainMenuButton != null)
        {
            mainMenuButton.onClick.RemoveAllListeners();
            mainMenuButton.onClick.AddListener(GoToMainMenu);
        }

        Debug.Log("PauseMenu Start — isPaused: " + isPaused);
    }

    public void TogglePause()
    {
        // Prevent double firing within 0.2 seconds
        if (Time.unscaledTime - lastPauseTime < 0.2f)
        {
            Debug.Log("TogglePause blocked — too fast");
            return;
        }
        lastPauseTime = Time.unscaledTime;

        Debug.Log("TogglePause called — isPaused: " + isPaused);
        if (isPaused) Resume();
        else Pause();
    }

    public void Pause()
    {
        isPaused = true;
        if (pausePanel != null) pausePanel.SetActive(true);
        Time.timeScale = 0f;
        Debug.Log("Paused successfully");
    }

    public void Resume()
    {
        isPaused = false;
        if (pausePanel != null) pausePanel.SetActive(false);
        Time.timeScale = 1f;
        Debug.Log("Resumed");
    }

    public bool IsPaused() { return isPaused; }

    public void OnSavePressed()
    {
        if (GameSaveManager.Get() == null)
        {
            Debug.LogError("GameSaveManager not found");
            return;
        }
        GameSaveManager.Get().SaveGame();
        StartCoroutine(ShowSavingPanel());
    }

    IEnumerator ShowSavingPanel()
    {
        if (savingPanel != null)
        {
            savingPanel.SetActive(true);
            yield return new WaitForSecondsRealtime(savingDisplayTime);
            savingPanel.SetActive(false);
        }
    }

    public void GoToMainMenu()
    {
        isPaused = false;
        Time.timeScale = 1f;
        SceneTransitionManager.Instance.LoadScene("MainMenu");
    }

    void OnDestroy()
    {
        Time.timeScale = 1f;
    }
}