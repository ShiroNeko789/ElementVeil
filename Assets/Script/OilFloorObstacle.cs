using UnityEngine;
using System.Collections;

public class OilFloorObstacle : MonoBehaviour
{
    [Header("Settings")]
    public float damagePerTick = 1f;         // 1 damage per tick on 5hp
    public float tickInterval = 2f;          // every 2 seconds
    public float neutralizedDuration = 5f;   // how long before oil recovers

    [Header("Visuals")]
    public SpriteRenderer oilSprite;
    public Color oilColor = new Color(0.1f, 0.1f, 0.1f, 0.9f);
    public Color neutralizedColor = new Color(0.4f, 0.4f, 0.4f, 0.6f);

    private bool isNeutralized = false;
    private bool playerInside = false;

    void Start()
    {
        if (oilSprite == null) oilSprite = GetComponent<SpriteRenderer>();
        if (oilSprite != null) oilSprite.color = oilColor;
    }

    // Called by OilThrowable on hit
    public void Neutralize()
    {
        if (isNeutralized) return;
        StartCoroutine(NeutralizeTemporarily());
    }

    IEnumerator NeutralizeTemporarily()
    {
        isNeutralized = true;

        // Lighten color to show neutralized
        if (oilSprite != null) oilSprite.color = neutralizedColor;

        yield return new WaitForSeconds(neutralizedDuration);

        // Recover — oil comes back
        isNeutralized = false;
        if (oilSprite != null) oilSprite.color = oilColor;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Player")) return;
        playerInside = true;
        StartCoroutine(DamagePlayer(other.gameObject));
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (!other.CompareTag("Player")) return;
        playerInside = false;
    }

    IEnumerator DamagePlayer(GameObject player)
    {
        PlayerHealth ph = player.GetComponent<PlayerHealth>();
        if (ph == null) yield break;

        while (playerInside)
        {
            if (!isNeutralized)
                ph.TakeDamage(Mathf.RoundToInt(damagePerTick), transform.position);
            yield return new WaitForSeconds(tickInterval);
        }
    }
}