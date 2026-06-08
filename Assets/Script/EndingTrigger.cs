using UnityEngine;
using System.Collections;

public class EndingTrigger : MonoBehaviour
{
    [Header("UI")]
    public GameObject endingUI;
    public float delayBeforeShow = 0.5f;

    private bool triggered = false;

    void Start()
    {
        if (endingUI != null) endingUI.SetActive(false);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (triggered) return;
        if (!other.CompareTag("Player")) return;
        triggered = true;
        StartCoroutine(ShowEnding());
    }

    IEnumerator ShowEnding()
    {
        // Freeze game
        Time.timeScale = 0f;

        // Disable player input
        MonoBehaviour[] playerScripts = FindObjectOfType<PlayerHealth>()
            ?.GetComponents<MonoBehaviour>();
        if (playerScripts != null)
            foreach (var s in playerScripts)
                if (s != FindObjectOfType<PlayerHealth>())
                    s.enabled = false;

        yield return new WaitForSecondsRealtime(delayBeforeShow);

        if (endingUI != null) endingUI.SetActive(true);
    }
}