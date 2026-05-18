using UnityEngine;
using System.Collections;

public class GreenWaterObstacle : MonoBehaviour
{
    [Header("Settings")]
    public int poisonDamagePerTick = 1;    // 1 damage per tick
    public float poisonTickInterval = 2f;  // every 2 seconds — slow drain on 5hp
    public float poisonDuration = 4f;      // linger time after leaving

    [Header("Neutralize Settings")]
    public int throwsRequired = 4;         // how many throws to fully neutralize
    private int currentThrows = 0;

    [Header("Visuals")]
    public SpriteRenderer waterSprite;
    public Color greenColor = new Color(0.2f, 0.8f, 0.2f, 1f);
    public Color blueColor = new Color(0.2f, 0.4f, 1f, 1f);

    private bool isNeutralized = false;
    private bool playerInside = false;

    void Start()
    {
        if (waterSprite == null) waterSprite = GetComponent<SpriteRenderer>();
        if (waterSprite != null) waterSprite.color = greenColor;
    }

    // Called by WaterThrowable each hit
    public void Neutralize()
    {
        if (isNeutralized) return;

        currentThrows++;
        Debug.Log("Water throws: " + currentThrows + "/" + throwsRequired);

        // Gradually shift color based on progress
        float progress = (float)currentThrows / throwsRequired;
        if (waterSprite != null)
            waterSprite.color = Color.Lerp(greenColor, blueColor, progress);

        if (currentThrows >= throwsRequired)
        {
            isNeutralized = true;
            StartCoroutine(FullyNeutralize());
        }
    }

    IEnumerator FullyNeutralize()
    {
        // Finish color transition
        if (waterSprite != null)
        {
            float elapsed = 0f;
            Color startColor = waterSprite.color;
            while (elapsed < 0.5f)
            {
                elapsed += Time.deltaTime;
                waterSprite.color = Color.Lerp(startColor, blueColor, elapsed / 0.5f);
                yield return null;
            }
            waterSprite.color = blueColor;
        }

        // Make walkable
        Collider2D col = GetComponent<Collider2D>();
        if (col != null) col.isTrigger = false;

        Debug.Log("Green water fully neutralized — now walkable");
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Player")) return;
        if (isNeutralized) return;
        playerInside = true;
        StartCoroutine(PoisonPlayer(other.gameObject));
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (!other.CompareTag("Player")) return;
        playerInside = false;
    }

    IEnumerator PoisonPlayer(GameObject player)
    {
        PlayerHealth ph = player.GetComponent<PlayerHealth>();
        if (ph == null) yield break;

        ph.SetPoisonVisual(true);
        Debug.Log("Player poisoned");

        // Damage while inside
        while (playerInside && !isNeutralized)
        {
            ph.TakePoisonDamage(poisonDamagePerTick);
            yield return new WaitForSeconds(poisonTickInterval);
        }

        // Linger poison after leaving
        float timer = 0f;
        while (timer < poisonDuration)
        {
            ph.TakePoisonDamage(poisonDamagePerTick);
            timer += poisonTickInterval;
            yield return new WaitForSeconds(poisonTickInterval);
        }

        ph.SetPoisonVisual(false);
        Debug.Log("Poison wore off");
    }
}