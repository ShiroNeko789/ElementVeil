using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using TMPro;

public class WorkbenchUI : MonoBehaviour
{
    [Header("Panels")]
    public GameObject workbenchPanel;

    [Header("Ingredient Slots")]
    public WorkbenchSlot slotA;
    public WorkbenchSlot slotB;

    [Header("Result")]
    public Image resultIcon;

    [Header("Item Grid")]
    public Transform itemGridContent;
    public GameObject itemSlotPrefab;

    [Header("Recipes")]
    public CraftRecipe[] allRecipes;

    [Header("Player")]
    public MonoBehaviour playerMovementScript;

    private CraftRecipe currentMatchedRecipe = null;
    private List<GameObject> spawnedSlots = new List<GameObject>();

    void Start()
    {
        workbenchPanel.SetActive(false);

        // Hide result icon child at start
        if (resultIcon != null)
        {
            Image[] resultImages = resultIcon.GetComponentsInChildren<Image>(true);
            if (resultImages.Length >= 2) resultImages[1].enabled = false;
            else resultIcon.enabled = false;
        }

        Inventory.Instance.onInventoryChangedCallback += RefreshItemGrid;
    }

    public void Open()
    {
        workbenchPanel.SetActive(true);
        RefreshItemGrid();
        OnSlotChanged();
        Time.timeScale = 0f;
        if (playerMovementScript != null) playerMovementScript.enabled = false;
        UIManager.Instance?.OnPanelOpened(workbenchPanel);
    }

    public void Close()
    {
        workbenchPanel.SetActive(false);
        slotA.ClearSlot();
        slotB.ClearSlot();
        OnSlotChanged();
        Time.timeScale = 1f;
        if (playerMovementScript != null) playerMovementScript.enabled = true;
        UIManager.Instance?.OnAllPanelsClosed();
    }

    public void ResetSlots()
    {
        slotA.ClearSlot();
        slotB.ClearSlot();
        OnSlotChanged();
        Debug.Log("Slots reset");
    }

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

            // Set icon using same approach as InventoryUI
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

    public void OnSlotChanged()
    {
        currentMatchedRecipe = FindMatchingRecipe();

        Image[] resultImages = resultIcon != null ?
            resultIcon.GetComponentsInChildren<Image>(true) : null;

        if (currentMatchedRecipe != null)
        {
            if (resultImages != null && resultImages.Length >= 2)
            {
                resultImages[1].sprite = currentMatchedRecipe.result.icon;
                resultImages[1].enabled = true;
                resultImages[1].color = Color.white;
            }
            else if (resultIcon != null)
            {
                resultIcon.sprite = currentMatchedRecipe.result.icon;
                resultIcon.enabled = true;
            }
        }
        else
        {
            if (resultImages != null && resultImages.Length >= 2)
                resultImages[1].enabled = false;
            else if (resultIcon != null)
                resultIcon.enabled = false;
        }
    }

    CraftRecipe FindMatchingRecipe()
    {
        Item a = slotA.heldItem;
        Item b = slotB.heldItem;
        if (a == null || b == null) return null;

        foreach (CraftRecipe recipe in allRecipes)
        {
            if (recipe.ingredients.Length < 2) continue;
            bool match =
                (recipe.ingredients[0].item == a && recipe.ingredients[1].item == b) ||
                (recipe.ingredients[0].item == b && recipe.ingredients[1].item == a);
            if (match) return recipe;
        }
        return null;
    }

    public void Craft()
    {
        if (currentMatchedRecipe == null) return;

        Inventory.Instance.AddItem(currentMatchedRecipe.result);

        slotA.ClearSlot();
        slotB.ClearSlot();
        OnSlotChanged();
        RefreshItemGrid();
    }
}