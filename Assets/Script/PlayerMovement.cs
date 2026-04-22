using System.Collections;
using UnityEngine;

[RequireComponent(typeof(Rigidbody2D), typeof(BoxCollider2D), typeof(Animator))]
public class PlayerController2D : MonoBehaviour
{
    [Header("Movement Settings")]
    public float moveSpeed = 10f;
    public float jumpForce = 16f;
    [Range(0f, 1f)] public float jumpCutMultiplier = 0.5f;

    [Header("Dash Settings")]
    public float dashSpeed = 25f;
    public float dashTime = 0.2f;
    public float dashCooldown = 0.5f;

    [Header("Ground Check")]
    public LayerMask groundLayer;
    public float groundCheckDistance = 0.1f;

    [Header("Death Settings")]
    public float deathJumpForce = 6f;    // Initial pop up
    public float deathGravity = 2f;      // How fast they fall while dead

    private Rigidbody2D rb;
    private BoxCollider2D coll;
    private Animator anim;

    private float horizontalInput;
    private float gravityScaleOriginal;

    private bool isDashing;
    private bool canDash = true;
    private bool isDead = false;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        coll = GetComponent<BoxCollider2D>();
        anim = GetComponent<Animator>();

        gravityScaleOriginal = rb.gravityScale;
    }

    void Update()
    {
        if (isDead) return;

        if (Input.GetKeyDown(KeyCode.LeftShift) && canDash)
        {
            StartCoroutine(PerformDash());
        }

        if (isDashing)
        {
            UpdateAnimator();
            return;
        }

        horizontalInput = Input.GetAxisRaw("Horizontal");

        if (Input.GetButtonDown("Jump") && IsGrounded())
        {
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, jumpForce);
        }

        if (Input.GetButtonUp("Jump") && rb.linearVelocity.y > 0)
        {
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, rb.linearVelocity.y * jumpCutMultiplier);
        }

        FlipSprite();
        UpdateAnimator();
    }

    void FixedUpdate()
    {
        if (isDashing || isDead) return;

        rb.linearVelocity = new Vector2(horizontalInput * moveSpeed, rb.linearVelocity.y);
    }

    // --- DETECTION ---

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Enemy") && !isDead)
        {
            Die();
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Enemy") && !isDead)
        {
            Die();
        }
    }

    // --- DEATH CORE ---

    private void Die()
    {
        isDead = true;

        // Reset all movement booleans
        anim.SetBool("IsDashing", false);
        anim.SetBool("IsJumping", false);
        anim.SetBool("IsGrounded", false);
        anim.SetFloat("Speed", 0f);

        // Start death animation
        anim.SetBool("IsDead", true);

        // Disable collider so they fall through floor
        coll.enabled = false;

        // Physics for dropping
        rb.gravityScale = deathGravity;
        rb.linearVelocity = new Vector2(0, deathJumpForce);

        // Note: No Coroutine here because we use the Animation Event instead!
    }

    // This method is called by the Animation Event at the end of the clip
    public void DestroyPlayer()
    {
        Destroy(gameObject);
        Debug.Log("Player object destroyed via Animation Event.");
    }

    // --- PLAYER ACTIONS ---

    private IEnumerator PerformDash()
    {
        canDash = false;
        isDashing = true;

        anim.SetBool("IsDashing", true);
        rb.linearVelocity = Vector2.zero;
        rb.gravityScale = 0f;

        float dashDirection = horizontalInput != 0 ? Mathf.Sign(horizontalInput) : transform.localScale.x;
        rb.linearVelocity = new Vector2(dashDirection * dashSpeed, 0f);

        yield return new WaitForSeconds(dashTime);

        if (!isDead) rb.gravityScale = gravityScaleOriginal;

        isDashing = false;
        anim.SetBool("IsDashing", false);

        yield return new WaitForSeconds(dashCooldown);
        canDash = true;
    }

    private void UpdateAnimator()
    {
        if (isDead) return;

        bool grounded = IsGrounded();
        anim.SetFloat("Speed", Mathf.Abs(horizontalInput));
        anim.SetBool("IsGrounded", grounded);
        anim.SetBool("IsJumping", !grounded && rb.linearVelocity.y > 0.1f);
    }

    private bool IsGrounded()
    {
        if (!coll.enabled) return false;

        RaycastHit2D hit = Physics2D.BoxCast(
            coll.bounds.center,
            coll.bounds.size,
            0f,
            Vector2.down,
            groundCheckDistance,
            groundLayer
        );

        return hit.collider != null;
    }

    private void FlipSprite()
    {
        if (horizontalInput > 0)
            transform.localScale = new Vector3(1, 1, 1);
        else if (horizontalInput < 0)
            transform.localScale = new Vector3(-1, 1, 1);
    }

    void OnDrawGizmos()
    {
        if (coll == null) coll = GetComponent<BoxCollider2D>();
        if (coll == null) return;

        Gizmos.color = Application.isPlaying && IsGrounded() ? Color.green : Color.red;

        Gizmos.DrawWireCube(
            coll.bounds.center + Vector3.down * groundCheckDistance,
            coll.bounds.size
        );
    }
}