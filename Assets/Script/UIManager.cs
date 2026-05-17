using UnityEngine;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
    public static UIManager Instance;

    [Header("Buttons")]
    public GameObject pauseButton;
    public GameObject inventoryButton;
    public GameObject interactButton;

    [Header("Panels")]
    public GameObject inventoryPanel;
    public GameObject workbenchPanel;
    public GameObject pausePanel;

    void Awake()
    {
        Instance = this;
    }

    public void OnPanelOpened(GameObject openedPanel)
    {
        SetNavButtons(false);

        if (openedPanel != inventoryPanel && inventoryPanel != null) inventoryPanel.SetActive(false);
        if (openedPanel != workbenchPanel && workbenchPanel != null) workbenchPanel.SetActive(false);
        if (openedPanel != pausePanel && pausePanel != null) pausePanel.SetActive(false);

        // Close pause menu if opening inventory or workbench
        if (PauseMenu.Instance != null && openedPanel != pausePanel)
            PauseMenu.Instance.Resume();
    }

    public void OnAllPanelsClosed()
    {
        SetNavButtons(true);
    }

    void SetNavButtons(bool active)
    {
        if (pauseButton != null) pauseButton.SetActive(active);
        if (inventoryButton != null) inventoryButton.SetActive(active);
    }
}