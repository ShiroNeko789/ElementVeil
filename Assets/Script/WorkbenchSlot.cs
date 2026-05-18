using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class WorkbenchSlot : MonoBehaviour, IDropHandler, IPointerEnterHandler, IPointerExitHandler
{
    public WorkbenchUI workbenchUI;
    public Item heldItem = null;

    private Image iconImage;
    private Image bgImage;

    void Awake()
    {
        Image[] images = GetComponentsInChildren<Image>(true);
        if (images.Length >= 2)
        {
            bgImage = images[0];
            iconImage = images[1];
        }
        else if (images.Length == 1)
        {
            bgImage = images[0];
            iconImage = images[0];
        }

        if (iconImage != null) iconImage.enabled = false;
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        Debug.Log("Pointer entered: " + gameObject.name);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        Debug.Log("Pointer exited: " + gameObject.name);
    }

    public void OnDrop(PointerEventData eventData)
    {
        Debug.Log("OnDrop fired on: " + gameObject.name);

        if (eventData.pointerDrag == null)
        {
            Debug.LogError("pointerDrag null");
            return;
        }

        DraggableItem dragged = eventData.pointerDrag.GetComponent<DraggableItem>();
        if (dragged == null)
        {
            Debug.LogError("No DraggableItem on dragged object");
            return;
        }

        if (dragged.item == null)
        {
            Debug.LogError("DraggableItem.item is null");
            return;
        }

        if (heldItem != null)
        {
            Debug.Log("Slot occupied by: " + heldItem.itemName);
            return;
        }

        heldItem = dragged.item;

        if (iconImage != null)
        {
            iconImage.sprite = heldItem.icon;
            iconImage.enabled = true;
            iconImage.color = Color.white;
        }

        Debug.Log("Placed: " + heldItem.itemName + " into " + gameObject.name);

        // THIS is the key line — must call OnSlotChanged after every drop
        if (workbenchUI != null)
            workbenchUI.OnSlotChanged();
        else
            Debug.LogError("workbenchUI is null on slot: " + gameObject.name);
    }

    public void ClearSlot()
    {
        heldItem = null;
        if (iconImage != null)
        {
            iconImage.sprite = null;
            iconImage.enabled = false;
        }
    }
}