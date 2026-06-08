using UnityEngine;
using System.Collections;

public class FloorLightning : MonoBehaviour
{
    [Header("Settings")]
    public float activeDuration = 2f;   // how long LightningContinue plays
    public int damage = 1;

    private Animator anim;
    private Collider2D col;
    private bool isActive = false;
    private bool playerInside = false;
    private GameObject currentPlayer;

    void Start()
    {
        anim = GetComponent<Animator>();
        col = GetComponent<Collider2D>();

        // Collider off at start — enabled after Lightning intro finishes
        if (col != null) col.enabled = false;

        StartCoroutine(LightningSequence());
    }

    IEnumerator LightningSequence()
    {
        // Play Lightning intro animation
        anim.Play("Lightning");

        // Wait for Lightning clip to finish (0.3s based on your setup)
        yield return new WaitForSeconds(0.3f);

        // Enable collider — lightning is now active
        if (col != null) col.enabled = true;
        isActive = true;

        // Play LightningContinue loop
        anim.Play("LightningContinue");

        // Stay active for duration
        yield return new WaitForSeconds(activeDuration);

        // Disable collider — stop damaging
        isActive = false;
        if (col != null) col.enabled = false;

        // Play LightningEnd
        anim.Play("LightningEnd");

        // Wait for end animation to finish then destroy
        yield return new WaitForSeconds(0.3f);

        Destroy(gameObject);
    }

    IEnumerator DamageWhileInside()
    {
        while (playerInside && isActive)
        {
            // Check glove protection
            GloveController glove = currentPlayer?.GetComponent<GloveController>();
            if (glove != null && glove.IsGloveActive()) yield break;

            PlayerHealth ph = currentPlayer?.GetComponent<PlayerHealth>();
            ph?.TakeLightningDamage(damage);

            yield return new WaitForSeconds(0.3f);
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Player")) return;

        GloveController glove = other.GetComponent<GloveController>();
        if (glove != null && glove.IsGloveActive()) return;

        playerInside = true;
        currentPlayer = other.gameObject;
        StartCoroutine(DamageWhileInside());
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (!other.CompareTag("Player")) return;
        playerInside = false;
        currentPlayer = null;
    }
}