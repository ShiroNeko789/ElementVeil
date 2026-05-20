using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using TMPro;


public class BalloonInsertPanel : MonoBehaviour
{
    [Header("Panel Visuals — child GameObject to show/hide (NOT this object)")]
    public GameObject panelVisuals;

    [Header("Slot")]
    public BalloonItemSlot itemSlot;

    [Header("Inventory Grid")]
    public Transform itemGridContent;
    public GameObject itemSlotPrefab;

    [Header("Hint Text")]
    public TextMeshProUGUI requiredItemHintText;

    [Header("Feedback")]
    public TextMeshProUGUI feedbackText;

    private HotAirBalloon balloon;
    private Item requiredItem;
    private List<GameObject> spawnedSlots = new List<GameObject>();

    void Awake()
    {
        // Hide visuals at start, but THIS GameObject stays active
        // so coroutines can always run on it
        if (panelVisuals != null) panelVisuals.SetActive(false);
        if (itemSlot != null) itemSlot.insertPanel = this;
    }

    void Start()
    {
        Inventory.Instance.onInventoryChangedCallback += RefreshGrid;
    }

    // ── Open / Close ───────────────────────────────────────────────────────

    public void Open(HotAirBalloon targetBalloon, Item needed)
    {
        balloon = targetBalloon;
        requiredItem = needed;

        if (panelVisuals != null) panelVisuals.SetActive(true);
        if (feedbackText != null) feedbackText.text = "";

        if (requiredItemHintText != null)
            requiredItemHintText.text = needed != null ? "Needed: " + needed.itemName : "";

        if (itemSlot != null) itemSlot.ClearSlot();
        RefreshGrid();
    }

    public void Close()
    {
        if (panelVisuals != null) panelVisuals.SetActive(false);
        if (itemSlot != null) itemSlot.ClearSlot();
    }

    // ── Grid — identical to WorkbenchUI.RefreshItemGrid ───────────────────

    void RefreshGrid()
    {
        if (itemGridContent == null || itemSlotPrefab == null) return;

        foreach (var s in spawnedSlots) Destroy(s);
        spawnedSlots.Clear();

        foreach (Item item in Inventory.Instance.items)
        {
            if (item == null) continue;

            GameObject slotObj = Instantiate(itemSlotPrefab, itemGridContent);
            spawnedSlots.Add(slotObj);

            Image[] images = slotObj.GetComponentsInChildren<Image>(true);
            if (images.Length >= 2)
            {
                images[1].sprite = item.icon;
                images[1].color = Color.white;
                images[1].enabled = true;
            }
            else if (images.Length == 1)
            {
                images[0].sprite = item.icon;
                images[0].color = Color.white;
            }

            DraggableItem drag = slotObj.GetComponent<DraggableItem>();
            if (drag != null) drag.item = item;
        }
    }

    // ── Insert Button ──────────────────────────────────────────────────────

    public void InsertItem()
    {
        if (itemSlot == null || itemSlot.heldItem == null)
        {
            ShowFeedback("Drag an item into the slot first!", Color.yellow);
            return;
        }

        if (itemSlot.heldItem != requiredItem)
        {
            ShowFeedback("Wrong item! Need: " + requiredItem?.itemName, Color.red);
            StartCoroutine(FlashSlotRed());
            return;
        }

        // Correct item — consume and proceed to minigame
        Inventory.Instance.RemoveItem(itemSlot.heldItem);
        balloon.OnItemInserted();
    }

    // ── Cancel Button ──────────────────────────────────────────────────────

    public void OnCancel()
    {
        balloon?.OnCancelled();
    }

    // ── Feedback ───────────────────────────────────────────────────────────

    void ShowFeedback(string message, Color color)
    {
        if (feedbackText != null)
        {
            feedbackText.text = message;
            feedbackText.color = color;
        }
        Debug.Log("[BalloonInsert] " + message);
    }

    IEnumerator FlashSlotRed()
    {
        if (itemSlot == null) yield break;
        Image slotBg = itemSlot.GetComponent<Image>();
        if (slotBg == null) yield break;

        Color original = slotBg.color;
        slotBg.color = Color.red;
        yield return new WaitForSecondsRealtime(0.5f);
        slotBg.color = original;
    }

    public void OnSlotChanged() { } // Called by BalloonItemSlot, reserved for future preview logic
}