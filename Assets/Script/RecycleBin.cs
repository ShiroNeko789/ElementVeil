using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class RecycleBin : MonoBehaviour, IInteractable
{
    [Header("Bin Settings")]
    public BinColor binColor;
    public RubbishBoss boss;

    [Header("Visual Feedback")]
    public SpriteRenderer binSprite;
    public Color correctColor = Color.green;
    public Color wrongColor = Color.red;
    private Color originalColor;

    void Start()
    {
        if (binSprite == null) binSprite = GetComponent<SpriteRenderer>();
        if (binSprite != null) originalColor = binSprite.color;
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
    }

    public void OnInteract()
    {
        RubbishCarrier carrier = FindObjectOfType<RubbishCarrier>();
        if (carrier == null || carrier.CurrentRubbish == null)
        {
            Debug.Log("No rubbish to deposit");
            return;
        }

        GroundRubbish rubbish = carrier.TakeRubbish();
        bool correct = IsCorrectBin(rubbish.rubbishType);

        if (correct)
        {
            Debug.Log("Correct bin! " + rubbish.rubbishType + " in " + binColor + " bin");
            boss?.OnCorrectRecycle();
            StartCoroutine(FlashBin(correctColor));
        }
        else
        {
            Debug.Log("Wrong bin! " + rubbish.rubbishType +
                      " does not go in " + binColor + " bin");
            StartCoroutine(FlashBin(wrongColor));

            // Spawn rubbish back on ground
            rubbish.gameObject.SetActive(true);
            rubbish.transform.position = transform.position +
                new Vector3(Random.Range(-1f, 1f), 0.5f, 0f);
            carrier.PickupRubbish(rubbish); // give back to player
        }
    }

    bool IsCorrectBin(RubbishType type)
    {
        switch (type)
        {
            case RubbishType.Paper:
                return binColor == BinColor.Blue;
            case RubbishType.Glass:
                return binColor == BinColor.Brown;
            case RubbishType.Can:
                return binColor == BinColor.Orange;
            case RubbishType.PlasticBag:
                return binColor == BinColor.Orange;
            default:
                return false;
        }
    }

    IEnumerator FlashBin(Color flashColor)
    {
        if (binSprite != null)
        {
            binSprite.color = flashColor;
            yield return new WaitForSeconds(0.4f);
            binSprite.color = originalColor;
        }
    }
}