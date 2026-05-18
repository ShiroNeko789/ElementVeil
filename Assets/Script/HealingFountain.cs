using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class HealingFountain : MonoBehaviour, IInteractable
{
    [Header("UI")]
    public GameObject interactButton;
    public GameObject savePanel;
    public float savePanelDuration = 2f;

    [Header("References")]
    public PlayerHealth playerHealth;
    public Animator fountainAnimator;

    private bool playerNearby = false;
    private bool hasActivated = false;

    void Start()
    {
        if (interactButton != null) interactButton.SetActive(false);
        if (savePanel != null) savePanel.SetActive(false);

        // Play idle immediately on start
        if (fountainAnimator != null)
            fountainAnimator.Play("RecoveryIdle");
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Player")) return;
        if (playerHealth == null)
            playerHealth = other.GetComponent<PlayerHealth>();
        InteractManager.Instance.RegisterInteractable(this);
    }

    // Replace OnTriggerExit2D
    private void OnTriggerExit2D(Collider2D other)
    {
        if (!other.CompareTag("Player")) return;
        InteractManager.Instance.UnregisterInteractable(this);
    }

    public void OnInteract()
    {
        StartCoroutine(HealSequence());
    }

    IEnumerator HealSequence()
    {
        if (!hasActivated)
        {
            hasActivated = true;

            // Play RecoveryStart once
            if (fountainAnimator != null)
                fountainAnimator.Play("RecoveryStart");

            yield return new WaitForSeconds(GetAnimationLength("RecoveryStart"));
        }

        // Then play RecoveryContinue loop
        if (fountainAnimator != null)
            fountainAnimator.Play("RecoveryContinue");

        playerHealth.RestoreFullHealth();

        Time.timeScale = 0f;
        Rigidbody2D playerRb = playerHealth.GetComponent<Rigidbody2D>();
        if (playerRb != null) playerRb.linearVelocity = Vector2.zero;

        GameSaveManager.Get()?.SaveGame();

        if (savePanel != null)
        {
            savePanel.SetActive(true);
            yield return new WaitForSecondsRealtime(savePanelDuration);
            savePanel.SetActive(false);
        }

        Time.timeScale = 1f;
    }

    float GetAnimationLength(string animName)
    {
        if (fountainAnimator == null) return 1f;
        RuntimeAnimatorController ac = fountainAnimator.runtimeAnimatorController;
        foreach (AnimationClip clip in ac.animationClips)
            if (clip.name == animName) return clip.length;
        return 1f;
    }
}