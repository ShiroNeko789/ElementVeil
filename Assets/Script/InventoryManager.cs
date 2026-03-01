using System.Collections.Generic;
using UnityEngine;

public class InventoryManager : MonoBehaviour
{
    public static InventoryManager Instance { get; private set; }

    [Header("Inventory Data")]
    public int maxSlots = 20;
    public List<string> itemNames = new List<string>();

    void Awake()
    {
        if (Instance == null) Instance = this;
    }

    // Call this from any script: InventoryManager.Instance.AddItem("Sword");
    public bool AddItem(string name)
    {
        if (itemNames.Count >= maxSlots)
        {
            Debug.Log("Inventory is full!");
            return false;
        }

        itemNames.Add(name);
        Debug.Log("Picked up: " + name);
        return true;
    }

    public void RemoveItem(string name)
    {
        if (itemNames.Contains(name))
        {
            itemNames.Remove(name);
        }
    }
}