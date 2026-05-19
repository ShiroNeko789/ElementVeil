using UnityEngine;
using System.Collections;

// Attach to a SEPARATE child GameObject that covers the wind area.
// Its Collider2D must have Is Trigger = true.
public class WindZone : MonoBehaviour
{
    public WindBlowMachine machine;

    [Header("Wind Force")]
    public float windForce = 15f;
    public float windLiftForce = 8f;

    [Header("Gravity Compensation")]
    [Tooltip("Match this to your player Rigidbody2D gravity scale (yours is 5).")]
    public float playerGravityScale = 5f;
    [Tooltip("1 = just barely floats. 2 = lifts upward comfortably.")]
    public float liftMultiplier = 2f;

    [Header("Speed Caps")]
    public float maxHorizontalSpeed = 12f;
    public float maxVerticalSpeed = 10f;

    private Rigidbody2D playerRb;
    private bool playerInside = false;
    private Coroutine windCoroutine = null;

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Player")) return;

        playerRb = other.GetComponent<Rigidbody2D>();
        playerInside = true;

        Debug.Log("[WindZone] Player entered. Machine activated: " + machine.isActivated);

        if (machine.isActivated)
            StartWindCoroutine();
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (!other.CompareTag("Player")) return;

        Debug.Log("[WindZone] Player exited.");
        playerInside = false;
        playerRb = null;
        StopWindCoroutine();
    }

    public void OnMachineActivated()
    {
        Debug.Log("[WindZone] OnMachineActivated. playerInside: " + playerInside);
        if (playerInside && playerRb != null)
            StartWindCoroutine();
    }

    private void StartWindCoroutine()
    {
        if (windCoroutine != null) return;
        windCoroutine = StartCoroutine(ApplyWind());
    }

    private void StopWindCoroutine()
    {
        if (windCoroutine != null)
        {
            StopCoroutine(windCoroutine);
            windCoroutine = null;
        }
    }

    IEnumerator ApplyWind()
    {
        Debug.Log("[WindZone] ApplyWind coroutine started.");

        while (playerInside && machine.isActivated)
        {
            if (playerRb != null)
            {
                // Calculate exactly how much force is needed to beat gravity at scale 5,
                // then multiply so the player actually rises
                float gravityCompensation = Physics2D.gravity.magnitude * playerGravityScale * playerRb.mass;
                float totalLift = (gravityCompensation * liftMultiplier) + windLiftForce;

                playerRb.AddForce(new Vector2(windForce, totalLift), ForceMode2D.Force);

                // Cap both axes
                float clampedX = Mathf.Clamp(playerRb.linearVelocity.x, -maxHorizontalSpeed, maxHorizontalSpeed);
                float clampedY = Mathf.Clamp(playerRb.linearVelocity.y, -maxVerticalSpeed, maxVerticalSpeed);
                playerRb.linearVelocity = new Vector2(clampedX, clampedY);
            }

            // WaitForFixedUpdate syncs with physics engine — much more reliable
            // than WaitForSeconds for fighting gravity
            yield return new WaitForFixedUpdate();
        }

        Debug.Log("[WindZone] ApplyWind ended.");
        windCoroutine = null;
    }
}