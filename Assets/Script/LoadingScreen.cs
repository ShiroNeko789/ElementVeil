using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Collections;
using TMPro;

public class LoadingScreen : MonoBehaviour
{
    public static string TargetScene = "";

    [Header("UI")]
    public Image progressBarFill;
    public TextMeshProUGUI loadingText;

    [Header("Wipe Transition")]
    public RectTransform wipePanel;      // full screen black panel
    public float wipeInDuration = 0.5f;  // how fast black covers screen
    public float wipeOutDuration = 0.8f; // how fast black reveals scene

    private string[] loadingMessages = {
        "Loading...",
        "Preparing world...",
        "Almost there..."
    };

    void Start()
    {
        if (string.IsNullOrEmpty(TargetScene))
        {
            TargetScene = "MainMenu";
        }

        // Start wipe panel fully covering screen (right side)
        if (wipePanel != null)
            wipePanel.anchoredPosition = new Vector2(0, 0);

        StartCoroutine(LoadAsync());
    }

    IEnumerator LoadAsync()
    {
        // Wipe IN — black panel already covers screen, just start loading
        yield return StartCoroutine(WipeOut()); // reveal loading screen first

        AsyncOperation op = SceneManager.LoadSceneAsync(TargetScene);
        op.allowSceneActivation = false;

        float displayProgress = 0f;
        float targetProgress = 0f;
        float currentSpeed = 0f;
        int msgIndex = 0;

        while (!op.isDone)
        {
            float realProgress = Mathf.Clamp01(op.progress / 0.9f);

            targetProgress = Mathf.MoveTowards(
                targetProgress,
                realProgress,
                Time.deltaTime * Random.Range(0.05f, 0.4f)
            );

            bool stutter = Random.value < 0.01f;
            if (!stutter)
            {
                float desiredSpeed = Random.Range(0.3f, 2.5f);
                currentSpeed = Mathf.Lerp(currentSpeed, desiredSpeed, Time.deltaTime * 3f);
                displayProgress = Mathf.MoveTowards(displayProgress, targetProgress, Time.deltaTime * currentSpeed);
            }

            displayProgress = Mathf.Clamp(displayProgress, 0f, 0.95f);

            if (progressBarFill != null)
                progressBarFill.fillAmount = displayProgress;

            int newMsg = Mathf.FloorToInt(displayProgress * loadingMessages.Length);
            newMsg = Mathf.Clamp(newMsg, 0, loadingMessages.Length - 1);
            if (newMsg != msgIndex)
            {
                msgIndex = newMsg;
                if (loadingText != null)
                    loadingText.text = loadingMessages[msgIndex];
            }

            if (op.progress >= 0.9f)
            {
                // Fill bar to 100%
                while (displayProgress < 1f)
                {
                    displayProgress = Mathf.MoveTowards(displayProgress, 1f, Time.deltaTime * 1.5f);
                    if (progressBarFill != null) progressBarFill.fillAmount = displayProgress;
                    yield return null;
                }

                if (loadingText != null) loadingText.text = "Done!";
                yield return new WaitForSeconds(0.2f);

                // Wipe IN black before switching scene
                yield return StartCoroutine(WipeIn());

                op.allowSceneActivation = true;
            }

            yield return null;
        }
    }

    // Black panel slides in from right to left — covers screen
    IEnumerator WipeIn()
    {
        if (wipePanel == null) yield break;

        float screenWidth = ((RectTransform)wipePanel.parent).rect.width;
        float elapsed = 0f;

        // Start from right edge
        wipePanel.anchoredPosition = new Vector2(screenWidth, 0);

        while (elapsed < wipeInDuration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.SmoothStep(0f, 1f, elapsed / wipeInDuration);
            // Slide from right (screenWidth) to center (0)
            wipePanel.anchoredPosition = new Vector2(Mathf.Lerp(screenWidth, 0, t), 0);
            yield return null;
        }

        wipePanel.anchoredPosition = Vector2.zero;
    }

    // Black panel slides out from right to left — reveals scene
    IEnumerator WipeOut()
    {
        if (wipePanel == null) yield break;

        float screenWidth = ((RectTransform)wipePanel.parent).rect.width;
        float elapsed = 0f;

        // Start covering screen
        wipePanel.anchoredPosition = Vector2.zero;

        while (elapsed < wipeOutDuration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.SmoothStep(0f, 1f, elapsed / wipeOutDuration);
            // Slide from center (0) to left (-screenWidth)
            wipePanel.anchoredPosition = new Vector2(Mathf.Lerp(0, -screenWidth, t), 0);
            yield return null;
        }

        wipePanel.anchoredPosition = new Vector2(-screenWidth, 0);
    }
}