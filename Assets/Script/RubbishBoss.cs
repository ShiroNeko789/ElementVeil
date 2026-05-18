using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class RubbishBoss : MonoBehaviour
{
    [Header("Health")]
    public float maxHealth = 50f;
    private float currentHealth;
    public Image healthBarFill;
    public GameObject bossHUD;
    private float targetFillAmount = 1f;

    [Header("Attack 1 — Hand Throw (RubbishAttack)")]
    [Tooltip("Place your empty GameObjects on the RIGHT side of the screen here.")]
    public Transform[] handSpawnPoints;
    public GameObject handPrefab;
    public float handAttackCooldown = 4f;

    [Header("Attack 2 — Rubbish Spray (RubbishAttack2)")]
    public Transform rubbishSprayOrigin;
    public GameObject[] safeRubbishPrefabs;    // Safe, non-damaging ground pickups
    public GameObject[] hazardRubbishPrefabs;  // Dangerous ghost-like flying hazards
    public int rubbishSprayCount = 8;
    public float rubbishSprayCooldown = 6f;
    [Range(0f, 45f)] public float spraySpreadAngle = 20f;

    [Header("Spray Distance Controls")]
    public float minSprayForce = 6f;
    public float maxSprayForce = 12f;

    [Header("Recycle Bins")]
    public RecycleBin[] recycleBins;
    public float damagePerCorrectRecycle = 5f;

    [Header("Phase Two")]
    public float phaseTwoHealthThreshold = 0.5f;
    private bool isPhaseTwo = false;

    private Animator anim;
    private PolygonCollider2D col;
    private Rigidbody2D rb;
    private Transform player;
    private bool isDead = false;
    private bool isInvulnerable = false;
    private bool isAttacking = false;

    void Start()
    {
        currentHealth = maxHealth;
        anim = GetComponent<Animator>();
        col = GetComponent<PolygonCollider2D>();
        rb = GetComponent<Rigidbody2D>();

        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null) player = playerObj.transform;

        if (rb != null) rb.simulated = true;

        if (bossHUD != null) bossHUD.SetActive(true);
        if (healthBarFill != null) healthBarFill.fillAmount = 1f;

        StartCoroutine(MainBossRoutine());
    }

    void Update()
    {
        if (isDead || player == null) return;

        float dir = player.position.x - transform.position.x;
        transform.localScale = new Vector3(dir > 0 ? 1f : -1f, 1f, 1f);

        if (healthBarFill != null)
        {
            healthBarFill.fillAmount = Mathf.Lerp(
                healthBarFill.fillAmount, targetFillAmount, Time.deltaTime * 5f);
        }
    }

    IEnumerator MainBossRoutine()
    {
        yield return new WaitForSeconds(1.0f);

        while (!isDead)
        {
            if (isInvulnerable || isAttacking) { yield return null; continue; }

            int attack = Random.Range(0, 2);
            if (attack == 0)
                yield return StartCoroutine(HandThrowAttack());
            else
                yield return StartCoroutine(RubbishSprayAttack());

            yield return new WaitForSeconds(isPhaseTwo ? 0.6f : 1.5f);
        }
    }

    IEnumerator HandThrowAttack()
    {
        isAttacking = true;
        anim.SetTrigger("Attack1");
        yield return new WaitForSeconds(0.5f);

        if (handSpawnPoints.Length > 0 && handPrefab != null)
        {
            int randomIndex = Random.Range(0, handSpawnPoints.Length);
            Transform chosenSpawnPoint = handSpawnPoints[randomIndex];
            Instantiate(handPrefab, chosenSpawnPoint.position, Quaternion.identity);
        }

        yield return new WaitForSeconds(handAttackCooldown);
        isAttacking = false;
    }

    IEnumerator RubbishSprayAttack()
    {
        isAttacking = true;
        anim.SetTrigger("Attack2");
        yield return new WaitForSeconds(0.8f);

        Vector3 spawnPos = rubbishSprayOrigin != null ? rubbishSprayOrigin.position : transform.position;
        float facingDir = transform.localScale.x;

        // GUARANTEED MINIMUM 2 SAFE PICKUPS DROP FIRST EVERY TIME
        int guaranteedSafeCount = 2;
        if (safeRubbishPrefabs.Length > 0)
        {
            for (int i = 0; i < guaranteedSafeCount; i++)
            {
                int randIdx = Random.Range(0, safeRubbishPrefabs.Length);
                GameObject safeTrash = Instantiate(safeRubbishPrefabs[randIdx], spawnPos, Quaternion.identity);

                Collider2D trashCollider = safeTrash.GetComponent<Collider2D>();
                if (trashCollider != null && col != null)
                {
                    Physics2D.IgnoreCollision(trashCollider, col, true);
                }

                // Inject the landing script helper
                if (safeTrash.GetComponent<SafeGroundTrash>() == null)
                {
                    safeTrash.AddComponent<SafeGroundTrash>();
                }

                Rigidbody2D trashRb = safeTrash.GetComponent<Rigidbody2D>();
                if (trashRb != null)
                {
                    float horizontalToss = Random.Range(3f, 6f) * facingDir;
                    trashRb.linearVelocity = new Vector2(horizontalToss, Random.Range(4f, 7f));
                }
            }
        }

        // SPAWN THE DANGEROUS HAZARDS THAT FLY OUT OF THE MAP
        for (int i = 0; i < rubbishSprayCount; i++)
        {
            if (hazardRubbishPrefabs.Length == 0 || player == null) break;

            int randIdx = Random.Range(0, hazardRubbishPrefabs.Length);
            GameObject hazard = Instantiate(hazardRubbishPrefabs[randIdx], spawnPos, Quaternion.identity);

            RubbishProjectile rp = hazard.GetComponent<RubbishProjectile>();
            if (rp != null)
            {
                Vector2 directionToPlayer = (player.position - spawnPos).normalized;
                float baseAngle = Mathf.Atan2(directionToPlayer.y, Mathf.Abs(directionToPlayer.x)) * Mathf.Rad2Deg;
                float finalAngle = baseAngle + Random.Range(-spraySpreadAngle, spraySpreadAngle);

                float randomLaunchForce = Random.Range(minSprayForce, maxSprayForce);
                if (isPhaseTwo) randomLaunchForce *= 1.3f;

                rp.Launch(finalAngle, facingDir, randomLaunchForce);
            }

            yield return new WaitForSeconds(0.08f);
        }

        yield return new WaitForSeconds(rubbishSprayCooldown);
        isAttacking = false;
    }

    public void OnCorrectRecycle()
    {
        TakeDamage(damagePerCorrectRecycle, true);
    }

    public void TakeDamage(float amount, bool bypassInvulnerability = false)
    {
        if (isDead) return;
        if (isInvulnerable && !bypassInvulnerability) return;

        currentHealth -= amount;
        currentHealth = Mathf.Max(currentHealth, 0f);
        targetFillAmount = currentHealth / maxHealth;

        StartCoroutine(FlashDamage());

        if (currentHealth <= maxHealth * phaseTwoHealthThreshold && !isPhaseTwo)
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

        SpriteRenderer sr = GetComponent<SpriteRenderer>();
        if (sr != null)
        {
            for (int i = 0; i < 4; i++)
            {
                sr.color = Color.red;
                yield return new WaitForSeconds(0.2f);
                sr.color = Color.white;
                yield return new WaitForSeconds(0.2f);
            }
        }
        else
        {
            yield return new WaitForSeconds(1.6f);
        }

        rubbishSprayCount += 4;
        isInvulnerable = false;
    }

    IEnumerator Death()
    {
        isDead = true;
        anim.SetTrigger("Die");

        // Keep rb and collider ACTIVE during the death animation
        // so the player can't walk through the boss body while it plays out
        if (rb != null) rb.simulated = false; // Stop physics movement, but collider stays

        targetFillAmount = 0f;

        // Wait for death animation to finish BEFORE disabling the collider
        yield return new WaitForSeconds(1f);

        if (col != null) col.enabled = false; // NOW it's safe to remove the solid body

        if (bossHUD != null) bossHUD.SetActive(false);
    }

    public bool IsDead() { return isDead; }
}

// ==========================================
// FIXED HELPER FOR INTERACT MANAGER MATRIX
// ==========================================
public class SafeGroundTrash : MonoBehaviour
{
    private Rigidbody2D rb;
    private bool landed = false;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    void OnCollisionEnter2D(Collision2D collision)
    {
        if (landed) return;

        // Detect floor tags or environment names
        if (collision.gameObject.CompareTag("Ground") || collision.gameObject.name.Contains("ground") || collision.gameObject.name.Contains("wall"))
        {
            landed = true;

            if (rb != null)
            {
                rb.linearVelocity = Vector2.zero;
                rb.angularVelocity = 0f;
                rb.bodyType = RigidbodyType2D.Kinematic; // Lock physical body movement
                rb.useFullKinematicContacts = true;
            }

            // FIXED FOR INTERACT MANAGER: Instead of modifying the existing solid collider into a trigger, 
            // we dynamically generate a secondary trigger box specifically for the Player detection layers!
            BoxCollider2D interactionTrigger = gameObject.AddComponent<BoxCollider2D>();
            interactionTrigger.isTrigger = true;

            // Inflate trigger bounds slightly outward so it forces register even if player stands directly on it
            interactionTrigger.size = new Vector2(1.5f, 1.5f);
        }
    }
}