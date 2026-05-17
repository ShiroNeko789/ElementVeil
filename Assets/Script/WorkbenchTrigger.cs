using UnityEngine;
using UnityEngine.UI;

public class WorkbenchTrigger : MonoBehaviour
{
    public WorkbenchUI workbenchUI;
    public GameObject interactButton;

    private bool playerNearby = false;

    void Start()
    {
        if (interactButton != null) interactButton.SetActive(false);
        else Debug.LogError("Interact button not assigned on WorkbenchTrigger!");

        if (workbenchUI == null) Debug.LogError("WorkbenchUI not assigned on WorkbenchTrigger!");
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        Debug.Log("Something entered trigger: " + other.gameObject.name);
        if (!other.CompareTag("Player")) return;
        Debug.Log("Player entered workbench trigger");
        playerNearby = true;
        if (interactButton != null) interactButton.SetActive(true);
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (!other.CompareTag("Player")) return;
        playerNearby = false;
        if (interactButton != null) interactButton.SetActive(false);
    }

    public void OnInteractPressed()
    {
        Debug.Log("Interact pressed, playerNearby: " + playerNearby);
        if (!playerNearby) return;
        workbenchUI.Open();
        interactButton.SetActive(false);
    }
}