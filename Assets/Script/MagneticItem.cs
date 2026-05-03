using UnityEngine;

public class MagneticItem : MonoBehaviour
{
    public bool isNorthItem = true; // Check this in Inspector for Red items
    private Rigidbody2D rb;
    private PlayerMagnet playerScript;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        playerScript = GameObject.FindGameObjectWithTag("Player").GetComponent<PlayerMagnet>();
    }

    private void OnMouseDown()
    {
        Debug.Log("CLICK DETECTED ON: " + gameObject.name); // ADD THIS
        if (playerScript != null && playerScript.magnetModeActive)
        {
            playerScript.SelectNewTarget(this);
        }
    }

    public void ApplyMagneticForce(Vector2 playerPos, float power, bool playerIsNorth)
    {
        Vector2 direction = (Vector2)playerPos - (Vector2)transform.position;

        // Logic: If Player and Item are the SAME, push away (-1). If DIFFERENT, pull (+1).
        float forceDir = (playerIsNorth == isNorthItem) ? -0.5f : 0.5f;

        rb.linearVelocity = direction.normalized * power * forceDir;
    }

    public void SetHighlight(bool active)
    {
        GetComponent<SpriteRenderer>().color = active ? Color.yellow : (isNorthItem ? Color.red : Color.blue);
    }

    public void StopMoving() => rb.linearVelocity = Vector2.zero;

    void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            Vector2 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            RaycastHit2D hit = Physics2D.Raycast(mousePos, Vector2.zero);

            if (hit.collider != null)
            {
                Debug.Log("MANUAL RAYCAST HIT: " + hit.collider.name);
            }
            else
            {
                Debug.Log("RAYCAST HIT NOTHING AT: " + mousePos);
            }
        }
    }
}