using UnityEngine;
using UnityEngine.UI;

public class PlayerMagnet : MonoBehaviour
{
    [Header("UI Groups")]
    public GameObject combatUIGroup; // Drag the 'CombatUI' object here
    public GameObject magnetUIGroup; // Drag the 'MagnetUI' object here

    [Header("Polarity Settings")]
    public Image polarityButtonImage; // The button that turns Red/Blue
    public bool playerIsNorth = true;
    public bool magnetModeActive = false;

    private MagneticItem selectedItem;

    void Start()
    {
        // Set the initial state when the game starts
        magnetModeActive = false;
        combatUIGroup.SetActive(true);
        magnetUIGroup.SetActive(false);
    }

    // LINK THIS TO YOUR 'SWITCH' BUTTON
    public void ToggleMagnetMode()
    {
        magnetModeActive = !magnetModeActive;

        if (magnetModeActive)
        {
            combatUIGroup.SetActive(false); // Hide Shoot/Jump
            magnetUIGroup.SetActive(true);  // Show Polarity
        }
        else
        {
            combatUIGroup.SetActive(true);  // Show Shoot/Jump
            magnetUIGroup.SetActive(false); // Hide Polarity
            DeselectTarget();
        }
    }

    // LINK THIS TO YOUR 'POLARITY' BUTTON
    public void TogglePolarity()
    {
        playerIsNorth = !playerIsNorth;
        // Visual feedback
        polarityButtonImage.color = playerIsNorth ? Color.red : Color.blue;
    }

    public void SelectNewTarget(MagneticItem item)
    {
        if (selectedItem != null) selectedItem.SetHighlight(false);
        selectedItem = item;
        selectedItem.SetHighlight(true);
    }

    private void DeselectTarget()
    {
        if (selectedItem != null)
        {
            selectedItem.SetHighlight(false);
            selectedItem.StopMoving();
        }
        selectedItem = null;
    }

    void Update()
    {
        if (magnetModeActive && selectedItem != null)
        {
            // Move based on the logic we built (Opposites Attract / Likes Repel)
            selectedItem.ApplyMagneticForce(transform.position, 10f, playerIsNorth);
        }
    }
}