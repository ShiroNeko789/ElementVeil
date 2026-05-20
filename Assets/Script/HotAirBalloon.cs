using UnityEngine;
using System.Collections;


public class HotAirBalloon : MonoBehaviour, IInteractable
{
    [Header("Required Item")]
    public Item requiredItem;

    [Header("Panels — both must be ALWAYS ACTIVE GameObjects in the scene")]
    public BalloonInsertPanel insertPanel;
    public BalloonMiniGamePanel miniGamePanel;

    [Header("Balloon Movement")]
    public float riseSpeed = 3f;
    public LayerMask groundLayerMask;

    [Header("Player")]
    public MonoBehaviour playerMovementScript;

    private bool isLaunched = false;
    private bool isRising = false;
    private Rigidbody2D rb;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    // ── IInteractable ──────────────────────────────────────────────────────

    public void OnInteract()
    {
        if (isLaunched) return;
        OpenInsertPanel();
    }

    // ── Insert Panel ───────────────────────────────────────────────────────

    void OpenInsertPanel()
    {
        insertPanel.Open(this, requiredItem);
        Time.timeScale = 0f;
        if (playerMovementScript != null) playerMovementScript.enabled = false;
        UIManager.Instance?.OnPanelOpened(insertPanel.gameObject);
    }

    // Called by BalloonInsertPanel when correct item is confirmed
    public void OnItemInserted()
    {
        insertPanel.Close();
        OpenMiniGame();
    }

    // ── Mini Game Panel ────────────────────────────────────────────────────

    void OpenMiniGame()
    {
        // timeScale stays 0 — MiniGame uses UnscaledDeltaTime internally
        miniGamePanel.Open(this);
    }

    // Called by BalloonMiniGamePanel when progress bar is full
    public void OnMiniGameWon()
    {
        miniGamePanel.Close();

        Time.timeScale = 1f;
        if (playerMovementScript != null) playerMovementScript.enabled = true;
        UIManager.Instance?.OnAllPanelsClosed();

        isLaunched = true;
        isRising = true;

        if (rb != null) rb.gravityScale = 0f;

        StartCoroutine(RiseBalloon());
    }

    // ── Close / Cancel (player presses Close on insert panel) ─────────────

    public void OnCancelled()
    {
        insertPanel.Close();
        Time.timeScale = 1f;
        if (playerMovementScript != null) playerMovementScript.enabled = true;
        UIManager.Instance?.OnAllPanelsClosed();
    }

    // ── Interact zone callbacks ────────────────────────────────────────────

    public void OnPlayerEnter()
    {
        if (!isLaunched)
            InteractManager.Instance.RegisterInteractable(this);
    }

    public void OnPlayerExit()
    {
        InteractManager.Instance.UnregisterInteractable(this);
    }

    // ── Balloon rising ─────────────────────────────────────────────────────

    IEnumerator RiseBalloon()
    {
        Debug.Log("[Balloon] Rising!");
        while (isRising)
        {
            if (rb != null)
                rb.linearVelocity = new Vector2(0f, riseSpeed);
            yield return null;
        }
        if (rb != null) rb.linearVelocity = Vector2.zero;
        Debug.Log("[Balloon] Stopped.");
    }

    void OnCollisionEnter2D(Collision2D collision)
    {
        if (!isRising) return;
        if (((1 << collision.gameObject.layer) & groundLayerMask) != 0)
        {
            isRising = false;
            if (rb != null) rb.linearVelocity = Vector2.zero;
            Debug.Log("[Balloon] Hit ceiling: " + collision.gameObject.name);
        }
    }
}