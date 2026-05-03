using UnityEngine;

public class MagneticPlatform : MonoBehaviour
{
    public bool isNorthItem = true; // Red = North, Blue = South
    public float moveSpeed = 5f;

    private Rigidbody2D rb;
    private PlayerMagnet playerScript;
    private bool isSelected = false;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        // Automatically find the player
        playerScript = GameObject.FindGameObjectWithTag("Player").GetComponent<PlayerMagnet>();

        // Ensure Y position never changes even if something heavy lands on it
        rb.constraints = RigidbodyConstraints2D.FreezePositionY | RigidbodyConstraints2D.FreezeRotation;
    }

    private void OnMouseDown()
    {
        if (playerScript != null && playerScript.magnetModeActive)
        {
            playerScript.SelectNewTarget(this.GetComponent<MagneticItem>());
            isSelected = true;
        }
    }

    // This handles the specialized sliding logic
    public void Slide(Vector2 playerPos, bool playerIsNorth)
    {
        // Calculate direction only on the X axis
        float directionX = playerPos.x - transform.position.x;

        // Opposites Attract (Pull), Likes Repel (Push)
        float forceDir = (playerIsNorth == isNorthItem) ? -1f : 1f;

        // Apply velocity only to X, keep Y at 0
        float finalMove = Mathf.Sign(directionX) * moveSpeed * forceDir;
        rb.linearVelocity = new Vector2(finalMove, 0);
    }

    public void Stop()
    {
        rb.linearVelocity = Vector2.zero;
    }
}