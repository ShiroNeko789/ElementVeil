using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;

public class RubbishCarrier : MonoBehaviour
{
    public static RubbishCarrier Instance;

    [Header("UI — shows held rubbish")]
    public GameObject heldRubbishUI;
    public Image heldRubbishIcon;
    public TextMeshProUGUI heldRubbishName;

    // We no longer need the hardcoded sprite fields here!

    private Queue<GroundRubbish> heldRubbish = new Queue<GroundRubbish>();
    private Queue<Sprite> heldSprites = new Queue<Sprite>(); // Keeps track of unique visual styles

    public GroundRubbish CurrentRubbish => heldRubbish.Count > 0 ? heldRubbish.Peek() : null;

    void Awake() { Instance = this; }

    void Start()
    {
        // Explicitly ensure UI is completely hidden at start
        if (heldRubbishUI != null) heldRubbishUI.SetActive(false);
    }

    public void PickupRubbish(GroundRubbish rubbish)
    {
        heldRubbish.Enqueue(rubbish);

        // Grab the exact sprite variant this specific item was using
        SpriteRenderer sr = rubbish.GetComponent<SpriteRenderer>();
        heldSprites.Enqueue(sr != null ? sr.sprite : null);

        UpdateUI();
    }

    public GroundRubbish TakeRubbish()
    {
        if (heldRubbish.Count == 0) return null;

        GroundRubbish r = heldRubbish.Dequeue();
        heldSprites.Dequeue(); // Clear matching sprite

        UpdateUI();
        return r;
    }

    void UpdateUI()
    {
        // If empty, turn the UI completely off
        if (heldRubbish.Count == 0)
        {
            if (heldRubbishUI != null) heldRubbishUI.SetActive(false);
            return;
        }

        // If we have items, turn the UI on!
        if (heldRubbishUI != null) heldRubbishUI.SetActive(true);

        GroundRubbish current = heldRubbish.Peek();
        Sprite currentSprite = heldSprites.Peek();

        if (heldRubbishIcon != null && currentSprite != null)
            heldRubbishIcon.sprite = currentSprite;

        if (heldRubbishName != null)
            heldRubbishName.text = current.rubbishType.ToString();
    }
}