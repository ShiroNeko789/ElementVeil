using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using TMPro;

public class Chest : MonoBehaviour, IInteractable
{
    [Header("Contents")]
    public ChestItemEntry[] contents;

    [Header("Animator")]
    public Animator chestAnimator;

    [Header("UI")]
    public GameObject chestPanel;
    public Transform itemContainer;
    public GameObject chestItemButtonPrefab;

    // Tracks which items have been claimed this session
    private HashSet<int> claimedIndexes = new HashSet<int>();
    private bool isOpen = false;

    void Start()
    {
        if (chestPanel != null) chestPanel.SetActive(false);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Player")) return;
        InteractManager.Instance.RegisterInteractable(this);
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (!other.CompareTag("Player")) return;
        InteractManager.Instance.UnregisterInteractable(this);
        ClosePanel();
    }

    public void OnInteract()
    {
        if (isOpen) return;
        OpenChestPanel();
    }

    void OpenChestPanel()
    {
        if (chestPanel == null) return;

        if (chestAnimator != null)
            chestAnimator.SetTrigger("Open");

        // Clear old buttons
        foreach (Transform child in itemContainer)
            Destroy(child.gameObject);

        // Spawn button for each item
        for (int i = 0; i < contents.Length; i++)
        {
            ChestItemEntry entry = contents[i];
            if (entry.item == null) continue;

            GameObject btnObj = Instantiate(chestItemButtonPrefab, itemContainer);

            // Set icon
            Image[] images = btnObj.GetComponentsInChildren<Image>(true);
            Sprite iconSprite = entry.icon != null ? entry.icon : entry.item.icon;
            if (images.Length >= 2)
            {
                images[1].sprite = iconSprite;
                images[1].color = Color.white;
                images[1].enabled = true;
            }

            // Set name
            TextMeshProUGUI label = btnObj.GetComponentInChildren<TextMeshProUGUI>();
            if (label != null) label.text = entry.item.itemName;

            // If already claimed this session — grey out immediately
            CanvasGroup cg = btnObj.GetComponent<CanvasGroup>();
            if (cg == null) cg = btnObj.AddComponent<CanvasGroup>();

            if (claimedIndexes.Contains(i))
            {
                // Already claimed — show greyed out, not interactable
                cg.alpha = 0.4f;
                cg.interactable = false;
            }
            else
            {
                // Not claimed — wire button
                int capturedIndex = i;
                Item capturedItem = entry.item;
                btnObj.GetComponent<Button>().onClick.AddListener(
                    () => CollectItem(capturedItem, capturedIndex, cg));
            }
        }

        isOpen = true;
        chestPanel.SetActive(true);
        Time.timeScale = 0f;
    }

    void CollectItem(Item item, int index, CanvasGroup cg)
    {
        // Add to inventory
        Inventory.Instance.AddItem(item);
        Debug.Log("Collected: " + item.itemName);

        // Mark as claimed this session
        claimedIndexes.Add(index);

        // Grey out button
        cg.alpha = 0.4f;
        cg.interactable = false;
    }

    public void ClosePanel()
    {
        if (chestPanel != null) chestPanel.SetActive(false);
        Time.timeScale = 1f;
        isOpen = false;
    }
}