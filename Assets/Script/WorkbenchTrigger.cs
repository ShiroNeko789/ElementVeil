using UnityEngine;

public class WorkbenchTrigger : MonoBehaviour, IInteractable
{
    public WorkbenchUI workbenchUI;

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Player")) return;
        InteractManager.Instance.RegisterInteractable(this);
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (!other.CompareTag("Player")) return;
        InteractManager.Instance.UnregisterInteractable(this);
    }

    public void OnInteract()
    {
        workbenchUI.Open();
    }

    public interface IInteractable
    {
        void OnInteract();
    }
}