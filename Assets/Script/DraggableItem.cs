using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class DraggableItem : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    public Item item;
    private Transform originalParent;
    private int originalSiblingIndex;
    private CanvasGroup canvasGroup;
    private Canvas rootCanvas;
    private RectTransform rectTransform;

    void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
        canvasGroup = GetComponent<CanvasGroup>();
        if (canvasGroup == null) canvasGroup = gameObject.AddComponent<CanvasGroup>();
        rootCanvas = GetComponentInParent<Canvas>();
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        originalParent = transform.parent;
        originalSiblingIndex = transform.GetSiblingIndex();

        // Convert world position before reparenting
        Vector3 worldPos = transform.position;

        transform.SetParent(rootCanvas.transform, true);
        transform.SetAsLastSibling();

        // Restore world position after reparent so it doesn't jump
        transform.position = worldPos;

        canvasGroup.blocksRaycasts = false;
    }

    public void OnDrag(PointerEventData eventData)
    {
        transform.position = eventData.position;
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        canvasGroup.blocksRaycasts = true;
        // Snap back to original position if not dropped on valid slot
        transform.SetParent(originalParent, true);
        transform.SetSiblingIndex(originalSiblingIndex);
        transform.localPosition = Vector3.zero;
    }
}