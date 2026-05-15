using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class MushroomBoss : MonoBehaviour
{
    [Header("Health & Visuals")]
    public float maxHealth = 30f;
    private float currentHealth;
    public Color damageColor = Color.red;
    private Color originalColor;
    private SpriteRenderer spriteRenderer;

    [Header("Health Bar")]
    public GameObject bossHUD;
    public Image healthBarFill;          // Image Type must be Filled + Horizontal in Inspector
    public float healthBarLerpSpeed = 5f;
    private float targetFillAmount = 1f;

    [Header("Movement")]
    public float walkSpeed = 1.2f;
    public float phaseTwoSpeed = 2.5f;
    private float currentSpeed;

    [Header("Phase Two - Ceiling Attack")]
    public float ceilingY = 40f;
    public float returnY = 25f;
    public float ceilingNailInterval = 0.18f;
    public int ceilingNailCount = 20;
    public float ceilingDuration = 3f;

    [Header("Setup")]
    public Transform player;
    public GameObject nailPrefab;
    public Transform[] nailSpawnPoints;
    public ExitWall exitWall;            // drag your ExitWall GameObject here

    private Rigidbody2D rb;
    private Animator anim;
    private PolygonCollider2D col;
    private bool isInvulnerable = false;
    private bool isPhaseTwo = false;
    private bool isDead = false;

    void Start()
    {
        currentHealth = maxHealth;
        currentSpeed = walkSpeed;
        rb = GetComponent<Rigidbody2D>();
        anim = GetComponent<Animator>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        col = GetComponent<PolygonCollider2D>();

        if (col == null) Debug.LogError("NO COLLIDER FOUND ON BOSS");
        if (rb == null) Debug.LogError("NO RIGIDBODY FOUND ON BOSS");

        if (spriteRenderer != null) originalColor = spriteRenderer.color;
        if (player == null) player = GameObject.FindGameObjectWithTag("Player").transform;

        if (bossHUD != null) bossHUD.SetActive(false);

        // Make sure fill starts at full
        if (healthBarFill != null)
        {
            healthBarFill.fillAmount = 1f;
            targetFillAmount = 1f;
        }

        rb.simulated = false;
    }

    void Update()
    {
        if (isDead || player == null) return;

        float dirToPlayer = player.position.x - transform.position.x;
        transform.localScale = new Vector3(dirToPlayer > 0 ? 1f : -1f, 1f, 1f);

        // Smoothly animate health bar fill toward target
        if (healthBarFill != null && Mathf.Abs(healthBarFill.fillAmount - targetFillAmount) > 0.001f)
        {
            healthBarFill.fillAmount = Mathf.Lerp(
                healthBarFill.fillAmount,
                targetFillAmount,
                Time.deltaTime * healthBarLerpSpeed
            );
        }
    }

    public IEnumerator PlayIntroScream()
    {
        isInvulnerable = true;
        rb.simulated = false;

        anim.SetTrigger("Scream");
        yield return new WaitForSeconds(2f);

        isInvulnerable = false;
        rb.simulated = true;

        StartCoroutine(MainBossRoutine());
    }

    IEnumerator MainBossRoutine()
    {
        while (!isDead)
        {
            if (isInvulnerable) { yield return null; continue; }

            yield return StartCoroutine(WalkPhase(3f));

            int choice = Random.Range(0, 2);
            if (choice == 0) yield return StartCoroutine(JumpAttack());
            else yield return StartCoroutine(SlashAttack());

            rb.linearVelocity = new Vector2(0, rb.linearVelocity.y);
            yield return new WaitForSeconds(isPhaseTwo ? 0.6f : 1.2f);
        }
    }

    IEnumerator WalkPhase(float duration)
    {
        float timer = 0;
        while (timer < duration && !isDead && !isInvulnerable)
        {
            float moveDir = (player.position.x > transform.position.x) ? 1 : -1;
            rb.linearVelocity = new Vector2(moveDir * currentSpeed, rb.linearVelocity.y);
            timer += Time.deltaTime;
            yield return null;
        }
    }

    IEnumerator JumpAttack()
    {
        anim.SetTrigger("Jump");
        float jumpDir = (player.position.x > transform.position.x) ? 1 : -1;
        rb.linearVelocity = new Vector2(jumpDir * (currentSpeed * 2), 12f);
        yield return new WaitForSeconds(1.2f);

        anim.SetTrigger("Land");
        SpawnNails(isPhaseTwo ? 8 : 4);
        rb.linearVelocity = Vector2.zero;
    }

    IEnumerator SlashAttack()
    {
        float dashDir = (player.position.x > transform.position.x) ? 1 : -1;
        rb.linearVelocity = new Vector2(dashDir * (currentSpeed * 3f), rb.linearVelocity.y);
        yield return new WaitForSeconds(0.4f);
        rb.linearVelocity = Vector2.zero;
        anim.SetTrigger("Swipe");
        yield return new WaitForSeconds(0.6f);
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            collision.gameObject.GetComponent<PlayerHealth>()?.TakeDamage(1, transform.position);
        }
    }

    public void TakeDamage(float amount)
    {
        if (isInvulnerable || isDead) return;
        currentHealth -= amount;
        currentHealth = Mathf.Max(currentHealth, 0f);

        // Set target — Update() lerps fill toward this each frame
        targetFillAmount = currentHealth / maxHealth;

        // Force an immediate partial update so it never looks frozen
        if (healthBarFill != null)
            healthBarFill.fillAmount = Mathf.Max(
                healthBarFill.fillAmount - 0.01f,  // nudge it so lerp activates
                targetFillAmount
            );

        StartCoroutine(FlashDamage());

        if (currentHealth <= maxHealth * 0.5f && !isPhaseTwo)
            StartCoroutine(PhaseTransition());

        if (currentHealth <= 0)
            StartCoroutine(Death());
    }

    IEnumerator FlashDamage()
    {
        spriteRenderer.color = damageColor;
        yield return new WaitForSeconds(0.1f);
        spriteRenderer.color = originalColor;
    }

    IEnumerator PhaseTransition()
    {
        isInvulnerable = true;
        isPhaseTwo = true;
        rb.linearVelocity = Vector2.zero;

        anim.SetTrigger("Scream");
        yield return new WaitForSeconds(2f);

        currentSpeed = phaseTwoSpeed;
        yield return StartCoroutine(CeilingAttack());

        isInvulnerable = false;
    }

    IEnumerator CeilingAttack()
    {
        rb.linearVelocity = Vector2.zero;
        rb.simulated = false;
        col.enabled = false;
        spriteRenderer.enabled = false;

        transform.position = new Vector2(transform.position.x, ceilingY);

        for (int i = 0; i < ceilingNailCount; i++)
        {
            if (nailSpawnPoints.Length > 0)
            {
                int idx = Random.Range(0, nailSpawnPoints.Length);
                Instantiate(nailPrefab, nailSpawnPoints[idx].position, Quaternion.identity);
            }
            yield return new WaitForSeconds(ceilingNailInterval);
        }

        yield return new WaitForSeconds(ceilingDuration);

        transform.position = new Vector2(player.position.x, returnY);
        yield return new WaitForSeconds(0.05f);

        rb.simulated = true;
        rb.linearVelocity = Vector2.zero;
        col.enabled = true;
        spriteRenderer.enabled = true;

        anim.SetTrigger("Land");
        yield return new WaitForSeconds(0.5f);
    }

    void SpawnNails(int count)
    {
        if (nailSpawnPoints.Length == 0) return;
        for (int i = 0; i < count; i++)
        {
            int idx = Random.Range(0, nailSpawnPoints.Length);
            Instantiate(nailPrefab, nailSpawnPoints[idx].position, Quaternion.identity);
        }
    }

    IEnumerator Death()
    {
        isDead = true;
        rb.linearVelocity = Vector2.zero;
        anim.SetTrigger("Die");
        if (col != null) col.enabled = false;
        rb.simulated = false;

        // Drain health bar to zero visually
        targetFillAmount = 0f;
        yield return new WaitForSeconds(1f);

        // Hide HUD
        if (bossHUD != null) bossHUD.SetActive(false);

        // Open the exit
        if (exitWall != null) exitWall.OpenWall();
    }
}