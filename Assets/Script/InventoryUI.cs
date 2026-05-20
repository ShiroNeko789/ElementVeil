using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using TMPro;

public class InventoryUI : MonoBehaviour
{
    [Header("References")]
    public GameObject inventoryPanel;
    public GameObject combatUI;
    public GameObject itemSlotPrefab;
    public Transform itemGrid;

    [Header("Description Panel")]
    public GameObject descriptionPanel;
    public Image descItemImage;
    public TextMeshProUGUI descItemName;
    public TextMeshProUGUI descItemDescription;

    [Header("Item Slot")]
    public Image slotIconTemplate;

    [Header("Equipment Slots")]
    public EquipmentSlot[] equipmentSlots;

    private List<GameObject> spawnedSlots = new List<GameObject>();

    void Start()
    {
        // Check every reference before using it
        if (inventoryPanel == null) { Debug.LogError("InventoryUI: inventoryPanel not assigned"); return; }
        if (itemSlotPrefab == null) { Debug.LogError("InventoryUI: itemSlotPrefab not assigned"); return; }
        if (itemGrid == null) { Debug.LogError("InventoryUI: itemGrid not assigned"); return; }
        if (descriptionPanel == null) { Debug.LogError("InventoryUI: descriptionPanel not assigned"); return; }
        if (Inventory.Instance == null) { Debug.LogError("InventoryUI: Inventory.Instance is null — is GameManager in the scene?"); return; }

        inventoryPanel.SetActive(false);
        descriptionPanel.SetActive(false);
        Inventory.Instance.onInventoryChangedCallback += RefreshUI;
        Debug.Log("InventoryUI started successfully");
    }

    [Header("Player")]
    public MonoBehaviour playerMovementScript;

    public void ToggleInventory()
    {
        if (inventoryPanel == null) return;
        bool open = !inventoryPanel.activeSelf;
        inventoryPanel.SetActive(open);
        if (combatUI != null) combatUI.SetActive(!open);
        if (open)
        {
            RefreshUI();
            UIManager.Instance?.OnPanelOpened(inventoryPanel);
        }
        else
        {
            if (descriptionPanel != null) descriptionPanel.SetActive(false);
            UIManager.Instance?.OnAllPanelsClosed();
        }
        Time.timeScale = open ? 0f : 1f;
        if (playerMovementScript != null) playerMovementScript.enabled = !open;
    }
    public void RefreshUI()
    {
        if (Inventory.Instance == null) { Debug.LogError("RefreshUI: Inventory.Instance is null"); return; }
        Debug.Log("RefreshUI called, item count: " + Inventory.Instance.items.Count);

        foreach (var s in spawnedSlots) Destroy(s);
        spawnedSlots.Clear();

        foreach (Item item in Inventory.Instance.items)
        {
            if (item == null) continue;

            GameObject slotObj = Instantiate(itemSlotPrefab, itemGrid);
            spawnedSlots.Add(slotObj);

            // Find Icon by index — first Image on the root, second is the Icon child
            Image[] images = slotObj.GetComponentsInChildren<Image>();
            // images[0] = root background, images[1] = Icon child
            if (images.Length >= 2)
            {
                images[1].sprite = item.icon;
                images[1].color = Color.white;
            }
            else if (images.Length == 1)
            {
                images[0].sprite = item.icon;
                images[0].color = Color.white;
            }

            Button btn = slotObj.GetComponent<Button>();
            if (btn != null)
            {
                Item captured = item;
                btn.onClick.AddListener(() => ShowDescription(captured));
            }

            DraggableItem drag = slotObj.GetComponent<DraggableItem>();
            if (drag != null) drag.item = item;
        }
    }

    void ShowDescription(Item item)
    {
        if (descriptionPanel == null) return;
        descriptionPanel.SetActive(true);
        if (descItemImage != null) descItemImage.sprite = item.icon;
        if (descItemName != null) descItemName.text = item.itemName;
        if (descItemDescription != null) descItemDescription.text = item.description;
    }

    public void CloseDescription()
    {
        if (descriptionPanel != null) descriptionPanel.SetActive(false);
    }
}