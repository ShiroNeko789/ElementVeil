using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using TMPro;


public class BalloonMiniGamePanel : MonoBehaviour
{
    [Header("Panel Visuals — child to show/hide (NOT this object)")]
    public GameObject panelVisuals;

    [Header("Progress Bar")]
    [Tooltip("Image Type = Filled, Fill Method = Vertical, Fill Origin = Bottom")]
    public Image progressFill;
    public float fillRate = 0.18f;   // Progress added per button press
    public float drainRate = 0.12f;  // Progress lost per second when idle

    [Header("Buttons")]
    public Button leftButton;
    public Button rightButton;

    [Header("Button Flash Colors")]
    public Image leftButtonImage;
    public Image rightButtonImage;
    public Color pressedColor = new Color(0.5f, 1f, 0.5f);
    public Color normalColor = Color.white;

    [Header("Text")]
    public TextMeshProUGUI instructionText;

    [Header("Panel Shake (assign panel's RectTransform)")]
    public RectTransform panelRect;
    public float shakeAmount = 3f;

    private float currentProgress = 0f;
    private bool isActive = false;
    private HotAirBalloon balloon;
    private Coroutine drainCoroutine;
    private Vector3 originalPanelPos;

    void Awake()
    {
        // Visuals hidden, but THIS object stays active always
        if (panelVisuals != null) panelVisuals.SetActive(false);
        if (progressFill != null) progressFill.fillAmount = 0f;

        if (leftButton != null) leftButton.onClick.AddListener(OnLeftPress);
        if (rightButton != null) rightButton.onClick.AddListener(OnRightPress);

        if (panelRect != null) originalPanelPos = panelRect.anchoredPosition3D;
    }

    // ── Open / Close ───────────────────────────────────────────────────────

    public void Open(HotAirBalloon targetBalloon)
    {
        balloon = targetBalloon;
        currentProgress = 0f;
        isActive = true;

        if (progressFill != null) progressFill.fillAmount = 0f;
        if (panelVisuals != null) panelVisuals.SetActive(true);
        if (instructionText != null) instructionText.text = "Keep pressing to launch!";

        // Drain loop uses unscaled time so it works with timeScale = 0
        drainCoroutine = StartCoroutine(DrainLoop());
    }

    public void Close()
    {
        isActive = false;
        if (drainCoroutine != null) { StopCoroutine(drainCoroutine); drainCoroutine = null; }
        if (panelVisuals != null) panelVisuals.SetActive(false);
        currentProgress = 0f;
        if (progressFill != null) progressFill.fillAmount = 0f;
    }

    // ── Button Handlers ────────────────────────────────────────────────────

    public void OnLeftPress()
    {
        if (!isActive) return;
        AddProgress();
        StartCoroutine(FlashButton(leftButtonImage));
    }

    public void OnRightPress()
    {
        if (!isActive) return;
        AddProgress();
        StartCoroutine(FlashButton(rightButtonImage));
    }

    void AddProgress()
    {
        currentProgress = Mathf.Clamp01(currentProgress + fillRate);
        UpdateBar();
        if (panelRect != null) StartCoroutine(ShakeRoutine());

        if (instructionText != null)
        {
            if (currentProgress < 0.33f) instructionText.text = "Keep pressing!";
            else if (currentProgress < 0.66f) instructionText.text = "Almost there!";
            else instructionText.text = "Don't stop now!";
        }

        if (currentProgress >= 1f)
            StartCoroutine(WinSequence());
    }

    // ── Drain Loop ─────────────────────────────────────────────────────────

    IEnumerator DrainLoop()
    {
        while (isActive)
        {
            // unscaledDeltaTime works even when timeScale = 0
            currentProgress = Mathf.Clamp01(currentProgress - drainRate * Time.unscaledDeltaTime);
            UpdateBar();
            yield return null;
        }
    }

    void UpdateBar()
    {
        if (progressFill != null) progressFill.fillAmount = currentProgress;
    }

    // ── Win ────────────────────────────────────────────────────────────────

    IEnumerator WinSequence()
    {
        isActive = false;
        if (drainCoroutine != null) { StopCoroutine(drainCoroutine); drainCoroutine = null; }

        if (progressFill != null) progressFill.fillAmount = 1f;
        if (instructionText != null) instructionText.text = "LAUNCHING! 🎈";

        yield return new WaitForSecondsRealtime(0.8f);

        balloon?.OnMiniGameWon();
    }

    // ── Visuals ────────────────────────────────────────────────────────────

    IEnumerator FlashButton(Image img)
    {
        if (img == null) yield break;
        img.color = pressedColor;
        yield return new WaitForSecondsRealtime(0.08f);
        img.color = normalColor;
    }

    IEnumerator ShakeRoutine()
    {
        float elapsed = 0f;
        while (elapsed < 0.1f)
        {
            panelRect.anchoredPosition = (Vector2)originalPanelPos + Random.insideUnitCircle * shakeAmount;
            elapsed += Time.unscaledDeltaTime;
            yield return null;
        }
        panelRect.anchoredPosition = originalPanelPos;
    }
}