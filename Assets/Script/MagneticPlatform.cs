using UnityEngine;

public class MagneticPlatform : MonoBehaviour
{
    public bool isNorthItem = true;
    public float moveSpeed = 5f;

    private Rigidbody2D rb;
    private PlayerMagnet playerScript;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        playerScript = GameObject.FindGameObjectWithTag("Player").GetComponent<PlayerMagnet>();

        // Physics Setup: Kinematic is best for moving platforms
        rb.bodyType = RigidbodyType2D.Kinematic;
        rb.useFullKinematicContacts = true;
        rb.constraints = RigidbodyConstraints2D.FreezePositionY | RigidbodyConstraints2D.FreezeRotation;
    }

    // NO parenting code here! This prevents the "small player" bug.

    public void Slide(Vector2 playerPos, bool playerIsNorth)
    {
        float directionX = playerPos.x - transform.position.x;
        float forceDir = (playerIsNorth == isNorthItem) ? -1f : 1f;
        float finalMove = Mathf.Sign(directionX) * moveSpeed * forceDir;

        // Move the platform using velocity
        rb.linearVelocity = new Vector2(finalMove, 0);
    }

    public void Stop()
    {
        rb.linearVelocity = Vector2.zero;
    }
}