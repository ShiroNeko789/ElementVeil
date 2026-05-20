using UnityEngine;
using System.Collections.Generic;

public class EquipmentManager : MonoBehaviour
{
    public static EquipmentManager Instance;

    private Dictionary<EquipmentType, Equipment> equippedItems
        = new Dictionary<EquipmentType, Equipment>();

    public delegate void OnEquipmentChanged(EquipmentType type, Equipment item);
    public event OnEquipmentChanged onEquipmentChangedCallback;

    void Awake() { Instance = this; }

    public void Equip(Equipment item)
    {
        equippedItems[item.equipmentType] = item;
        onEquipmentChangedCallback?.Invoke(item.equipmentType, item);
        Debug.Log("Equipped: " + item.itemName);
    }

    public void Unequip(EquipmentType type)
    {
        if (equippedItems.ContainsKey(type))
        {
            Equipment removed = equippedItems[type];
            equippedItems.Remove(type);
            onEquipmentChangedCallback?.Invoke(type, null);
            Debug.Log("Unequipped: " + removed.itemName);
        }
    }

    public Equipment GetEquipped(EquipmentType type)
    {
        return equippedItems.ContainsKey(type) ? equippedItems[type] : null;
    }

    public bool HasEquipped(EquipmentType type)
    {
        return equippedItems.ContainsKey(type) && equippedItems[type] != null;
    }
}