using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class MushroomBoss : MonoBehaviour
{
    [Header("Health & Visuals")]
    public float maxHealth = 30f;
    private float currentHealth;
    public Slider healthSlider;
    public Color damageColor = Color.red;
    private Color originalColor;
    private SpriteRenderer spriteRenderer;

    [Header("Movement")]
    public float walkSpeed = 1.2f;
    public float phaseTwoSpeed = 2.5f;
    private float currentSpeed;

    [Header("Phase Two - Ceiling Attack")]
    public float ceilingY = 10f;              // Y position above the room (off screen)
    public float returnY = 1f;               // Y position boss lands back on ground
    public float ceilingNailInterval = 0.18f; // How fast nails rain down
    public int ceilingNailCount = 20;         // Total nails during ceiling phase
    public float ceilingDuration = 3f;        // How long boss stays hidden up top

    [Header("Setup")]
    public Transform player;
    public GameObject nailPrefab;
    public Transform[] nailSpawnPoints;

    private Rigidbody2D rb;
    private Animator anim;
    private Collider2D col;
    private bool isInvulnerable = false;
    private bool isPhaseTwo = false;
    private bool isDead = false;
    private bool fightStarted = false;

    void Start()
    {
        currentHealth = maxHealth;
        currentSpeed = walkSpeed;
        rb = GetComponent<Rigidbody2D>();
        anim = GetComponent<Animator>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        col = GetComponent<Collider2D>();

        if (spriteRenderer != null) originalColor = spriteRenderer.color;
        if (player == null) player = GameObject.FindGameObjectWithTag("Player").transform;

        // Don't start routine here — wait for BossRoomTrigger to call PlayIntroScream
        rb.simulated = false;
    }

    // Add this method
    public IEnumerator PlayIntroScream()
    {
        isInvulnerable = true;
        rb.simulated = false;

        anim.SetTrigger("Scream");
        yield return new WaitForSeconds(2f);

        isInvulnerable = false;
        rb.simulated = true;
        fightStarted = true;

        StartCoroutine(MainBossRoutine());
    }

    void Update()
    {
        if (isDead || player == null) return;
        float dirToPlayer = player.position.x - transform.position.x;
        transform.localScale = new Vector3(dirToPlayer > 0 ? 1f : -1f, 1f, 1f);
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
        if (healthSlider != null) healthSlider.value = currentHealth;
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

        // Play scream animation
        anim.SetTrigger("Scream");
        yield return new WaitForSeconds(2f); // let MushroomScream finish

        // Speed up all movement for phase 2
        currentSpeed = phaseTwoSpeed;

        // Now do the ceiling attack
        yield return StartCoroutine(CeilingAttack());

        isInvulnerable = false;
    }

    IEnumerator CeilingAttack()
    {
        // 1. Boss flies up off screen
        rb.simulated = false;             // disable physics so we move it manually
        col.enabled = false;              // no collisions while off screen
        spriteRenderer.enabled = false;   // hide the boss

        // Teleport above the room
        transform.position = new Vector2(transform.position.x, ceilingY);

        // 2. Rain nails down rapidly from random spawn points
        for (int i = 0; i < ceilingNailCount; i++)
        {
            if (nailSpawnPoints.Length > 0)
            {
                int idx = Random.Range(0, nailSpawnPoints.Length);
                Instantiate(nailPrefab, nailSpawnPoints[idx].position, Quaternion.identity);
            }
            yield return new WaitForSeconds(ceilingNailInterval);
        }

        // 3. Wait any remaining ceiling duration
        yield return new WaitForSeconds(ceilingDuration);

        // 4. Boss slams back down to the ground
        transform.position = new Vector2(player.position.x, returnY); // land near player
        rb.simulated = true;
        col.enabled = true;
        spriteRenderer.enabled = true;

        anim.SetTrigger("Land"); // reuse your landing animation
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
        anim.SetTrigger("Die");
        col.enabled = false;
        rb.simulated = false;
        yield return null;
    }

    public bool IsDead()
    {
        return isDead;
    }
}