using UnityEngine;

public class ItemPickup : MonoBehaviour
{
    public Item item;

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Player")) return;
        Inventory.Instance.AddItem(item);
        // Register this pickup as collected before destroying
        GameSaveManager.Instance.RegisterCollectedPickup(gameObject.name);
        Destroy(gameObject);
    }
}