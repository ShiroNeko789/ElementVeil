using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Collections;
using TMPro;

public class MainMenuUI : MonoBehaviour
{
    [Header("Buttons")]
    public Button newGameButton;
    public Button loadGameButton;
    public Button settingsButton;

    [Header("Feedback")]
    public GameObject noSaveText;       // "No saved data" message
    public GameObject loadingPanel;     // simple "Loading..." panel

    [Header("Scene Name")]
    public string gameSceneName = "Game"; // exact name of your game scene

    void Start()
    {
        if (noSaveText != null) noSaveText.SetActive(false);
        if (loadingPanel != null) loadingPanel.SetActive(false);

        newGameButton.onClick.AddListener(StartNewGame);
        loadGameButton.onClick.AddListener(LoadGame);

        // Grey out Load button if no save exists
        if (!SaveSystem.HasSave())
        {
            loadGameButton.interactable = false;
            ColorBlock cb = loadGameButton.colors;
            cb.disabledColor = new Color(0.5f, 0.5f, 0.5f, 0.5f);
            loadGameButton.colors = cb;
        }
    }

    void StartNewGame()
    {
        SaveSystem.DeleteSave();
        PlayerPrefs.SetInt("ShouldLoadSave", 0);
        PlayerPrefs.SetInt("IsNewGame", 1);  // only new game clears inventory
        PlayerPrefs.Save();
        StartCoroutine(LoadScene(false));
    }

    void LoadGame()
    {
        if (!SaveSystem.HasSave())
        {
            StartCoroutine(ShowNoSaveMessage());
            return;
        }
        PlayerPrefs.SetInt("IsNewGame", 0);
        StartCoroutine(LoadScene(true));
    }

    IEnumerator LoadScene(bool shouldLoad)
    {
        if (loadingPanel != null) loadingPanel.SetActive(true);
        PlayerPrefs.SetInt("ShouldLoadSave", shouldLoad ? 1 : 0);
        PlayerPrefs.Save();
        yield return new WaitForSecondsRealtime(0.3f);

        // Use transition manager instead of direct load
        SceneTransitionManager.Instance.LoadScene(gameSceneName);
    }

    IEnumerator ShowNoSaveMessage()
    {
        if (noSaveText != null)
        {
            noSaveText.SetActive(true);
            yield return new WaitForSecondsRealtime(2f);
            noSaveText.SetActive(false);
        }
    }
}