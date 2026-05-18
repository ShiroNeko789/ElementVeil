using UnityEngine;

public class OilThrowable : MonoBehaviour
{
    public float lifetime = 5f;

    void Start()
    {
        Destroy(gameObject, lifetime);
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        OilFloorObstacle oil = collision.gameObject.GetComponent<OilFloorObstacle>();
        if (oil != null) oil.Neutralize();

        Collider2D[] nearby = Physics2D.OverlapCircleAll(transform.position, 2f);
        foreach (Collider2D col in nearby)
        {
            OilFloorObstacle o = col.GetComponent<OilFloorObstacle>();
            if (o != null) o.Neutralize();
        }

        Destroy(gameObject);
    }
}