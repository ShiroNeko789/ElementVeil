using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;

public class EquipmentSlot : MonoBehaviour, IDropHandler
{
    [Header("Settings")]
    public EquipmentType acceptedType;  // what type this slot accepts

    [Header("UI")]
    public Image slotIcon;              // shows equipped item
    public Image slotBackground;        // slot background image
    public TextMeshProUGUI slotLabel;   // "Glove", "Boots" etc
    public Sprite emptySprite;          // shown when nothing equipped

    private Equipment equippedItem = null;

    void Start()
    {
        if (slotLabel != null) slotLabel.text = acceptedType.ToString();
        UpdateSlotUI(null);

        // Listen for equipment changes
        if (EquipmentManager.Instance != null)
            EquipmentManager.Instance.onEquipmentChangedCallback += OnEquipmentChanged;
    }

    void OnDestroy()
    {
        if (EquipmentManager.Instance != null)
            EquipmentManager.Instance.onEquipmentChangedCallback -= OnEquipmentChanged;
    }

    void OnEquipmentChanged(EquipmentType type, Equipment item)
    {
        if (type == acceptedType) UpdateSlotUI(item);
    }

    public void OnDrop(PointerEventData eventData)
    {
        DraggableItem dragged = eventData.pointerDrag?.GetComponent<DraggableItem>();
        if (dragged == null) return;

        // Must be an Equipment item
        Equipment equip = dragged.item as Equipment;
        if (equip == null)
        {
            Debug.Log("Not an equipment item");
            return;
        }

        // Must match slot type
        if (equip.equipmentType != acceptedType)
        {
            Debug.Log("Wrong equipment type for this slot");
            return;
        }

        EquipmentManager.Instance.Equip(equip);
        equippedItem = equip;
        UpdateSlotUI(equip);
    }

    void UpdateSlotUI(Equipment item)
    {
        if (slotIcon == null) return;
        if (item == null)
        {
            slotIcon.sprite = emptySprite;
            slotIcon.color = new Color(1f, 1f, 1f, 0.3f);
        }
        else
        {
            slotIcon.sprite = item.icon;
            slotIcon.color = Color.white;
        }
    }

    // Double tap to unequip
    public void OnUnequipPressed()
    {
        EquipmentManager.Instance.Unequip(acceptedType);
        equippedItem = null;
        UpdateSlotUI(null);
    }
}