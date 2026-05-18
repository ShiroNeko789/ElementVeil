using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;

public class ThrowSystem : MonoBehaviour
{
    public static ThrowSystem Instance;

    [Header("UI — Throwable Slot")]
    public Image selectedIcon;
    public Sprite emptySlotSprite;

    [Header("UI — Selector Panel")]
    public GameObject selectorPanel;
    public Transform itemContainer;
    public GameObject throwableButtonPrefab;

    [Header("UI — Throw Button")]
    public GameObject throwButton;

    [Header("Cooldown")]
    public float throwCooldown = 1f;
    public Image throwButtonCooldownFill;
    private bool isOnCooldown = false;

    [Header("Throw Settings")]
    public Transform throwOrigin;

    private List<ThrowableItem> throwables = new List<ThrowableItem>();
    private int selectedIndex = -1;
    private bool selectorOpen = false;
    private List<GameObject> spawnedButtons = new List<GameObject>();

    void Awake() { Instance = this; }

    void Start()
    {
        if (selectorPanel != null) selectorPanel.SetActive(false);
        if (throwButtonCooldownFill != null) throwButtonCooldownFill.enabled = false;
        Inventory.Instance.onInventoryChangedCallback += RefreshThrowables;
        RefreshThrowables();
        UpdateSelectedIcon();
    }

    void RefreshThrowables()
    {
        throwables.Clear();
        foreach (Item item in Inventory.Instance.items)
            if (item is ThrowableItem t) throwables.Add(t);

        if (selectedIndex >= throwables.Count)
            selectedIndex = throwables.Count > 0 ? 0 : -1;

        UpdateSelectedIcon();
    }

    public void ToggleSelector()
    {
        if (throwables.Count == 0) return;

        selectorOpen = !selectorOpen;

        if (selectorOpen) OpenSelector();
        else CloseSelector();
    }

    void OpenSelector()
    {
        foreach (var b in spawnedButtons) Destroy(b);
        spawnedButtons.Clear();

        foreach (ThrowableItem item in throwables)
        {
            GameObject btn = Instantiate(throwableButtonPrefab, itemContainer);
            spawnedButtons.Add(btn);

            Image[] images = btn.GetComponentsInChildren<Image>(true);
            if (images.Length >= 2)
            {
                images[1].sprite = item.icon;
                images[1].color = Color.white;
                images[1].enabled = true;
            }

            ThrowableItem captured = item;
            btn.GetComponent<Button>().onClick.AddListener(() => SelectThrowable(captured));
        }

        selectorPanel.SetActive(true);
        StartCoroutine(AnimatePanel(true));
    }

    void CloseSelector()
    {
        StartCoroutine(AnimatePanel(false));
    }

    IEnumerator AnimatePanel(bool expanding)
    {
        RectTransform rt = selectorPanel.GetComponent<RectTransform>();
        float startWidth = expanding ? 0f : rt.sizeDelta.x;
        float targetWidth = expanding ? (throwables.Count * 90f) : 0f;
        float elapsed = 0f;
        float duration = 0.2f;

        if (expanding) selectorPanel.SetActive(true);

        while (elapsed < duration)
        {
            elapsed += Time.unscaledDeltaTime;
            float t = Mathf.SmoothStep(0f, 1f, elapsed / duration);
            rt.sizeDelta = new Vector2(Mathf.Lerp(startWidth, targetWidth, t), rt.sizeDelta.y);
            yield return null;
        }

        rt.sizeDelta = new Vector2(targetWidth, rt.sizeDelta.y);

        if (!expanding)
        {
            selectorPanel.SetActive(false);
            foreach (var b in spawnedButtons) Destroy(b);
            spawnedButtons.Clear();
        }
    }

    void SelectThrowable(ThrowableItem item)
    {
        selectedIndex = throwables.IndexOf(item);
        UpdateSelectedIcon();
        selectorOpen = false;
        CloseSelector();
        Debug.Log("Selected throwable: " + item.itemName);
    }

    void UpdateSelectedIcon()
    {
        if (selectedIcon == null) return;
        if (selectedIndex < 0 || throwables.Count == 0)
        {
            selectedIcon.sprite = emptySlotSprite;
            selectedIcon.color = new Color(1f, 1f, 1f, 0.3f);
        }
        else
        {
            selectedIcon.sprite = throwables[selectedIndex].icon;
            selectedIcon.color = Color.white;
        }
    }

    public void Throw()
    {
        if (isOnCooldown)
        {
            Debug.Log("Throw on cooldown");
            return;
        }

        if (selectedIndex < 0 || throwables.Count == 0)
        {
            Debug.LogWarning("No throwable selected");
            return;
        }

        ThrowableItem selected = throwables[selectedIndex];
        if (selected.projectilePrefab == null)
        {
            Debug.LogWarning("No projectile prefab on: " + selected.itemName);
            return;
        }

        Vector3 spawnPos = throwOrigin != null ? throwOrigin.position : transform.position;
        GameObject proj = Instantiate(selected.projectilePrefab, spawnPos, Quaternion.identity);

        float facingDir = transform.localScale.x >= 0 ? 1f : -1f;
        float clampedAngle = Mathf.Clamp(selected.throwAngle, 10f, 60f);
        float angleRad = clampedAngle * Mathf.Deg2Rad;

        Vector2 throwDir = new Vector2(
            facingDir * Mathf.Cos(angleRad),
            Mathf.Sin(angleRad)
        );

        Rigidbody2D rb = proj.GetComponent<Rigidbody2D>();
        if (rb != null)
        {
            rb.gravityScale = 2f;
            rb.linearVelocity = throwDir * selected.throwForce;
        }

        Debug.Log("Threw: " + selected.itemName);
        StartCoroutine(ThrowCooldown());
    }

    IEnumerator ThrowCooldown()
    {
        isOnCooldown = true;

        if (throwButtonCooldownFill != null)
        {
            throwButtonCooldownFill.fillAmount = 1f;
            throwButtonCooldownFill.enabled = true;
        }

        float elapsed = 0f;
        while (elapsed < throwCooldown)
        {
            elapsed += Time.deltaTime;
            if (throwButtonCooldownFill != null)
                throwButtonCooldownFill.fillAmount = 1f - (elapsed / throwCooldown);
            yield return null;
        }

        if (throwButtonCooldownFill != null)
        {
            throwButtonCooldownFill.fillAmount = 0f;
            throwButtonCooldownFill.enabled = false;
        }

        isOnCooldown = false;
    }
}