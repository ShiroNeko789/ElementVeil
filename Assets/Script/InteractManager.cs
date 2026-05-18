using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class InteractManager : MonoBehaviour
{
    public static InteractManager Instance;

    [Header("Shoot Button")]
    public Button shootButton;              // your existing shoot button
    public Image shootButtonImage;          // image on shoot button
    public Sprite shootSprite;             // normal shoot icon
    public Sprite interactSprite;          // interact icon (e.g. hand/exclamation)

    private IInteractable currentInteractable = null;
    private bool isInteractMode = false;

    void Awake()
    {
        Instance = this;
    }

    // Called by interactables when player enters their trigger
    public void RegisterInteractable(IInteractable interactable)
    {
        currentInteractable = interactable;
        SetInteractMode(true);
    }

    // Called by interactables when player exits their trigger
    public void UnregisterInteractable(IInteractable interactable)
    {
        if (currentInteractable == interactable)
        {
            currentInteractable = null;
            SetInteractMode(false);
        }
    }

    void SetInteractMode(bool interact)
    {
        isInteractMode = interact;

        // Swap sprite
        if (shootButtonImage != null)
            shootButtonImage.sprite = interact ? interactSprite : shootSprite;

        // Rewire button
        shootButton.onClick.RemoveAllListeners();
        if (interact)
            shootButton.onClick.AddListener(OnInteractPressed);
        else
            shootButton.onClick.AddListener(OnShootPressed);
    }

    void OnInteractPressed()
    {
        currentInteractable?.OnInteract();
    }

    void OnShootPressed()
    {
        // Call your existing shoot method here
        // e.g. GetComponent<PlayerShooting>()?.Shoot();
        Debug.Log("Shoot pressed");
    }
}