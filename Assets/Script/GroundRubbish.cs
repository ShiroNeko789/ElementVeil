using UnityEngine;
using System.Collections;

/// <summary>
/// Attached ONLY to the ground rubbish prefab (spawned after landing).
/// 
/// PREFAB SETUP:
/// - Root object: SpriteRenderer + Rigidbody2D + a SOLID collider (isTrigger = OFF) for physics.
/// - Child object named "InteractTrigger": a CircleCollider2D or BoxCollider2D with isTrigger = ON.
///   The GroundRubbish script auto-creates this child if it doesn't exist.
/// 
/// This keeps physics collision (solid) and player detection (trigger) on separate colliders,
/// which avoids the conflict that caused player damage and blocked pickup.
/// </summary>
public class GroundRubbish : MonoBehaviour, IInteractable
{
    public RubbishType rubbishType;

    [Header("Lifetime")]
    [Tooltip("How long (seconds) before this rubbish despawns. Set to 0 to never despawn.")]
    public float lifetime = 10f;
    [Tooltip("How many seconds before despawn the flashing warning starts.")]
    public float flashWarningDuration = 3f;

    private bool isPickedUp = false;

    private void Start()
    {
        EnsureInteractTrigger();

        if (lifetime > 0f)
            StartCoroutine(LifetimeRoutine());
    }

    IEnumerator LifetimeRoutine()
    {
        // Wait until the flash warning window
        float waitTime = lifetime - flashWarningDuration;
        if (waitTime > 0f)
            yield return new WaitForSeconds(waitTime);

        // Flash to warn the player it's about to vanish
        SpriteRenderer sr = GetComponent<SpriteRenderer>();
        float elapsed = 0f;
        float flashInterval = 0.2f;

        while (elapsed < flashWarningDuration)
        {
            if (isPickedUp) yield break; // Picked up mid-flash, stop cleanly

            if (sr != null) sr.enabled = !sr.enabled;
            yield return new WaitForSeconds(flashInterval);
            elapsed += flashInterval;
        }

        if (isPickedUp) yield break;

        // Unregister from interact manager before destroying
        InteractManager.Instance.UnregisterInteractable(this);
        Destroy(gameObject);
    }

    /// <summary>
    /// Automatically creates a trigger child for player detection if one doesn't already exist.
    /// This means you can just drop GroundRubbish.cs on a prefab and it self-configures.
    /// </summary>
    private void EnsureInteractTrigger()
    {
        // Check if a trigger child already exists (set up manually in prefab)
        Transform existing = transform.Find("InteractTrigger");
        if (existing != null) return;

        // Auto-create trigger child
        GameObject triggerChild = new GameObject("InteractTrigger");
        triggerChild.transform.SetParent(transform, false);
        triggerChild.layer = gameObject.layer;

        // Add a trigger collider slightly larger than the object for comfortable pickup range
        CircleCollider2D trigger = triggerChild.AddComponent<CircleCollider2D>();
        trigger.isTrigger = true;
        trigger.radius = 0.8f;

        // Route trigger events back to this script via a helper
        GroundRubbishTrigger helper = triggerChild.AddComponent<GroundRubbishTrigger>();
        helper.owner = this;
    }

    // Called by GroundRubbishTrigger when the player enters
    public void OnPlayerEnter()
    {
        InteractManager.Instance.RegisterInteractable(this);
    }

    // Called by GroundRubbishTrigger when the player exits
    public void OnPlayerExit()
    {
        InteractManager.Instance.UnregisterInteractable(this);
    }

    public void OnInteract()
    {
        RubbishCarrier carrier = RubbishCarrier.Instance;
        if (carrier == null) return;

        isPickedUp = true; // Stops the lifetime coroutine from destroying after pickup
        carrier.PickupRubbish(this);
        InteractManager.Instance.UnregisterInteractable(this);
        gameObject.SetActive(false);
    }
}

/// <summary>
/// Lives on the InteractTrigger child object.
/// Detects the Player tag and notifies the parent GroundRubbish.
/// </summary>
public class GroundRubbishTrigger : MonoBehaviour
{
    [HideInInspector] public GroundRubbish owner;

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player") && owner != null)
            owner.OnPlayerEnter();
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player") && owner != null)
            owner.OnPlayerExit();
    }
}