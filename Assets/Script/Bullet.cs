using UnityEngine;

public class Bullet : MonoBehaviour
{
    public float speed = 15f;
    public float lifeTime = 2f;
    public int damage = 1;
    public float rustPower = 20f;
    private Vector2 direction;
    private bool hasHit = false;

    public void SetDirection(Vector2 dir, float playerScaleX)
    {
        direction = dir.normalized;
        Vector3 newScale = transform.localScale;
        newScale.x = Mathf.Abs(newScale.x) * Mathf.Sign(playerScaleX);
        transform.localScale = newScale;
    }

    void Start()
    {
        Destroy(gameObject, lifeTime);
    }

    void Update()
    {
        if (hasHit) return;
        transform.Translate(direction * speed * Time.deltaTime, Space.World);
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (hasHit) return;

        EnemyHealth enemy = collision.GetComponent<EnemyHealth>();
        if (enemy != null)
        {
            enemy.TakeDamage(damage);
            PlayExplode();
            return;
        }

        MushroomBoss boss = collision.GetComponentInParent<MushroomBoss>();
        if (boss != null)
        {
            boss.TakeDamage(damage);
            PlayExplode();
            return;
        }

        // Hit SmokeBoss — only damages when vulnerable
        SmokeBoss smokeBoss = collision.GetComponentInParent<SmokeBoss>();
        if (smokeBoss != null)
        {
            smokeBoss.TakeDamage(damage);
            PlayExplode();
            return;
        }

        RustableItem rustItem = collision.GetComponent<RustableItem>();
        if (rustItem != null)
        {
            rustItem.ApplyWaterDamage(rustPower);
            PlayExplode();
            return;
        }

        if (collision.CompareTag("Ground"))
        {
            PlayExplode();
        }
    }

    void PlayExplode()
    {
        hasHit = true;
        Collider2D col = GetComponent<Collider2D>();
        if (col != null) col.enabled = false;

        Animator anim = GetComponent<Animator>();
        if (anim != null)
        {
            anim.SetTrigger("Explode");
            float clipLength = 0.3f;
            foreach (AnimationClip clip in anim.runtimeAnimatorController.animationClips)
            {
                if (clip.name == "BulletExplode")
                {
                    clipLength = clip.length;
                    break;
                }
            }
            Destroy(gameObject, clipLength);
        }
        else
        {
            Destroy(gameObject);
        }
    }
}