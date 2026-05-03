using UnityEngine;

public class RustableItem : MonoBehaviour
{
    [Header("Health Settings")]
    public float maxHealth = 100f;
    private float currentHealth;

    [Header("Visuals")]
    public Color healthyColor = Color.white;
    public Color rustyColor = new Color(0.3f, 0.15f, 0.05f); // Dark Brown

    private SpriteRenderer sr;

    void Start()
    {
        sr = GetComponent<SpriteRenderer>();
        currentHealth = maxHealth;
        sr.color = healthyColor;
    }

    // This function is called when water hits the item
    public void ApplyWaterDamage(float amount)
    {
        currentHealth -= amount;

        // Ensure health doesn't go below 0
        currentHealth = Mathf.Clamp(currentHealth, 0, maxHealth);

        // Update the color based on health percentage
        UpdateColor();

        if (currentHealth <= 0)
        {
            RustAway();
        }
    }

    void UpdateColor()
    {
        // Calculate health percentage (1.0 = healthy, 0.0 = fully rusted)
        float healthPercent = currentHealth / maxHealth;

        // Lerp (Linear Interpolation) between healthy and rusty colors
        // As healthPercent drops, the color moves toward rustyColor
        sr.color = Color.Lerp(rustyColor, healthyColor, healthPercent);

        // Optional: Make it slightly darker/transparent as it vanishes
        float alpha = Mathf.Lerp(0.2f, 1.0f, healthPercent);
        sr.color = new Color(sr.color.r, sr.color.g, sr.color.b, alpha);
    }

    void RustAway()
    {
        // Add particle effects here if you have them!
        Debug.Log("Obstacle rusted away!");
        Destroy(gameObject);
    }
}