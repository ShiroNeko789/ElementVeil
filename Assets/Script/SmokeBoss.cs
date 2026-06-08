using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class SmokeBoss : MonoBehaviour
{
    [Header("Health")]
    public float maxHealth = 4f;
    private float currentHealth;
    public Image healthBarFill;
    public GameObject bossHUD;
    private float targetFillAmount = 1f;

    [Header("References")]
    public Transform player;

    [Header("Mosquito Movement")]
    public float flySpeed = 2.5f;
    public float phaseTwoSpeed = 4f;
    public float hoverAmplitude = 0.8f;
    public float hoverFrequency = 1.5f;
    public float chaseRange = 6f;
    public float retreatDistance = 2f;
    private float hoverTimer = 0f;
    private Vector3 originalScale;
    private bool isAttacking = false;   // separate flag from isInvulnerable

    [Header("Attack 1 — Bouncing Ball")]
    public GameObject bouncingBallPrefab;
    public Transform ballSpawnPoint;
    public int ballsPerAttack = 2;
    public float ballSpawnDelay = 0.4f;
    public float ballAttackCooldown = 4f;

    [Header("Attack 2 — Lightning Spawn")]
    public Transform[] lightningSpawnPoints;
    public GameObject lightningPrefab;
    public float lightningAttackDuration = 3f;
    public float lightningSpawnInterval = 0.3f;
    public float lightningAttackCooldown = 5f;

    [Header("Phase Two")]
    public float phaseTwoThreshold = 0.5f;
    private bool isPhaseTwo = false;

    private Rigidbody2D rb;
    private Animator anim;
    private PolygonCollider2D col;
    private bool isDead = false;
    private bool isInvulnerable = false;
    private float currentSpeed;

    void Start()
    {
        originalScale = transform.localScale;
        currentHealth = maxHealth;
        currentSpeed = flySpeed;
        rb = GetComponent<Rigidbody2D>();
        anim = GetComponent<Animator>();
        col = GetComponent<PolygonCollider2D>();

        if (player == null)
            player = GameObject.FindGameObjectWithTag("Player").transform;

        if (bossHUD != null) bossHUD.SetActive(false);
        if (healthBarFill != null) healthBarFill.fillAmount = 1f;

        rb.gravityScale = 0f;
        rb.simulated = true;

        StartCoroutine(MainBossRoutine());
        StartCoroutine(MosquitoMovement());
    }

    void Update()
    {
        if (isDead || player == null) return;

        float dir = player.position.x - transform.position.x;
        transform.localScale = new Vector3(
            dir > 0 ? Mathf.Abs(originalScale.x) : -Mathf.Abs(originalScale.x),
            originalScale.y,
            originalScale.z
        );

        if (healthBarFill != null)
            healthBarFill.fillAmount = Mathf.Lerp(
                healthBarFill.fillAmount, targetFillAmount, Time.deltaTime * 5f);

        hoverTimer += Time.deltaTime * hoverFrequency;
    }

    IEnumerator MosquitoMovement()
    {
        while (!isDead)
        {
            // Only stop movement when explicitly attacking
            // Keep moving during invulnerable phase transitions
            if (isAttacking)
            {
                // During attack just hover in place
                float hoverY = Mathf.Sin(hoverTimer) * hoverAmplitude * 0.5f;
                rb.linearVelocity = Vector2.Lerp(
                    rb.linearVelocity,
                    new Vector2(0, hoverY),
                    Time.deltaTime * 5f
                );
                yield return null;
                continue;
            }

            if (player == null) { yield return null; continue; }

            Vector2 toPlayer = (Vector2)player.position - (Vector2)transform.position;
            float distToPlayer = toPlayer.magnitude;
            float hoverOffset = Mathf.Sin(hoverTimer) * hoverAmplitude;

            Vector2 targetVelocity;

            if (distToPlayer > chaseRange)
            {
                // Chase player
                Vector2 chaseDir = toPlayer.normalized;
                targetVelocity = new Vector2(
                    chaseDir.x * currentSpeed,
                    chaseDir.y * currentSpeed + hoverOffset
                );
            }
            else if (distToPlayer < retreatDistance)
            {
                // Retreat
                Vector2 retreatDir = -toPlayer.normalized;
                targetVelocity = new Vector2(
                    retreatDir.x * currentSpeed,
                    retreatDir.y * currentSpeed + hoverOffset
                );
            }
            else
            {
                // Circle and hover
                Vector2 perpendicular = new Vector2(
                    -toPlayer.normalized.y,
                    toPlayer.normalized.x
                );
                targetVelocity = new Vector2(
                    perpendicular.x * currentSpeed * 0.8f,
                    hoverOffset * 2f
                );
            }

            rb.linearVelocity = Vector2.Lerp(
                rb.linearVelocity, targetVelocity, Time.deltaTime * 3f);

            yield return null;
        }

        rb.linearVelocity = Vector2.zero;
    }

    IEnumerator MainBossRoutine()
    {
        yield return new WaitForSeconds(2f);

        while (!isDead)
        {
            // Wait until not in phase transition
            while (isInvulnerable) yield return null;

            int attack = Random.Range(0, 2);
            if (attack == 0)
                yield return StartCoroutine(BouncingBallAttack());
            else
                yield return StartCoroutine(LightningAttack());

            yield return new WaitForSeconds(isPhaseTwo ? 0.8f : 1.5f);
        }
    }

    IEnumerator BouncingBallAttack()
    {
        isAttacking = true;
        anim.SetTrigger("SmokeAttack");
        yield return new WaitForSeconds(0.5f);

        int count = isPhaseTwo ? ballsPerAttack + 1 : ballsPerAttack;
        for (int i = 0; i < count; i++)
        {
            if (bouncingBallPrefab == null) continue;

            Vector3 spawnPos = ballSpawnPoint != null ?
                ballSpawnPoint.position : transform.position;

            GameObject ball = Instantiate(bouncingBallPrefab,
                spawnPos, Quaternion.identity);

            BouncingBall bb = ball.GetComponent<BouncingBall>();
            if (bb != null)
            {
                float dir = transform.localScale.x > 0 ? 1f : -1f;
                bb.Launch(dir, this);
            }

            yield return new WaitForSeconds(ballSpawnDelay);
        }

        yield return new WaitForSeconds(ballAttackCooldown);
        isAttacking = false;
    }

    IEnumerator LightningAttack()
    {
        isAttacking = true;
        yield return new WaitForSeconds(0.3f);

        float elapsed = 0f;
        while (elapsed < lightningAttackDuration)
        {
            if (lightningSpawnPoints.Length > 0)
            {
                int idx = Random.Range(0, lightningSpawnPoints.Length);
                if (lightningPrefab != null)
                    Instantiate(lightningPrefab,
                        lightningSpawnPoints[idx].position,
                        Quaternion.identity);
            }
            elapsed += lightningSpawnInterval;
            yield return new WaitForSeconds(lightningSpawnInterval);
        }

        yield return new WaitForSeconds(lightningAttackCooldown);
        isAttacking = false;
    }

    public void TakeDamage(float amount)
    {
        if (isDead) return;
        currentHealth -= amount;
        currentHealth = Mathf.Max(currentHealth, 0f);
        targetFillAmount = currentHealth / maxHealth;
        StartCoroutine(FlashDamage());
        Debug.Log("SmokeBoss hit — " + currentHealth + "/" + maxHealth);

        if (currentHealth <= maxHealth * phaseTwoThreshold && !isPhaseTwo)
            StartCoroutine(PhaseTwo());

        if (currentHealth <= 0)
            StartCoroutine(Death());
    }

    IEnumerator FlashDamage()
    {
        SpriteRenderer sr = GetComponent<SpriteRenderer>();
        if (sr != null)
        {
            sr.color = Color.red;
            yield return new WaitForSeconds(0.1f);
            sr.color = Color.white;
        }
    }

    IEnumerator PhaseTwo()
    {
        isPhaseTwo = true;
        isInvulnerable = true;
        isAttacking = true;

        anim.SetTrigger("SmokeIdle");
        yield return new WaitForSeconds(2f);

        currentSpeed = phaseTwoSpeed;
        hoverFrequency *= 1.5f;
        hoverAmplitude *= 1.3f;
        ballsPerAttack++;
        lightningSpawnInterval *= 0.7f;

        isInvulnerable = false;
        isAttacking = false;
    }

    IEnumerator Death()
    {
        isDead = true;
        isAttacking = false;
        rb.linearVelocity = Vector2.zero;
        anim.SetTrigger("SmokeDead");
        if (col != null) col.enabled = false;
        rb.simulated = false;
        targetFillAmount = 0f;
        yield return new WaitForSeconds(1f);
        if (bossHUD != null) bossHUD.SetActive(false);
    }

    public bool IsDead() { return isDead; }
}