using UnityEngine;

public class PlayerController2D : MonoBehaviour
{
    public float moveSpeed = 10f;
    public float jumpForce = 16f;
    public LayerMask groundLayer;
    public float groundCheckDistance = 0.5f;

    [Header("Mobile Controls")]
    public Joystick joystick;
    public bool useMobileControls = true;

    private Rigidbody2D rb;
    private BoxCollider2D coll;
    private float horizontalInput;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        coll = GetComponent<BoxCollider2D>();
        rb.freezeRotation = true;
    }

    void Update()
    {
        // 1. Movement Input
        if (useMobileControls && joystick != null)
        {
            float joyX = joystick.Horizontal;
            float keyX = Input.GetAxisRaw("Horizontal");
            horizontalInput = Mathf.Abs(joyX) > 0.1f ? joyX : keyX;
        }
        else
        {
            horizontalInput = Input.GetAxisRaw("Horizontal");
        }

        // 2. PC Jump Input (Space)
        if (Input.GetButtonDown("Jump") && IsGrounded())
        {
            Jump();
        }

        // 3. Flip Sprite
        if (horizontalInput > 0.01f) transform.localScale = new Vector3(1, 1, 1);
        else if (horizontalInput < -0.01f) transform.localScale = new Vector3(-1, 1, 1);

        CheckEnemyProximity();
    }

    // Must be Public for UI Button OnClick
    public void Jump()
    {
        if (IsGrounded())
        {
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, jumpForce);
            Debug.Log("Jump Success!");
        }
        else
        {
            Debug.Log("Jump failed: Not Grounded. Check your Ground Layer!");
        }
    }

    void CheckEnemyProximity()
    {
        GameObject[] enemies = GameObject.FindGameObjectsWithTag("Enemy");
        foreach (GameObject enemy in enemies)
        {
            // Calculate distance ignoring Z axis
            float distance = Vector2.Distance(transform.position, enemy.transform.position);
            if (distance < 1.2f)
            {
                GetComponent<PlayerHealth>().TakeDamage(1, enemy.transform.position);
            }
        }
    }

    void FixedUpdate()
    {
        rb.linearVelocity = new Vector2(horizontalInput * moveSpeed, rb.linearVelocity.y);
    }

    public bool IsGrounded()
    {
        // This draws a box under the player to check for the 'Ground' layer
        RaycastHit2D hit = Physics2D.BoxCast(coll.bounds.center, coll.bounds.size, 0f, Vector2.down, groundCheckDistance, groundLayer);
        return hit.collider != null;
    }
}