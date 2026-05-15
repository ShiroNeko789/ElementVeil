using UnityEngine;
using System.Collections;

public class PlayerHealth : MonoBehaviour
{
    public int maxHealth = 5;
    public int currentHealth;
    private bool isInvincible = false;

    [Header("Repel / Knockback")]
    public float knockbackForce = 12f;
    public float knockbackUpwardFactor = 4f;

    [Header("Visuals")]
    public Animator healthBarAnimator;
    public SpriteRenderer playerSprite;

    // Set these in Inspector after creating your layers
    public int normalLayer;       // the "Player" layer number
    public int invincibleLayer;   // the "PlayerInvincible" layer number

    private Rigidbody2D rb;

    void Start()
    {
        currentHealth = maxHealth;
        rb = GetComponent<Rigidbody2D>();
        // Grab layer numbers automatically by name so you don't have to type numbers
        normalLayer = LayerMask.NameToLayer("Player");
        invincibleLayer = LayerMask.NameToLayer("PlayerInvincible");
        UpdateUI();
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Enemy") && !isInvincible)
        {
            TakeDamage(1, collision.transform.position);
        }
    }

    public void TakeDamage(int damage, Vector2 enemyPosition)
    {
        if (isInvincible || currentHealth <= 0) return;
        currentHealth -= damage;
        UpdateUI();

        if (currentHealth <= 0)
        {
            gameObject.SetActive(false);
        }
        else
        {
            StopAllCoroutines();
            StartCoroutine(ApplyKnockback(enemyPosition));
            StartCoroutine(IFrames());
        }
    }

    private IEnumerator ApplyKnockback(Vector2 enemyPos)
    {
        float pushDir = (transform.position.x < enemyPos.x) ? -1f : 1f;
        if (Mathf.Abs(transform.position.x - enemyPos.x) < 0.05f) pushDir = 1f;
        rb.linearVelocity = Vector2.zero;
        rb.AddForce(new Vector2(pushDir * knockbackForce, knockbackUpwardFactor), ForceMode2D.Impulse);
        yield return new WaitForSeconds(0.2f);
    }

    void UpdateUI()
    {
        if (healthBarAnimator != null)
            healthBarAnimator.SetInteger("currentHealth", currentHealth);
    }

    IEnumerator IFrames()
    {
        isInvincible = true;
        gameObject.layer = invincibleLayer; // swap layer → physics ignores enemy collision instantly

        for (int i = 0; i < 5; i++)
        {
            playerSprite.color = new Color(1, 1, 1, 0.2f);
            yield return new WaitForSeconds(0.1f);
            playerSprite.color = Color.white;
            yield return new WaitForSeconds(0.1f);
        }

        gameObject.layer = normalLayer; // swap back → collision resumes
        isInvincible = false;
    }

    public void RestoreFullHealth()
    {
        currentHealth = maxHealth;
        UpdateUI();
        if (playerSprite != null)
            StartCoroutine(HealFlash());
    }

    private IEnumerator HealFlash()
    {
        playerSprite.color = Color.green;
        yield return new WaitForSeconds(0.3f);
        playerSprite.color = Color.white;
    }
}