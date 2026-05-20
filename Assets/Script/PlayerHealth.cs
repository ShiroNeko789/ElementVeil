using UnityEngine;
using System.Collections;

public class PlayerHealth : MonoBehaviour
{
    public int maxHealth = 5;
    public int currentHealth;
    private bool isInvincible = false;
    private bool isPoisoned = false;

    [Header("Repel / Knockback")]
    public float knockbackForce = 12f;
    public float knockbackUpwardFactor = 4f;

    [Header("Visuals")]
    public Animator healthBarAnimator;
    public SpriteRenderer playerSprite;

    public int normalLayer;
    public int invincibleLayer;

    private Rigidbody2D rb;

    // Key used to carry health across scenes
    private const string HealthCarryKey = "CarriedHealth";

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        normalLayer = LayerMask.NameToLayer("Player");
        invincibleLayer = LayerMask.NameToLayer("PlayerInvincible");

        // ── Restore carried health if GameSaveManager passed one ──────────
        // GameSaveManager.SaveHealthForSceneTransition() writes this key
        // before loading the next scene. We read it once and delete it so
        // a future fresh start doesn't accidentally reuse it.
        if (PlayerPrefs.HasKey(HealthCarryKey))
        {
            currentHealth = PlayerPrefs.GetInt(HealthCarryKey);
            currentHealth = Mathf.Clamp(currentHealth, 1, maxHealth); // never arrive dead
            PlayerPrefs.DeleteKey(HealthCarryKey);
            PlayerPrefs.Save();
            Debug.Log("[PlayerHealth] Restored carried health: " + currentHealth);
        }
        else
        {
            // No carry key → brand-new game or explicit new-game reset
            currentHealth = maxHealth;
            Debug.Log("[PlayerHealth] No carry key — starting at max health: " + currentHealth);
        }

        UpdateUI();
    }

    // ── Called by GameSaveManager just before loading a new scene ─────────
    public void SaveHealthForTransition()
    {
        PlayerPrefs.SetInt(HealthCarryKey, currentHealth);
        PlayerPrefs.Save();
        Debug.Log("[PlayerHealth] Saved health for transition: " + currentHealth);
    }

    // ─────────────────────────────────────────────────────────────────────
    //  Everything below is unchanged from your original
    // ─────────────────────────────────────────────────────────────────────

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Enemy") && !isInvincible)
            TakeDamage(1, collision.transform.position);
    }

    public void TakeDamage(int damage, Vector2 enemyPosition)
    {
        if (isInvincible || currentHealth <= 0) return;
        currentHealth -= damage;
        currentHealth = Mathf.Max(currentHealth, 0);
        UpdateUI();
        Debug.Log("Player took damage: " + damage + " | Health: " + currentHealth);

        if (currentHealth <= 0)
        {
            gameObject.SetActive(false);
        }
        else
        {
            StopCoroutine("ApplyKnockback");
            StartCoroutine(ApplyKnockback(enemyPosition));
            if (!isPoisoned)
                StartCoroutine(IFrames());
        }
    }

    public void TakePoisonDamage(int damage)
    {
        if (currentHealth <= 0) return;
        currentHealth -= damage;
        currentHealth = Mathf.Max(currentHealth, 0);
        UpdateUI();
        Debug.Log("Poison damage: " + damage + " | Health: " + currentHealth);
        if (currentHealth <= 0) gameObject.SetActive(false);
    }

    private IEnumerator ApplyKnockback(Vector2 enemyPos)
    {
        float pushDir = (transform.position.x < enemyPos.x) ? -1f : 1f;
        if (Mathf.Abs(transform.position.x - enemyPos.x) < 0.05f) pushDir = 1f;
        rb.linearVelocity = Vector2.zero;
        rb.AddForce(new Vector2(pushDir * knockbackForce, knockbackUpwardFactor),
            ForceMode2D.Impulse);
        yield return new WaitForSeconds(0.2f);
    }

    public void UpdateUIPublic() { UpdateUI(); }

    void UpdateUI()
    {
        if (healthBarAnimator != null)
            healthBarAnimator.SetInteger("currentHealth", currentHealth);
    }

    IEnumerator IFrames()
    {
        isInvincible = true;
        gameObject.layer = invincibleLayer;
        for (int i = 0; i < 5; i++)
        {
            if (!isPoisoned) playerSprite.color = new Color(1, 1, 1, 0.2f);
            yield return new WaitForSeconds(0.1f);
            if (!isPoisoned) playerSprite.color = Color.white;
            yield return new WaitForSeconds(0.1f);
        }
        gameObject.layer = normalLayer;
        isInvincible = false;
    }

    public void SetPoisonVisual(bool poisoned)
    {
        isPoisoned = poisoned;
        if (playerSprite == null) return;
        if (poisoned)
            playerSprite.color = new Color(0.5f, 0f, 0.8f, 1f);
        else if (!isInvincible)
            playerSprite.color = Color.white;
    }

    public void RestoreFullHealth()
    {
        currentHealth = maxHealth;
        UpdateUI();
        if (playerSprite != null) StartCoroutine(HealFlash());
    }

    private IEnumerator HealFlash()
    {
        playerSprite.color = Color.green;
        yield return new WaitForSeconds(0.3f);
        playerSprite.color = Color.white;
    }

    public void TakeLightningDamage(int damage)
    {
        if (currentHealth <= 0) return;
        currentHealth -= damage;
        currentHealth = Mathf.Max(currentHealth, 0);
        UpdateUI();
        StartCoroutine(LightningFlash());
        Debug.Log("Lightning damage: " + damage + " | Health: " + currentHealth);
        if (currentHealth <= 0) gameObject.SetActive(false);
    }

    IEnumerator LightningFlash()
    {
        if (playerSprite != null)
        {
            playerSprite.color = new Color(1f, 1f, 0f, 1f);
            yield return new WaitForSeconds(0.1f);
            if (!isPoisoned) playerSprite.color = Color.white;
        }
    }
}