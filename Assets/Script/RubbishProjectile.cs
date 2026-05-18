using UnityEngine;

/// <summary>
/// Attached ONLY to the flying projectile prefab.
/// - isTrigger must be ON on this prefab's collider.
/// - Do NOT put GroundRubbish.cs on this prefab.
/// - Assign groundRubbishPrefab in the Inspector for safe pickups.
/// </summary>
public class RubbishProjectile : MonoBehaviour
{
    public RubbishType rubbishType;
    public float damage = 1f;

    [Header("Safe Pickup (leave empty for hazard-only projectiles)")]
    [Tooltip("Assign a ground rubbish prefab here to make this a safe pickup. Leave null to make it a damaging hazard.")]
    public GameObject groundRubbishPrefab;

    // Derived at runtime: if a groundRubbishPrefab is assigned, this is a safe pickup
    private bool isSafePickup => groundRubbishPrefab != null;

    private bool hasLanded = false;
    private Rigidbody2D rb;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    public void Launch(float angleDeg, float facingDir, float force)
    {
        float rad = angleDeg * Mathf.Deg2Rad;
        Vector2 dir = new Vector2(facingDir * Mathf.Cos(rad), Mathf.Sin(rad));
        rb.gravityScale = 1.5f;
        rb.linearVelocity = dir * force;
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (hasLanded) return;

        // --- Hit the PLAYER ---
        if (collision.CompareTag("Player"))
        {
            if (!isSafePickup)
            {
                // Only hazards damage the player
                collision.GetComponent<PlayerHealth>()?.TakeDamage(Mathf.RoundToInt(damage), transform.position);
                Destroy(gameObject);
            }
            // Safe pickups do nothing when touching the player mid-flight — just pass through
            return;
        }

        // --- Hit the GROUND ---
        if (collision.CompareTag("Ground") || collision.gameObject.layer == LayerMask.NameToLayer("Ground"))
        {
            hasLanded = true;

            if (isSafePickup)
            {
                // Spawn the interactable ground rubbish object
                GameObject gr = Instantiate(groundRubbishPrefab, transform.position, Quaternion.identity);
                GroundRubbish grScript = gr.GetComponent<GroundRubbish>();
                if (grScript != null)
                    grScript.rubbishType = rubbishType;
            }

            Destroy(gameObject);
        }
    }
}