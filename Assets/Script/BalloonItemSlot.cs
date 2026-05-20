using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;


public class BalloonItemSlot : MonoBehaviour, IDropHandler
{
    [HideInInspector] public BalloonInsertPanel insertPanel;

    public Item heldItem = null;

    [Header("Visuals")]
    public Image iconImage;
    public Sprite emptySprite;

    void Start()
    {
        ClearSlot();
    }

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
        insertPanel?.OnSlotChanged();
    }

    public void ClearSlot()
    {
        heldItem = null;
        if (iconImage != null)
        {
            iconImage.sprite = emptySprite;
            iconImage.color = emptySprite != null ? Color.white : Color.clear;
        }
        insertPanel?.OnSlotChanged();
    }
}