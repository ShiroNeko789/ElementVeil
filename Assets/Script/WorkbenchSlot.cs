using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class WorkbenchSlot : MonoBehaviour, IDropHandler, IPointerEnterHandler, IPointerExitHandler
{
    public WorkbenchUI workbenchUI;
    public Item heldItem = null;

    private Image iconImage;      // the Icon child image
    private Image bgImage;        // the background slot image

    void Awake()
    {
        Image[] images = GetComponentsInChildren<Image>(true);
        // images[0] = background (root), images[1] = Icon child
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

        // Hide icon at start
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
        if (eventData.pointerDrag == null) { Debug.LogError("pointerDrag null"); return; }

        DraggableItem dragged = eventData.pointerDrag.GetComponent<DraggableItem>();
        if (dragged == null) { Debug.LogError("No DraggableItem on dragged object"); return; }
        if (dragged.item == null) { Debug.LogError("DraggableItem.item is null"); return; }
        if (heldItem != null) { Debug.Log("Slot occupied by: " + heldItem.itemName); return; }

        heldItem = dragged.item;

        // Show icon on the child Image, not the background
        if (iconImage != null)
        {
            iconImage.sprite = heldItem.icon;
            iconImage.enabled = true;
            iconImage.color = Color.white;
        }

        Debug.Log("Placed: " + heldItem.itemName);
        workbenchUI.OnSlotChanged();
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