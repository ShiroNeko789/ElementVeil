using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System.Collections;

public class GloveController : MonoBehaviour,
    IPointerDownHandler, IPointerUpHandler
{
    public static GloveController Instance;

    [Header("UI")]
    public GameObject gloveButton;
    public Image gloveButtonImage;
    public Sprite gloveEquippedSprite;
    public Sprite gloveUnequippedSprite;

    [Header("Magnet Switch Button")]
    public Image magnetSwitchIcon;
    public Sprite magnetModeSprite;
    public Sprite gloveModeSprite;

    [Header("Active Glow — World Space")]
    public GameObject glowEffect;       // child GameObject on player with SpriteRenderer
    public SpriteRenderer glowRenderer; // SpriteRenderer on glowEffect
    public Color glowColor = new Color(1f, 0.9f, 0f, 0.7f); // yellow

    [Header("Settings")]
    public float gloveActiveDuration = -1f; // -1 = hold to activate

    private bool gloveActive = false;
    private bool gloveEquipped = false;

    void Awake() { Instance = this; }

    void Start()
    {
        if (gloveButton != null) gloveButton.SetActive(false);
        if (glowEffect != null) glowEffect.SetActive(false);

        if (EquipmentManager.Instance != null)
            EquipmentManager.Instance.onEquipmentChangedCallback += OnEquipmentChanged;
    }

    void OnDestroy()
    {
        if (EquipmentManager.Instance != null)
            EquipmentManager.Instance.onEquipmentChangedCallback -= OnEquipmentChanged;
    }

    void OnEquipmentChanged(EquipmentType type, Equipment item)
    {
        if (type != EquipmentType.Glove) return;
        gloveEquipped = item != null;
        UpdateGloveUI();
    }

    void UpdateGloveUI()
    {
        if (gloveButton != null) gloveButton.SetActive(gloveEquipped);

        if (magnetSwitchIcon != null)
            magnetSwitchIcon.sprite = gloveEquipped ? gloveModeSprite : magnetModeSprite;

        if (gloveButtonImage != null)
            gloveButtonImage.sprite = gloveEquipped ?
                gloveEquippedSprite : gloveUnequippedSprite;
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        if (!gloveEquipped) return;
        ActivateGlove();
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        if (gloveActiveDuration < 0f)
            DeactivateGlove();
    }

    void ActivateGlove()
    {
        gloveActive = true;

        // Show yellow glow behind player
        if (glowEffect != null) glowEffect.SetActive(true);
        if (glowRenderer != null) glowRenderer.color = glowColor;

        Debug.Log("Glove activated");
    }

    void DeactivateGlove()
    {
        gloveActive = false;

        // Hide glow
        if (glowEffect != null) glowEffect.SetActive(false);

        Debug.Log("Glove deactivated");
    }

    public bool IsGloveActive() { return gloveActive && gloveEquipped; }
    public bool IsGloveEquipped() { return gloveEquipped; }

    public void PressGlove()
    {
        if (!gloveEquipped) return;
        ActivateGlove();
    }

    public void ReleaseGlove()
    {
        if (gloveActiveDuration < 0f)
            DeactivateGlove();
    }
}