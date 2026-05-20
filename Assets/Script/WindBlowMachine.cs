using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;

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

    [Header("UI — Item Slot")]
    public WindMachineSlot itemSlot;

    [Header("UI — Inventory Grid")]
    public Transform itemGridContent;
    public GameObject itemSlotPrefab;

    [Header("Animator")]
    public Animator machineAnimator;

    [Header("Wind Zone")]
    public WindZone windZone;

    [Header("Player")]
    public MonoBehaviour playerMovementScript;

    private List<GameObject> spawnedSlots = new List<GameObject>();

    void Start()
    {
        // Hide panel at start
        if (machinePanel != null)
            machinePanel.SetActive(false);

        // Hide VFX at start
        if (activatedVFX != null)
            activatedVFX.SetActive(false);

        // Assign owner to slot
        if (itemSlot != null)
            itemSlot.windMachine = this;

        // Refresh inventory when inventory changes
        if (Inventory.Instance != null)
            Inventory.Instance.onInventoryChangedCallback += RefreshItemGrid;
    }

    // ─────────────────────────────────────────────
    // OPEN PANEL
    // ─────────────────────────────────────────────

    public void OpenPanel()
    {
        // Already activated
        if (isActivated)
            return;

        if (machinePanel != null)
            machinePanel.SetActive(true);

        RefreshItemGrid();

        // Pause game
        Time.timeScale = 0f;

        // Disable player movement
        if (playerMovementScript != null)
            playerMovementScript.enabled = false;

        // Notify UI manager
        UIManager.Instance?.OnPanelOpened(machinePanel);
    }

    // ─────────────────────────────────────────────
    // CLOSE PANEL
    // ─────────────────────────────────────────────

    public void ClosePanel()
    {
        if (machinePanel != null)
            machinePanel.SetActive(false);

        // Clear inserted item
        if (itemSlot != null)
            itemSlot.ClearSlot();

        // Resume game
        Time.timeScale = 1f;

        // Enable movement
        if (playerMovementScript != null)
            playerMovementScript.enabled = true;

        UIManager.Instance?.OnAllPanelsClosed();
    }

    // ─────────────────────────────────────────────
    // REFRESH INVENTORY GRID
    // ─────────────────────────────────────────────

    public void RefreshItemGrid()
    {
        if (itemGridContent == null || itemSlotPrefab == null)
            return;

        // Clear old slots
        foreach (GameObject obj in spawnedSlots)
        {
            Destroy(obj);
        }

        spawnedSlots.Clear();

        // Spawn new inventory slots
        foreach (Item item in Inventory.Instance.items)
        {
            if (item == null)
                continue;

            GameObject slotObj =
                Instantiate(itemSlotPrefab, itemGridContent);

            spawnedSlots.Add(slotObj);

            // Set icon
            Image[] images =
                slotObj.GetComponentsInChildren<Image>(true);

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

            // Assign draggable item
            DraggableItem drag =
                slotObj.GetComponent<DraggableItem>();

            if (drag != null)
                drag.item = item;
        }
    }

    // ─────────────────────────────────────────────
    // SLOT CHANGED
    // ─────────────────────────────────────────────

    public void OnSlotChanged()
    {
        // Optional visual feedback
    }

    // ─────────────────────────────────────────────
    // INSERT ITEM
    // ─────────────────────────────────────────────

    public void InsertItem()
    {
        // No item inserted
        if (itemSlot == null || itemSlot.heldItem == null)
        {
            Debug.Log("No item placed in slot.");
            StartCoroutine(FlashSlotRed());
            return;
        }

        // Wrong item
        if (itemSlot.heldItem != requiredItem)
        {
            Debug.Log(
                "Wrong item! Need: " +
                requiredItem?.itemName +
                " | Got: " +
                itemSlot.heldItem.itemName
            );

            StartCoroutine(FlashSlotRed());
            return;
        }

        // Remove item from inventory
        Inventory.Instance.RemoveItem(itemSlot.heldItem);

        // Activate machine
        isActivated = true;

        // Play machine animation
        if (machineAnimator != null)
            machineAnimator.SetTrigger("Activate");

        // Enable VFX
        if (activatedVFX != null)
            activatedVFX.SetActive(true);

        Debug.Log(
            "Wind machine activated with: " +
            itemSlot.heldItem.itemName
        );

        // Activate wind zone
        if (windZone != null)
            windZone.ActivateWind();

        // Close panel
        ClosePanel();
    }

    // ─────────────────────────────────────────────
    // FLASH SLOT RED
    // ─────────────────────────────────────────────

    IEnumerator FlashSlotRed()
    {
        if (itemSlot != null)
        {
            Image slotImage =
                itemSlot.GetComponentInChildren<Image>();

            if (slotImage != null)
            {
                Color originalColor = slotImage.color;

                slotImage.color = Color.red;

                yield return new WaitForSecondsRealtime(0.5f);

                slotImage.color = originalColor;
            }
        }
    }

    // ─────────────────────────────────────────────
    // CLEANUP
    // ─────────────────────────────────────────────

    private void OnDestroy()
    {
        if (Inventory.Instance != null)
        {
            Inventory.Instance.onInventoryChangedCallback
                -= RefreshItemGrid;
        }
    }
}