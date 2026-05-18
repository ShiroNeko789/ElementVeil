using UnityEngine;

public class CleanThrowable : MonoBehaviour
{
    public float lifetime = 5f;

    void Start()
    {
        Destroy(gameObject, lifetime);
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        RubbishObstacle rubbish = collision.gameObject.GetComponent<RubbishObstacle>();
        if (rubbish != null) rubbish.ClearArea(transform.position);

        // Also check nearby for rubbish
        Collider2D[] nearby = Physics2D.OverlapCircleAll(transform.position, 2f);
        foreach (Collider2D col in nearby)
        {
            RubbishObstacle r = col.GetComponent<RubbishObstacle>();
            if (r != null) r.ClearArea(transform.position);
        }

        Destroy(gameObject);
    }
}