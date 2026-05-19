using UnityEngine;

// Attach this to the INTERACT ZONE child GameObject (not the machine itself).
// Make sure its Collider2D has Is Trigger = true.
public class WindInteractZone : MonoBehaviour, IInteractable
{
    public WindBlowMachine machine;

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Player")) return;

        // Pass THIS (WindInteractZone) as the interactable — not the machine.
        // The old code called machine.OnPlayerEnterInteract() which did
        // GetComponent<IInteractable>() on the machine GameObject, returning null.
        InteractManager.Instance.RegisterInteractable(this);
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (!other.CompareTag("Player")) return;

        InteractManager.Instance.UnregisterInteractable(this);

        // Also close the panel if player walks away
        machine.ClosePanel();
    }

    public void OnInteract()
    {
        machine.OpenPanel();
    }
}