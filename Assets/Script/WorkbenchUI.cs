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
        // Auto-assign this WorkbenchUI to both slots
        if (slotA != null) slotA.workbenchUI = this;
        if (slotB != null) slotB.workbenchUI = this;

        workbenchPanel.SetActive(false);

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
        Debug.Log("OnSlotChanged — matched recipe: " +
                  (currentMatchedRecipe == null ? "NULL" : currentMatchedRecipe.recipeName));

        Image[] resultImages = resultIcon != null ?
            resultIcon.GetComponentsInChildren<Image>(true) : null;

        Debug.Log("Result images count: " +
                  (resultImages == null ? "NULL" : resultImages.Length.ToString()));

        if (currentMatchedRecipe != null)
        {
            if (resultImages != null && resultImages.Length >= 2)
            {
                resultImages[1].sprite = currentMatchedRecipe.result.icon;
                resultImages[1].enabled = true;
                resultImages[1].color = Color.white;
                Debug.Log("Set result icon to: " + currentMatchedRecipe.result.itemName);
            }
            else if (resultIcon != null)
            {
                resultIcon.sprite = currentMatchedRecipe.result.icon;
                resultIcon.enabled = true;
                Debug.Log("Set result icon (root) to: " + currentMatchedRecipe.result.itemName);
            }
            else
            {
                Debug.LogError("resultIcon is NULL — not assigned in Inspector");
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

        Debug.Log("Finding recipe — SlotA: " + (a == null ? "NULL" : a.itemName) +
                  " | SlotB: " + (b == null ? "NULL" : b.itemName));

        if (a == null || b == null) return null;

        foreach (CraftRecipe recipe in allRecipes)
        {
            if (recipe == null) { Debug.LogWarning("Null recipe in allRecipes"); continue; }
            if (recipe.ingredients.Length < 2)
            {
                Debug.LogWarning("Recipe " + recipe.name + " has less than 2 ingredients");
                continue;
            }

            Debug.Log("Checking recipe: " + recipe.recipeName +
                      " needs: " + recipe.ingredients[0].item?.itemName +
                      " + " + recipe.ingredients[1].item?.itemName);

            Debug.Log("SlotA ref: " + a.GetInstanceID() +
                      " Recipe ingredient 0 ref: " +
                      recipe.ingredients[0].item?.GetInstanceID());

            Debug.Log("SlotB ref: " + b.GetInstanceID() +
                      " Recipe ingredient 1 ref: " +
                      recipe.ingredients[1].item?.GetInstanceID());

            bool match =
                (recipe.ingredients[0].item == a && recipe.ingredients[1].item == b) ||
                (recipe.ingredients[0].item == b && recipe.ingredients[1].item == a);

            if (match)
            {
                Debug.Log("Recipe matched: " + recipe.recipeName);
                return recipe;
            }
        }

        Debug.Log("No matching recipe found");
        return null;
    }

    public void Craft()
    {
        if (currentMatchedRecipe == null)
        {
            Debug.LogWarning("Craft called but no matched recipe");
            return;
        }

        Debug.Log("Crafting: " + currentMatchedRecipe.result.itemName);
        Inventory.Instance.AddItem(currentMatchedRecipe.result);

        slotA.ClearSlot();
        slotB.ClearSlot();
        OnSlotChanged();
        RefreshItemGrid();
    }
}