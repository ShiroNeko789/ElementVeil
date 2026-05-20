using UnityEngine;

public enum EquipmentType
{
    Glove,
    Boots,
    Helmet
    // Add more equipment types here
}

[CreateAssetMenu(fileName = "NewEquipment", menuName = "Inventory/Equipment")]
public class Equipment : Item
{
    [Header("Equipment Settings")]
    public EquipmentType equipmentType;
}