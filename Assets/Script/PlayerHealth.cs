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

    void Start()
    {
        currentHealth = maxHealth;
        rb = GetComponent<Rigidbody2D>();
        normalLayer = LayerMask.NameToLayer("Player");
        invincibleLayer = LayerMask.NameToLayer("PlayerInvincible");
        UpdateUI();
    }

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
            // Only start iframes if not poisoned — poison handles its own visuals
            if (!isPoisoned)
                StartCoroutine(IFrames());
        }
    }

    // Poison damage bypasses iframes
    public void TakePoisonDamage(int damage)
    {
        if (currentHealth <= 0) return;
        currentHealth -= damage;
        currentHealth = Mathf.Max(currentHealth, 0);
        UpdateUI();
        Debug.Log("Poison damage: " + damage + " | Health: " + currentHealth);

        if (currentHealth <= 0)
            gameObject.SetActive(false);
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
}