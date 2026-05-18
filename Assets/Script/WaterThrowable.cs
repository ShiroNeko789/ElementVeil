using UnityEngine;

public class WaterThrowable : MonoBehaviour
{
    public float lifetime = 5f;

    void Start()
    {
        Destroy(gameObject, lifetime);
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        // Hit green water
        GreenWaterObstacle water = collision.gameObject.GetComponent<GreenWaterObstacle>();
        if (water != null) water.Neutralize();

        Destroy(gameObject);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        GreenWaterObstacle water = other.GetComponent<GreenWaterObstacle>();
        if (water != null)
        {
            water.Neutralize();
            Destroy(gameObject);
        }
    }
}