using UnityEngine;
using System.Collections;

public class LightningStrike : MonoBehaviour
{
    [Header("Settings")]
    public int damagePerHit = 1;
    public float damageInterval = 0.3f;  // very fast damage, no cd

    [Header("Visual")]
    public Color lightningColor = new Color(1f, 1f, 0f, 1f);

    private bool playerInside = false;
    private GameObject currentPlayer;

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Player")) return;

        // Check if player has glove equipped and activated
        GloveController glove = other.GetComponent<GloveController>();
        if (glove != null && glove.IsGloveActive())
        {
            Debug.Log("Glove protected player from lightning");
            return;
        }

        playerInside = true;
        currentPlayer = other.gameObject;
        StartCoroutine(DamagePlayer(other.gameObject));
    }

    private void OnTriggerStay2D(Collider2D other)
    {
        if (!other.CompareTag("Player")) return;

        // Recheck glove status while inside
        GloveController glove = other.GetComponent<GloveController>();
        if (glove != null && glove.IsGloveActive())
            playerInside = false;
        else
            playerInside = true;
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (!other.CompareTag("Player")) return;
        playerInside = false;
        currentPlayer = null;
    }

    IEnumerator DamagePlayer(GameObject player)
    {
        PlayerHealth ph = player.GetComponent<PlayerHealth>();
        if (ph == null) yield break;

        while (playerInside)
        {
            // Bypass iframes — lightning hits fast
            ph.TakeLightningDamage(damagePerHit);
            yield return new WaitForSeconds(damageInterval);
        }
    }
}