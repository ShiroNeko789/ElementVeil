using UnityEngine;
using System.Collections;

public class BouncingBall : MonoBehaviour
{
    [Header("Settings")]
    public float speed = 8f;
    public float damage = 1f;
    public float lifetime = 8f;
    public int maxBounces = 6;

    private Rigidbody2D rb;
    private Vector2 moveDir;
    private int bounceCount = 0;
    private bool hasHitPlayer = false;
    private SmokeBoss owner;
    private bool isReflected = false;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    public void Launch(float dirX, SmokeBoss boss)
    {
        owner = boss;
        moveDir = new Vector2(dirX, -0.3f).normalized;
        rb.gravityScale = 0f;
        rb.linearVelocity = moveDir * speed;
        Destroy(gameObject, lifetime);
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        // Hit bullet — reflect ball back toward boss
        Bullet bullet = collision.GetComponent<Bullet>();
        if (bullet != null && !isReflected)
        {
            isReflected = true;

            // Reflect toward boss
            if (owner != null)
            {
                Vector2 toBoss = ((Vector2)owner.transform.position -
                    (Vector2)transform.position).normalized;
                rb.linearVelocity = toBoss * (speed * 1.5f);
                moveDir = toBoss;
            }
            return;
        }

        // Reflected ball hits boss
        if (isReflected)
        {
            SmokeBoss boss = collision.GetComponent<SmokeBoss>();
            if (boss != null)
            {
                boss.TakeDamage(1f); // 1 hit damage
                Destroy(gameObject);
                return;
            }
        }

        // Hit player — only if not reflected
        if (collision.CompareTag("Player") && !isReflected)
        {
            if (!hasHitPlayer)
            {
                hasHitPlayer = true;
                collision.GetComponent<PlayerHealth>()
                    ?.TakeDamage(Mathf.RoundToInt(damage), transform.position);
                Destroy(gameObject);
            }
            return;
        }

        // Hit wall or floor — bounce
        if (!collision.CompareTag("Enemy") &&
            !collision.CompareTag("Player") &&
            collision.GetComponent<SmokeBoss>() == null &&
            collision.GetComponent<Bullet>() == null)
        {
            bounceCount++;
            if (bounceCount >= maxBounces)
            {
                Destroy(gameObject);
                return;
            }

            // Use contact normal for reflection
            ContactFilter2D filter = new ContactFilter2D();
            RaycastHit2D hit = Physics2D.Raycast(
                transform.position, moveDir, 0.5f);
            if (hit.collider != null)
            {
                moveDir = Vector2.Reflect(moveDir, hit.normal);
                rb.linearVelocity = moveDir * speed;
            }
        }
    }
}