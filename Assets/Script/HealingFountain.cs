using UnityEngine;

public class HealingFountain : MonoBehaviour
{
    private bool playerInRange = false;
    private PlayerHealth playerHealth;

    [Header("Mobile UI")]
    public GameObject interactButton; // Drag the UI Button from your Canvas here

    void Start()
    {
        // Ensure the button starts hidden when the game begins
        if (interactButton != null)
        {
            interactButton.SetActive(false);
        }
    }

    // Link this function to the OnClick() event of your UI Button
    public void Interact()
    {
        // Only heal if the player is in range and actually needs health
        if (playerInRange && playerHealth != null)
        {
            if (playerHealth.currentHealth < playerHealth.maxHealth)
            {
                playerHealth.RestoreFullHealth();
                Debug.Log("Health Restored!");

                // Optional: Hide button after use if the fountain is one-time use
                // interactButton.SetActive(false);
            }
            else
            {
                Debug.Log("Player already at full health.");
            }
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            playerInRange = true;
            playerHealth = other.GetComponent<PlayerHealth>();

            // Show the interact button
            if (interactButton != null)
            {
                interactButton.SetActive(true);
            }
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            playerInRange = false;
            playerHealth = null;

            // Hide the interact button when walking away
            if (interactButton != null)
            {
                interactButton.SetActive(false);
            }
        }
    }
}