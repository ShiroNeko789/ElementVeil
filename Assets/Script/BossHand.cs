using UnityEngine;
using System.Collections;

public class BossHand : MonoBehaviour
{
    public float damage = 1f;
    public float speed = 12f;
    public float lifetime = 4f;
    private bool hasHit = false;

    void Start()
    {
        Rigidbody2D rb = GetComponent<Rigidbody2D>();
        if (rb != null)
        {
            // Shoots straight to the left screen side (-1, 0) automatically
            rb.linearVelocity = Vector2.left * speed;
        }

        // LOCKED IN CODE: Forces the hand to be exactly -90 degrees on the Z axis
        transform.rotation = Quaternion.Euler(0f, 0f, -90f);

        StartCoroutine(DestroyAfter(lifetime));
    }

    IEnumerator DestroyAfter(float time)
    {
        yield return new WaitForSeconds(time);
        Destroy(gameObject);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (hasHit) return;

        // Only registers hits on the player
        if (other.CompareTag("Player"))
        {
            hasHit = true;

            other.GetComponent<PlayerHealth>()
                ?.TakeDamage(Mathf.RoundToInt(damage), transform.position);

            Destroy(gameObject);
        }
    }
}