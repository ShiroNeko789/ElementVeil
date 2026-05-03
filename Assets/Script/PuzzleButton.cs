using UnityEngine;

public class PuzzleButton : MonoBehaviour
{
    [Header("Separate Light Reference")]
    public SpriteRenderer lightRenderer; // Drag the distant Light object here
    public Sprite redLightSprite;        // Your Red image
    public Sprite greenLightSprite;      // Your Green image

    [Header("Linked Floor")]
    public FallingFloor targetFloor;

    private void OnTriggerEnter2D(Collider2D other)
    {
        // Detects the Metal Block
        if (other.CompareTag("Metal") || other.GetComponent<MagneticItem>() != null)
        {
            SetPuzzleState(true);
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        // Detects when the Metal Block is moved away
        if (other.CompareTag("Metal") || other.GetComponent<MagneticItem>() != null)
        {
            SetPuzzleState(false);
        }
    }

    void SetPuzzleState(bool active)
    {
        if (active)
        {
            if (lightRenderer != null) lightRenderer.sprite = greenLightSprite;
            if (targetFloor != null) targetFloor.Fall();
        }
        else
        {
            if (lightRenderer != null) lightRenderer.sprite = redLightSprite;
            if (targetFloor != null) targetFloor.ResetFloor();
        }
    }
}