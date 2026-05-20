using UnityEngine;
using System.Collections;

public class WindZone : MonoBehaviour
{
    public WindBlowMachine machine;

    [Header("Wind Force")]
    public float windForce = 35f;
    public float windLiftForce = 8f;
    public float boostInterval = 0.02f;
    public float maxSpeed = 20f;

    [Header("Wind Visual")]
    public GameObject windVisual;
    public Animator windAnimator;

    private Rigidbody2D playerRb;
    private bool playerInside = false;
    private Coroutine windCoroutine;

    void Start()
    {
        // Hide wind effect at start
        if (windVisual != null)
            windVisual.SetActive(false);
    }

    // ─────────────────────────────────────────────
    // ACTIVATE WIND
    // ─────────────────────────────────────────────

    public void ActivateWind()
    {
        // Show wind visual
        if (windVisual != null)
            windVisual.SetActive(true);

        // Play animation
        if (windAnimator != null)
            windAnimator.Play("WindZone");

        Debug.Log("Wind zone activated");
    }

    // ─────────────────────────────────────────────
    // DEACTIVATE WIND
    // ─────────────────────────────────────────────

    public void DeactivateWind()
    {
        if (windVisual != null)
            windVisual.SetActive(false);

        playerInside = false;

        if (windCoroutine != null)
            StopCoroutine(windCoroutine);
    }

    // ─────────────────────────────────────────────
    // PLAYER ENTER
    // ─────────────────────────────────────────────

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Player"))
            return;

        if (machine == null)
            return;

        if (!machine.isActivated)
            return;

        playerRb = other.GetComponent<Rigidbody2D>();

        if (playerRb == null)
            return;

        playerInside = true;

        // Start wind
        windCoroutine = StartCoroutine(ApplyWind());
    }

    // ─────────────────────────────────────────────
    // PLAYER EXIT
    // ─────────────────────────────────────────────

    private void OnTriggerExit2D(Collider2D other)
    {
        if (!other.CompareTag("Player"))
            return;

        playerInside = false;
        playerRb = null;

        if (windCoroutine != null)
            StopCoroutine(windCoroutine);
    }

    // ─────────────────────────────────────────────
    // APPLY WIND
    // ─────────────────────────────────────────────

    IEnumerator ApplyWind()
    {
        while (playerInside && machine.isActivated)
        {
            if (playerRb != null)
            {
                // Push player
                playerRb.AddForce(
                    new Vector2(windForce, windLiftForce),
                    ForceMode2D.Impulse
                );

                // Clamp horizontal speed
                if (playerRb.linearVelocity.x > maxSpeed)
                {
                    playerRb.linearVelocity = new Vector2(
                        maxSpeed,
                        playerRb.linearVelocity.y
                    );
                }
            }

            yield return new WaitForSeconds(boostInterval);
        }
    }
}