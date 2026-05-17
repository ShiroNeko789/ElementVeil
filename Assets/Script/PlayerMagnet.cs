using UnityEngine;
using UnityEngine.UI;

public class PlayerMagnet : MonoBehaviour
{
    [Header("UI Groups")]
    public GameObject combatUIGroup;
    public GameObject magnetUIGroup;

    [Header("Polarity Button Images")]
    public Image polarityButtonImage;
    public Sprite northSprite;
    public Sprite southSprite;

    [Header("Switch Button Image")]
    public Image switchButtonImage;     // Image on the button that toggles between modes
    public Sprite shootModeSprite;      // shown when currently in shoot mode
    public Sprite magnetModeSprite;     // shown when currently in magnet mode

    [Header("Polarity Settings")]
    public bool playerIsNorth = true;
    public bool magnetModeActive = false;

    private MagneticItem selectedItem;

    void Start()
    {
        magnetModeActive = false;
        combatUIGroup.SetActive(true);
        magnetUIGroup.SetActive(false);
        UpdatePolarityImage();
        UpdateSwitchButtonImage();
    }

    public void ToggleMagnetMode()
    {
        magnetModeActive = !magnetModeActive;

        if (magnetModeActive)
        {
            combatUIGroup.SetActive(false);
            magnetUIGroup.SetActive(true);
        }
        else
        {
            combatUIGroup.SetActive(true);
            magnetUIGroup.SetActive(false);
            DeselectTarget();
        }

        UpdateSwitchButtonImage();
    }

    public void TogglePolarity()
    {
        playerIsNorth = !playerIsNorth;
        UpdatePolarityImage();
    }

    void UpdatePolarityImage()
    {
        if (polarityButtonImage == null) return;
        polarityButtonImage.sprite = playerIsNorth ? northSprite : southSprite;
    }

    void UpdateSwitchButtonImage()
    {
        if (switchButtonImage == null) return;
        // Show magnet icon when in shoot mode (press to go to magnet)
        // Show shoot icon when in magnet mode (press to go back to shoot)
        switchButtonImage.sprite = magnetModeActive ? shootModeSprite : magnetModeSprite;
    }

    public void SelectNewTarget(MagneticItem item)
    {
        if (selectedItem != null) selectedItem.SetHighlight(false);
        selectedItem = item;
        selectedItem.SetHighlight(true);
    }

    private void DeselectTarget()
    {
        if (selectedItem != null)
        {
            selectedItem.SetHighlight(false);
            selectedItem.StopMoving();
        }
        selectedItem = null;
    }

    void Update()
    {
        if (magnetModeActive && selectedItem != null)
            selectedItem.ApplyMagneticForce(transform.position, 10f, playerIsNorth);
    }
}