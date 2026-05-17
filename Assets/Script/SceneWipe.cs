using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class SceneWipe : MonoBehaviour
{
    public static SceneWipe Instance;

    [Header("Wipe Panel")]
    public RectTransform wipePanel;     // full screen black panel in THIS scene
    public float wipeOutDuration = 0.8f;

    void Awake()
    {
        Instance = this;
    }

    void Start()
    {
        // Every time scene loads, wipe out from black to reveal the scene
        StartCoroutine(WipeOut());
    }

    IEnumerator WipeOut()
    {
        if (wipePanel == null) yield break;

        float screenWidth = ((RectTransform)wipePanel.parent).rect.width;
        float elapsed = 0f;

        // Start fully covering screen
        wipePanel.anchoredPosition = Vector2.zero;
        wipePanel.gameObject.SetActive(true);

        while (elapsed < wipeOutDuration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.SmoothStep(0f, 1f, elapsed / wipeOutDuration);
            // Slide left to reveal scene underneath
            wipePanel.anchoredPosition = new Vector2(Mathf.Lerp(0, -screenWidth, t), 0);
            yield return null;
        }

        // Fully off screen — hide it
        wipePanel.anchoredPosition = new Vector2(-screenWidth, 0);
        wipePanel.gameObject.SetActive(false);
    }

    // Call this before leaving the scene to wipe back to black
    public IEnumerator WipeIn(float duration = 0.5f)
    {
        if (wipePanel == null) yield break;

        float screenWidth = ((RectTransform)wipePanel.parent).rect.width;
        float elapsed = 0f;

        wipePanel.anchoredPosition = new Vector2(screenWidth, 0);
        wipePanel.gameObject.SetActive(true);

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.SmoothStep(0f, 1f, elapsed / duration);
            wipePanel.anchoredPosition = new Vector2(Mathf.Lerp(screenWidth, 0, t), 0);
            yield return null;
        }

        wipePanel.anchoredPosition = Vector2.zero;
    }
}