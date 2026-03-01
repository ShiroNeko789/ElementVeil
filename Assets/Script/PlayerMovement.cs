using System.Collections;
using UnityEngine;

[RequireComponent(typeof(Rigidbody2D), typeof(BoxCollider2D))]
public class PlayerController2D : MonoBehaviour
{
    [Header("Movement Settings")]
    public float moveSpeed = 10f;
    public float jumpForce = 16f;
    [Range(0f, 1f)] public float jumpCutMultiplier = 0.5f;

    [Header("Dash Settings")]
    public float dashSpeed = 25f; // Increased for better feel
    public float dashTime = 0.2f;
    public float dashCooldown = 0.5f; // Shorter cooldown for snappier gameplay

    [Header("Ground Check")]
    public LayerMask groundLayer;
    public float groundCheckDistance = 0.1f;

    private Rigidbody2D rb;
    private BoxCollider2D coll;
    private float horizontalInput;
    private float gravityScaleOriginal;
    private bool isDashing;
    private bool canDash = true;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        coll = GetComponent<BoxCollider2D>();
        gravityScaleOriginal = rb.gravityScale;
    }

    void Update()
    {
        // We handle Dash input FIRST so it can interrupt anything
        if (Input.GetKeyDown(KeyCode.LeftShift) && canDash)
        {
            StartCoroutine(PerformDash());
        }

        if (isDashing) return; // Stop other inputs ONLY after dash has started

        horizontalInput = Input.GetAxisRaw("Horizontal");

        // Jump Logic
        if (Input.GetButtonDown("Jump") && IsGrounded())
        {
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, jumpForce);
        }

        // Variable Jump Height
        if (Input.GetButtonUp("Jump") && rb.linearVelocity.y > 0)
        {
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, rb.linearVelocity.y * jumpCutMultiplier);
        }

        FlipSprite();
    }

    void FixedUpdate()
    {
        if (isDashing) return;
        rb.linearVelocity = new Vector2(horizontalInput * moveSpeed, rb.linearVelocity.y);
    }

    private IEnumerator PerformDash()
    {
        canDash = false;
        isDashing = true;

        // 1. CLEAR VELOCITY: This stops the jump mid-air immediately
        rb.linearVelocity = Vector2.zero;

        // 2. DISABLE GRAVITY: Keeps the dash perfectly horizontal
        rb.gravityScale = 0f;

        // 3. DIRECTION: Use movement keys, or if standing still, use the way player is facing
        float dashDirection = horizontalInput != 0 ? Mathf.Sign(horizontalInput) : transform.localScale.x;

        // Apply the dash burst
        rb.linearVelocity = new Vector2(dashDirection * dashSpeed, 0f);

        yield return new WaitForSeconds(dashTime);

        // 4. RESET: Put gravity back so player falls after dash ends
        rb.gravityScale = gravityScaleOriginal;
        isDashing = false;

        yield return new WaitForSeconds(dashCooldown);
        canDash = true;
    }

    private bool IsGrounded()
    {
        RaycastHit2D hit = Physics2D.BoxCast(coll.bounds.center, coll.bounds.size, 0f, Vector2.down, groundCheckDistance, groundLayer);
        return hit.collider != null;
    }

    private void FlipSprite()
    {
        if (horizontalInput > 0) transform.localScale = new Vector3(1, 1, 1);
        else if (horizontalInput < 0) transform.localScale = new Vector3(-1, 1, 1);
    }

    void OnDrawGizmos()
    {
        if (coll == null) coll = GetComponent<BoxCollider2D>();
        if (coll == null) return;
        Gizmos.color = IsGrounded() ? Color.green : Color.red;
        Gizmos.DrawWireCube(coll.bounds.center + Vector3.down * groundCheckDistance, coll.bounds.size);
    }
}