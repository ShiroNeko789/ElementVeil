using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

/// <summary>
/// Drop target slot on the Wind Machine panel.
/// Mirrors WorkbenchSlot — attach to the slot UI GameObject.
/// Set isTrigger on the Image so it receives drop events.
/// </summary>
public class WindMachineSlot : MonoBehaviour, IDropHandler
{
    [HideInInspector] public WindBlowMachine windMachine;

    public Item heldItem = null;

    [Header("Slot Visuals")]
    public Image iconImage;          // The child Image that shows the item icon
    public Sprite emptySprite;       // Sprite shown when slot is empty

    void Start()
    {
        ClearSlot();
    }

    // ── IDropHandler — fires when a DraggableItem is released over this slot ──

    public void OnDrop(PointerEventData eventData)
    {
        DraggableItem dragged = eventData.pointerDrag?.GetComponent<DraggableItem>();
        if (dragged == null || dragged.item == null) return;

        SetItem(dragged.item);
    }

    public void SetItem(Item item)
    {
        heldItem = item;

        if (iconImage != null)
        {
            iconImage.sprite = item.icon;
            iconImage.color = Color.white;
            iconImage.enabled = true;
        }

        windMachine?.OnSlotChanged();
    }

    public void ClearSlot()
    {
        heldItem = null;

        if (iconImage != null)
        {
            iconImage.sprite = emptySprite;
            iconImage.color = emptySprite != null ? Color.white : Color.clear;
        }

        windMachine?.OnSlotChanged();
    }
}