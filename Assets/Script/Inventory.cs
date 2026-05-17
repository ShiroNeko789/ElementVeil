using UnityEngine;
using System.Collections.Generic;

public class Inventory : MonoBehaviour
{
    public static Inventory Instance;

    public List<Item> items = new List<Item>();

    public delegate void OnInventoryChanged();
    public event OnInventoryChanged onInventoryChangedCallback;

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);
        Debug.Log("Inventory instance created: " + GetInstanceID());
    }

    public void AddItem(Item item)
    {
        if (items.Contains(item)) return;
        items.Add(item);
        Debug.Log("Inventory added: " + item.itemName + " | Total: " + items.Count + " | Instance: " + GetInstanceID());
        onInventoryChangedCallback?.Invoke();
    }

    public bool HasItem(Item item)
    {
        return items.Contains(item);
    }

    public void RemoveItem(Item item)
    {
        items.Remove(item);
        onInventoryChangedCallback?.Invoke();
    }

    public void TriggerCallback()
    {
        onInventoryChangedCallback?.Invoke();
    }
}