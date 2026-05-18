using UnityEngine;
using System.Collections;

public class ThrowableProjectile : MonoBehaviour
{
    [Header("Settings")]
    public float damage = 2f;
    public float lifetime = 5f;
    public GameObject explosionEffect;  // optional particle/sprite on impact

    private bool hasHit = false;

    void Start()
    {
        StartCoroutine(DestroyAfterTime());
    }

    IEnumerator DestroyAfterTime()
    {
        yield return new WaitForSeconds(lifetime);
        Destroy(gameObject);
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (hasHit) return;
        hasHit = true;

        // Damage enemy if hit
        if (collision.gameObject.CompareTag("Enemy"))
            collision.gameObject.GetComponent<MushroomBoss>()?.TakeDamage(damage);

        if (explosionEffect != null)
            Instantiate(explosionEffect, transform.position, Quaternion.identity);

        Destroy(gameObject);
    }
}