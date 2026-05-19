using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using TMPro;

public class WindBlowMachine : MonoBehaviour
{
    [Header("Required Item To Activate")]
    public Item requiredItem;

    [Header("Wind Settings")]
    public float windForce = 15f;
    public float windLiftForce = 3f;
    public bool isActivated = false;

    [Header("UI — Panel")]
    public GameObject machinePanel;
    public Sprite emptySlotSprite;
    public GameObject activatedVFX;

    [Header("UI — Item Slot (drag target)")]
    public WindMachineSlot itemSlot;           // The single drop slot on the panel

    [Header("UI — Inventory Grid (like Workbench)")]
    public Transform itemGridContent;          // ScrollView Content transform
    public GameObject itemSlotPrefab;          // Same prefab you use in WorkbenchUI

    [Header("Animator")]
    public Animator machineAnimator;

    [Header("Wind Zone (to notify on activation)")]
    public WindZone windZone;

    [Header("Player")]
    public MonoBehaviour playerMovementScript;

    private List<GameObject> spawnedSlots = new List<GameObject>();

    void Start()
    {
        if (machinePanel != null) machinePanel.SetActive(false);
        if (activatedVFX != null) activatedVFX.SetActive(false);

        // Tell the slot who its owner is (same pattern as WorkbenchSlot → WorkbenchUI)
        if (itemSlot != null) itemSlot.windMachine = this;

        Inventory.Instance.onInventoryChangedCallback += RefreshItemGrid;
    }

    // ── Panel ──────────────────────────────────────────────────────────────

    public void OpenPanel()
    {
        if (isActivated) return;
        machinePanel.SetActive(true);
        RefreshItemGrid();
        Time.timeScale = 0f;
        if (playerMovementScript != null) playerMovementScript.enabled = false;
        UIManager.Instance?.OnPanelOpened(machinePanel);
    }

    public void ClosePanel()
    {
        if (machinePanel != null) machinePanel.SetActive(false);
        // Return any uninserted item sitting in the slot back to inventory display
        if (itemSlot != null) itemSlot.ClearSlot();
        Time.timeScale = 1f;
        if (playerMovementScript != null) playerMovementScript.enabled = true;
        UIManager.Instance?.OnAllPanelsClosed();
    }

    // ── Grid (mirrors WorkbenchUI.RefreshItemGrid exactly) ────────────────

    public void RefreshItemGrid()
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

    // ── Called by WindMachineSlot when an item is dropped onto it ─────────

    public void OnSlotChanged()
    {
        // Visual feedback: show what's in the slot
        // The slot handles its own icon display (like WorkbenchSlot does)
        // Nothing extra needed here unless you want a "confirm" preview
    }

    // ── Insert button on panel calls this ─────────────────────────────────

    public void InsertItem()
    {
        if (itemSlot == null || itemSlot.heldItem == null)
        {
            Debug.Log("No item placed in slot.");
            StartCoroutine(FlashSlotRed());
            return;
        }

        if (itemSlot.heldItem != requiredItem)
        {
            Debug.Log("Wrong item! Need: " + requiredItem?.itemName + " | Got: " + itemSlot.heldItem.itemName);
            StartCoroutine(FlashSlotRed());
            return;
        }

        // Consume item from inventory
        Inventory.Instance.RemoveItem(itemSlot.heldItem);

        // Activate
        isActivated = true;

        if (machineAnimator != null)
            machineAnimator.SetTrigger("Activate");

        if (activatedVFX != null)
            activatedVFX.SetActive(true);

        Debug.Log("Wind machine activated with: " + itemSlot.heldItem.itemName);

        // Notify wind zone in case player is already standing inside
        windZone?.OnMachineActivated();

        ClosePanel();
    }

    IEnumerator FlashSlotRed()
    {
        if (itemSlot != null)
        {
            Image slotImage = itemSlot.GetComponentInChildren<Image>();
            if (slotImage != null)
            {
                slotImage.color = Color.red;
                yield return new WaitForSecondsRealtime(0.5f);
                slotImage.color = Color.white;
            }
        }
    }
}