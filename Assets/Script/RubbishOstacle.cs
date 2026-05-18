using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class RubbishObstacle : MonoBehaviour
{
    [Header("Settings")]
    public float clearRadius = 3f;       // how much rubbish each throw clears
    public int totalRubbishPieces = 10;  // total child pieces

    [Header("Visuals")]
    public List<GameObject> rubbishPieces = new List<GameObject>();

    private int remainingPieces;

    void Start()
    {
        // Auto-collect child pieces if not manually assigned
        if (rubbishPieces.Count == 0)
            foreach (Transform child in transform)
                rubbishPieces.Add(child.gameObject);

        remainingPieces = rubbishPieces.Count;
    }

    // Called by CleanThrowable on hit
    public void ClearArea(Vector3 hitPosition)
    {
        StartCoroutine(ClearNearby(hitPosition));
    }

    IEnumerator ClearNearby(Vector3 hitPos)
    {
        List<GameObject> toRemove = new List<GameObject>();

        foreach (GameObject piece in rubbishPieces)
        {
            if (piece == null) continue;
            if (Vector3.Distance(piece.transform.position, hitPos) <= clearRadius)
                toRemove.Add(piece);
        }

        foreach (GameObject piece in toRemove)
        {
            rubbishPieces.Remove(piece);
            remainingPieces--;

            // Fade out before destroying
            SpriteRenderer sr = piece.GetComponent<SpriteRenderer>();
            if (sr != null)
            {
                float elapsed = 0f;
                while (elapsed < 0.3f)
                {
                    elapsed += Time.deltaTime;
                    Color c = sr.color;
                    c.a = Mathf.Lerp(1f, 0f, elapsed / 0.3f);
                    sr.color = c;
                    yield return null;
                }
            }
            Destroy(piece);
        }

        // If all cleared remove collider
        if (remainingPieces <= 0)
        {
            Collider2D col = GetComponent<Collider2D>();
            if (col != null) col.enabled = false;
        }
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (!collision.gameObject.CompareTag("Player")) return;
        collision.gameObject.GetComponent<PlayerHealth>()
            ?.TakeDamage(1, transform.position);
    }
}