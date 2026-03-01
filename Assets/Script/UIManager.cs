using UnityEngine;
using UnityEngine.SceneManagement; // Added for Restart/Quit functions

public class UIManager : MonoBehaviour
{
    public static UIManager Instance { get; private set; }

    [Header("Menu Panels")]
    public GameObject inventoryPanel;
    public GameObject pauseMenuPanel;

    [Header("Status")]
    public bool isPaused = false;

    void Awake()
    {
        // Singleton setup
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    void Update()
    {
        // PC Controls (Will be buttons on Mobile later)
        if (Input.GetKeyDown(KeyCode.I) || Input.GetKeyDown(KeyCode.Tab))
        {
            ToggleInventory();
        }

        if (Input.GetKeyDown(KeyCode.Escape))
        {
            TogglePauseMenu();
        }
    }

    public void ToggleInventory()
    {
        if (pauseMenuPanel.activeSelf) return; // Don't open inventory if paused

        bool open = !inventoryPanel.activeSelf;
        inventoryPanel.SetActive(open);

        if (open) PauseGame();
        else ResumeGame();
    }

    public void TogglePauseMenu()
    {
        if (inventoryPanel.activeSelf) inventoryPanel.SetActive(false);

        bool open = !pauseMenuPanel.activeSelf;
        pauseMenuPanel.SetActive(open);

        if (open) PauseGame();
        else ResumeGame();
    }

    public void PauseGame()
    {
        Time.timeScale = 0f;
        isPaused = true;
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    public void ResumeGame()
    {
        Time.timeScale = 1f;
        isPaused = false;
        // In 2D games, we usually keep the cursor visible or confined
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    // Future-Proof Functions for your Buttons
    public void RestartLevel()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    public void QuitGame()
    {
        Application.Quit();
        Debug.Log("Game Exited");
    }
}