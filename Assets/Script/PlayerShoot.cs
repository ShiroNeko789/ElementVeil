using UnityEngine;

public class PlayerShoot : MonoBehaviour
{
    public GameObject bulletPrefab;
    public Transform firePoint;

    public float shootCooldown = 0.5f;
    private float lastShootTime;

    void Update()
    {
        // Keeps keyboard 'J' working for PC testing
        if (Input.GetKeyDown(KeyCode.J))
        {
            Shoot();
        }
    }

    // Changed to PUBLIC so the UI Button can call it
    public void Shoot()
    {
        // Check if enough time has passed since the last shot
        if (Time.time >= lastShootTime + shootCooldown)
        {
            if (bulletPrefab == null || firePoint == null) return;

            GameObject bullet = Instantiate(bulletPrefab, firePoint.position, Quaternion.identity);

            // Determine direction based on player's current facing scale
            Vector2 direction = transform.localScale.x > 0 ? Vector2.right : Vector2.left;

            // Call the SetDirection function on your Bullet script
            bullet.GetComponent<Bullet>().SetDirection(direction, transform.localScale.x);

            // Update the cooldown timer
            lastShootTime = Time.time;
            Debug.Log("Water Shot!");
        }
    }
}