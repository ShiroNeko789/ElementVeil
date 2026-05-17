using UnityEngine;
using UnityEngine.UI;
using System;
using System.Collections;

public class HealingFountain : MonoBehaviour
{
    [Header("UI")]
    public GameObject interactButton;   // shows when player is near
    public GameObject savePanel;        // the PSP-style saving panel
    public float savePanelDuration = 2f;

    [Header("References")]
    public PlayerHealth playerHealth;

    private bool playerNearby = false;

    void Start()
    {
        if (interactButton != null) interactButton.SetActive(false);
        if (savePanel != null) savePanel.SetActive(false);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Player")) return;
        playerNearby = true;
        if (interactButton != null) interactButton.SetActive(true);
        if (playerHealth == null)
            playerHealth = other.GetComponent<PlayerHealth>();
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (!other.CompareTag("Player")) return;
        playerNearby = false;
        if (interactButton != null) interactButton.SetActive(false);
    }

    public void OnInteractPressed()
    {
        if (!playerNearby) return;
        playerHealth.RestoreFullHealth();
        interactButton.SetActive(false);
        StartCoroutine(ShowSavePanel());
    }

    IEnumerator ShowSavePanel()
    {
        // Freeze player
        Time.timeScale = 0f;
        Rigidbody2D playerRb = playerHealth.GetComponent<Rigidbody2D>();
        if (playerRb != null) playerRb.linearVelocity = Vector2.zero;

        // Save the game
        GameSaveManager.Get()?.SaveGame();

        if (savePanel != null)
        {
            savePanel.SetActive(true);
            yield return new WaitForSecondsRealtime(savePanelDuration);
            savePanel.SetActive(false);
        }

        // Unfreeze player
        Time.timeScale = 1f;
    }
}