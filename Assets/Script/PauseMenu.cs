using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Collections;

public class PauseMenu : MonoBehaviour
{
    [Header("Panels")]
    public GameObject pausePanel;
    public GameObject savingPanel;

    [Header("Buttons")]
    public Button pauseButton;
    public Button resumeButton;
    public Button saveButton;
    public Button mainMenuButton;

    public float savingDisplayTime = 1.5f;
    public static PauseMenu Instance;
    private bool isPaused = false;

    void Awake()
    {
        Instance = this;
        Time.timeScale = 1f;
        isPaused = false;
    }

    void Start()
    {
        // Force panels inactive
        if (pausePanel != null) pausePanel.SetActive(false);
        if (savingPanel != null) savingPanel.SetActive(false);

        // Wire buttons fresh every start — removes stale listeners first
        WireButton(pauseButton, TogglePause);
        WireButton(resumeButton, Resume);
        WireButton(saveButton, OnSavePressed);
        WireButton(mainMenuButton, GoToMainMenu);

        Debug.Log("PauseMenu Start wired — ID: " + GetInstanceID());
    }

    // Helper that clears old listeners then adds fresh one
    void WireButton(Button btn, UnityEngine.Events.UnityAction action)
    {
        if (btn == null)
        {
            Debug.LogWarning("PauseMenu: a button is not assigned in Inspector");
            return;
        }
        btn.onClick.RemoveAllListeners();
        btn.onClick.AddListener(action);
        Debug.Log("Wired button: " + btn.gameObject.name);
    }

    public void TogglePause()
    {
        Debug.Log("TogglePause — isPaused: " + isPaused + " timeScale: " + Time.timeScale);
        if (isPaused) Resume();
        else Pause();
    }

    public void Pause()
    {
        Debug.Log("Pause() called");
        Debug.Log("pausePanel is null: " + (pausePanel == null));
        if (pausePanel != null)
        {
            Debug.Log("pausePanel parent active: " + pausePanel.transform.parent.gameObject.activeSelf);
            pausePanel.SetActive(true);
            Debug.Log("pausePanel active after set: " + pausePanel.activeSelf);
        }
        isPaused = true;
        Time.timeScale = 0f;
    }

    public void Resume()
    {
        isPaused = false;
        if (pausePanel != null) pausePanel.SetActive(false);
        Time.timeScale = 1f;
        Debug.Log("Resumed");
    }

    public void OnSavePressed()
    {
        Debug.Log("Save pressed");

        if (GameSaveManager.Get() == null)
        {
            Debug.LogError("GameSaveManager not found anywhere in scene");
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
        // Always restore time when this object is destroyed
        Time.timeScale = 1f;
    }

    void Update()
    {
        if (pausePanel == null)
            Debug.LogError("pausePanel is NULL on: " + gameObject.name);
    }
}