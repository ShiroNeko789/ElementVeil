using UnityEngine;
using System.Collections;

public class DeathZone : MonoBehaviour
{
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Player")) return;
        StartCoroutine(FallDeath(other.gameObject));
    }

    IEnumerator FallDeath(GameObject player)
    {
        // Freeze player immediately
        Rigidbody2D rb = player.GetComponent<Rigidbody2D>();
        if (rb != null)
        {
            rb.linearVelocity = Vector2.zero;
            rb.simulated = false;
        }

        // Wipe to black
        if (SceneWipe.Instance != null)
            yield return StartCoroutine(SceneWipe.Instance.WipeIn(0.4f));

        // Reload from last save
        if (SaveSystem.HasSave())
        {
            PlayerPrefs.SetInt("ShouldLoadSave", 1);
            PlayerPrefs.Save();
        }

        // Re-enable physics before scene reload
        if (rb != null) rb.simulated = true;

        // Reload current scene
        UnityEngine.SceneManagement.SceneManager.LoadScene(
            UnityEngine.SceneManagement.SceneManager.GetActiveScene().name
        );
    }
}