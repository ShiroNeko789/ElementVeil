using UnityEngine;

public class Bullet : MonoBehaviour
{
    public float speed = 15f;
    public float lifeTime = 2f;
    public int damage = 1;
    public float rustPower = 20f; // How much "rust" this water drop applies

    private Vector2 direction;

    public void SetDirection(Vector2 dir, float playerScaleX)
    {
        direction = dir.normalized;
        Vector3 newScale = transform.localScale;
        newScale.x = Mathf.Abs(newScale.x) * Mathf.Sign(playerScaleX);
        transform.localScale = newScale;
    }

    void Start()
    {
        Destroy(gameObject, lifeTime);
    }

    void Update()
    {
        transform.Translate(direction * speed * Time.deltaTime, Space.World);
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        // 1. Check for Enemies (Existing logic)
        EnemyHealth enemy = collision.GetComponent<EnemyHealth>();
        if (enemy != null)
        {
            enemy.TakeDamage(damage);
            Destroy(gameObject);
            return;
        }

        // 2. NEW: Check for Rustable Items (Puzzle Obstacles)
        RustableItem rustItem = collision.GetComponent<RustableItem>();
        if (rustItem != null)
        {
            rustItem.ApplyWaterDamage(rustPower);
            Destroy(gameObject); // Water disappears when hitting the metal
            return;
        }

        // 3. Check for Walls/Ground
        if (collision.CompareTag("Ground"))
        {
            Destroy(gameObject);
        }
    }
}